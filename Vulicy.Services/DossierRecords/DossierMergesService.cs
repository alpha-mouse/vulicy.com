using Vulicy.Domain;

namespace Vulicy.Services;

public interface IDossierMergesService
{
    Task<DossierRecordMergeSuggestion?> GetNextMergeSuggestion();
    Task IgnoreMergeSuggestion(int id);
}

public class DossierMergesService(IDossierRecordMergeSuggestionRepository dossierRecordMergeSuggestionRepository) : IDossierMergesService
{
    public Task<DossierRecordMergeSuggestion?> GetNextMergeSuggestion()
    {
        return dossierRecordMergeSuggestionRepository.GetNext();
    }

    public async Task IgnoreMergeSuggestion(int id)
    {
        var suggestion = await dossierRecordMergeSuggestionRepository.GetByIdTracked(id);
        if (suggestion == null)
            throw new InvalidOperationException("Suggestion not found");

        dossierRecordMergeSuggestionRepository.Delete(suggestion);
        await dossierRecordMergeSuggestionRepository.SaveChanges();
    }
}