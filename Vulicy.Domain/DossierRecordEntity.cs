namespace Vulicy.Domain;

public class DossierRecordEntity : Entity
{
    public string NameBeTarask { get; set; }
    public string NameBeNark { get; set; }
    public string? NameRu { get; set; }
    public string? Description { get; set; }

    public int? NamingCategoryId { get; set; }
    public NamingCategoryEntity? NamingCategory { get; set; }
}