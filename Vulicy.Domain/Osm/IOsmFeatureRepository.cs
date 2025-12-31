using NetTopologySuite.Geometries;

namespace Vulicy.Domain;

public interface IOsmFeatureRepository
{
    Task<List<OsmFeatureEntity>> GetUnmatchedIntersectingTracking(Geometry geometry);
}