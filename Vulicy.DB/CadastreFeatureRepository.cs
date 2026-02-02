using Microsoft.EntityFrameworkCore;
using Npgsql;
using Vulicy.DB.Configurations;
using Vulicy.Domain;

namespace Vulicy.DB;

public class CadastreFeatureRepository(VulicyDbContext context)
    : RepositoryBase<CadastreFeatureEntity, string>(context)
        , ICadastreFeatureRepository
{
    public Task<byte[]?> GetTile(int z, int x, int y)
    {
        const string query = $"""
            select ST_AsMVT(tile, 'streets') as "Value" from (
              select
                cf."{nameof(CadastreFeatureEntity.Id)}",
                ST_AsMVTGeom(ST_Transform(cf."{nameof(CadastreFeatureEntity.Geometry)}", 3857), ST_TileEnvelope(@z, @x, @y), 4096, 64, true) AS geom,
                cf."{nameof(CadastreFeatureEntity.ElementNameBel)}",
                cf."{nameof(CadastreFeatureEntity.ElementName)}",
                cf."{nameof(CadastreFeatureEntity.FeatureId)}",
                cf."{nameof(CadastreFeatureEntity.ElementTypeShortNameBel)}",
                cf."{nameof(CadastreFeatureEntity.ShortInfo)}"
              from "{CadastreFeatureConfiguration.TableName}" cf
              where cf."{nameof(CadastreFeatureEntity.Geometry)}" && ST_Transform(ST_TileEnvelope(@z, @x, @y), 4326)
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
}