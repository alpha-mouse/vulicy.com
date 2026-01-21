using NetTopologySuite.Geometries;

namespace Vulicy.Domain;

public class FeatureBaseEntity : Entity<int>
{
    public string NameBeTarask { get; set; } = null!;
    public string NameBeNark { get; set; } = null!;
    public string? NameRu { get; set; }
    public ClassificationGrade Classification { get; set; }
    public FeatureType Type { get; set; }
    public string? RenamingReason { get; set; }
    public string? HistoricNames { get; set; }
    public string? Comment { get; set; }
    public bool HistoricPossible { get; set; }
    public string? YearNamed { get; set; }
    public bool IsDeleted { get; set; }
    public string? ForumRelativeLink { get; set; }

    public Geometry Geometry { get; set; } = null!;

    public int LastModifiedById { get; set; }

    public int? NamingCategoryId { get; set; }

    public int? DossierRecordId { get; set; }
}

public class FeatureEntity : FeatureBaseEntity
{
    public CadastreFeatureEntity? CadastreFeature { get; set; }
    public List<OsmFeatureEntity> OsmFeatures { get; set; } = [];
    public UserEntity LastModifiedBy { get; set; }
    public NamingCategoryEntity? NamingCategory { get; set; }
    public DossierRecordEntity? DossierRecord { get; set; }
}

public class FeatureHistoricEntity : FeatureBaseEntity, IHistoricEntity<int>
{
    public DateTime ChangeDateTime { get; set; }
    public int InHistoryById { get; set; }

    public static FeatureHistoricEntity FromBase(FeatureEntity entity) =>
        new()
        {
            Id = entity.Id,
            CreatedDateTime = entity.CreatedDateTime,
            ModifiedDateTime = entity.ModifiedDateTime,
            NameBeTarask = entity.NameBeTarask,
            NameBeNark = entity.NameBeNark,
            NameRu = entity.NameRu,
            Classification = entity.Classification,
            Type = entity.Type,
            RenamingReason = entity.RenamingReason,
            HistoricNames = entity.HistoricNames,
            Comment = entity.Comment,
            HistoricPossible = entity.HistoricPossible,
            YearNamed = entity.YearNamed,
            IsDeleted = entity.IsDeleted,
            ForumRelativeLink = entity.ForumRelativeLink,
            Geometry = entity.Geometry,
            NamingCategoryId = entity.NamingCategoryId,
            DossierRecordId = entity.DossierRecordId,
        };
}