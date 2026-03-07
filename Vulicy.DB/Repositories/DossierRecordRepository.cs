using Microsoft.EntityFrameworkCore;
using Npgsql;
using Vulicy.DB.Configurations;
using Vulicy.Domain;

namespace Vulicy.DB.Repositories;

public class DossierRecordRepository(VulicyDbContext dbContext)
    : RepositoryBase<DossierRecordEntity, int>(dbContext)
        , IDossierRecordRepository
{
    private const string SearchColumnsList =
        $"""
         dr."{nameof(DossierRecordEntity.Id)}",
         dr."{nameof(DossierRecordEntity.NameBeTarask)}",
         dr."{nameof(DossierRecordEntity.NameBeNark)}",
         dr."{nameof(DossierRecordEntity.NameRu)}",
         dr."{nameof(DossierRecordEntity.DescriptionBe)}",
         dr."{nameof(DossierRecordEntity.DescriptionRu)}",
         dr."{nameof(DossierRecordEntity.Classification)}",
         dr."{nameof(DossierRecordEntity.NamingCategoryId)}",
         dr."{nameof(DossierRecordEntity.ForumRelativeLink)}"
         """;

    private const string SearchBaseQuery =
        $"""
         select
             {SearchColumnsList},
             count(f."{nameof(FeatureEntity.Id)}") as "{nameof(DossierRecordSearchResult.NumFeatures)}"
         from "{DossierRecordConfiguration.TableName}" dr
         left outer join "{FeatureConfiguration.TableName}" f on dr."{nameof(DossierRecordEntity.Id)}" = f."{nameof(FeatureEntity.DossierRecordId)}"

         """;

    public Task<bool> HasAny()
    {
        return Entities.AnyAsync();
    }

    public Task<List<DossierRecordEntity>> GetRangeTracked(int skip, int take)
    {
        return Entities
            .AsTracking()
            .OrderBy(e => e.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public Task<List<DossierRecordSearchResult>> SearchByName(string? query, int skip, int take)
    {
        var cleanedQuery = DatabaseHelpers.CleanQuery(query);

        const string orderAndLimit =
            $"""
            group by {SearchColumnsList}
            order by dr."{nameof(DossierRecordEntity.NameBeTarask)}"
            limit @take offset @skip
            """;

        const string queryWithSearch = SearchBaseQuery +
            $"""
            where
                dr."{nameof(DossierRecordEntity.NameBeTarask)}" ilike @query
                or dr."{nameof(DossierRecordEntity.NameBeNark)}" ilike @query
                or dr."{nameof(DossierRecordEntity.NameRu)}" ilike @query

            """
            + orderAndLimit;
        const string queryWithoutSearch = SearchBaseQuery + orderAndLimit;

        IQueryable<DossierRecordSearchResult> result;
        if (string.IsNullOrEmpty(cleanedQuery))
        {
            result = Context.Database
                .SqlQueryRaw<DossierRecordSearchResult>(queryWithoutSearch,
                    new NpgsqlParameter("skip", skip),
                    new NpgsqlParameter("take", take)
                );
        }
        else
        {
            result = Context.Database
                .SqlQueryRaw<DossierRecordSearchResult>(queryWithSearch,
                    new NpgsqlParameter("query", $"%{cleanedQuery}%"),
                    new NpgsqlParameter("skip", skip),
                    new NpgsqlParameter("take", take)
                );
        }

        return result.ToListAsync();
    }

    public Task<DossierRecordSearchResult?> SearchById(int id)
    {
        const string query = SearchBaseQuery +
            $"""
             where dr."{nameof(DossierRecordEntity.Id)}" = @id
             group by {SearchColumnsList}
             """;

        return Context.Database
            .SqlQueryRaw<DossierRecordSearchResult>(query,
                new NpgsqlParameter("id", id)
            )
            .FirstOrDefaultAsync();
    }

    public Task<bool> HasFeatures(int id)
    {
        return Context.Set<FeatureEntity>().AnyAsync(f => f.DossierRecordId == id);
    }

    public Task RelinkFeatures(int fromDossierRecordId, int toDossierRecordId)
    {
        const string command = $"""
                                update "{FeatureConfiguration.TableName}"
                                set "{nameof(FeatureEntity.DossierRecordId)}" = @toDossierRecordId
                                where "{nameof(FeatureEntity.DossierRecordId)}" = @fromDossierRecordId
                                """;

        return Context.Database.ExecuteSqlRawAsync(
            command,
            new NpgsqlParameter("fromDossierRecordId", fromDossierRecordId),
            new NpgsqlParameter("toDossierRecordId", toDossierRecordId)
            );
    }

    public void Delete(DossierRecordEntity entity)
    {
        Context.Remove(entity);
    }

    public Task<List<DossierRecordEntity>> FindByDescriptions(string descriptionBe, string descriptionRu)
    {
        if (string.IsNullOrEmpty(descriptionBe) && string.IsNullOrEmpty(descriptionRu))
            return Task.FromResult(new List<DossierRecordEntity>(0));

        var query = Entities;
        if (!string.IsNullOrEmpty(descriptionBe))
            query = query.Where(x => x.DescriptionBe == descriptionBe || x.AlternativeDescriptionsBe != null && x.AlternativeDescriptionsBe.Contains(descriptionBe));
        if (!string.IsNullOrEmpty(descriptionRu))
            query = query.Where(x => x.DescriptionRu == descriptionRu || x.AlternativeDescriptionsRu != null && x.AlternativeDescriptionsRu.Contains(descriptionRu));
        return query.ToListAsync();
    }

    public async Task UpdateForumLink(int dossierRecordId, string forumRelativeLink, int userId)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        var dossierRecord = await Entities.AsTracking().FirstOrDefaultAsync(x => x.Id == dossierRecordId);
        if (dossierRecord != null)
        {
            var history = DossierRecordHistoricEntity.FromBase(dossierRecord);
            history.ChangeDateTime = DateTime.UtcNow;
            history.InHistoryById = userId;
            Context.Add(history);

            dossierRecord.ForumRelativeLink = forumRelativeLink;
            dossierRecord.LastModifiedById = userId;
            await Context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }

    public async Task SetForumLinkIfEmpty(int dossierRecordId, string forumRelativeLink, int userId)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        var dossierRecord = await Entities.AsTracking().FirstOrDefaultAsync(x => x.Id == dossierRecordId);
        if (dossierRecord is { ForumRelativeLink: null })
        {
            var history = DossierRecordHistoricEntity.FromBase(dossierRecord);
            history.ChangeDateTime = DateTime.UtcNow;
            history.InHistoryById = userId;
            Context.Add(history);

            dossierRecord.ForumRelativeLink = forumRelativeLink;
            dossierRecord.LastModifiedById = userId;
            await Context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }
}