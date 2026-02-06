using NetTopologySuite.Geometries;

namespace Vulicy.Domain;

public record CadastreFeatureSearchResult(
    string Id,
    Geometry Geometry,
    string? ElementNameBel,
    string ElementName,
    int? FeatureId,
    string? ElementTypeShortNameBel,
    string? ShortInfo,
    string? Location);