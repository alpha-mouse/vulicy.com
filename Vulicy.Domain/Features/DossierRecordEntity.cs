namespace Vulicy.Domain;

public class DossierRecordEntity : Entity<int>
{
    public string? NameBeTarask { get; set; }
    public string? NameBeNark { get; set; }
    public string? NameRu { get; set; }
    public string? DescriptionBe { get; set; }
    public string? DescriptionRu { get; set; }
    public List<string>? PossibleNamesBeNark { get; set; }
    public List<string>? PossibleNamesRu { get; set; }
    public ClassificationGrade Classification { get; set; }

    public int? NamingCategoryId { get; set; }
    public NamingCategoryEntity? NamingCategory { get; set; }
}