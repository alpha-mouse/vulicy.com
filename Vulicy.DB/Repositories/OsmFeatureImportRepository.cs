using Microsoft.EntityFrameworkCore;
using Npgsql;
using Vulicy.DB.Configurations;
using Vulicy.Domain;

namespace Vulicy.DB.Repositories;

public class OsmFeatureImportRepository(VulicyDbContext dbContext)
    : RepositoryBase<OsmFeatureImportEntity, long>(dbContext)
        , IOsmFeatureImportRepository
{
    public Task MarkImport(int importId)
    {
        const string query =
            $"""
            merge into "{OsmFeatureImportConfiguration.TableName}" as target
                using "{OsmFeatureConfiguration.TableName}" as source
            on target."{nameof(OsmFeatureImportEntity.Id)}" = source."{nameof(OsmFeatureEntity.Id)}"
                and target."{nameof(OsmFeatureImportEntity.Type)}" = source."{nameof(OsmFeatureEntity.Type)}"
                and target."{nameof(OsmFeatureImportEntity.ImportId)}" = @importId
            when matched
                and target."{nameof(OsmFeatureEntity.Geometry)}" != source."{nameof(OsmFeatureEntity.Geometry)}"
                or target."{nameof(OsmFeatureEntity.Tags)}" != source."{nameof(OsmFeatureEntity.Tags)}"
                then
            update set
                "{nameof(OsmFeatureImportEntity.DoUpdate)}" = true
            when not matched by target then
            insert
                (
                "{nameof(OsmFeatureImportEntity.ImportId)}",
                "{nameof(OsmFeatureImportEntity.Id)}",
                "{nameof(OsmFeatureImportEntity.CreatedDateTime)}",
                "{nameof(OsmFeatureImportEntity.ModifiedDateTime)}",
                
                "{nameof(OsmFeatureImportEntity.FeatureId)}",
                "{nameof(OsmFeatureImportEntity.Type)}",
                "{nameof(OsmFeatureImportEntity.Geometry)}",
                "{nameof(OsmFeatureImportEntity.Tags)}",
                "{nameof(OsmFeatureImportEntity.IsDeleted)}",
                
                "{nameof(OsmFeatureImportEntity.DoUpdate)}"
                )
            values
                (
                @importId,
                source."{nameof(OsmFeatureEntity.Id)}",
                source."{nameof(OsmFeatureEntity.CreatedDateTime)}",
                source."{nameof(OsmFeatureEntity.ModifiedDateTime)}",
                
                source."{nameof(OsmFeatureEntity.FeatureId)}",
                source."{nameof(OsmFeatureEntity.Type)}",
                source."{nameof(OsmFeatureEntity.Geometry)}",
                source."{nameof(OsmFeatureEntity.Tags)}",
                true,
                
                true
                )
            when not matched by source then
            update set
                "{nameof(OsmFeatureImportEntity.DoUpdate)}" = true
            ;
            """;
        return Context.Database.ExecuteSqlRawAsync(query, new NpgsqlParameter("importId", importId));
    }

    public Task StoreImportHistorySnapshot(int importId, DateTime now)
    {
        const string query =
            $"""
            with "UpdatingFeatures" as (
                 select "{nameof(OsmFeatureImportEntity.Id)}", "{nameof(OsmFeatureImportEntity.Type)}"
                 from "{OsmFeatureImportConfiguration.TableName}"
                 where "{nameof(OsmFeatureImportEntity.ImportId)}" = @importId
                    and "{nameof(OsmFeatureImportEntity.DoUpdate)}" = true
            )
            insert into "{OsmFeatureHistoricConfiguration.TableName}"
                (
                "{nameof(OsmFeatureHistoricEntity.Id)}",
                "{nameof(OsmFeatureHistoricEntity.CreatedDateTime)}",
                "{nameof(OsmFeatureHistoricEntity.ModifiedDateTime)}",
                
                "{nameof(OsmFeatureHistoricEntity.FeatureId)}",
                "{nameof(OsmFeatureHistoricEntity.Type)}",
                "{nameof(OsmFeatureHistoricEntity.Geometry)}",
                "{nameof(OsmFeatureHistoricEntity.Tags)}",
                "{nameof(OsmFeatureHistoricEntity.IsDeleted)}",
                
                "{nameof(OsmFeatureHistoricEntity.ChangeDateTime)}"
                )
            select
                f."{nameof(OsmFeatureEntity.Id)}",
                f."{nameof(OsmFeatureEntity.CreatedDateTime)}",
                f."{nameof(OsmFeatureEntity.ModifiedDateTime)}",
                
                f."{nameof(OsmFeatureEntity.FeatureId)}",
                f."{nameof(OsmFeatureEntity.Type)}",
                f."{nameof(OsmFeatureEntity.Geometry)}",
                f."{nameof(OsmFeatureEntity.Tags)}",
                f."{nameof(OsmFeatureEntity.IsDeleted)}",
                
                @now
            from "{OsmFeatureConfiguration.TableName}" f
            join "UpdatingFeatures" uf
                on f."{nameof(OsmFeatureEntity.Id)}" = uf."{nameof(OsmFeatureImportEntity.Id)}"
                and f."{nameof(OsmFeatureEntity.Type)}" = uf."{nameof(OsmFeatureImportEntity.Type)}"
            ;
            """;
        return Context.Database.ExecuteSqlRawAsync(
            query,
            new NpgsqlParameter("importId", importId),
            new NpgsqlParameter("now", now)
        );
    }

    public Task UpdateFromImport(int importId, DateTime now)
    {
        const string query =
            $"""
            with "UpdatingFeatures" as (
                select *
                from "{OsmFeatureImportConfiguration.TableName}"
                where "{nameof(OsmFeatureImportEntity.ImportId)}" = @importId
                    and "{nameof(OsmFeatureImportEntity.DoUpdate)}" = true
            )
            merge into "{OsmFeatureConfiguration.TableName}" as target
                using "UpdatingFeatures" as source
            on target."{nameof(OsmFeatureEntity.Id)}" = source."{nameof(OsmFeatureImportEntity.Id)}"
                and target."{nameof(OsmFeatureEntity.Type)}" = source."{nameof(OsmFeatureImportEntity.Type)}"
            when matched then
            update set
                "{nameof(OsmFeatureEntity.ModifiedDateTime)}" = @now,
                "{nameof(OsmFeatureEntity.Geometry)}" = source."{nameof(OsmFeatureImportEntity.Geometry)}",
                "{nameof(OsmFeatureEntity.Tags)}" = source."{nameof(OsmFeatureImportEntity.Tags)}",
                "{nameof(OsmFeatureEntity.IsDeleted)}" = source."{nameof(OsmFeatureImportEntity.IsDeleted)}",
                "{nameof(OsmFeatureEntity.GeometryUpdatePending)}" = source."{nameof(OsmFeatureImportEntity.IsDeleted)}" or target."{nameof(OsmFeatureEntity.Geometry)}" != source."{nameof(OsmFeatureImportEntity.Geometry)}"
            when not matched then
            insert
                (
                "{nameof(OsmFeatureEntity.Id)}",
                "{nameof(OsmFeatureEntity.CreatedDateTime)}",
                "{nameof(OsmFeatureEntity.ModifiedDateTime)}",
                "{nameof(OsmFeatureEntity.Type)}",
                
                "{nameof(OsmFeatureEntity.Geometry)}",
                "{nameof(OsmFeatureEntity.Tags)}",
                "{nameof(OsmFeatureEntity.IsDeleted)}",
                "{nameof(OsmFeatureEntity.GeometryUpdatePending)}"
                )
                values
                (
                source."{nameof(OsmFeatureImportEntity.Id)}",
                @now,
                @now,
                source."{nameof(OsmFeatureImportEntity.Type)}",
                
                source."{nameof(OsmFeatureImportEntity.Geometry)}",
                source."{nameof(OsmFeatureImportEntity.Tags)}",
                source."{nameof(OsmFeatureImportEntity.IsDeleted)}",
                false
                )
            ;
            """;
        return Context.Database.ExecuteSqlRawAsync(
            query,
            new NpgsqlParameter("importId", importId),
            new NpgsqlParameter("now", now)
        );
    }

    public Task Truncate()
    {
        const string query =
            $"""
             truncate table "{OsmFeatureImportConfiguration.TableName}";
             """;
        return Context.Database.ExecuteSqlRawAsync(query);
    }
}