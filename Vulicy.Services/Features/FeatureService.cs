using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Linemerge;
using Vulicy.Domain;

namespace Vulicy.Services;

public interface IFeatureService
{
    Task<List<FeatureSearchResult>> GetByDossierRecord(int dossierRecordId);
    Task<FeatureSearchResult> CreateFeatureFromSources(FeatureCreateFromSourcesRequest featureEditRequest, int userId);
    Task<Geometry> LinkOsmFeature(int id, OsmId osmId, int userId);
    Task RecomputeGeometry(int id);
    Task EditFeature(int id, FeatureEditRequest featureEditRequest, int userId);
    Task<FeatureTileMinimalDetails> GetFeaturePreview(GetFeaturePreviewRequest request);
}

public class FeatureService(
    IFeatureRepository featureRepository,
    IFeatureHistoricRepository featureHistoricRepository,
    IOsmFeatureRepository osmFeatureRepository,
    ICadastreFeatureRepository cadastreFeatureRepository,
    IInitialCadastreFeatureImportRepository initialCadastreFeatureImportRepository,
    IDossierRecordRepository dossierRecordRepository,
    INamingCategoryRepository namingCategoryRepository,
    IAdministrativeRepository administrativeRepository
    ) : IFeatureService
{
    public Task<List<FeatureSearchResult>> GetByDossierRecord(int dossierRecordId)
    {
        return featureRepository.GetByDossierRecord(dossierRecordId);
    }

    public async Task<FeatureSearchResult> CreateFeatureFromSources(FeatureCreateFromSourcesRequest featureEditRequest, int userId)
    {
        await using var transaction = await featureRepository.BeginTransaction();
        var osmFeature = await osmFeatureRepository.GetByIdTracked(featureEditRequest.OsmType, featureEditRequest.OsmId);
        var cadastreFeature = await cadastreFeatureRepository.GetByIdTracked(featureEditRequest.CadastreId);

        CheckForLinking(osmFeature);
        CheckForLinking(cadastreFeature);

        var administrative = await administrativeRepository.GetByCadastreAte(cadastreFeature.Ate);
        if (administrative == null)
            throw new InvalidOperationException("Administrative not found");

        var feature = new FeatureEntity();

        MapFromRequest(featureEditRequest, feature);

        feature.Geometry = osmFeature.Geometry;
        feature.AdministrativeId = administrative.Id;
        feature.LastModifiedById = userId;
        osmFeature.Feature = feature;
        cadastreFeature.Feature = feature;
        featureRepository.Add(feature);
        await featureRepository.SaveChanges();
        await transaction.Commit();

        return new FeatureSearchResult(
            feature.Id,
            feature.NameBeTarask,
            feature.NameBeNark,
            feature.NameRu,
            administrative.NameBeTarask,
            feature.Type,
            feature.Geometry
        );
    }

    public async Task<Geometry> LinkOsmFeature(int id, OsmId osmId, int userId)
    {
        await using var transaction = await featureRepository.BeginTransaction();
        var feature = await featureRepository.GetByIdTracked(id);
        if (feature == null)
            throw new InvalidOperationException("Feature not found");

        var osmFeature = await osmFeatureRepository.GetByIdTracked(osmId.Type, osmId.Id);
        CheckForLinking(osmFeature);

        var lineMerger = new LineMerger();
        lineMerger.Add(feature.Geometry);
        lineMerger.Add(osmFeature.Geometry);

        feature.Geometry = lineMerger.ToMerged();
        feature.LastModifiedById = userId;
        osmFeature.Feature = feature;
        await featureRepository.SaveChanges();
        await transaction.Commit();

        return feature.Geometry;
    }

    public async Task RecomputeGeometry(int id)
    {
        await using var transaction = await featureRepository.BeginTransaction();
        var feature = await featureRepository.GetByIdTracked(id);
        if (feature == null)
            throw new InvalidOperationException("Feature not found");

        var osmFeatures = await osmFeatureRepository.GetByFeatureId(id);

        var lineMerger = new LineMerger();
        foreach (var osmFeature in osmFeatures)
            lineMerger.Add(osmFeature.Geometry);

        feature.Geometry = lineMerger.ToMerged();
        await featureRepository.SaveChanges();
        await transaction.Commit();
    }

    public async Task EditFeature(int id, FeatureEditRequest featureEditRequest, int userId)
    {
        await using var transaction = await featureRepository.BeginTransaction();
        var feature = await featureRepository.GetByIdTracked(id);
        if (feature != null)
        {
            var history = FeatureHistoricEntity.FromBase(feature);
            history.ChangeDateTime = DateTime.UtcNow;
            history.InHistoryById = userId;
            featureHistoricRepository.Add(history);

            MapFromRequest(featureEditRequest, feature);

            feature.LastModifiedById = userId;
            await featureRepository.SaveChanges();
            await transaction.Commit();
        }
    }

    public async Task<FeatureTileMinimalDetails> GetFeaturePreview(GetFeaturePreviewRequest request)
    {
        var osmFeature = await osmFeatureRepository.GetById(request.OsmType, request.OsmId);
        var cadastreFeature = await cadastreFeatureRepository.GetById(request.CadastreId);

        CheckForLinking(osmFeature);
        CheckForLinking(cadastreFeature);

        var initialCadastre = await initialCadastreFeatureImportRepository.GetById(request.CadastreId);
        var dossierRecords = string.IsNullOrEmpty(initialCadastre?.Reason) && string.IsNullOrEmpty(cadastreFeature.ShortInfo)
            ? []
            : await dossierRecordRepository.FindByDescriptions(initialCadastre?.Reason, cadastreFeature.ShortInfo);

        int? namingCategoryId = null;
        if (initialCadastre?.NameCategory != null)
            namingCategoryId = await namingCategoryRepository.GetIdByName(initialCadastre.NameCategory);

        var (type, namesBe, namesRu, nameBeTarask) = ImportPipeline.TryParseOsmFeatureName(osmFeature);
        namesBe ??= [];
        namesRu ??= [];
        var dossierRecord = dossierRecords.FirstOrDefault(dr => dr.PossibleNamesBeNark?.Intersect(namesBe).Any() == true || dr.PossibleNamesRu?.Intersect(namesRu).Any() == true);
        var nameBe = namesBe.FirstOrDefault() ?? cadastreFeature.ElementNameBel;
        var nameRu = namesRu.FirstOrDefault() ?? cadastreFeature.ElementName;
        type ??= (FeatureType)cadastreFeature.ElementType;
        return new FeatureTileMinimalDetails(
            osmFeature.Geometry,
            nameBeTarask ?? nameBe,
            nameBe,
            nameRu,
            initialCadastre != null && (dossierRecord == null || initialCadastre.Classification != (int)dossierRecord.Classification) ? (ClassificationGrade)(initialCadastre.Classification ?? 0) : ClassificationGrade.None,
            type.Value,
            initialCadastre?.Reason,
            initialCadastre?.HistoricName,
            initialCadastre?.HistoricPossible ?? false,
            initialCadastre?.YearNamed,
            initialCadastre?.Comment,
            namingCategoryId != dossierRecord?.NamingCategoryId ? namingCategoryId : null,
            dossierRecord?.Id,
            dossierRecord?.NameBeTarask
        );
    }

    private static void CheckForLinking(OsmFeatureEntity? osmFeature)
    {
        if (osmFeature == null)
            throw new InvalidOperationException("OSM feature not found");
        if (osmFeature.FeatureId != null)
            throw new InvalidOperationException("OSM feature already linked");
    }

    private static void CheckForLinking(CadastreFeatureEntity? cadastreFeature)
    {
        if (cadastreFeature == null)
            throw new InvalidOperationException("Cadastre feature not found");
        if (cadastreFeature.FeatureId != null)
            throw new InvalidOperationException("Cadastre feature already linked");
    }

    private static void MapFromRequest(FeatureEditRequest featureEditRequest, FeatureEntity feature)
    {
        feature.NameBeTarask = featureEditRequest.NameBeTarask;
        feature.NameBeNark = featureEditRequest.NameBeNark;
        feature.NameRu = featureEditRequest.NameRu;
        feature.Classification = featureEditRequest.Classification;
        feature.Type = featureEditRequest.Type;
        feature.RenamingReason = featureEditRequest.RenamingReason;
        feature.HistoricNames = featureEditRequest.HistoricNames;
        feature.Comment = featureEditRequest.Comment;
        feature.HistoricPossible = featureEditRequest.HistoricPossible;
        feature.YearNamed = featureEditRequest.YearNamed;
        feature.NamingCategoryId = featureEditRequest.NamingCategoryId;
        feature.DossierRecordId = featureEditRequest.DossierRecordId;
    }
}