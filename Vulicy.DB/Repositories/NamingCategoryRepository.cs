using Microsoft.EntityFrameworkCore;
using Npgsql;
using Vulicy.DB.Configurations;
using Vulicy.Domain;

namespace Vulicy.DB.Repositories;

public class NamingCategoryRepository(VulicyDbContext dbContext)
    : RepositoryBase<NamingCategoryEntity, int>(dbContext)
        , INamingCategoryRepository
{
    public Task<bool> HasAny()
    {
        return Entities.AnyAsync();
    }

    public Task MergeFromCadastreInitial(DateTime now)
    {
        const string query =
            $"""
            with "PossibleNamingCategories" as (
                select distinct "{nameof(InitialCadastreFeatureImportEntity.NameCategory)}"
                from "{InitialCadastreFeatureImportConfiguration.TableName}"
                where "{nameof(InitialCadastreFeatureImportEntity.NameCategory)}" is not null
            )
            merge into "{NamingCategoryConfiguration.TableName}" as target
                using "PossibleNamingCategories" as source
            on target."{nameof(NamingCategoryEntity.Name)}" = source."{nameof(InitialCadastreFeatureImportEntity.NameCategory)}"
            when not matched then
            insert
                (
                "{nameof(NamingCategoryEntity.Name)}",
                "{nameof(NamingCategoryEntity.CreatedDateTime)}",
                "{nameof(NamingCategoryEntity.ModifiedDateTime)}"
                )
            values
                (
                source."{nameof(InitialCadastreFeatureImportEntity.NameCategory)}",
                @now,
                @now
                )
            ;
            """;
        return Context.Database.ExecuteSqlRawAsync(
            query,
            new NpgsqlParameter("now", now)
        );
    }

    public Task<int?> GetIdByName(string name)
    {
        return Entities
            .Where(x => x.Name == name)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();
    }
}