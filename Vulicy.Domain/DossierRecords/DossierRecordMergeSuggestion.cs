namespace Vulicy.Domain;

public record DossierRecordMergeSuggestion(int Id, DossierRecordSearchResult LeftRecord, DossierRecordSearchResult RightRecord)
{
}