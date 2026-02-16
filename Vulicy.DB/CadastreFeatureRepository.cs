using System.Data;
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
                cf."{nameof(CadastreFeatureEntity.AteNameBel)}"as "location",
                icfi."{nameof(InitialCadastreFeatureImportEntity.Reason)}" as "reason",
                icfi."{nameof(InitialCadastreFeatureImportEntity.Classification)}" as "classification",
                icfi."{nameof(InitialCadastreFeatureImportEntity.Comment)}" as "comment",
                icfi."{nameof(InitialCadastreFeatureImportEntity.HistoricName)}" as "historicName",
                COALESCE(icfi."{nameof(InitialCadastreFeatureImportEntity.HistoricPossible)}", false) as "historicPossible",
                icfi."{nameof(InitialCadastreFeatureImportEntity.YearNamed)}" as "yearNamed",
                icfi."{nameof(InitialCadastreFeatureImportEntity.NameCategory)}" as "nameCategory"
              from "{CadastreFeatureConfiguration.TableName}" cf
              left outer join "{InitialCadastreFeatureImportConfiguration.TableName}" icfi on cf."{nameof(CadastreFeatureEntity.Id)}" = icfi."{nameof(InitialCadastreFeatureImportEntity.Id)}"
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
        var cleanedQuery = DatabaseHelpers.CleanQueryTerms(query);

        if (cleanedQuery.Count == 0)
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
                 cf."{nameof(CadastreFeatureEntity.AteNameBel)}" as {nameof(CadastreFeatureSearchResult.Location)},
                 icfi."{nameof(InitialCadastreFeatureImportEntity.Reason)}",
                 icfi."{nameof(InitialCadastreFeatureImportEntity.Classification)}",
                 icfi."{nameof(InitialCadastreFeatureImportEntity.Comment)}",
                 icfi."{nameof(InitialCadastreFeatureImportEntity.HistoricName)}",
                 COALESCE(icfi."{nameof(InitialCadastreFeatureImportEntity.HistoricPossible)}", false) as "{nameof(InitialCadastreFeatureImportEntity.HistoricPossible)}",
                 icfi."{nameof(InitialCadastreFeatureImportEntity.YearNamed)}",
                 icfi."{nameof(InitialCadastreFeatureImportEntity.NameCategory)}"
             from "{CadastreFeatureConfiguration.TableName}" cf
             left outer join "{InitialCadastreFeatureImportConfiguration.TableName}" icfi on cf."{nameof(CadastreFeatureEntity.Id)}" = icfi."{nameof(InitialCadastreFeatureImportEntity.Id)}"
             where (
                   (
                   @term0 is null
                   or @term0 is not null and cf."{nameof(CadastreFeatureEntity.ElementNameBel)}" ilike @term0
                   or @term0 is not null and cf."{nameof(CadastreFeatureEntity.ElementName)}" ilike @term0
                   or @term0 is not null and cf."{nameof(CadastreFeatureEntity.AteName)}" ilike @term0
                   or @term0 is not null and cf."{nameof(CadastreFeatureEntity.AteNameBel)}" ilike @term0
                   ) and (
                   @term1 is null
                   or @term1 is not null and cf."{nameof(CadastreFeatureEntity.ElementNameBel)}" ilike @term1
                   or @term1 is not null and cf."{nameof(CadastreFeatureEntity.ElementName)}" ilike @term1
                   or @term1 is not null and cf."{nameof(CadastreFeatureEntity.AteName)}" ilike @term1
                   or @term1 is not null and cf."{nameof(CadastreFeatureEntity.AteNameBel)}" ilike @term1
                   ) and (
                   @term2 is null
                   or @term2 is not null and cf."{nameof(CadastreFeatureEntity.ElementNameBel)}" ilike @term2
                   or @term2 is not null and cf."{nameof(CadastreFeatureEntity.ElementName)}" ilike @term2
                   or @term2 is not null and cf."{nameof(CadastreFeatureEntity.AteName)}" ilike @term2
                   or @term2 is not null and cf."{nameof(CadastreFeatureEntity.AteNameBel)}" ilike @term2
                   )
               )
             and cf."{nameof(CadastreFeatureEntity.FeatureId)}" is null

             """;

        const string searchWithoutCoordinates = baseSearchSql + "limit 20";
        const string searchWithCoordinates = baseSearchSql +
                                             $"""
                                              order by cf."{nameof(OsmFeatureEntity.Geometry)}" <-> ST_SetSRID(ST_MakePoint(@lng, @lat), 4326)
                                              limit 20
                                              """;

        var parameters = new List<NpgsqlParameter>(5)
        {
            cleanedQuery.Count >= 1 ? new NpgsqlParameter("term0", cleanedQuery[0]) : new NpgsqlParameter("term0", DbType.AnsiString) { NpgsqlValue = DBNull.Value },
            cleanedQuery.Count >= 2 ? new NpgsqlParameter("term1", cleanedQuery[1]) : new NpgsqlParameter("term1", DbType.AnsiString) { NpgsqlValue = DBNull.Value },
            cleanedQuery.Count >= 3 ? new NpgsqlParameter("term2", cleanedQuery[2]) : new NpgsqlParameter("term2", DbType.AnsiString) { NpgsqlValue = DBNull.Value },
        };
        if (lat.HasValue && lng.HasValue)
        {
            parameters.Add(new NpgsqlParameter("lat", lat.Value));
            parameters.Add(new NpgsqlParameter("lng", lng.Value));
        }

        return Context.Database
                .SqlQueryRaw<CadastreFeatureSearchResult>(searchWithCoordinates, parameters.ToArray())
                .ToListAsync();
    }
}