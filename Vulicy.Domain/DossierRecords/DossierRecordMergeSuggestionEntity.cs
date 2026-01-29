namespace Vulicy.Domain;

public class DossierRecordMergeSuggestionEntity : Entity<int>
{
    public int LeftRecordId { get; set; }
    public int RightRecordId { get; set; }

    public DossierRecordEntity LeftRecord { get; set; } = null!;
    public DossierRecordEntity RightRecord { get; set; } = null!;
}