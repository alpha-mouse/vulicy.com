namespace Vulicy.Domain;

public record FeatureSearchResult(int Id, string NameBeTarask, string NameBeNark, string? NameRu, string? Location, FeatureType Type, double Latitude, double Longitude);