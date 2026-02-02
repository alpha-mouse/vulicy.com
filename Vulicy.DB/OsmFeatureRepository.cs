using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Npgsql;
using Vulicy.DB.Configurations;
using Vulicy.Domain;

namespace Vulicy.DB;

public class OsmFeatureRepository(VulicyDbContext context) : IOsmFeatureRepository
{
    public VulicyDbContext Context { get; } = context;
    protected IQueryable<OsmFeatureEntity> Entities => Context.Set<OsmFeatureEntity>();

    public Task<byte[]?> GetTile(int z, int x, int y)
    {
        const string query = $"""
            select ST_AsMVT(tile, 'streets') as "Value" from (
              select
                of."{nameof(OsmFeatureEntity.Id)}",
                ST_AsMVTGeom(ST_Transform(of."{nameof(OsmFeatureEntity.Geometry)}", 3857), ST_TileEnvelope(@z, @x, @y), 4096, 64, true) AS geom,
                of."{nameof(OsmFeatureEntity.Type)}",
                of."{nameof(OsmFeatureEntity.FeatureId)}",
                of."{nameof(OsmFeatureEntity.Tags)}"
              from "{OsmFeatureConfiguration.TableName}" of
              where of."{nameof(OsmFeatureEntity.Geometry)}" && ST_Transform(ST_TileEnvelope(@z, @x, @y), 4326)
            ) AS tile
            """;

        return Context.Database
            .SqlQueryRaw<byte[]>(query,
                new NpgsqlParameter("z", z),
                new NpgsqlParameter("x", x),
                new NpgsqlParameter("y", y))
            .FirstOrDefaultAsync();
    }

    public Task<List<OsmFeatureEntity>> GetUnmatchedIntersectingTracking(Geometry geometry)
    {
        return Entities
            .AsTracking()
            .Where(x => x.FeatureId == null && !x.IsDeleted && x.Geometry.Intersects(geometry))
            .ToListAsync();
    }
}