using NetTopologySuite.Geometries;

namespace Vulicy.Domain;

public interface IOsmFeatureRepository
{
    Task<byte[]?> GetTile(int z, int x, int y);
    Task<List<OsmFeatureEntity>> GetUnmatchedIntersectingTracking(Geometry geometry);
    Task<List<OsmFeatureSearchResult>> SearchUnmatchedByName(string query, double? lat = null, double? lng = null);
}