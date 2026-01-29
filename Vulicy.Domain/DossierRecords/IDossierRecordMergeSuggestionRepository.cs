namespace Vulicy.Domain;

public interface IDossierRecordMergeSuggestionRepository : IRepository<DossierRecordMergeSuggestionEntity, int>
{
    Task<DossierRecordMergeSuggestion?> GetNext();
    void Delete(DossierRecordMergeSuggestionEntity suggestion);
}