using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Npgsql;
using Vulicy.DB.Configurations;
using Vulicy.Domain;

namespace Vulicy.DB.Repositories;

public partial class OsmFeatureRepository(VulicyDbContext context) : IOsmFeatureRepository
{
    public VulicyDbContext Context { get; } = context;
    protected IQueryable<OsmFeatureEntity> Entities => Context.Set<OsmFeatureEntity>();

    public Task<byte[]?> GetUnmatchedTile(int z, int x, int y)
    {
        const string query = $"""
            select ST_AsMVT(tile, 'streets') as "Value" from (
              select
                of."{nameof(OsmFeatureEntity.Id)}" as "id",
                of."{nameof(OsmFeatureEntity.Type)}" as "type",
                of."{nameof(OsmFeatureEntity.Tags)}"::text as "tags",
                of."{nameof(OsmFeatureEntity.Tags)}"->>'highway' as "highway",
                of."{nameof(OsmFeatureEntity.Tags)}"->>'name' as "name",
                of."{nameof(OsmFeatureEntity.Tags)}"->>'name:be' as "name:be",
                of."{nameof(OsmFeatureEntity.Tags)}"->>'name:be-tarask' as "name:be-tarask",
                of."{nameof(OsmFeatureEntity.Tags)}"->>'name:ru' as "name:ru",
                ST_AsMVTGeom(ST_Transform(of."{nameof(OsmFeatureEntity.Geometry)}", 3857), ST_TileEnvelope(@z, @x, @y), 4096, 64, true) as "geom",
                of."{nameof(OsmFeatureEntity.FeatureId)}" as "featureId"
              from "{OsmFeatureConfiguration.TableName}" of
              where of."{nameof(OsmFeatureEntity.Geometry)}" && ST_Transform(ST_TileEnvelope(@z, @x, @y), 4326)
                and of."{nameof(OsmFeatureEntity.FeatureId)}" is null
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

    public Task<List<OsmFeatureSearchResult>> SearchUnmatched(string query, double? lat = null, double? lng = null)
    {
        var idMatch = IdSearchRegex().Match(query);
        long? idQuery = null;
        string? cleanedQuery = null;
        if (idMatch.Success)
            idQuery = long.Parse(idMatch.Groups["id"].Value);
        else
            cleanedQuery = DatabaseHelpers.CleanQuery(query);

        if (string.IsNullOrWhiteSpace(cleanedQuery) && idQuery == null)
            return Task.FromResult(new List<OsmFeatureSearchResult>());

        const string baseSearchSql = 
            $"""
             select
                 of."{nameof(OsmFeatureEntity.Id)}",
                 of."{nameof(OsmFeatureEntity.Type)}",
                 of."{nameof(OsmFeatureEntity.Tags)}",
                 of."{nameof(OsmFeatureEntity.Tags)}"->>'highway' as "{nameof(OsmFeatureSearchResult.Highway)}",
                 of."{nameof(OsmFeatureEntity.Geometry)}",
                 null as "{nameof(OsmFeatureEntity.FeatureId)}"
             from "{OsmFeatureConfiguration.TableName}" of
             where of."{nameof(OsmFeatureEntity.FeatureId)}" is null

             """;

        const string idSearchSql = baseSearchSql + $" and of.\"{nameof(OsmFeatureEntity.Id)}\" = @idQuery ";
        const string textSearchSql = baseSearchSql +
            $"""
             and (
                   of."{nameof(OsmFeatureEntity.Tags)}"->>'name' ilike @query
                   or of."{nameof(OsmFeatureEntity.Tags)}"->>'name:be' ilike @query
                   or of."{nameof(OsmFeatureEntity.Tags)}"->>'name:be-tarask' ilike @query
                   or of."{nameof(OsmFeatureEntity.Tags)}"->>'name:ru' ilike @query
               )
             """;

        const string limit20 = " limit 20";
        const string coordinatesProximityLimit20 =
            $"""
             order by of."{nameof(OsmFeatureEntity.Geometry)}" <-> ST_SetSRID(ST_MakePoint(@lng, @lat), 4326)
             limit 20
             """;

        const string textSearchWithoutCoordinates = textSearchSql + limit20;
        const string textSearchWithCoordinates = textSearchSql + coordinatesProximityLimit20;
        const string idSearchWithoutCoordinates = idSearchSql + limit20;
        const string idSearchWithCoordinates = idSearchSql + coordinatesProximityLimit20;

        var coordinateProvided = lat.HasValue && lng.HasValue;
        var querySql = (idQuery.HasValue, coordinateProvided) switch
        {
            (false, false) => textSearchWithoutCoordinates,
            (false, true) => textSearchWithCoordinates,
            (true, false) => idSearchWithoutCoordinates,
            (true, true) => idSearchWithCoordinates,
        };

        return Context.Database
            .SqlQueryRaw<OsmFeatureSearchResult>(querySql,
                new NpgsqlParameter("idQuery", idQuery ?? 0),
                new NpgsqlParameter("query", $"%{cleanedQuery}%"),
                new NpgsqlParameter("lat", lat ?? 0),
                new NpgsqlParameter("lng", lng ?? 0)
            )
            .ToListAsync();
    }

    public Task<OsmFeatureEntity?> GetById(OsmType type, long id)
    {
        return Entities
            .FirstOrDefaultAsync(x => x.Type == type && x.Id == id);
    }

    public Task<OsmFeatureEntity?> GetByIdTracked(OsmType type, long id)
    {
        return Entities
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Type == type && x.Id == id);
    }

    public Task<List<OsmFeatureEntity>> GetByFeatureId(int featureId)
    {
        return Entities
            .Where(x => x.FeatureId == featureId)
            .ToListAsync();
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"^\s*(?<id>\d+)\s*$")]
    private static partial System.Text.RegularExpressions.Regex IdSearchRegex();
}