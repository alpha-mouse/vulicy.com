using NetTopologySuite.Geometries;

namespace Vulicy.Domain;

public record CadastreFeatureSearchResult(string Id, string ElementNameBel, string ElementName, string? Location, string ElementTypeNameBel, Geometry Geometry);