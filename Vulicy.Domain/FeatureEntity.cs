namespace Vulicy.Domain;

public class FeatureEntity : Entity
{
    public string NameBeTarask { get; set; }
    public string NameBeNark { get; set; }
    public string? NameRu { get; set; }
    public ClassificationGrade Classification { get; set; }
    public FeatureType Type { get; set; }
    public string? RenamingReason { get; set; }
    public string? HistoricNames { get; set; }
    public string? Comment { get; set; }
    public bool HistoricPossible { get; set; }
    public string? YearNamed { get; set; }

    public string? ForumRelativeLink { get; set; }

    // todo geometry
    // todo bounding box?
    // todo something else?

    public int? NamingCategoryId { get; set; }
    public NamingCategoryEntity? NamingCategory { get; set; }

    public int? DossierRecordId { get; set; }
    public DossierRecordEntity? DossierRecord { get; set; }
}