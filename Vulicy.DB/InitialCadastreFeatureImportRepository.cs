using Microsoft.EntityFrameworkCore;
using Vulicy.Domain;

namespace Vulicy.DB;

public class InitialCadastreFeatureImportRepository(VulicyDbContext dbContext)
    : RepositoryBase<InitialCadastreFeatureImportEntity, string>(dbContext)
        , IInitialCadastreFeatureImportRepository
{
    public Task<bool> HasAny()
    {
        return Entities.AnyAsync();
    }

    public async Task<List<(string? reason, string? shortInfo, string? nameCategory, ClassificationGrade classificationGrade)>> GetDossierCandidateData()
    {
        return await (
                from cadastreFeature in Context.Set<CadastreFeatureEntity>()
                join initialImport in Context.Set<InitialCadastreFeatureImportEntity>() on cadastreFeature.Id equals initialImport.Id into initialImportJoin
                from initialImport in initialImportJoin.DefaultIfEmpty()
                where cadastreFeature.ShortInfo != null || initialImport != null && initialImport.Reason != null
                select new
                {
                    initialImport.Reason,
                    cadastreFeature.ShortInfo,
                    initialImport.NameCategory,
                    initialImport.Classification,
                })
            .Distinct()
            .AsAsyncEnumerable()
            .Select(x => (x.Reason, x.ShortInfo, x.NameCategory, x.Classification == null ? ClassificationGrade.None : (ClassificationGrade)x.Classification.Value))
            .ToListAsync();
    }

    public async Task<List<(string id, string? reason, string? nameCategory)>> GetReasonsAndNameCategories()
    {
        return await Entities
            .Select(x => new
            {
                x.Id,
                x.Reason,
                x.NameCategory,
            })
            .AsAsyncEnumerable()
            .Select(x => (x.Id, x.Reason, x.NameCategory))
            .ToListAsync();
    }
}