namespace Vulicy.Domain;

public record DossierRecordSearchResult(int Id, string? NameBeTarask, string? NameBeNark, string? NameRu, string? DescriptionBe, string? DescriptionRu, ClassificationGrade Classification, int? NamingCategoryId, int NumFeatures, string? ForumRelativeLink);