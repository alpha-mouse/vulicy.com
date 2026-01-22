using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using OsmSharp;
using OsmSharp.Streams;
using Vulicy.Domain;

namespace Vulicy.Services;

public partial class OsmImportService(
    IOsmFeatureImportRepository osmFeatureImportRepository,
    ILogger<OsmImportService>? logger
) : IOsmImportService
{
    private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 4326);

    public async Task StageImport(int importId, string localPath, CancellationToken cancellationToken)
    {
        if (!File.Exists(localPath))
            throw new FileNotFoundException("Import file not found", localPath);

        await using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);

        using OsmStreamSource source = new PBFOsmStreamSource(stream);

        // Dictionary to store node coordinates to resolve way geometries.
        // For Belarus, there are roughly 40 million nodes.
        // A coordinate (16 bytes) + ID (8 bytes) + Dictionary overhead.
        // This is memory intensive but the most reliable way to resolve geometries in one pass.
        var nodeCache = new Dictionary<long, ValueTuple<double, double>>();

        var wayCount = 0;
        var nodeCount = 0;

        foreach (var osmGeo in source)
        {
            if (osmGeo is Node { Id: not null } node)
            {
                if (node is { Latitude: not null, Longitude: not null })
                {
                    nodeCache[node.Id.Value] = new(node.Longitude.Value, node.Latitude.Value);
                    nodeCount++;
                    if (nodeCount % 1000000 == 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        LogCachedNodes(nodeCount / 1000000);
                    }
                }
            }
            // it is a common practice that nodes will be the first elements in OSM PBF files, so by the time we reach ways, all nodes are already cached
            else if (osmGeo is Way { Tags: not null, Id: not null } way
                     && way.Tags.ContainsKey("highway")
                     && (way.Tags.ContainsKey("name") || way.Tags.ContainsKey("name:be") || way.Tags.ContainsKey("name:ru") || way.Tags.ContainsKey("name:be-tarask")))
            {
                var coordinates = ResolveCoordinates(way, nodeCache);
                if (coordinates == null) continue;

                var geometry = GeometryFactory.CreateLineString(coordinates);

                var entity = new OsmFeatureImportEntity
                {
                    Id = way.Id.Value,
                    ImportId = importId,
                    Type = OsmType.Way,
                    Geometry = geometry,
                    Tags = way.Tags.ToDictionary(t => t.Key, t => t.Value),
                    IsDeleted = false,
                };

                osmFeatureImportRepository.Add(entity);
                wayCount++;

                if (wayCount % 1000 == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await osmFeatureImportRepository.SaveChanges();
                    osmFeatureImportRepository.ClearChangeTracker();

                    if (wayCount % 10000 == 0)
                        LogStagedHighways(wayCount);
                }
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        await osmFeatureImportRepository.SaveChanges();
        osmFeatureImportRepository.ClearChangeTracker();
        LogStaged(wayCount, nodeCount);
    }

    public async Task MergeImport(int importId, CancellationToken cancellationToken)
    {
        await osmFeatureImportRepository.MarkImport(importId);
        LogMarkedImport(importId);

        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTime.UtcNow;
        await osmFeatureImportRepository.StoreImportHistorySnapshot(importId, now);
        LogStoredSnapshot(importId);

        cancellationToken.ThrowIfCancellationRequested();
        await osmFeatureImportRepository.UpdateFromImport(importId, now);
        LogUpdatedFromImport(importId);
    }

    public Task ClearImport()
    {
        return osmFeatureImportRepository.Truncate();
    }

    private static Coordinate[]? ResolveCoordinates(Way way, Dictionary<long, ValueTuple<double, double>> nodeCache)
    {
        if (way.Nodes == null || way.Nodes.Length < 2) return null;

        var coordinates = new Coordinate[way.Nodes.Length];
        for (var i = 0; i < way.Nodes.Length; i++)
        {
            if (nodeCache.TryGetValue(way.Nodes[i], out var coordinate))
            {
                coordinates[i] = coordinate;
            }
            else
            {
                // Incomplete way, likely it crossed the boundary of the extract
                return null;
            }
        }

        return coordinates;
    }

    [LoggerMessage(LogLevel.Information, "Cached {nodeMillionCount} million nodes")]
    private partial void LogCachedNodes(int nodeMillionCount);

    [LoggerMessage(LogLevel.Information, "Staged {wayCount} highways")]
    private partial void LogStagedHighways(int wayCount);

    [LoggerMessage(LogLevel.Information, "Staging of current ways complete. Total highways: {wayCount}, Total nodes: {nodeCount}")]
    private partial void LogStaged(int wayCount, int nodeCount);

    [LoggerMessage(LogLevel.Information, "Marked import {importId} records")]
    private partial void LogMarkedImport(int importId);

    [LoggerMessage(LogLevel.Information, "Stored history snapshot for import {importId}")]
    private partial void LogStoredSnapshot(int importId);

    [LoggerMessage(LogLevel.Information, "Updated features from import {importId}")]
    private partial void LogUpdatedFromImport(int importId);
}