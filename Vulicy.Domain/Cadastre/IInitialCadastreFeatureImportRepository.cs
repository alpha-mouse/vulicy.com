namespace Vulicy.Domain;

public interface IInitialCadastreFeatureImportRepository : IRepository<InitialCadastreFeatureImportEntity, string>
{
    Task<bool> HasAny();
    Task<List<(string? reason, string? shortInfo, string? nameCategory, ClassificationGrade classificationGrade)>> GetReasonShortInfoPairs();
    Task<List<(string id, string? reason, string? nameCategory)>> GetReasonsAndNameCategories();
}