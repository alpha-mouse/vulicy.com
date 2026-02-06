using Microsoft.EntityFrameworkCore;
using Npgsql;
using Vulicy.DB.Configurations;
using Vulicy.Domain;

namespace Vulicy.DB;

public class CadastreFeatureRepository(VulicyDbContext context)
    : RepositoryBase<CadastreFeatureEntity, string>(context)
        , ICadastreFeatureRepository
{
    public Task<byte[]?> GetUnmatchedTile(int z, int x, int y)
    {
        const string query = $"""
            select ST_AsMVT(tile, 'streets') as "Value" from (
              select
                cf."{nameof(CadastreFeatureEntity.Id)}" as "id",
                ST_AsMVTGeom(ST_Transform(cf."{nameof(CadastreFeatureEntity.Geometry)}", 3857), ST_TileEnvelope(@z, @x, @y), 4096, 64, true) as "geom",
                cf."{nameof(CadastreFeatureEntity.ElementNameBel)}" as "elementNameBel",
                cf."{nameof(CadastreFeatureEntity.ElementName)}" as "elementName",
                cf."{nameof(CadastreFeatureEntity.FeatureId)}" as "featureId",
                cf."{nameof(CadastreFeatureEntity.ElementTypeShortNameBel)}" as "elementTypeShortNameBel",
                cf."{nameof(CadastreFeatureEntity.ShortInfo)}" as "shortInfo",
                cf."{nameof(CadastreFeatureEntity.AteNameBel)}"as "location"
              from "{CadastreFeatureConfiguration.TableName}" cf
              where cf."{nameof(CadastreFeatureEntity.Geometry)}" && ST_Transform(ST_TileEnvelope(@z, @x, @y), 4326)
                and cf."{nameof(CadastreFeatureEntity.FeatureId)}" is null
            ) AS tile
            """;

        return Context.Database
            .SqlQueryRaw<byte[]>(query,
                new NpgsqlParameter("z", z),
                new NpgsqlParameter("x", x),
                new NpgsqlParameter("y", y))
            .FirstOrDefaultAsync();
    }

    public Task<List<int>> GetUnmatchedAtes()
    {
        return Entities
            .Where(x => x.FeatureId == null && !x.IsDeleted)
            .Select(x => x.Ate)
            .Distinct()
            .ToListAsync();
    }

    public Task<List<CadastreFeatureEntity>> GetUnmatchedByAteTracking(int ate)
    {
        return Entities
            .AsTracking()
            .Where(x => x.FeatureId == null && !x.IsDeleted && x.Ate == ate)
            .ToListAsync();
    }

    public Task<List<int>> GetAllAtes()
    {
        return Entities
            .Where(x => x.Feature != null)
            .Select(x => x.Ate)
            .Distinct()
            .ToListAsync();
    }

    public Task<List<CadastreFeatureSearchResult>> SearchUnmatchedByName(string query, double? lat = null, double? lng = null)
    {
        var cleanedQuery = DatabaseHelpers.CleanQuery(query);

        if (string.IsNullOrWhiteSpace(cleanedQuery))
            return Task.FromResult(new List<CadastreFeatureSearchResult>());

        const string baseSearchSql = 
            $"""
             select
                 cf."{nameof(CadastreFeatureEntity.Id)}",
                 cf."{nameof(CadastreFeatureEntity.Geometry)}",
                 cf."{nameof(CadastreFeatureEntity.ElementNameBel)}",
                 cf."{nameof(CadastreFeatureEntity.ElementName)}",
                 null as "{nameof(CadastreFeatureEntity.FeatureId)}",
                 cf."{nameof(CadastreFeatureEntity.ElementTypeShortNameBel)}",
                 cf."{nameof(CadastreFeatureEntity.ShortInfo)}",
                 cf."{nameof(CadastreFeatureEntity.AteNameBel)}" as {nameof(CadastreFeatureSearchResult.Location)}
             from "{CadastreFeatureConfiguration.TableName}" cf
             where (
                   cf."{nameof(CadastreFeatureEntity.ElementNameBel)}" ilike @query
                   or cf."{nameof(CadastreFeatureEntity.ElementName)}" ilike @query
               )
             and cf."{nameof(CadastreFeatureEntity.FeatureId)}" is null

             """;

        const string searchWithoutCoordinates = baseSearchSql + "limit 20";
        const string searchWithCoordinates = baseSearchSql +
                                             $"""
                                              order by cf."{nameof(OsmFeatureEntity.Geometry)}" <-> ST_SetSRID(ST_MakePoint(@lng, @lat), 4326)
                                              limit 20
                                              """;

        IQueryable<CadastreFeatureSearchResult> result;
        if (lat.HasValue && lng.HasValue)
        {
            result = Context.Database
                .SqlQueryRaw<CadastreFeatureSearchResult>(searchWithCoordinates,
                    new NpgsqlParameter("query", $"%{cleanedQuery}%"),
                    new NpgsqlParameter("lat", lat.Value),
                    new NpgsqlParameter("lng", lng.Value)
                );
        }
        else
        {
            result = Context.Database
                .SqlQueryRaw<CadastreFeatureSearchResult>(searchWithoutCoordinates,
                    new NpgsqlParameter("query", $"%{cleanedQuery}%")
                );
        }

        return result.ToListAsync();
    }
}