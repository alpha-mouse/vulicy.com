using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore;
using Vulicy.Domain;

namespace Vulicy.DB;

public class OsmFeatureRepository(VulicyDbContext context) : IOsmFeatureRepository
{
    public VulicyDbContext Context { get; } = context;
    protected IQueryable<OsmFeatureEntity> Entities => Context.Set<OsmFeatureEntity>();

    public Task<List<OsmFeatureEntity>> GetUnmatchedIntersectingTracking(Geometry geometry)
    {
        return Entities
            .AsTracking()
            .Where(x => x.FeatureId == null && !x.IsDeleted && x.Geometry.Intersects(geometry))
            .ToListAsync();
    }
}