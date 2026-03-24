using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Npgsql;
using Vulicy.DB.Configurations;
using Vulicy.Domain;

namespace Vulicy.DB.Repositories;

public class AdministrativeRepository(VulicyDbContext dbContext)
    : RepositoryBase<AdministrativeEntity, int>(dbContext)
        , IAdministrativeRepository
{
    public Task CreateMissingAdministrativeFromCadastre()
    {
        const string command =
            $"""
             -- вобласьці
             with "Regions" as (
                select distinct cf."{nameof(CadastreFeatureEntity.RegionName)}",
                                cf."{nameof(CadastreFeatureEntity.RegionNameBel)}",
                                case "{nameof(CadastreFeatureEntity.RegionNameBel)}"
                                     when 'Брэсцкая'        then 1 -- Берасьцейская
                                     when 'Віцебская'       then 2 -- Віцебская
                                     when 'Гродзенская'     then 3 -- Гарадзенская
                                     when 'Гомельская'      then 4 -- Гомельская
                                     when 'Магілёўская'     then 5 -- Магілёўская
                                     when 'Мінская'         then 6 -- Менская
                                 end as "ExpectedId"
                from "{CadastreFeatureConfiguration.TableName}" cf
                where "{nameof(CadastreFeatureEntity.RegionName)}" is not null
                order by case "{nameof(CadastreFeatureEntity.RegionNameBel)}"
                     when 'Брэсцкая'        then 1 -- Берасьцейская
                     when 'Віцебская'       then 2 -- Віцебская
                     when 'Гродзенская'     then 3 -- Гарадзенская
                     when 'Гомельская'      then 4 -- Гомельская
                     when 'Магілёўская'     then 5 -- Магілёўская
                     when 'Мінская'         then 6 -- Менская
                 end
             )
             merge into "{AdministrativeConfiguration.TableName}" as target
                using "Regions" as source 
             on target."{nameof(AdministrativeEntity.Type)}" = @region
                and target."{nameof(AdministrativeEntity.NameRu)}" = source."{nameof(CadastreFeatureEntity.RegionName)}"
             when not matched then
             insert
                 (
                 "{nameof(AdministrativeEntity.CreatedDateTime)}",
                 "{nameof(AdministrativeEntity.ModifiedDateTime)}",
                 
                 "{nameof(AdministrativeEntity.NameBeTarask)}",
                 "{nameof(AdministrativeEntity.NameBeNark)}",
                 "{nameof(AdministrativeEntity.NameRu)}",
                 "{nameof(AdministrativeEntity.Type)}"
                 )
             values
                 (
                 now(), 
                 now(),
                 source."{nameof(CadastreFeatureEntity.RegionNameBel)}",
                 source."{nameof(CadastreFeatureEntity.RegionNameBel)}",
                 source."{nameof(CadastreFeatureEntity.RegionName)}",
                 @region
                 )
             ;

             -- сталіцы (У Беларусі тры сталіцы...)
             with "Capitals" as (
                select distinct cf."{nameof(CadastreFeatureEntity.AteName)}",
                                cf."{nameof(CadastreFeatureEntity.AteNameBel)}",
                                cf."{nameof(CadastreFeatureEntity.Ate)}"
                from "{CadastreFeatureConfiguration.TableName}" cf
                where "{nameof(CadastreFeatureEntity.RegionName)}" is null
             )
             merge into "{AdministrativeConfiguration.TableName}" as target
                using "Capitals" as source 
             on target."{nameof(AdministrativeEntity.Type)}" = @capital
                and target."{nameof(AdministrativeEntity.NameRu)}" = source."{nameof(CadastreFeatureEntity.AteName)}"
             when not matched then
             insert
                 (
                 "{nameof(AdministrativeEntity.CreatedDateTime)}",
                 "{nameof(AdministrativeEntity.ModifiedDateTime)}",
                 
                 "{nameof(AdministrativeEntity.NameBeTarask)}",
                 "{nameof(AdministrativeEntity.NameBeNark)}",
                 "{nameof(AdministrativeEntity.NameRu)}",
                 "{nameof(AdministrativeEntity.Type)}",
                 "{nameof(AdministrativeEntity.CadastreAte)}"
                 )
             values
                 (
                 now(), 
                 now(),
                 source."{nameof(CadastreFeatureEntity.AteNameBel)}",
                 source."{nameof(CadastreFeatureEntity.AteNameBel)}",
                 source."{nameof(CadastreFeatureEntity.AteName)}",
                 @capital,
                 source."{nameof(CadastreFeatureEntity.Ate)}"
                 )
             ;
             
             -- раёны
             with "Districts" as (
                select distinct cf."{nameof(CadastreFeatureEntity.DistrictName)}",
                                cf."{nameof(CadastreFeatureEntity.DistrictNameBel)}",
                                a."{nameof(AdministrativeEntity.Id)}" as "{nameof(AdministrativeEntity.ParentRegionId)}"
                from "{CadastreFeatureConfiguration.TableName}" cf
                join "{AdministrativeConfiguration.TableName}" a on cf."{nameof(CadastreFeatureEntity.RegionName)}" = a."{nameof(AdministrativeEntity.NameRu)}" and a."{nameof(AdministrativeEntity.Type)}" = @region
                where "{nameof(CadastreFeatureEntity.RegionName)}" is not null
                  and "{nameof(CadastreFeatureEntity.DistrictName)}" is not null
             )
             merge into "{AdministrativeConfiguration.TableName}" as target
                using "Districts" as source 
             on target."{nameof(AdministrativeEntity.Type)}" = @district
                and target."{nameof(AdministrativeEntity.NameRu)}" = source."{nameof(CadastreFeatureEntity.DistrictName)}"
             when not matched then
             insert
                 (
                 "{nameof(AdministrativeEntity.CreatedDateTime)}",
                 "{nameof(AdministrativeEntity.ModifiedDateTime)}",
                 
                 "{nameof(AdministrativeEntity.NameBeTarask)}",
                 "{nameof(AdministrativeEntity.NameBeNark)}",
                 "{nameof(AdministrativeEntity.NameRu)}",
                 "{nameof(AdministrativeEntity.ParentRegionId)}",
                 "{nameof(AdministrativeEntity.Type)}"
                 )
             values
                 (
                 now(), 
                 now(),
                 source."{nameof(CadastreFeatureEntity.DistrictNameBel)}",
                 source."{nameof(CadastreFeatureEntity.DistrictNameBel)}",
                 source."{nameof(CadastreFeatureEntity.DistrictName)}",
                 source."{nameof(AdministrativeEntity.ParentRegionId)}",
                 @district
                 )
             ;
             
             -- пасялковыя саветы
             with "VillageCouncils" as (
                select distinct cf."{nameof(CadastreFeatureEntity.VillageCouncilName)}",
                                cf."{nameof(CadastreFeatureEntity.VillageCouncilNameBel)}",
                                a."{nameof(AdministrativeEntity.ParentRegionId)}",
                                a."{nameof(AdministrativeEntity.Id)}" as "{nameof(AdministrativeEntity.ParentDistrictId)}"
                from "{CadastreFeatureConfiguration.TableName}" cf
                join "{AdministrativeConfiguration.TableName}" a on cf."{nameof(CadastreFeatureEntity.DistrictName)}" = a."{nameof(AdministrativeEntity.NameRu)}" and a."{nameof(AdministrativeEntity.Type)}" = @district
                join "{AdministrativeConfiguration.TableName}" ar on cf."{nameof(CadastreFeatureEntity.RegionName)}" = ar."{nameof(AdministrativeEntity.NameRu)}" and ar."{nameof(AdministrativeEntity.Type)}" = @region and a."{nameof(AdministrativeEntity.ParentRegionId)}" = ar."{nameof(AdministrativeEntity.Id)}"
                where "{nameof(CadastreFeatureEntity.RegionName)}" is not null
                  and "{nameof(CadastreFeatureEntity.DistrictName)}" is not null
                  and "{nameof(CadastreFeatureEntity.VillageCouncilName)}" is not null
             )
             merge into "{AdministrativeConfiguration.TableName}" as target
                using "VillageCouncils" as source 
             on target."{nameof(AdministrativeEntity.Type)}" = @villageCouncil
                and target."{nameof(AdministrativeEntity.NameRu)}" = source."{nameof(CadastreFeatureEntity.VillageCouncilName)}"
                and target."{nameof(AdministrativeEntity.ParentDistrictId)}" = source."{nameof(AdministrativeEntity.ParentDistrictId)}"
             when not matched then
             insert
                 (
                 "{nameof(AdministrativeEntity.CreatedDateTime)}",
                 "{nameof(AdministrativeEntity.ModifiedDateTime)}",
                 
                 "{nameof(AdministrativeEntity.NameBeTarask)}",
                 "{nameof(AdministrativeEntity.NameBeNark)}",
                 "{nameof(AdministrativeEntity.NameRu)}",
                 "{nameof(AdministrativeEntity.ParentRegionId)}",
                 "{nameof(AdministrativeEntity.ParentDistrictId)}",
                 "{nameof(AdministrativeEntity.Type)}"
                 )
             values
                 (
                 now(), 
                 now(),
                 source."{nameof(CadastreFeatureEntity.VillageCouncilNameBel)}",
                 source."{nameof(CadastreFeatureEntity.VillageCouncilNameBel)}",
                 source."{nameof(CadastreFeatureEntity.VillageCouncilName)}",
                 source."{nameof(AdministrativeEntity.ParentRegionId)}",
                 source."{nameof(AdministrativeEntity.ParentDistrictId)}",
                 @villageCouncil
                 )
             ;
             
             -- тэрмінальныя населеныя пункты
             with "Terminal" as (
                select distinct cf."{nameof(CadastreFeatureEntity.AteName)}",
                                cf."{nameof(CadastreFeatureEntity.AteNameBel)}",
                                ar."{nameof(AdministrativeEntity.Id)}" as "{nameof(AdministrativeEntity.ParentRegionId)}",
                                ad."{nameof(AdministrativeEntity.Id)}" as "{nameof(AdministrativeEntity.ParentDistrictId)}",
                                avc."{nameof(AdministrativeEntity.Id)}" as "{nameof(AdministrativeEntity.ParentVillageCouncilId)}",
                                cf."{nameof(CadastreFeatureEntity.CategoryName)}",
                                cf."{nameof(CadastreFeatureEntity.Ate)}"
                from "{CadastreFeatureConfiguration.TableName}" cf
                join "{AdministrativeConfiguration.TableName}" ar on cf."{nameof(CadastreFeatureEntity.RegionName)}" = ar."{nameof(AdministrativeEntity.NameRu)}" and ar."{nameof(AdministrativeEntity.Type)}" = @region
                left join "{AdministrativeConfiguration.TableName}" ad on cf."{nameof(CadastreFeatureEntity.DistrictName)}" = ad."{nameof(AdministrativeEntity.NameRu)}" and ad."{nameof(AdministrativeEntity.Type)}" = @district and ad."{nameof(AdministrativeEntity.ParentRegionId)}" = ar."{nameof(AdministrativeEntity.Id)}"
                left join "{AdministrativeConfiguration.TableName}" avc on cf."{nameof(CadastreFeatureEntity.VillageCouncilName)}" = avc."{nameof(AdministrativeEntity.NameRu)}" and avc."{nameof(AdministrativeEntity.Type)}" = @villageCouncil and avc."{nameof(AdministrativeEntity.ParentDistrictId)}" = ad."{nameof(AdministrativeEntity.Id)}"
             )
             merge into "{AdministrativeConfiguration.TableName}" as target
                using "Terminal" as source 
             on target."{nameof(AdministrativeEntity.NameRu)}" = source."{nameof(CadastreFeatureEntity.AteName)}"
                and (target."{nameof(AdministrativeEntity.ParentRegionId)}" is null and source."{nameof(AdministrativeEntity.ParentRegionId)}" is null or target."{nameof(AdministrativeEntity.ParentRegionId)}" = source."{nameof(AdministrativeEntity.ParentRegionId)}")
                and (target."{nameof(AdministrativeEntity.ParentDistrictId)}" is null and source."{nameof(AdministrativeEntity.ParentDistrictId)}" is null or target."{nameof(AdministrativeEntity.ParentDistrictId)}" = source."{nameof(AdministrativeEntity.ParentDistrictId)}")
                and (target."{nameof(AdministrativeEntity.ParentVillageCouncilId)}" is null and source."{nameof(AdministrativeEntity.ParentVillageCouncilId)}" is null or target."{nameof(AdministrativeEntity.ParentVillageCouncilId)}" = source."{nameof(AdministrativeEntity.ParentVillageCouncilId)}")
             when not matched then
             insert
                 (
                 "{nameof(AdministrativeEntity.CreatedDateTime)}",
                 "{nameof(AdministrativeEntity.ModifiedDateTime)}",
                 
                 "{nameof(AdministrativeEntity.NameBeTarask)}",
                 "{nameof(AdministrativeEntity.NameBeNark)}",
                 "{nameof(AdministrativeEntity.NameRu)}",
                 "{nameof(AdministrativeEntity.ParentRegionId)}",
                 "{nameof(AdministrativeEntity.ParentDistrictId)}",
                 "{nameof(AdministrativeEntity.ParentVillageCouncilId)}",
                 "{nameof(AdministrativeEntity.Type)}",
                 "{nameof(AdministrativeEntity.CadastreAte)}"
                 )
             values
                 (
                 now(), 
                 now(),
                 coalesce(source."{nameof(CadastreFeatureEntity.AteNameBel)}", ''),
                 coalesce(source."{nameof(CadastreFeatureEntity.AteNameBel)}", ''),
                 coalesce(source."{nameof(CadastreFeatureEntity.AteName)}", ''),
                 source."{nameof(AdministrativeEntity.ParentRegionId)}",
                 source."{nameof(AdministrativeEntity.ParentDistrictId)}",
                 source."{nameof(AdministrativeEntity.ParentVillageCouncilId)}",
                 case source."{nameof(CadastreFeatureEntity.CategoryName)}"
                     when 'Столица Республики Беларусь'                              then @capital
                     when 'Город областного подчинения (АТЕ)'                        then @regionCenterCity
                     when 'Город районного подчинения (АТЕ)'                         then @districtCenterCity
                     when 'Город районного подчинения (ТЕ)'                          then @otherCity
                     when 'Поселок городского типа - городской поселок (АТЕ)'        then @centerTown
                     when 'Поселок городского типа - городской поселок (ТЕ)'         then @otherTown
                     when 'Поселок городского типа - курортный поселок (ТЕ)'         then @resortTown
                     when 'Поселок городского типа - рабочий поселок (ТЕ)'           then @workTown
                     when 'Сельский населенный пункт - хутор'                        then @villageHomestead
                     when 'Сельский населенный пункт - поселок'                      then @villageSettlement
                     when 'Сельский населенный пункт - деревня'                      then @villageHamlet
                     when 'Сельский населенный пункт - агрогородок'                  then @villageAgroTown
                     when 'Особая экономическая зона'                                then @specialEconomicZone
                     else @unknown
                 end,
                 source."{nameof(CadastreFeatureEntity.Ate)}"
                 )
             ;
             """;

        return Context.Database.ExecuteSqlRawAsync(
            command,
            new NpgsqlParameter("capital", (int)AdministrativeType.Capital),
            new NpgsqlParameter("region", (int)AdministrativeType.Region),
            new NpgsqlParameter("district", (int)AdministrativeType.District),
            new NpgsqlParameter("villageCouncil", (int)AdministrativeType.VillageCouncil),
            new NpgsqlParameter("regionCenterCity", (int)AdministrativeType.RegionCenterCity),
            new NpgsqlParameter("districtCenterCity", (int)AdministrativeType.DistrictCenterCity),
            new NpgsqlParameter("otherCity", (int)AdministrativeType.OtherCity),
            new NpgsqlParameter("centerTown", (int)AdministrativeType.CenterTown),
            new NpgsqlParameter("otherTown", (int)AdministrativeType.OtherTown),
            new NpgsqlParameter("resortTown", (int)AdministrativeType.ResortTown),
            new NpgsqlParameter("workTown", (int)AdministrativeType.WorkTown),
            new NpgsqlParameter("villageHomestead", (int)AdministrativeType.VillageHomestead),
            new NpgsqlParameter("villageSettlement", (int)AdministrativeType.VillageSettlement),
            new NpgsqlParameter("villageHamlet", (int)AdministrativeType.VillageHamlet),
            new NpgsqlParameter("villageAgroTown", (int)AdministrativeType.VillageAgroTown),
            new NpgsqlParameter("specialEconomicZone", (int)AdministrativeType.SpecialEconomicZone),
            new NpgsqlParameter("unknown", (object)(int)AdministrativeType.Unknown)
        );
    }

    public Task SetAdministrativeOnFeatures()
    {
        const string command =
            $"""
             with "AssignedAdministrative" as (
                select f."{nameof(FeatureEntity.Id)}" as "FeatureId",
                       a."{nameof(AdministrativeEntity.Id)}" as "AdministrativeId"
                from "{FeatureConfiguration.TableName}" f
                join "{CadastreFeatureConfiguration.TableName}" cf on f."{nameof(FeatureEntity.Id)}" = cf."{nameof(CadastreFeatureEntity.FeatureId)}"
                join "{AdministrativeConfiguration.TableName}" a on cf."{nameof(CadastreFeatureEntity.Ate)}" = a."{nameof(AdministrativeEntity.CadastreAte)}"
                where "{nameof(FeatureEntity.AdministrativeId)}" is null
             )
             update "{FeatureConfiguration.TableName}" as f
             set
                 "{nameof(FeatureEntity.AdministrativeId)}" = aa."AdministrativeId"
             from "AssignedAdministrative" as aa
             where f."{nameof(FeatureEntity.Id)}" = aa."FeatureId"
             ;
             """;

        return Context.Database.ExecuteSqlRawAsync(command);
    }

    public Task<AdministrativeEntity?> GetByCadastreAte(int cadastreAte)
    {
        return Entities.FirstOrDefaultAsync(x => x.CadastreAte == cadastreAte);
    }

    public Task<List<AdministrativeEntity>> GetBatchTracking(int greaterThanId, int take)
    {
        return Entities
            .AsTracking()
            .Where(x => x.Id > greaterThanId)
            .OrderBy(x => x.Id)
            .Take(take)
            .ToListAsync();
    }

    public Task<AdministrativeEntity?> FindIntersecting(Geometry geometry)
    {
        return Entities
            .FirstOrDefaultAsync(x => x.Boundary != null && x.Boundary.Intersects(geometry));
    }
}