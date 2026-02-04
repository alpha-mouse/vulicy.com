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

    public Task<List<OsmFeatureSearchResult>> SearchUnmatchedByName(string query, double? lat = null, double? lng = null)
    {
        var cleanedQuery = DatabaseHelpers.CleanQuery(query);

        if (string.IsNullOrWhiteSpace(cleanedQuery))
            return Task.FromResult(new List<OsmFeatureSearchResult>());

        const string baseSearchSql = 
            $"""
             select
                 of."{nameof(OsmFeatureEntity.Id)}",
                 of."{nameof(OsmFeatureEntity.Tags)}",
                 of."{nameof(OsmFeatureEntity.Type)}",
                 of."{nameof(OsmFeatureEntity.Geometry)}"
             from "{OsmFeatureConfiguration.TableName}" of
             where (
                   of."{nameof(OsmFeatureEntity.Tags)}"->>'name' ilike @query
                   or of."{nameof(OsmFeatureEntity.Tags)}"->>'name:be' ilike @query
                   or of."{nameof(OsmFeatureEntity.Tags)}"->>'name:be-tarask' ilike @query
                   or of."{nameof(OsmFeatureEntity.Tags)}"->>'name:ru' ilike @query
               )
             and of."{nameof(OsmFeatureEntity.FeatureId)}" is null

             """;

        const string searchWithoutCoordinates = baseSearchSql + "limit 20";
        const string searchWithCoordinates = baseSearchSql +
                                             $"""
                                              order by of."{nameof(OsmFeatureEntity.Geometry)}" <-> ST_SetSRID(ST_MakePoint(@lng, @lat), 4326)
                                              limit 20
                                              """;

        IQueryable<OsmFeatureSearchResult> result;
        if (lat.HasValue && lng.HasValue)
        {
            result = Context.Database
                .SqlQueryRaw<OsmFeatureSearchResult>(searchWithCoordinates,
                    new NpgsqlParameter("query", $"%{cleanedQuery}%"),
                    new NpgsqlParameter("lat", lat.Value),
                    new NpgsqlParameter("lng", lng.Value)
                );
        }
        else
        {
            result = Context.Database
                .SqlQueryRaw<OsmFeatureSearchResult>(searchWithoutCoordinates,
                    new NpgsqlParameter("query", $"%{cleanedQuery}%")
                );
        }

        return result.ToListAsync();
    }
}