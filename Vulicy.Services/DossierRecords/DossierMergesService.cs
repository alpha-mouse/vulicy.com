using Vulicy.Domain;

namespace Vulicy.Services;

public interface IDossierMergesService
{
    Task<DossierRecordMergeSuggestion?> GetNextMergeSuggestion();
    Task PostponeMergeSuggestion(int id);
    Task IgnoreMergeSuggestion(int id);
}

public class DossierMergesService(IDossierRecordMergeSuggestionRepository dossierRecordMergeSuggestionRepository) : IDossierMergesService
{
    public Task<DossierRecordMergeSuggestion?> GetNextMergeSuggestion()
    {
        return dossierRecordMergeSuggestionRepository.GetNext();
    }

    public async Task PostponeMergeSuggestion(int id)
    {
        var suggestion = await dossierRecordMergeSuggestionRepository.GetByIdTracked(id);
        if (suggestion == null)
            throw new InvalidOperationException("Suggestion not found");

        dossierRecordMergeSuggestionRepository.Delete(suggestion);
        var now = DateTime.UtcNow;
        dossierRecordMergeSuggestionRepository.Add(new ()
        {
            LeftRecordId = suggestion.LeftRecordId,
            RightRecordId = suggestion.RightRecordId,
            CreatedDateTime = now,
            ModifiedDateTime = now,
        });
        await dossierRecordMergeSuggestionRepository.SaveChanges();
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