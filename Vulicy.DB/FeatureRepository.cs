using Microsoft.EntityFrameworkCore;
using Npgsql;
using Vulicy.DB.Configurations;
using Vulicy.Domain;

namespace Vulicy.DB;

public class FeatureRepository(VulicyDbContext dbContext)
    : RepositoryBase<FeatureEntity, int>(dbContext)
        , IFeatureRepository
{
    public Task<byte[]?> GetTile(int z, int x, int y)
    {
        const string query = $"""
            select ST_AsMVT(tile, 'streets') as "Value" from (
              select
                f."{nameof(FeatureEntity.Id)}",
                ST_AsMVTGeom(ST_Transform(f."{nameof(FeatureEntity.Geometry)}", 3857), ST_TileEnvelope(@z, @x, @y), 4096, 64, true) AS geom,
                f."{nameof(FeatureEntity.NameBeTarask)}",
                f."{nameof(FeatureEntity.NameBeNark)}",
                f."{nameof(FeatureEntity.NameRu)}",
                case when f."{nameof(FeatureEntity.Classification)}" = 0 then dr."{nameof(DossierRecordEntity.Classification)}" else f."{nameof(FeatureEntity.Classification)}" end as "{nameof(FeatureEntity.Classification)}",
                f."{nameof(FeatureEntity.Type)}",
                coalesce(f."{nameof(FeatureEntity.RenamingReason)}", dr."{nameof(DossierRecordEntity.DescriptionBe)}") as "{nameof(FeatureEntity.RenamingReason)}",
                dr."{nameof(DossierRecordEntity.NameBeTarask)}" as "EtymologyBeTarask",
                f."{nameof(FeatureEntity.HistoricNames)}",
                f."{nameof(FeatureEntity.HistoricPossible)}",
                f."{nameof(FeatureEntity.YearNamed)}",
                f."{nameof(FeatureEntity.ForumRelativeLink)}",
                coalesce (f."{nameof(FeatureEntity.NamingCategoryId)}", dr."{nameof(DossierRecordEntity.NamingCategoryId)}") as "{nameof(FeatureEntity.NamingCategoryId)}"
              from "{FeatureConfiguration.TableName}" f
              left outer join "{DossierRecordConfiguration.TableName}" dr on f."{nameof(FeatureEntity.DossierRecordId)}" = dr."{nameof(DossierRecordEntity.Id)}"
              where f."{nameof(FeatureEntity.Geometry)}" && ST_Transform(ST_TileEnvelope(@z, @x, @y), 4326)
            ) AS tile
            """;

        return Context.Database
            .SqlQueryRaw<byte[]>(query,
                new NpgsqlParameter("z", z),
                new NpgsqlParameter("x", x),
                new NpgsqlParameter("y", y))
            .FirstOrDefaultAsync();
    }

    public Task<List<FeatureSearchResult>> SearchByName(string query, double? lat = null, double? lng = null)
    {
        var cleanedQuery = DatabaseHelpers.CleanQuery(query);

        if (string.IsNullOrWhiteSpace(cleanedQuery))
            return Task.FromResult(new List<FeatureSearchResult>());

        const string baseSearchSql = $"""
             select
                 f."{nameof(FeatureEntity.Id)}",
                 f."{nameof(FeatureEntity.NameBeTarask)}",
                 f."{nameof(FeatureEntity.NameBeNark)}",
                 f."{nameof(FeatureEntity.NameRu)}",
                 f."{nameof(FeatureEntity.Type)}",
                 cf."{nameof(CadastreFeatureEntity.AteNameBel)}" as "{nameof(FeatureSearchResult.Location)}",
                 f."{nameof(FeatureEntity.Geometry)}"
             from "{FeatureConfiguration.TableName}" f
             left outer join "{CadastreFeatureConfiguration.TableName}" cf on f."{nameof(FeatureEntity.Id)}" = cf."{nameof(CadastreFeatureEntity.FeatureId)}"
             where (
                   f."{nameof(FeatureEntity.NameBeTarask)}" ilike @query
                   or f."{nameof(FeatureEntity.NameBeNark)}" ilike @query
                   or f."{nameof(FeatureEntity.NameRu)}" ilike @query
               )
             
             """;

        const string searchWithoutCoordinates = baseSearchSql + "limit 20";
        const string searchWithCoordinates = baseSearchSql +
             $"""
             order by f."{nameof(FeatureEntity.Geometry)}" <-> ST_SetSRID(ST_MakePoint(@lng, @lat), 4326)
             limit 20
             """;

        IQueryable<FeatureSearchResult> result;
        if (lat.HasValue && lng.HasValue)
        {
            result = Context.Database
                .SqlQueryRaw<FeatureSearchResult>(searchWithCoordinates,
                    new NpgsqlParameter("query", $"%{cleanedQuery}%"),
                    new NpgsqlParameter("lat", lat.Value),
                    new NpgsqlParameter("lng", lng.Value)
                );
        }
        else
        {
            result = Context.Database
                .SqlQueryRaw<FeatureSearchResult>(searchWithoutCoordinates,
                    new NpgsqlParameter("query", $"%{cleanedQuery}%")
                );
        }

        return result.ToListAsync();
    }

    public Task<byte[]?> GetTileDetails(int z, int x, int y)
    {
        const string query = $"""
            select ST_AsMVT(tile, 'streets') as "Value" from (
              select
                f."{nameof(FeatureEntity.Id)}",
                ST_AsMVTGeom(ST_Transform(f."{nameof(FeatureEntity.Geometry)}", 3857), ST_TileEnvelope(@z, @x, @y), 4096, 64, true) AS geom,
                f."{nameof(FeatureEntity.NameBeTarask)}",
                f."{nameof(FeatureEntity.NameBeNark)}",
                f."{nameof(FeatureEntity.NameRu)}",
                f."{nameof(FeatureEntity.Classification)}",
                f."{nameof(FeatureEntity.Type)}",
                f."{nameof(FeatureEntity.RenamingReason)}",
                f."{nameof(FeatureEntity.HistoricNames)}",
                f."{nameof(FeatureEntity.HistoricPossible)}",
                f."{nameof(FeatureEntity.Comment)}",
                dr."{nameof(DossierRecordEntity.Id)}" as "DossierRecordId",
                dr."{nameof(DossierRecordEntity.NameBeTarask)}" as "DossierRecordNameBeTarask",
                dr."{nameof(DossierRecordEntity.Classification)}" as "DossierRecordClassification",
                dr."{nameof(DossierRecordEntity.DescriptionBe)}" as "DossierRecordDescriptionBe",
                dr."{nameof(DossierRecordEntity.DescriptionRu)}" as "DossierRecordDescriptionRu",
                dr."{nameof(DossierRecordEntity.NamingCategoryId)}" as "DossierRecordNamingCategoryId",
                f."{nameof(FeatureEntity.YearNamed)}",
                f."{nameof(FeatureEntity.ForumRelativeLink)}",
                f."{nameof(FeatureEntity.NamingCategoryId)}"
              from "{FeatureConfiguration.TableName}" f
              left outer join "{DossierRecordConfiguration.TableName}" dr on f."{nameof(FeatureEntity.DossierRecordId)}" = dr."{nameof(DossierRecordEntity.Id)}"
              where f."{nameof(FeatureEntity.Geometry)}" && ST_Transform(ST_TileEnvelope(@z, @x, @y), 4326)
            ) AS tile
            """;

        return Context.Database
            .SqlQueryRaw<byte[]>(query,
                new NpgsqlParameter("z", z),
                new NpgsqlParameter("x", x),
                new NpgsqlParameter("y", y))
            .FirstOrDefaultAsync();
    }

    public Task<List<FeatureSearchResult>> GetByDossierRecord(int dossierRecordId)
    {
        return Entities
            .Where(x => x.DossierRecordId == dossierRecordId)
            .Select(x => new FeatureSearchResult(
                x.Id,
                x.NameBeTarask,
                x.NameBeNark,
                x.NameRu,
                x.CadastreFeature == null ? null : x.CadastreFeature.AteNameBel,
                x.Type,
                x.Geometry
            ))
            .ToListAsync();
    }

    public Task<ForumTopicData?> GetCreateForumTopicData(int id)
    {
        const string query = $"""
            select
                f."{nameof(FeatureEntity.Type)}",
                coalesce(f."{nameof(FeatureEntity.NameBeTarask)}", f."{nameof(FeatureEntity.NameBeNark)}", f."{nameof(FeatureEntity.NameRu)}") as "Name",
                ST_Y(ST_Centroid(f."{nameof(FeatureEntity.Geometry)}")) as "Lat",
                ST_X(ST_Centroid(f."{nameof(FeatureEntity.Geometry)}")) as "Lng",
                f."{nameof(FeatureEntity.ForumRelativeLink)}"
            from "{FeatureConfiguration.TableName}" f
            where f."{nameof(FeatureEntity.Id)}" = @id
            """;

        return Context.Database
            .SqlQueryRaw<ForumTopicData>(query, new NpgsqlParameter("id", id))
            .FirstOrDefaultAsync();
    }

    public async Task UpdateForumLink(int featureId, string forumRelativeLink, int userId)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        var feature = await Entities.AsTracking().FirstOrDefaultAsync(x => x.Id == featureId);
        if (feature != null)
        {
            var history = FeatureHistoricEntity.FromBase(feature);
            history.ChangeDateTime = DateTime.UtcNow;
            history.InHistoryById = userId;
            Context.Add(history);

            feature.ForumRelativeLink = forumRelativeLink;
            feature.LastModifiedById = userId;
            await Context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }

    public async Task SetForumLinkIfEmpty(int featureId, string forumRelativeLink, int userId)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        var feature = await Entities.AsTracking().FirstOrDefaultAsync(x => x.Id == featureId);
        if (feature is { ForumRelativeLink: null })
        {
            var history = FeatureHistoricEntity.FromBase(feature);
            history.ChangeDateTime = DateTime.UtcNow;
            history.InHistoryById = userId;
            Context.Add(history);

            feature.ForumRelativeLink = forumRelativeLink;
            feature.LastModifiedById = userId;
            await Context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }


    public Task MirrorIsDeletedFromCadastre(DateTime now)
    {
        const string query =
            $"""
             with "DeletingFeatures" as (
                select f."{nameof(FeatureEntity.Id)}"
                from "{FeatureConfiguration.TableName}" as f
                join "{CadastreFeatureConfiguration.TableName}" as cf on f."{nameof(FeatureEntity.Id)}" = cf."{nameof(CadastreFeatureEntity.FeatureId)}"
                where cf."{nameof(CadastreFeatureEntity.IsDeleted)}"
             )
             insert into "{FeatureHistoricConfiguration.TableName}" (
                 "{nameof(FeatureHistoricEntity.Id)}",
                 "{nameof(FeatureHistoricEntity.CreatedDateTime)}",
                 "{nameof(FeatureHistoricEntity.ModifiedDateTime)}",
                 "{nameof(FeatureHistoricEntity.NameBeTarask)}",
                 "{nameof(FeatureHistoricEntity.NameBeNark)}",
                 "{nameof(FeatureHistoricEntity.NameRu)}",
                 "{nameof(FeatureHistoricEntity.Classification)}",
                 "{nameof(FeatureHistoricEntity.Type)}",
                 "{nameof(FeatureHistoricEntity.RenamingReason)}",
                 "{nameof(FeatureHistoricEntity.HistoricNames)}",
                 "{nameof(FeatureHistoricEntity.Comment)}",
                 "{nameof(FeatureHistoricEntity.HistoricPossible)}",
                 "{nameof(FeatureHistoricEntity.YearNamed)}",
                 "{nameof(FeatureHistoricEntity.ForumRelativeLink)}",
                 "{nameof(FeatureHistoricEntity.Geometry)}",
                 "{nameof(FeatureHistoricEntity.NamingCategoryId)}",
                 "{nameof(FeatureHistoricEntity.DossierRecordId)}",
                 "{nameof(FeatureHistoricEntity.ChangeDateTime)}"
             )
             select
                 f."{nameof(FeatureEntity.Id)}",
                 f."{nameof(FeatureEntity.CreatedDateTime)}",
                 f."{nameof(FeatureEntity.ModifiedDateTime)}",
                 f."{nameof(FeatureEntity.NameBeTarask)}",
                 f."{nameof(FeatureEntity.NameBeNark)}",
                 f."{nameof(FeatureEntity.NameRu)}",
                 f."{nameof(FeatureEntity.Classification)}",
                 f."{nameof(FeatureEntity.Type)}",
                 f."{nameof(FeatureEntity.RenamingReason)}",
                 f."{nameof(FeatureEntity.HistoricNames)}",
                 f."{nameof(FeatureEntity.Comment)}",
                 f."{nameof(FeatureEntity.HistoricPossible)}",
                 f."{nameof(FeatureEntity.YearNamed)}",
                 f."{nameof(FeatureEntity.ForumRelativeLink)}",
                 f."{nameof(FeatureEntity.Geometry)}",
                 f."{nameof(FeatureEntity.NamingCategoryId)}",
                 f."{nameof(FeatureEntity.DossierRecordId)}",
                 @now
             from "{FeatureConfiguration.TableName}" f
             join "DeletingFeatures" df on f."{nameof(FeatureEntity.Id)}" = df."{nameof(FeatureEntity.Id)}"
             ;

             with "DeletingFeatures" as (
                select f."{nameof(FeatureEntity.Id)}"
                from "{FeatureConfiguration.TableName}" as f
                join "{CadastreFeatureConfiguration.TableName}" as cf on f."{nameof(FeatureEntity.Id)}" = cf."{nameof(CadastreFeatureEntity.FeatureId)}"
                where cf."{nameof(CadastreFeatureEntity.IsDeleted)}"
             )
             delete from "{FeatureConfiguration.TableName}" f
             using "DeletingFeatures" df
             where f."{nameof(FeatureEntity.Id)}" = df."{nameof(FeatureEntity.Id)}"
             ;
             """;
        return Context.Database.ExecuteSqlRawAsync(query, new NpgsqlParameter("now", now));
    }

    public Task<List<FeatureEntity>> GetByAteWithCadastreTracking(int ate)
    {
        return Entities
            .AsTracking()
            .Include(x => x.CadastreFeature)
            .Where(x => x.CadastreFeature != null && x.CadastreFeature.Ate == ate)
            .ToListAsync();
    }

    public Task<List<FeatureEntity>> GetByAteWithImportsTracking(int ate)
    {
        return Entities
            .AsTracking()
            .Include(x => x.CadastreFeature)
            .Include(x => x.OsmFeatures)
            .Where(x => x.CadastreFeature != null && x.CadastreFeature.Ate == ate)
            .ToListAsync();
    }

    public Task<List<FeatureEntity>> GetNextForGeometryUpdateTracking(int batchSize)
    {
        return Entities
            .Include(x => x.OsmFeatures)
            .Where(x => x.OsmFeatures.Any(x => x.GeometryUpdatePending))
            // unstable ordering doesn't matter here, anyway everything will be processed eventually
            .Take(batchSize)
            .ToListAsync();
    }

    public Task MirrorFromInitialCadastre()
    {
        const string query =
            $"""
             with "UpdatingFeatures" as (
                select cf."{nameof(CadastreFeatureEntity.FeatureId)}",
                    icfi."{nameof(InitialCadastreFeatureImportEntity.HistoricName)}",
                    icfi."{nameof(InitialCadastreFeatureImportEntity.Comment)}",
                    icfi."{nameof(InitialCadastreFeatureImportEntity.HistoricPossible)}",
                    icfi."{nameof(InitialCadastreFeatureImportEntity.YearNamed)}"
                from "{CadastreFeatureConfiguration.TableName}" as cf
                join "{InitialCadastreFeatureImportConfiguration.TableName}" as icfi on cf."{nameof(CadastreFeatureEntity.Id)}" = icfi."{nameof(InitialCadastreFeatureImportEntity.Id)}"
                where cf."{nameof(CadastreFeatureEntity.FeatureId)}" is not null
             )
             update "{FeatureConfiguration.TableName}" as f
             set
                 "{nameof(FeatureEntity.HistoricNames)}" = uf."{nameof(InitialCadastreFeatureImportEntity.HistoricName)}",
                 "{nameof(FeatureEntity.Comment)}" = uf."{nameof(InitialCadastreFeatureImportEntity.Comment)}",
                 "{nameof(FeatureEntity.HistoricPossible)}" = uf."{nameof(InitialCadastreFeatureImportEntity.HistoricPossible)}",
                 "{nameof(FeatureEntity.YearNamed)}" = uf."{nameof(InitialCadastreFeatureImportEntity.YearNamed)}"
             from "UpdatingFeatures" as uf
             where f."{nameof(FeatureEntity.Id)}" = uf."{nameof(CadastreFeatureEntity.FeatureId)}"
             ;
             """;
        return Context.Database.ExecuteSqlRawAsync(query);
    }

    public Task AssignClassificationsFromInitialCadastre()
    {
        const string query =
            $"""
             with "UpdatingFeatures" as (
                select cf."{nameof(CadastreFeatureEntity.FeatureId)}",
                    icfi."{nameof(InitialCadastreFeatureImportEntity.Classification)}",
                    f."{nameof(FeatureEntity.DossierRecordId)}"
                from "{CadastreFeatureConfiguration.TableName}" as cf
                join "{InitialCadastreFeatureImportConfiguration.TableName}" as icfi on cf."{nameof(CadastreFeatureEntity.Id)}" = icfi."{nameof(InitialCadastreFeatureImportEntity.Id)}"
                join "{FeatureConfiguration.TableName}" as f on cf."{nameof(CadastreFeatureEntity.FeatureId)}" = f."{nameof(FeatureEntity.Id)}"
                where cf."{nameof(CadastreFeatureEntity.FeatureId)}" is not null
             )
             update "{FeatureConfiguration.TableName}" as f
             set
                 "{nameof(FeatureEntity.Classification)}" = uf."{nameof(InitialCadastreFeatureImportEntity.Classification)}"
             from "UpdatingFeatures" as uf
             left outer join "{DossierRecordConfiguration.TableName}" dr
                on uf."{nameof(FeatureEntity.DossierRecordId)}" = dr."{nameof(DossierRecordEntity.Id)}"
             where f."{nameof(FeatureEntity.Id)}" = uf."{nameof(CadastreFeatureEntity.FeatureId)}"
                and (dr."{nameof(DossierRecordEntity.Id)}" is null or dr."{nameof(DossierRecordEntity.Classification)}" != uf."{nameof(InitialCadastreFeatureImportEntity.Classification)}")
             ;
             """;
        return Context.Database.ExecuteSqlRawAsync(query);
    }
}