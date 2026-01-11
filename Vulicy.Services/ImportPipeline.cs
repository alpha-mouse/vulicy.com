using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Linemerge;
using Vulicy.Domain;

namespace Vulicy.Services;

public partial class ImportPipeline(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ImportPipeline>? logger
) : IImportPipeline
{
    private const int DossierSimilarityThreshold = 3;

    private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 4326);

    public void StartRunning(int importId, ImportType importType, CancellationToken cancellationToken)
    {
        _ = Run(importId, importType, cancellationToken);
    }

    private async Task Run(int importId, ImportType importType, CancellationToken cancellationToken)
    {
        try
        {
            if (!await DownloadImport(importId, cancellationToken))
                return;

            cancellationToken.ThrowIfCancellationRequested();
            if (!await StageImport(importId, importType, cancellationToken))
                return;

            cancellationToken.ThrowIfCancellationRequested();
            if (!await MergeImport(importId, importType, cancellationToken))
                return;

            cancellationToken.ThrowIfCancellationRequested();
            await CleanImport(importId, importType, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await MatchMissingOsmCadastre(cancellationToken);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            LogCatastrophicImportError(e);
        }
    }

    private async Task<bool> DownloadImport(int importId, CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var importRepository = scope.ServiceProvider.GetRequiredService<IImportRepository>();
        var import = await importRepository.GetByIdTracked(importId);
        if (import is not { Status: ImportStatus.Pending })
        {
            LogNotDownloading(importId, import?.Status);
            return false;
        }

        try
        {
            LogDownloadStarting(importId, import.DownloadUrl);

            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(import.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using (var fileStream = new FileStream(import.LocalPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                await response.Content.CopyToAsync(fileStream, cancellationToken);
            }

            import.Status = ImportStatus.Downloaded;
            await importRepository.SaveChanges();

            LogDownloadComplete(importId, import.LocalPath);
            return true;
        }
        catch (Exception e)
        {
            LogDownloadError(e, importId);

            import.Status = ImportStatus.DownloadFailed;
            import.Error = e.Message;
            await importRepository.SaveChanges();
            return false;
        }
    }

    private async Task<bool> StageImport(int importId, ImportType importType, CancellationToken cancellationToken)
    {
        await using var operationScope = serviceScopeFactory.CreateAsyncScope();
        var importRepository = operationScope.ServiceProvider.GetRequiredService<IImportRepository>();
        var import = await importRepository.GetByIdTracked(importId);
        if (import is not { Status: ImportStatus.Downloaded })
        {
            LogNotStaging(importId, import?.Status);
            return false;
        }

        await using var importServiceScope = serviceScopeFactory.CreateAsyncScope();
        try
        {
            LogStagingStarting(importId, import.LocalPath);
            if (importType == ImportType.OpenStreetMap)
                await importServiceScope.ServiceProvider.GetRequiredService<IOsmImportService>().StageImport(importId, import.LocalPath, cancellationToken);
            else
            {
                var importService = importServiceScope.ServiceProvider.GetRequiredService<ICadastreImportService>();
                var considerInitial = importType == ImportType.CadastreInitial;
                await importService.StageImport(import.Id, import.LocalPath, considerInitial, cancellationToken);
            }

            import.Status = ImportStatus.Staged;
            await importRepository.SaveChanges();
            LogStagingComplete(importId);
            return true;
        }
        catch (Exception e)
        {
            LogStagingError(e, importId);
            import.Status = ImportStatus.StagingFailed;
            import.Error = e.Message;
            await importRepository.SaveChanges();
            return false;
        }
    }

    private async Task<bool> MergeImport(int importId, ImportType importType, CancellationToken cancellationToken)
    {
        await using var operationScope = serviceScopeFactory.CreateAsyncScope();
        var importRepository = operationScope.ServiceProvider.GetRequiredService<IImportRepository>();
        var import = await importRepository.GetByIdTracked(importId);
        if (import is not { Status: ImportStatus.Staged })
        {
            LogNotMerging(importId, import?.Status);
            return false;
        }

        var importServiceType = importType switch
        {
            ImportType.OpenStreetMap => typeof(IOsmImportService),
            ImportType.Cadastre or ImportType.CadastreInitial => typeof(ICadastreImportService),
            _ => throw new NotSupportedException($"Import type {importType} is not supported"),
        };

        await using var importServiceScope = serviceScopeFactory.CreateAsyncScope();
        var importService = (IImportService)importServiceScope.ServiceProvider.GetRequiredService(importServiceType);
        try
        {
            LogMergingStarting(importId);
            await importService.MergeImport(import.Id, cancellationToken);

            import.Status = ImportStatus.Complete;
            await importRepository.SaveChanges();
            LogMergingComplete(importId);
            return true;
        }
        catch (Exception e)
        {
            LogMergingError(e, importId);
            import.Status = ImportStatus.Failed;
            import.Error = e.Message;
            await importRepository.SaveChanges();
            return false;
        }
    }

    private async Task CleanImport(int importId, ImportType importType, CancellationToken cancellationToken)
    {
        await using var operationScope = serviceScopeFactory.CreateAsyncScope();
        var importRepository = operationScope.ServiceProvider.GetRequiredService<IImportRepository>();
        var import = await importRepository.GetByIdTracked(importId);
        if (import == null)
        {
            LogNotFound(importId);
            return;
        }

        if (import.Cleared)
            return;

        var importServiceType = importType switch
        {
            ImportType.OpenStreetMap => typeof(IOsmImportService),
            ImportType.Cadastre or ImportType.CadastreInitial => typeof(ICadastreImportService),
            _ => throw new NotSupportedException($"Import type {importType} is not supported"),
        };

        await using var importServiceScope = serviceScopeFactory.CreateAsyncScope();
        var importService = (IImportService)importServiceScope.ServiceProvider.GetRequiredService(importServiceType);
        try
        {
            LogClearingStarting(importId);
            if (File.Exists(import.LocalPath))
                File.Delete(import.LocalPath);
            await importService.ClearImport();

            import.Cleared = true;
            await importRepository.SaveChanges();
            LogClearingComplete(importId);
        }
        catch (Exception e)
        {
            LogClearingError(e, importId);
            await importRepository.SaveChanges();
        }
    }

    public async Task MatchMissingOsmCadastre(CancellationToken cancellationToken)
    {
        // 0.001 is roughly 100-150 meters in degrees at Belarus latitude (53°N)
        // It's used as a small tolerance for features that should touch/intersect but might have slight precision differences
        const double geometryDelta = 0.001;

        var now = DateTime.UtcNow;

        await MirrorIsDeletedFromCadastre();

        await CreateNewFeatures();

        await LinkNewOsmToExistingFeatures();

        await UpdatePendingFeatureGeometries();

        return;

        async Task MirrorIsDeletedFromCadastre()
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var featureRepository = scope.ServiceProvider.GetRequiredService<IFeatureRepository>();

            await featureRepository.MirrorIsDeletedFromCadastre(now);
        }

        async Task CreateNewFeatures()
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var cadastreFeatureRepository = scope.ServiceProvider.GetRequiredService<ICadastreFeatureRepository>();
            var osmFeatureRepository = scope.ServiceProvider.GetRequiredService<IOsmFeatureRepository>();
            var featureRepository = scope.ServiceProvider.GetRequiredService<IFeatureRepository>();

            var unmatchedAtes = await cadastreFeatureRepository.GetUnmatchedAtes();

            LogMatchingAtesFound(unmatchedAtes.Count);
            var newCount = 0;
            var matchingOsm = new List<OsmFeatureEntity>();
            foreach (var ate in unmatchedAtes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var cadastreFeatures = await cadastreFeatureRepository.GetUnmatchedByAteTracking(ate);
                if (cadastreFeatures.Count == 0) continue;

                var bbox = new Envelope();
                foreach (var cadastreFeature in cadastreFeatures)
                    if (cadastreFeature.Geometry != null)
                        bbox.ExpandToInclude(cadastreFeature.Geometry.EnvelopeInternal);

                if (bbox.IsNull) continue;

                var osmFeatures = await osmFeatureRepository.GetUnmatchedIntersectingTracking(GeometryFactory.ToGeometry(bbox));

                var osmCandidates = new List<OsmFeatureMatchCandidate>(osmFeatures.Count);
                foreach (var osm in osmFeatures)
                {
                    if (osm.Feature != null || osm.Tags == null) continue;

                    var matchResult = TryParseOsmFeatureName(osm);
                    if (matchResult != null)
                    {
                        osmCandidates.Add(matchResult);
                    }
                }

                foreach (var cadastre in cadastreFeatures)
                {
                    var cadastreFeatureType = (FeatureType)cadastre.ElementType;

                    string? nameBeTarask = null;
                    matchingOsm.Clear();

                    foreach (var candidate in osmCandidates)
                    {
                        // maybe we've already matched to something else in this pass. Unlikely, but still.
                        if (candidate.Feature.Feature != null) continue;

                        if (candidate.Type == cadastreFeatureType &&
                            (candidate.NameBe != null && string.Equals(candidate.NameBe, cadastre.ElementNameBel, StringComparison.OrdinalIgnoreCase)
                             || candidate.NameRu != null && string.Equals(candidate.NameRu, cadastre.ElementName, StringComparison.OrdinalIgnoreCase)))
                        {

                            if (cadastre.Geometry != null && (candidate.Feature.Geometry.Intersects(cadastre.Geometry) || candidate.Feature.Geometry.Distance(cadastre.Geometry) < geometryDelta))
                            {
                                nameBeTarask ??= candidate.NameBeTarask;
                                matchingOsm.Add(candidate.Feature);
                            }
                        }
                    }

                    if (matchingOsm.Count == 0) continue;

                    var feature = new FeatureEntity
                    {
                        Type = cadastreFeatureType,
                        NameRu = cadastre.ElementName,
                        NameBeNark = cadastre.ElementNameBel ?? "",
                        NameBeTarask = nameBeTarask ?? cadastre.ElementNameBel ?? "",
                        Geometry = AssembleOsmGeometry(matchingOsm),
                        CadastreFeature = cadastre,
                        OsmFeatures = matchingOsm.ToList(),
                    };

                    featureRepository.Add(feature);

                    newCount++;

                    if (newCount % 1000 == 0)
                        LogMatchingNewProgress(newCount);
                }

                await featureRepository.SaveChanges();
                featureRepository.ClearChangeTracker();
            }

            LogMatchingNewComplete(newCount);
        }

        async Task LinkNewOsmToExistingFeatures()
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var cadastreFeatureRepository = scope.ServiceProvider.GetRequiredService<ICadastreFeatureRepository>();
            var osmFeatureRepository = scope.ServiceProvider.GetRequiredService<IOsmFeatureRepository>();
            var featureRepository = scope.ServiceProvider.GetRequiredService<IFeatureRepository>();
            var featureHistoricRepository = scope.ServiceProvider.GetRequiredService<IFeatureHistoricRepository>();

            var ates = await cadastreFeatureRepository.GetAllAtes();
            LogUpdatingAtesFound(ates.Count);

            var matchingOsm = new List<OsmFeatureEntity>();
            var updatedCount = 0;
            foreach (var ate in ates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var existingFeatures = await featureRepository.GetByAteWithImportsTracking(ate);
                if (existingFeatures.Count == 0) continue;

                var bbox = new Envelope();
                foreach (var feature in existingFeatures)
                    bbox.ExpandToInclude(feature.Geometry.EnvelopeInternal);

                if (bbox.IsNull) continue;

                var osmFeatures = await osmFeatureRepository.GetUnmatchedIntersectingTracking(GeometryFactory.ToGeometry(bbox));
                var osmCandidates = new List<OsmFeatureMatchCandidate>(osmFeatures.Count);
                foreach (var osm in osmFeatures)
                {
                    if (osm.Feature != null || osm.Tags == null) continue;
                    var matchResult = TryParseOsmFeatureName(osm);
                    if (matchResult != null) osmCandidates.Add(matchResult);
                }

                if (osmCandidates.Count == 0) continue;

                var ateUpdatedCount = 0;
                foreach (var feature in existingFeatures)
                {
                    matchingOsm.Clear();
                    foreach (var candidate in osmCandidates)
                    {
                        // maybe we've already matched to something else in this pass. Unlikely, but still.
                        if (candidate.Feature.Feature != null) continue;

                        if (candidate.Type == feature.Type &&
                            (candidate.NameBe != null && string.Equals(candidate.NameBe, feature.NameBeNark, StringComparison.OrdinalIgnoreCase)
                             || candidate.NameRu != null && string.Equals(candidate.NameRu, feature.NameRu, StringComparison.OrdinalIgnoreCase)))
                        {
                            if (feature.Geometry != null && (candidate.Feature.Geometry.Intersects(feature.Geometry) || candidate.Feature.Geometry.Distance(feature.Geometry) < geometryDelta))
                            {
                                matchingOsm.Add(candidate.Feature);
                            }
                        }
                    }

                    if (matchingOsm.Count <= 0) continue;

                    var featureHistoricEntity = FeatureHistoricEntity.FromBase(feature);
                    featureHistoricEntity.ChangeDateTime = now;
                    featureHistoricRepository.Add(featureHistoricEntity);

                    foreach (var osmFeature in matchingOsm)
                        feature.OsmFeatures.Add(osmFeature);

                    foreach (var osmFeature in feature.OsmFeatures)
                        osmFeature.GeometryUpdatePending = false;

                    feature.Geometry = AssembleOsmGeometry(feature.OsmFeatures.Where(x => !x.IsDeleted));
                    ateUpdatedCount++;
                    updatedCount++;
                    if (updatedCount % 1000 == 0)
                        LogMatchingUpdateNewProgress(updatedCount);
                }

                if (ateUpdatedCount > 0)
                    await featureRepository.SaveChanges();

                featureRepository.ClearChangeTracker();
            }

            LogMatchingUpdateNewComplete(updatedCount);
        }

        async Task UpdatePendingFeatureGeometries()
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var featureRepository = scope.ServiceProvider.GetRequiredService<IFeatureRepository>();
            var featureHistoricRepository = scope.ServiceProvider.GetRequiredService<IFeatureHistoricRepository>();

            const int batchSize = 1000;
            var updatedCount = 0;
            while (true)
            {
                var featuresToUpdate = await featureRepository.GetNextForGeometryUpdateTracking(batchSize);
                if (featuresToUpdate.Count == 0)
                    break;

                foreach (var feature in featuresToUpdate)
                {
                    foreach (var osmFeature in feature.OsmFeatures)
                        osmFeature.GeometryUpdatePending = false;

                    var featureHistoricEntity = FeatureHistoricEntity.FromBase(feature);
                    featureHistoricEntity.ChangeDateTime = now;
                    featureHistoricRepository.Add(featureHistoricEntity);

                    feature.Geometry = AssembleOsmGeometry(feature.OsmFeatures.Where(x => !x.IsDeleted));
                    updatedCount++;
                    if (updatedCount % 1000 == 0)
                        LogMatchingUpdateChangedProgress(updatedCount);
                }

                await featureRepository.SaveChanges();
                featureRepository.ClearChangeTracker();
            }

            LogMatchingUpdateChangedComplete(updatedCount);
        }
    }

    public async Task InitializeNamingCategories()
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var namingCategoryRepository = scope.ServiceProvider.GetRequiredService<INamingCategoryRepository>();
        await namingCategoryRepository.MergeFromCadastreInitial(DateTime.UtcNow);

        LogNamingCategoriesInitialized();
    }

    public async Task InitializeDossierRecords()
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var dossierRecordRepository = scope.ServiceProvider.GetRequiredService<IDossierRecordRepository>();
        var namingCategoryRepository = scope.ServiceProvider.GetRequiredService<INamingCategoryRepository>();
        var initialCadastreFeatureImportRepository = scope.ServiceProvider.GetRequiredService<IInitialCadastreFeatureImportRepository>();

        if (await dossierRecordRepository.HasAny())
            throw new InvalidOperationException("Dossier records have already been initialized");

        var namingCategories = await namingCategoryRepository.GetAll();
        if (namingCategories.Count == 0)
            throw new InvalidOperationException("Naming categories not found, please import them first");

        var namingCategoriesDictionary = namingCategories.ToDictionary(x => x.Name);

        var candidateDossierRecords = await initialCadastreFeatureImportRepository.GetReasonShortInfoPairs();
        var recordsToCreate = new List<DossierRecordEntity>();

        LogCreatingDossierRecords();

        foreach (var (reason, shortInfo, nameCategory, classification) in candidateDossierRecords.Where(p => p is { reason: not null, shortInfo: not null }))
        {
            AddIfUnique(reason, shortInfo, nameCategory, classification);
            if (recordsToCreate.Count % 1000 == 0)
                LogDossierRecordsProgress(recordsToCreate.Count);
        }

        foreach (var (reason, shortInfo, nameCategory, classification) in candidateDossierRecords.Where(p => (p.reason == null) != (p.shortInfo == null)))
        {
            AddIfUnique(reason, shortInfo, nameCategory, classification);
            if (recordsToCreate.Count % 1000 == 0)
                LogDossierRecordsProgress(recordsToCreate.Count);
        }

        // Persist into the DB batching by 1000
        const int batchSize = 1000;
        for (int i = 0; i < recordsToCreate.Count; i += batchSize)
        {
            dossierRecordRepository.AddRange(recordsToCreate.Skip(i).Take(batchSize));
            await dossierRecordRepository.SaveChanges();
            dossierRecordRepository.ClearChangeTracker();
        }

        LogDossierRecordsComplete();

        return;

        void AddIfUnique(string? reason, string? shortInfo, string? nameCategory, ClassificationGrade classificationGrade)
        {
            var canonicalDossierRecord = recordsToCreate.FirstOrDefault(existing =>
            {
                if (reason != null && shortInfo != null)
                {
                    if (DossierSimilarityThreshold < Math.Abs((existing.DescriptionBe?.Length ?? 0) - reason.Length) + Math.Abs((existing.DescriptionRu?.Length ?? 0) - shortInfo.Length))
                        return false;
                    var distanceBe = existing.DescriptionBe == null ? reason.Length : NameHelpers.GetLevenshteinDistance(existing.DescriptionBe, reason);
                    var distanceRu = existing.DescriptionRu == null ? shortInfo.Length : NameHelpers.GetLevenshteinDistance(existing.DescriptionRu, shortInfo);
                    return distanceBe + distanceRu <= DossierSimilarityThreshold;
                }

                if (reason != null)
                    return existing.DescriptionBe != null
                           && Math.Abs(existing.DescriptionBe.Length - reason.Length) <= DossierSimilarityThreshold
                           && NameHelpers.GetLevenshteinDistance(existing.DescriptionBe, reason) <= DossierSimilarityThreshold;

                if (shortInfo != null)
                    return existing.DescriptionRu != null 
                           && Math.Abs(existing.DescriptionRu.Length - shortInfo.Length) <= DossierSimilarityThreshold
                           && NameHelpers.GetLevenshteinDistance(existing.DescriptionRu, shortInfo) <= DossierSimilarityThreshold;

                return false;
            });

            if (canonicalDossierRecord == null)
            {
                recordsToCreate.Add(new DossierRecordEntity
                {
                    DescriptionBe = reason,
                    DescriptionRu = shortInfo,
                    NamingCategoryId = nameCategory != null ? namingCategoriesDictionary.GetValueOrDefault(nameCategory)?.Id : null,
                    Classification = classificationGrade,
                });
            }
            else
            {
                LogCanonicalizingDossierRecord(canonicalDossierRecord.DescriptionBe, reason, canonicalDossierRecord.DescriptionRu, shortInfo);
                if (canonicalDossierRecord.Classification == ClassificationGrade.None)
                    canonicalDossierRecord.Classification = classificationGrade;
                else if (classificationGrade != ClassificationGrade.None && canonicalDossierRecord.Classification != classificationGrade)
                // if in different cities the same dossier record is marked with different classification grades, assume the most permissive one (numerically highest)
                    canonicalDossierRecord.Classification = (ClassificationGrade)Math.Max((int)canonicalDossierRecord.Classification, (int)classificationGrade);
            }
        }
    }

    public async Task InitializeFeaturesDossierCategoriesReferences(CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var dossierRecordRepository = scope.ServiceProvider.GetRequiredService<IDossierRecordRepository>();
        var namingCategoryRepository = scope.ServiceProvider.GetRequiredService<INamingCategoryRepository>();
        var initialCadastreFeatureImportRepository = scope.ServiceProvider.GetRequiredService<IInitialCadastreFeatureImportRepository>();
        var cadastreFeatureRepository = scope.ServiceProvider.GetRequiredService<ICadastreFeatureRepository>();
        var featureRepository = scope.ServiceProvider.GetRequiredService<IFeatureRepository>();

        var namingCategories = await namingCategoryRepository.GetAll();
        if (namingCategories.Count == 0)
            throw new InvalidOperationException("Naming categories not found, please import them first");

        var dossierRecords = await dossierRecordRepository.GetAll();
        if (dossierRecords.Count == 0)
            throw new InvalidOperationException("Dossier records not found, please import them first");

        var dossierRecordsPrepared = dossierRecords.Select(x => (
                record: x.Id,
                descriptionRu: x.DescriptionRu == null ? null : new Fastenshtein.Levenshtein(x.DescriptionRu),
                descriptionBe: x.DescriptionBe == null ? null : new Fastenshtein.Levenshtein(x.DescriptionBe)))
            .ToList();
        var recordsByDescriptionBe = dossierRecords.Where(x => x.DescriptionBe != null).ToLookup(x => x.DescriptionBe, x => x.Id);
        var recordsByDescriptionRu = dossierRecords.Where(x => x.DescriptionRu != null).ToLookup(x => x.DescriptionRu, x => x.Id);
        dossierRecords = null;

        var namingCategoriesDictionary = namingCategories.ToDictionary(x => x.Name);
        var initialCadastreFeatureImports = await initialCadastreFeatureImportRepository.GetReasonsAndNameCategories();
        var initialCadastreFeatureImportsDictionary = initialCadastreFeatureImports.ToDictionary(x => x.id);

        var ates = await cadastreFeatureRepository.GetAllAtes();

        var updatedCount = 0;

        foreach (var ate in ates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var features = await featureRepository.GetByAteWithCadastreTracking(ate);
            if (features.Count == 0) continue;

            foreach (var feature in features)
            {
                var featureUpdated = false;
                if (feature.DossierRecordId == null)
                {
                    var reason = initialCadastreFeatureImportsDictionary.GetValueOrDefault(feature.CadastreFeature!.Id).reason;
                    var shortInfo = feature.CadastreFeature.ShortInfo;

                    var dossierRecord = FindDossierMatch(reason, shortInfo);
                    featureUpdated |= dossierRecord != null;
                    feature.DossierRecordId = dossierRecord;
                }

                if (feature.DossierRecordId == null && feature.NamingCategoryId == null)
                {
                    var namingCategoryString = initialCadastreFeatureImportsDictionary.GetValueOrDefault(feature.CadastreFeature!.Id).nameCategory;

                    featureUpdated |= namingCategoryString != null;
                    feature.NamingCategoryId = namingCategoryString != null ? namingCategoriesDictionary[namingCategoryString].Id : null;
                }

                if (featureUpdated)
                    updatedCount++;

                if (updatedCount % 1000 == 0)
                    LogUpdateDossierLinkProgress(updatedCount);
            }
            await featureRepository.SaveChanges();
            featureRepository.ClearChangeTracker();
        }

        LogUpdateDossierLinkComplete(updatedCount);

        return;

        int? FindDossierMatch(string? reason, string? shortInfo)
        {
            if (reason == null && shortInfo == null) return null;

            // 1. Precise fast-path via lookups
            if (reason != null && shortInfo != null)
            {
                var match = recordsByDescriptionBe[reason].Intersect(recordsByDescriptionRu[shortInfo]).Cast<int?>().FirstOrDefault();
                if (match.HasValue) return match;
            }
            if (reason != null)
            {
                var match = recordsByDescriptionBe[reason].Cast<int?>().FirstOrDefault();
                if (match.HasValue) return match;
            }
            if (shortInfo != null)
            {
                var match = recordsByDescriptionRu[shortInfo].Cast<int?>().FirstOrDefault();
                if (match.HasValue) return match;
            }

            // 2. Fuzzy fallback consistent with AddIfUnique logic
            int? bestMatchId = null;
            int bestDistance = int.MaxValue;

            foreach (var record in dossierRecordsPrepared)
            {
                var distance = int.MaxValue;

                if (reason != null && shortInfo != null)
                {
                    var lengthExistingBe = record.descriptionBe?.StoredLength ?? 0;
                    var lengthExistingRu = record.descriptionRu?.StoredLength ?? 0;

                    if (Math.Abs(lengthExistingBe - reason.Length) + Math.Abs(lengthExistingRu - shortInfo.Length) <= DossierSimilarityThreshold)
                    {
                        var distanceBe = record.descriptionBe?.DistanceFrom(reason) ?? reason.Length;
                        var distanceRu = record.descriptionRu?.DistanceFrom(shortInfo) ?? shortInfo.Length;
                        distance = distanceBe + distanceRu;
                    }
                }
                else if (reason != null)
                {
                    if (record.descriptionBe != null && Math.Abs(record.descriptionBe.StoredLength - reason.Length) <= DossierSimilarityThreshold)
                        distance = record.descriptionBe.DistanceFrom(reason);
                }
                else if (shortInfo != null)
                {
                    if (record.descriptionRu != null && Math.Abs(record.descriptionRu.StoredLength - shortInfo.Length) <= DossierSimilarityThreshold)
                        distance = record.descriptionRu.DistanceFrom(shortInfo);
                }

                if (distance <= DossierSimilarityThreshold && distance < bestDistance)
                {
                    bestDistance = distance;
                    bestMatchId = record.record;
                    if (bestDistance == 0) break;
                }
            }

            return bestMatchId;
        }
    }

    public async Task MapFieldsFromInitialCadastreImport(CancellationToken applicationStopping)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var featureRepository = scope.ServiceProvider.GetRequiredService<IFeatureRepository>();

        await featureRepository.MirrorFromInitialCadastre();
        LogAssignedInitialFields();
        applicationStopping.ThrowIfCancellationRequested();
        await featureRepository.AssignClassificationsFromInitialCadastre();
        LogAssignedClassifications();
    }

    private static Geometry AssembleOsmGeometry(IEnumerable<OsmFeatureEntity> matchingOsm)
    {
        var lineMerger = new LineMerger();
        foreach (var osm in matchingOsm)
            lineMerger.Add(osm.Geometry);

        var mergedLines = lineMerger.GetMergedLineStrings();
        return mergedLines.Count switch
        {
            0 => GeometryFactory.CreateLineString(Array.Empty<Coordinate>()),
            1 => mergedLines[0],
            _ => GeometryFactory.CreateMultiLineString(mergedLines.Cast<LineString>().ToArray()),
        };
    }

    private record OsmFeatureMatchCandidate(OsmFeatureEntity Feature, FeatureType Type, string? NameBe, string? NameRu, string? NameBeTarask);

    private static OsmFeatureMatchCandidate? TryParseOsmFeatureName(OsmFeatureEntity osmFeature)
    {
        string? nameBe = null;
        string? nameRu = null;
        string? nameBeTarask = null;
        FeatureType? type = null;
        if (osmFeature.Tags.TryGetValue("name:be", out var fullNameBe) || osmFeature.Tags.TryGetValue("name", out fullNameBe))
        {
            var (parsedType, parsedName) = NameHelpers.ParseOsmCyrillicName(fullNameBe);
            if (parsedType != FeatureType.Unknown)
            {
                type = parsedType;
                nameBe = parsedName;
            }
            else
                nameBe = fullNameBe;
        }

        if (osmFeature.Tags.TryGetValue("name:be-tarask", out var fullNameBeTarask))
        {
            var (parsedType, parsedName) = NameHelpers.ParseOsmCyrillicName(fullNameBeTarask);
            if (parsedType != FeatureType.Unknown)
            {
                type ??= parsedType;
                nameBeTarask = parsedName;
            }
            else
                nameBeTarask = fullNameBeTarask;
        }

        if (osmFeature.Tags.TryGetValue("name:ru", out var fullNameRu))
        {
            var (parsedType, parsedName) = NameHelpers.ParseOsmCyrillicName(fullNameRu);
            if (parsedType != FeatureType.Unknown)
            {
                type ??= parsedType;
                nameRu = parsedName;
            }
            else
                nameRu = fullNameRu;
        }

        if (type == null) return null;

        return new OsmFeatureMatchCandidate(osmFeature, type.Value, nameBe, nameRu, nameBeTarask);
    }



    [LoggerMessage(LogLevel.Information, "Found {count} ATEs with unmatched cadastre features")]
    private partial void LogMatchingAtesFound(int count);

    [LoggerMessage(LogLevel.Information, "Matched {count} new features...")]
    private partial void LogMatchingNewProgress(int count);

    [LoggerMessage(LogLevel.Information, "Complete. Total {count} new features created")]
    private partial void LogMatchingNewComplete(int count);

    [LoggerMessage(LogLevel.Information, "Looking for OSM updates in {count} ATEs")]
    private partial void LogUpdatingAtesFound(int count);

    [LoggerMessage(LogLevel.Information, "Updated {count} features with new paths...")]
    private partial void LogMatchingUpdateNewProgress(int count);

    [LoggerMessage(LogLevel.Information, "Complete. Total {count} features updated with new paths")]
    private partial void LogMatchingUpdateNewComplete(int count);

    [LoggerMessage(LogLevel.Information, "Updated {count} features with changed paths...")]
    private partial void LogMatchingUpdateChangedProgress(int count);

    [LoggerMessage(LogLevel.Information, "Complete. Total {count} features updated with changed paths")]
    private partial void LogMatchingUpdateChangedComplete(int count);


    [LoggerMessage(LogLevel.Information, "Updated {count} features with dossier records...")]
    private partial void LogUpdateDossierLinkProgress(int count);

    [LoggerMessage(LogLevel.Information, "Complete. Total {count} features updated with dossier records")]
    private partial void LogUpdateDossierLinkComplete(int count);


    [LoggerMessage(LogLevel.Information, "Creating dossier records...")]
    private partial void LogCreatingDossierRecords();

    [LoggerMessage(LogLevel.Information, "Dossier records progress: {count}...")]
    private partial void LogDossierRecordsProgress(int count);

    [LoggerMessage(LogLevel.Information, "Dossier records canonicalization:\nCanonical Be: {canonicalBe}\n  Matched Be: {matchedBe}\nCanonical Ru: {canonicalRu}\n  Matched Ru: {matchedRu}")]
    private partial void LogCanonicalizingDossierRecord(string? canonicalBe, string? matchedBe, string? canonicalRu, string? matchedRu);

    [LoggerMessage(LogLevel.Information, "Dossier records creation complete")]
    private partial void LogDossierRecordsComplete();



    [LoggerMessage(LogLevel.Warning, "Not proceeding with downloading import {importId}, current status: {importStatus}")]
    private partial void LogNotDownloading(int importId, ImportStatus? importStatus);

    [LoggerMessage(LogLevel.Information, "Download starting for import {importId} from {url}")]
    private partial void LogDownloadStarting(int importId, string url);

    [LoggerMessage(LogLevel.Information, "Download complete for import {importId}. Saved to {path}")]
    private partial void LogDownloadComplete(int importId, string path);

    [LoggerMessage(LogLevel.Error, "Failed to download import {importId}")]
    private partial void LogDownloadError(Exception e, int importId);


    [LoggerMessage(LogLevel.Warning, "Not proceeding with staging import {importId}, current status: {importStatus}")]
    private partial void LogNotStaging(int importId, ImportStatus? importStatus);

    [LoggerMessage(LogLevel.Information, "Staging starting for import {importId} from file {file}")]
    private partial void LogStagingStarting(int importId, string file);

    [LoggerMessage(LogLevel.Information, "Staging complete for import {importId}")]
    private partial void LogStagingComplete(int importId);

    [LoggerMessage(LogLevel.Error, "Failed to stage import {importId}")]
    private partial void LogStagingError(Exception e, int importId);


    [LoggerMessage(LogLevel.Warning, "Not proceeding with merging import {importId}, current status: {importStatus}")]
    private partial void LogNotMerging(int importId, ImportStatus? importStatus);

    [LoggerMessage(LogLevel.Information, "Merging starting for import {importId}")]
    private partial void LogMergingStarting(int importId);

    [LoggerMessage(LogLevel.Information, "Merging complete for import {importId}")]
    private partial void LogMergingComplete(int importId);

    [LoggerMessage(LogLevel.Error, "Failed to merge import {importId}")]
    private partial void LogMergingError(Exception e, int importId);


    [LoggerMessage(LogLevel.Warning, "Import {importId} not found")]
    private partial void LogNotFound(int importId);

    [LoggerMessage(LogLevel.Information, "Clearing starting for import {importId}")]
    private partial void LogClearingStarting(int importId);

    [LoggerMessage(LogLevel.Information, "Clearing complete for import {importId}")]
    private partial void LogClearingComplete(int importId);

    [LoggerMessage(LogLevel.Error, "Failed to clear import {importId}")]
    private partial void LogClearingError(Exception e, int importId);

    [LoggerMessage(LogLevel.Critical, "Catastrophic import error")]
    private partial void LogCatastrophicImportError(Exception e);


    [LoggerMessage(LogLevel.Information, "Naming categories initialized")]
    private partial void LogNamingCategoriesInitialized();

    [LoggerMessage(LogLevel.Information, "Assigned fields from initial import")]
    private partial void LogAssignedInitialFields();

    [LoggerMessage(LogLevel.Information, "Assigned classifications from initial import")]
    private partial void LogAssignedClassifications();
}