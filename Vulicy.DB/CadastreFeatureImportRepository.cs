using Microsoft.EntityFrameworkCore;
using Npgsql;
using Vulicy.Domain;
using Vulicy.DB.Configurations;

namespace Vulicy.DB;

public class CadastreFeatureImportRepository(VulicyDbContext dbContext)
    : RepositoryBase<CadastreFeatureImportEntity, string>(dbContext)
        , ICadastreFeatureImportRepository
{
    public Task MarkImport(int importId)
    {
        const string query =
            $"""
            merge into "{CadastreFeatureImportConfiguration.TableName}" as target
                using "{CadastreFeatureConfiguration.TableName}" as source
            on target."{nameof(CadastreFeatureImportEntity.Id)}" = source."{nameof(CadastreFeatureEntity.Id)}"
                and target."{nameof(CadastreFeatureImportEntity.ImportId)}" = @importId
            when matched
                and target."{nameof(CadastreFeatureEntity.Geometry)}" != source."{nameof(CadastreFeatureEntity.Geometry)}"
                or target."{nameof(CadastreFeatureEntity.IdIae)}" != source."{nameof(CadastreFeatureEntity.IdIae)}"
                or target."{nameof(CadastreFeatureEntity.ParentAte)}" != source."{nameof(CadastreFeatureEntity.ParentAte)}"
                or target."{nameof(CadastreFeatureEntity.Region)}" != source."{nameof(CadastreFeatureEntity.Region)}"
                or target."{nameof(CadastreFeatureEntity.District)}" != source."{nameof(CadastreFeatureEntity.District)}"
                or target."{nameof(CadastreFeatureEntity.VillageCouncil)}" != source."{nameof(CadastreFeatureEntity.VillageCouncil)}"
                or target."{nameof(CadastreFeatureEntity.Ate)}" != source."{nameof(CadastreFeatureEntity.Ate)}"
                or target."{nameof(CadastreFeatureEntity.RegionName)}" != source."{nameof(CadastreFeatureEntity.RegionName)}"
                or target."{nameof(CadastreFeatureEntity.DistrictName)}" != source."{nameof(CadastreFeatureEntity.DistrictName)}"
                or target."{nameof(CadastreFeatureEntity.VillageCouncilName)}" != source."{nameof(CadastreFeatureEntity.VillageCouncilName)}"
                or target."{nameof(CadastreFeatureEntity.AteName)}" != source."{nameof(CadastreFeatureEntity.AteName)}"
                or target."{nameof(CadastreFeatureEntity.RegionNameBel)}" != source."{nameof(CadastreFeatureEntity.RegionNameBel)}"
                or target."{nameof(CadastreFeatureEntity.DistrictNameBel)}" != source."{nameof(CadastreFeatureEntity.DistrictNameBel)}"
                or target."{nameof(CadastreFeatureEntity.VillageCouncilNameBel)}" != source."{nameof(CadastreFeatureEntity.VillageCouncilNameBel)}"
                or target."{nameof(CadastreFeatureEntity.AteNameBel)}" != source."{nameof(CadastreFeatureEntity.AteNameBel)}"
                or target."{nameof(CadastreFeatureEntity.CategoryName)}" != source."{nameof(CadastreFeatureEntity.CategoryName)}"
                or target."{nameof(CadastreFeatureEntity.CategoryNameShort)}" != source."{nameof(CadastreFeatureEntity.CategoryNameShort)}"
                or target."{nameof(CadastreFeatureEntity.CategoryNameBel)}" != source."{nameof(CadastreFeatureEntity.CategoryNameBel)}"
                or target."{nameof(CadastreFeatureEntity.CategoryNameShortBel)}" != source."{nameof(CadastreFeatureEntity.CategoryNameShortBel)}"
                or target."{nameof(CadastreFeatureEntity.ElementType)}" != source."{nameof(CadastreFeatureEntity.ElementType)}"
                or target."{nameof(CadastreFeatureEntity.ElementTypeName)}" != source."{nameof(CadastreFeatureEntity.ElementTypeName)}"
                or target."{nameof(CadastreFeatureEntity.ElementTypeNameBel)}" != source."{nameof(CadastreFeatureEntity.ElementTypeNameBel)}"
                or target."{nameof(CadastreFeatureEntity.ElementTypeShortName)}" != source."{nameof(CadastreFeatureEntity.ElementTypeShortName)}"
                or target."{nameof(CadastreFeatureEntity.ElementTypeShortNameBel)}" != source."{nameof(CadastreFeatureEntity.ElementTypeShortNameBel)}"
                or target."{nameof(CadastreFeatureEntity.ElementName)}" != source."{nameof(CadastreFeatureEntity.ElementName)}"
                or target."{nameof(CadastreFeatureEntity.ElementNameBel)}" != source."{nameof(CadastreFeatureEntity.ElementNameBel)}"
                or target."{nameof(CadastreFeatureEntity.ShortInfo)}" != source."{nameof(CadastreFeatureEntity.ShortInfo)}"
                or target."{nameof(CadastreFeatureEntity.ObjectNumber)}" != source."{nameof(CadastreFeatureEntity.ObjectNumber)}"
                then
            update set
                "{nameof(CadastreFeatureImportEntity.DoUpdate)}" = true
            when not matched by target then
            insert
                (
                "{nameof(CadastreFeatureImportEntity.ImportId)}",
                "{nameof(CadastreFeatureImportEntity.Id)}",
                "{nameof(CadastreFeatureImportEntity.CreatedDateTime)}",
                "{nameof(CadastreFeatureImportEntity.ModifiedDateTime)}",
                
                "{nameof(CadastreFeatureImportEntity.Geometry)}",
                "{nameof(CadastreFeatureImportEntity.IdIae)}",
                "{nameof(CadastreFeatureImportEntity.ParentAte)}",
                "{nameof(CadastreFeatureImportEntity.Region)}",
                "{nameof(CadastreFeatureImportEntity.District)}",
                "{nameof(CadastreFeatureImportEntity.VillageCouncil)}",
                "{nameof(CadastreFeatureImportEntity.Ate)}",
                "{nameof(CadastreFeatureImportEntity.RegionName)}",
                "{nameof(CadastreFeatureImportEntity.DistrictName)}",
                "{nameof(CadastreFeatureImportEntity.VillageCouncilName)}",
                "{nameof(CadastreFeatureImportEntity.AteName)}",
                "{nameof(CadastreFeatureImportEntity.RegionNameBel)}",
                "{nameof(CadastreFeatureImportEntity.DistrictNameBel)}",
                "{nameof(CadastreFeatureImportEntity.VillageCouncilNameBel)}",
                "{nameof(CadastreFeatureImportEntity.AteNameBel)}",
                "{nameof(CadastreFeatureImportEntity.CategoryName)}",
                "{nameof(CadastreFeatureImportEntity.CategoryNameShort)}",
                "{nameof(CadastreFeatureImportEntity.CategoryNameBel)}",
                "{nameof(CadastreFeatureImportEntity.CategoryNameShortBel)}",
                "{nameof(CadastreFeatureImportEntity.ElementType)}",
                "{nameof(CadastreFeatureImportEntity.ElementTypeName)}",
                "{nameof(CadastreFeatureImportEntity.ElementTypeNameBel)}",
                "{nameof(CadastreFeatureImportEntity.ElementTypeShortName)}",
                "{nameof(CadastreFeatureImportEntity.ElementTypeShortNameBel)}",
                "{nameof(CadastreFeatureImportEntity.ElementName)}",
                "{nameof(CadastreFeatureImportEntity.ElementNameBel)}",
                "{nameof(CadastreFeatureImportEntity.ShortInfo)}",
                "{nameof(CadastreFeatureImportEntity.ObjectNumber)}",
                
                "{nameof(CadastreFeatureImportEntity.IsDeleted)}",
                "{nameof(CadastreFeatureImportEntity.DoUpdate)}"
                )
            values
                (
                @importId,
                source."{nameof(CadastreFeatureEntity.Id)}",
                source."{nameof(CadastreFeatureEntity.CreatedDateTime)}",
                source."{nameof(CadastreFeatureEntity.ModifiedDateTime)}",
                
                source."{nameof(CadastreFeatureEntity.Geometry)}",
                source."{nameof(CadastreFeatureEntity.IdIae)}",
                source."{nameof(CadastreFeatureEntity.ParentAte)}",
                source."{nameof(CadastreFeatureEntity.Region)}",
                source."{nameof(CadastreFeatureEntity.District)}",
                source."{nameof(CadastreFeatureEntity.VillageCouncil)}",
                source."{nameof(CadastreFeatureEntity.Ate)}",
                source."{nameof(CadastreFeatureEntity.RegionName)}",
                source."{nameof(CadastreFeatureEntity.DistrictName)}",
                source."{nameof(CadastreFeatureEntity.VillageCouncilName)}",
                source."{nameof(CadastreFeatureEntity.AteName)}",
                source."{nameof(CadastreFeatureEntity.RegionNameBel)}",
                source."{nameof(CadastreFeatureEntity.DistrictNameBel)}",
                source."{nameof(CadastreFeatureEntity.VillageCouncilNameBel)}",
                source."{nameof(CadastreFeatureEntity.AteNameBel)}",
                source."{nameof(CadastreFeatureEntity.CategoryName)}",
                source."{nameof(CadastreFeatureEntity.CategoryNameShort)}",
                source."{nameof(CadastreFeatureEntity.CategoryNameBel)}",
                source."{nameof(CadastreFeatureEntity.CategoryNameShortBel)}",
                source."{nameof(CadastreFeatureEntity.ElementType)}",
                source."{nameof(CadastreFeatureEntity.ElementTypeName)}",
                source."{nameof(CadastreFeatureEntity.ElementTypeNameBel)}",
                source."{nameof(CadastreFeatureEntity.ElementTypeShortName)}",
                source."{nameof(CadastreFeatureEntity.ElementTypeShortNameBel)}",
                source."{nameof(CadastreFeatureEntity.ElementName)}",
                source."{nameof(CadastreFeatureEntity.ElementNameBel)}",
                source."{nameof(CadastreFeatureEntity.ShortInfo)}",
                source."{nameof(CadastreFeatureEntity.ObjectNumber)}",
                
                true,
                true
                )
            when not matched by source then
            update set
                "{nameof(CadastreFeatureImportEntity.DoUpdate)}" = true
            ;
            """;
        return Context.Database.ExecuteSqlRawAsync(query, new NpgsqlParameter("importId", importId));
    }

    public Task StoreImportHistorySnapshot(int importId, DateTime now)
    {
        const string query =
            $"""
            with "UpdatingFeatures" as (
                select "{nameof(CadastreFeatureImportEntity.Id)}"
                from "{CadastreFeatureImportConfiguration.TableName}"
                where "{nameof(CadastreFeatureImportEntity.ImportId)}" = @importId
                    and "{nameof(CadastreFeatureImportEntity.DoUpdate)}" = true
            )
            insert into "{CadastreFeatureHistoricConfiguration.TableName}"
                (
                "{nameof(CadastreFeatureHistoricEntity.Id)}",
                "{nameof(CadastreFeatureHistoricEntity.CreatedDateTime)}",
                "{nameof(CadastreFeatureHistoricEntity.ModifiedDateTime)}",
                
                "{nameof(CadastreFeatureHistoricEntity.FeatureId)}",
                
                "{nameof(CadastreFeatureHistoricEntity.Geometry)}",
                "{nameof(CadastreFeatureHistoricEntity.IdIae)}",
                "{nameof(CadastreFeatureHistoricEntity.ParentAte)}",
                "{nameof(CadastreFeatureHistoricEntity.Region)}",
                "{nameof(CadastreFeatureHistoricEntity.District)}",
                "{nameof(CadastreFeatureHistoricEntity.VillageCouncil)}",
                "{nameof(CadastreFeatureHistoricEntity.Ate)}",
                "{nameof(CadastreFeatureHistoricEntity.RegionName)}",
                "{nameof(CadastreFeatureHistoricEntity.DistrictName)}",
                "{nameof(CadastreFeatureHistoricEntity.VillageCouncilName)}",
                "{nameof(CadastreFeatureHistoricEntity.AteName)}",
                "{nameof(CadastreFeatureHistoricEntity.RegionNameBel)}",
                "{nameof(CadastreFeatureHistoricEntity.DistrictNameBel)}",
                "{nameof(CadastreFeatureHistoricEntity.VillageCouncilNameBel)}",
                "{nameof(CadastreFeatureHistoricEntity.AteNameBel)}",
                "{nameof(CadastreFeatureHistoricEntity.CategoryName)}",
                "{nameof(CadastreFeatureHistoricEntity.CategoryNameShort)}",
                "{nameof(CadastreFeatureHistoricEntity.CategoryNameBel)}",
                "{nameof(CadastreFeatureHistoricEntity.CategoryNameShortBel)}",
                "{nameof(CadastreFeatureHistoricEntity.ElementType)}",
                "{nameof(CadastreFeatureHistoricEntity.ElementTypeName)}",
                "{nameof(CadastreFeatureHistoricEntity.ElementTypeNameBel)}",
                "{nameof(CadastreFeatureHistoricEntity.ElementTypeShortName)}",
                "{nameof(CadastreFeatureHistoricEntity.ElementTypeShortNameBel)}",
                "{nameof(CadastreFeatureHistoricEntity.ElementName)}",
                "{nameof(CadastreFeatureHistoricEntity.ElementNameBel)}",
                "{nameof(CadastreFeatureHistoricEntity.ShortInfo)}",
                "{nameof(CadastreFeatureHistoricEntity.ObjectNumber)}",
                
                "{nameof(CadastreFeatureHistoricEntity.IsDeleted)}",
                "{nameof(CadastreFeatureHistoricEntity.ChangeDateTime)}"
                )
            select
                f."{nameof(CadastreFeatureEntity.Id)}",
                f."{nameof(CadastreFeatureEntity.CreatedDateTime)}",
                f."{nameof(CadastreFeatureEntity.ModifiedDateTime)}",
                
                f."{nameof(CadastreFeatureEntity.FeatureId)}",
                
                f."{nameof(CadastreFeatureEntity.Geometry)}",
                f."{nameof(CadastreFeatureEntity.IdIae)}",
                f."{nameof(CadastreFeatureEntity.ParentAte)}",
                f."{nameof(CadastreFeatureEntity.Region)}",
                f."{nameof(CadastreFeatureEntity.District)}",
                f."{nameof(CadastreFeatureEntity.VillageCouncil)}",
                f."{nameof(CadastreFeatureEntity.Ate)}",
                f."{nameof(CadastreFeatureEntity.RegionName)}",
                f."{nameof(CadastreFeatureEntity.DistrictName)}",
                f."{nameof(CadastreFeatureEntity.VillageCouncilName)}",
                f."{nameof(CadastreFeatureEntity.AteName)}",
                f."{nameof(CadastreFeatureEntity.RegionNameBel)}",
                f."{nameof(CadastreFeatureEntity.DistrictNameBel)}",
                f."{nameof(CadastreFeatureEntity.VillageCouncilNameBel)}",
                f."{nameof(CadastreFeatureEntity.AteNameBel)}",
                f."{nameof(CadastreFeatureEntity.CategoryName)}",
                f."{nameof(CadastreFeatureEntity.CategoryNameShort)}",
                f."{nameof(CadastreFeatureEntity.CategoryNameBel)}",
                f."{nameof(CadastreFeatureEntity.CategoryNameShortBel)}",
                f."{nameof(CadastreFeatureEntity.ElementType)}",
                f."{nameof(CadastreFeatureEntity.ElementTypeName)}",
                f."{nameof(CadastreFeatureEntity.ElementTypeNameBel)}",
                f."{nameof(CadastreFeatureEntity.ElementTypeShortName)}",
                f."{nameof(CadastreFeatureEntity.ElementTypeShortNameBel)}",
                f."{nameof(CadastreFeatureEntity.ElementName)}",
                f."{nameof(CadastreFeatureEntity.ElementNameBel)}",
                f."{nameof(CadastreFeatureEntity.ShortInfo)}",
                f."{nameof(CadastreFeatureEntity.ObjectNumber)}",
                
                f."{nameof(CadastreFeatureEntity.IsDeleted)}",
                @now
            from "{CadastreFeatureConfiguration.TableName}" f
            join "UpdatingFeatures" uf
                on f."{nameof(CadastreFeatureEntity.Id)}" = uf."{nameof(CadastreFeatureImportEntity.Id)}"
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
                from "{CadastreFeatureImportConfiguration.TableName}"
                where "{nameof(CadastreFeatureImportEntity.ImportId)}" = @importId
                    and "{nameof(CadastreFeatureImportEntity.DoUpdate)}" = true
            )
            merge into "{CadastreFeatureConfiguration.TableName}" as target
                using "UpdatingFeatures" as source
            on target."{nameof(CadastreFeatureEntity.Id)}" = source."{nameof(CadastreFeatureImportEntity.Id)}"
            when matched then
            update set
                "{nameof(CadastreFeatureEntity.ModifiedDateTime)}" = @now,
                
                "{nameof(CadastreFeatureEntity.Geometry)}" = source."{nameof(CadastreFeatureEntity.Geometry)}",
                "{nameof(CadastreFeatureEntity.IdIae)}" = source."{nameof(CadastreFeatureEntity.IdIae)}",
                "{nameof(CadastreFeatureEntity.ParentAte)}" = source."{nameof(CadastreFeatureEntity.ParentAte)}",
                "{nameof(CadastreFeatureEntity.Region)}" = source."{nameof(CadastreFeatureEntity.Region)}",
                "{nameof(CadastreFeatureEntity.District)}" = source."{nameof(CadastreFeatureEntity.District)}",
                "{nameof(CadastreFeatureEntity.VillageCouncil)}" = source."{nameof(CadastreFeatureEntity.VillageCouncil)}",
                "{nameof(CadastreFeatureEntity.Ate)}" = source."{nameof(CadastreFeatureEntity.Ate)}",
                "{nameof(CadastreFeatureEntity.RegionName)}" = source."{nameof(CadastreFeatureEntity.RegionName)}",
                "{nameof(CadastreFeatureEntity.DistrictName)}" = source."{nameof(CadastreFeatureEntity.DistrictName)}",
                "{nameof(CadastreFeatureEntity.VillageCouncilName)}" = source."{nameof(CadastreFeatureEntity.VillageCouncilName)}",
                "{nameof(CadastreFeatureEntity.AteName)}" = source."{nameof(CadastreFeatureEntity.AteName)}",
                "{nameof(CadastreFeatureEntity.RegionNameBel)}" = source."{nameof(CadastreFeatureEntity.RegionNameBel)}",
                "{nameof(CadastreFeatureEntity.DistrictNameBel)}" = source."{nameof(CadastreFeatureEntity.DistrictNameBel)}",
                "{nameof(CadastreFeatureEntity.VillageCouncilNameBel)}" = source."{nameof(CadastreFeatureEntity.VillageCouncilNameBel)}",
                "{nameof(CadastreFeatureEntity.AteNameBel)}" = source."{nameof(CadastreFeatureEntity.AteNameBel)}",
                "{nameof(CadastreFeatureEntity.CategoryName)}" = source."{nameof(CadastreFeatureEntity.CategoryName)}",
                "{nameof(CadastreFeatureEntity.CategoryNameShort)}" = source."{nameof(CadastreFeatureEntity.CategoryNameShort)}",
                "{nameof(CadastreFeatureEntity.CategoryNameBel)}" = source."{nameof(CadastreFeatureEntity.CategoryNameBel)}",
                "{nameof(CadastreFeatureEntity.CategoryNameShortBel)}" = source."{nameof(CadastreFeatureEntity.CategoryNameShortBel)}",
                "{nameof(CadastreFeatureEntity.ElementType)}" = source."{nameof(CadastreFeatureEntity.ElementType)}",
                "{nameof(CadastreFeatureEntity.ElementTypeName)}" = source."{nameof(CadastreFeatureEntity.ElementTypeName)}",
                "{nameof(CadastreFeatureEntity.ElementTypeNameBel)}" = source."{nameof(CadastreFeatureEntity.ElementTypeNameBel)}",
                "{nameof(CadastreFeatureEntity.ElementTypeShortName)}" = source."{nameof(CadastreFeatureEntity.ElementTypeShortName)}",
                "{nameof(CadastreFeatureEntity.ElementTypeShortNameBel)}" = source."{nameof(CadastreFeatureEntity.ElementTypeShortNameBel)}",
                "{nameof(CadastreFeatureEntity.ElementName)}" = source."{nameof(CadastreFeatureEntity.ElementName)}",
                "{nameof(CadastreFeatureEntity.ElementNameBel)}" = source."{nameof(CadastreFeatureEntity.ElementNameBel)}",
                "{nameof(CadastreFeatureEntity.ShortInfo)}" = source."{nameof(CadastreFeatureEntity.ShortInfo)}",
                "{nameof(CadastreFeatureEntity.ObjectNumber)}" = source."{nameof(CadastreFeatureEntity.ObjectNumber)}",
                
                "{nameof(CadastreFeatureEntity.IsDeleted)}" = source."{nameof(CadastreFeatureImportEntity.IsDeleted)}"
            when not matched then
            insert
                (
                "{nameof(CadastreFeatureEntity.Id)}",
                "{nameof(CadastreFeatureEntity.CreatedDateTime)}",
                "{nameof(CadastreFeatureEntity.ModifiedDateTime)}",
                
                "{nameof(CadastreFeatureEntity.Geometry)}",
                "{nameof(CadastreFeatureEntity.IdIae)}",
                "{nameof(CadastreFeatureEntity.ParentAte)}",
                "{nameof(CadastreFeatureEntity.Region)}",
                "{nameof(CadastreFeatureEntity.District)}",
                "{nameof(CadastreFeatureEntity.VillageCouncil)}",
                "{nameof(CadastreFeatureEntity.Ate)}",
                "{nameof(CadastreFeatureEntity.RegionName)}",
                "{nameof(CadastreFeatureEntity.DistrictName)}",
                "{nameof(CadastreFeatureEntity.VillageCouncilName)}",
                "{nameof(CadastreFeatureEntity.AteName)}",
                "{nameof(CadastreFeatureEntity.RegionNameBel)}",
                "{nameof(CadastreFeatureEntity.DistrictNameBel)}",
                "{nameof(CadastreFeatureEntity.VillageCouncilNameBel)}",
                "{nameof(CadastreFeatureEntity.AteNameBel)}",
                "{nameof(CadastreFeatureEntity.CategoryName)}",
                "{nameof(CadastreFeatureEntity.CategoryNameShort)}",
                "{nameof(CadastreFeatureEntity.CategoryNameBel)}",
                "{nameof(CadastreFeatureEntity.CategoryNameShortBel)}",
                "{nameof(CadastreFeatureEntity.ElementType)}",
                "{nameof(CadastreFeatureEntity.ElementTypeName)}",
                "{nameof(CadastreFeatureEntity.ElementTypeNameBel)}",
                "{nameof(CadastreFeatureEntity.ElementTypeShortName)}",
                "{nameof(CadastreFeatureEntity.ElementTypeShortNameBel)}",
                "{nameof(CadastreFeatureEntity.ElementName)}",
                "{nameof(CadastreFeatureEntity.ElementNameBel)}",
                "{nameof(CadastreFeatureEntity.ShortInfo)}",
                "{nameof(CadastreFeatureEntity.ObjectNumber)}",
                "{nameof(CadastreFeatureEntity.IsDeleted)}"
                )
                values
                (
                source."{nameof(CadastreFeatureImportEntity.Id)}",
                @now,
                @now,
                
                source."{nameof(CadastreFeatureEntity.Geometry)}",
                source."{nameof(CadastreFeatureEntity.IdIae)}",
                source."{nameof(CadastreFeatureEntity.ParentAte)}",
                source."{nameof(CadastreFeatureEntity.Region)}",
                source."{nameof(CadastreFeatureEntity.District)}",
                source."{nameof(CadastreFeatureEntity.VillageCouncil)}",
                source."{nameof(CadastreFeatureEntity.Ate)}",
                source."{nameof(CadastreFeatureEntity.RegionName)}",
                source."{nameof(CadastreFeatureEntity.DistrictName)}",
                source."{nameof(CadastreFeatureEntity.VillageCouncilName)}",
                source."{nameof(CadastreFeatureEntity.AteName)}",
                source."{nameof(CadastreFeatureEntity.RegionNameBel)}",
                source."{nameof(CadastreFeatureEntity.DistrictNameBel)}",
                source."{nameof(CadastreFeatureEntity.VillageCouncilNameBel)}",
                source."{nameof(CadastreFeatureEntity.AteNameBel)}",
                source."{nameof(CadastreFeatureEntity.CategoryName)}",
                source."{nameof(CadastreFeatureEntity.CategoryNameShort)}",
                source."{nameof(CadastreFeatureEntity.CategoryNameBel)}",
                source."{nameof(CadastreFeatureEntity.CategoryNameShortBel)}",
                source."{nameof(CadastreFeatureEntity.ElementType)}",
                source."{nameof(CadastreFeatureEntity.ElementTypeName)}",
                source."{nameof(CadastreFeatureEntity.ElementTypeNameBel)}",
                source."{nameof(CadastreFeatureEntity.ElementTypeShortName)}",
                source."{nameof(CadastreFeatureEntity.ElementTypeShortNameBel)}",
                source."{nameof(CadastreFeatureEntity.ElementName)}",
                source."{nameof(CadastreFeatureEntity.ElementNameBel)}",
                source."{nameof(CadastreFeatureEntity.ShortInfo)}",
                source."{nameof(CadastreFeatureEntity.ObjectNumber)}",
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
        const string query = $"""
            truncate table "{CadastreFeatureImportConfiguration.TableName}";
            """;
        return Context.Database.ExecuteSqlRawAsync(query);
    }
}