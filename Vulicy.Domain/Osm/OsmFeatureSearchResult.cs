using NetTopologySuite.Geometries;

namespace Vulicy.Domain;

public record OsmFeatureSearchResult(long Id, OsmType Type, Dictionary<string, string> Tags, Geometry Geometry);