namespace Vulicy.Domain;

public interface IDossierRecordRepository : IRepository<DossierRecordEntity, int>
{
    Task<bool> HasAny();
    Task<List<DossierRecordEntity>> GetRangeTracked(int skip, int take);
    Task<List<DossierRecordSearchResult>> SearchByName(string? query, int skip, int take);
    Task<bool> HasFeatures(int id);
    Task RelinkFeatures(int fromDossierRecordId, int toDossierRecordId);
    void Delete(DossierRecordEntity entity);
    Task<List<DossierRecordEntity>> FindByDescriptions(string descriptionBe, string descriptionRu);
}