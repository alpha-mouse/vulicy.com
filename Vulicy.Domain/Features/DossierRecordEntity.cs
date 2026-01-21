namespace Vulicy.Domain;

public abstract class DossierRecordBaseEntity : Entity<int>
{
    public string? NameBeTarask { get; set; }
    public string? NameBeNark { get; set; }
    public string? NameRu { get; set; }
    public string? DescriptionBe { get; set; }
    public string? DescriptionRu { get; set; }
    public List<string>? PossibleNamesBeNark { get; set; }
    public List<string>? PossibleNamesRu { get; set; }
    public ClassificationGrade Classification { get; set; }

    public int LastModifiedById { get; set; }
    public int? NamingCategoryId { get; set; }
}

public class DossierRecordEntity : DossierRecordBaseEntity
{
    public NamingCategoryEntity? NamingCategory { get; set; }
    public UserEntity LastModifiedBy { get; set; }
}

public class DossierRecordHistoricEntity : DossierRecordBaseEntity, IHistoricEntity<int>
{
    public DateTime ChangeDateTime { get; set; }
    public int InHistoryById { get; set; }

    public static DossierRecordHistoricEntity FromBase(DossierRecordEntity entity) =>
        new()
        {
            Id = entity.Id,
            CreatedDateTime = entity.CreatedDateTime,
            ModifiedDateTime = entity.ModifiedDateTime,
            NameBeTarask = entity.NameBeTarask,
            NameBeNark = entity.NameBeNark,
            NameRu = entity.NameRu,
            DescriptionBe = entity.DescriptionBe,
            DescriptionRu = entity.DescriptionRu,
            PossibleNamesBeNark = entity.PossibleNamesBeNark,
            PossibleNamesRu = entity.PossibleNamesRu,
            Classification = entity.Classification,
            LastModifiedById = entity.LastModifiedById,
            NamingCategoryId = entity.NamingCategoryId,
        };
}