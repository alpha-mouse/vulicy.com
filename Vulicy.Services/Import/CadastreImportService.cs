using System.Text.Json;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Vulicy.Domain;

namespace Vulicy.Services;

public partial class CadastreImportService(
    ICadastreFeatureImportRepository cadastreFeatureImportRepository,
    IInitialCadastreFeatureImportRepository initialCadastreFeatureImportRepository,
    ILogger<CadastreImportService> logger
) : ICadastreImportService
{
    private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 4326);
    private static readonly HashSet<int> ConsideredTypes =
    [
        (int)FeatureType.Street,
        (int)FeatureType.Avenue,
        (int)FeatureType.Square,
        (int)FeatureType.Boulevard,
        (int)FeatureType.HighRoad,
        (int)FeatureType.Riverside,
        (int)FeatureType.Highway,
        (int)FeatureType.Roundabout,
        (int)FeatureType.Alley,
        (int)FeatureType.Driveway,
        (int)FeatureType.DeadEnd,
        (int)FeatureType.Descent,
        (int)FeatureType.Entryway,
        (int)FeatureType.Park,
        (int)FeatureType.PublicGarden,
    ];

    public async Task StageImport(int importId, string localPath, bool considerInitial, CancellationToken cancellationToken)
    {
        if (!File.Exists(localPath))
            throw new FileNotFoundException("Import file not found", localPath);

        await using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);

        var featureCount = 0;

        using (var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken))
        {
            var root = document.RootElement;
            if (root.TryGetProperty("features", out var features))
            {
                foreach (var feature in features.EnumerateArray())
                {
                    var id = feature.GetProperty("id").GetString()!;
                    var properties = feature.GetProperty("properties");
                    var elementTypeProperty = properties.GetProperty("element_type");
                    var ateProperty = properties.GetProperty("ate");
                    var geometryElement = feature.GetProperty("geometry");
                    if (elementTypeProperty.ValueKind == JsonValueKind.Null
                        || ateProperty.ValueKind == JsonValueKind.Null)
                        continue;

                    var elementType = elementTypeProperty.GetInt32();
                    if (!ConsideredTypes.Contains(elementType)) continue;

                    var geometry = ParseGeometry(geometryElement);

                    cadastreFeatureImportRepository.Add(new()
                    {
                        Id = id,
                        ImportId = importId,
                        Geometry = geometry,
                        IdIae = properties.GetProperty("id_iae").GetInt32(),
                        ParentAte = properties.GetProperty("parent_ate").GetString(),
                        Region = GetIntOrNull(properties, "region"),
                        District = GetIntOrNull(properties, "district"),
                        VillageCouncil = GetIntOrNull(properties, "village_council"),
                        Ate = ateProperty.GetInt32(),
                        RegionName = properties.GetProperty("region_name").GetString(),
                        DistrictName = properties.GetProperty("district_name").GetString(),
                        VillageCouncilName = properties.GetProperty("village_council_name").GetString(),
                        AteName = properties.GetProperty("ate_name").GetString()!,
                        RegionNameBel = properties.GetProperty("region_name_bel").GetString(),
                        DistrictNameBel = properties.GetProperty("district_name_bel").GetString(),
                        VillageCouncilNameBel = properties.GetProperty("village_council_name_bel").GetString(),
                        AteNameBel = properties.GetProperty("ate_name_bel").GetString(),
                        CategoryName = properties.GetProperty("category_name").GetString(),
                        CategoryNameShort = properties.GetProperty("category_name_short").GetString(),
                        CategoryNameBel = properties.GetProperty("category_name_bel").GetString(),
                        CategoryNameShortBel = properties.GetProperty("category_name_short_bel").GetString(),
                        ElementType = elementType,
                        ElementTypeName = properties.GetProperty("element_type_name").GetString()!,
                        ElementTypeNameBel = properties.GetProperty("element_type_name_bel").GetString()!,
                        ElementTypeShortName = properties.GetProperty("element_type_short_name").GetString(),
                        ElementTypeShortNameBel = properties.GetProperty("element_type_short_name_bel").GetString(),
                        ElementName = properties.GetProperty("element_name").GetString()!,
                        ElementNameBel = properties.GetProperty("element_name_bel").GetString(),
                        ShortInfo = properties.GetProperty("short_info").GetString(),
                        ObjectNumber = GetIntOrNull(properties, "objectnumber"),
                    });
                    if (considerInitial)
                    {
                        var classification = GetIntOrNull(properties, "classification");
                        var reason = properties.TryGetProperty("reason", out var reasonProperty) ? reasonProperty.GetString() : null;
                        var historicName = properties.TryGetProperty("historic_name", out var historicNameProperty) ? historicNameProperty.GetString() : null;
                        var nameCategory = properties.TryGetProperty("name_category", out var nameCategoryProperty) ? nameCategoryProperty.GetString() : null;
                        var comment = properties.TryGetProperty("comment", out var commentProperty) ? commentProperty.GetString() : null;
                        var yearNamed = properties.TryGetProperty("year_named", out var yearNamedProperty) ? yearNamedProperty.GetString() : null;
                        if (classification != null
                            || reason != null
                            || historicName != null
                            || nameCategory != null
                            || comment != null
                            || yearNamed != null)
                        {
                            initialCadastreFeatureImportRepository.Add(new ()
                            {
                                Id = id,
                                Classification = classification,
                                Reason = reason,
                                HistoricName = historicName,
                                NameCategory = nameCategory,
                                Comment = comment,
                                HistoricPossible = properties.TryGetProperty("historic_possible", out var historicPossibleProperty) && historicPossibleProperty.GetBoolean(),
                                YearNamed = yearNamed,
                            });
                        }
                    }
                    featureCount++;

                    if (featureCount % 1000 == 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        await cadastreFeatureImportRepository.SaveChanges();
                        cadastreFeatureImportRepository.ClearChangeTracker();
                        LogStagedFeatures(featureCount);
                    }
                }
            }
        }

        await cadastreFeatureImportRepository.SaveChanges();
        cadastreFeatureImportRepository.ClearChangeTracker();
        LogStagingComplete(featureCount);
    }

    private static int? GetIntOrNull(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null)
        {
            return prop.GetInt32();
        }
        return null;
    }

    private static Geometry? ParseGeometry(JsonElement geometryElement)
    {
        if (geometryElement.ValueKind == JsonValueKind.Null)
            return null;

        var type = geometryElement.GetProperty("type").GetString();
        var coordinates = geometryElement.GetProperty("coordinates");

        if (type == "LineString")
        {
            return GeometryFactory.CreateLineString(ParseCoordinates(coordinates));
        }
        if (type == "MultiLineString")
        {
            var lineStrings = new LineString[coordinates.GetArrayLength()];
            for (int i = 0; i < coordinates.GetArrayLength(); i++)
            {
                lineStrings[i] = GeometryFactory.CreateLineString(ParseCoordinates(coordinates[i]));
            }
            return GeometryFactory.CreateMultiLineString(lineStrings);
        }

        return null;
    }

    private static Coordinate[] ParseCoordinates(JsonElement coordinatesElement)
    {
        var result = new Coordinate[coordinatesElement.GetArrayLength()];
        for (int i = 0; i < coordinatesElement.GetArrayLength(); i++)
        {
            var point = coordinatesElement[i];
            result[i] = new Coordinate(point[0].GetDouble(), point[1].GetDouble());
        }
        return result;
    }

    public async Task MergeImport(int importId, CancellationToken cancellationToken)
    {
        await cadastreFeatureImportRepository.MarkImport(importId);
        LogMarkedImport(importId);

        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTime.UtcNow;
        await cadastreFeatureImportRepository.StoreImportHistorySnapshot(importId, now);
        LogStoredSnapshot(importId);

        cancellationToken.ThrowIfCancellationRequested();
        await cadastreFeatureImportRepository.UpdateFromImport(importId, now);
        LogUpdatedFromImport(importId);
    }

    public Task ClearImport()
    {
        return cadastreFeatureImportRepository.Truncate();
    }

    [LoggerMessage(LogLevel.Information, "Staged {featureCount} cadastre features")]
    private partial void LogStagedFeatures(int featureCount);

    [LoggerMessage(LogLevel.Information, "Staging of cadastre features complete. Total features: {featureCount}")]
    private partial void LogStagingComplete(int featureCount);

    [LoggerMessage(LogLevel.Information, "Marked import {importId} records")]
    private partial void LogMarkedImport(int importId);

    [LoggerMessage(LogLevel.Information, "Stored history snapshot for import {importId}")]
    private partial void LogStoredSnapshot(int importId);

    [LoggerMessage(LogLevel.Information, "Updated features from import {importId}")]
    private partial void LogUpdatedFromImport(int importId);
}
