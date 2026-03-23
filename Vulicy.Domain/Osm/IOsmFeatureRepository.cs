using NetTopologySuite.Geometries;

namespace Vulicy.Domain;

public interface IOsmFeatureRepository
{
    Task<byte[]?> GetUnmatchedTile(int z, int x, int y);
    Task<List<OsmFeatureEntity>> GetUnmatchedIntersectingTracking(Geometry geometry);
    Task<List<OsmFeatureSearchResult>> SearchUnmatched(string query, double? lat = null, double? lng = null);
    Task<OsmFeatureEntity?> GetById(OsmType type, long id);
    Task<OsmFeatureEntity?> GetByIdTracked(OsmType type, long id);
    Task<List<OsmFeatureEntity>> GetByFeatureId(int featureId);
}