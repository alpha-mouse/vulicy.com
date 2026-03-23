using NetTopologySuite.Geometries;

namespace Vulicy.Domain;

public record OsmFeatureSearchResult(
    long Id,
    OsmType Type,
    string? Highway,
    Dictionary<string, string> Tags,
    Geometry Geometry,
    int? FeatureId);