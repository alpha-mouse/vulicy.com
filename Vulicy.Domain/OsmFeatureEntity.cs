namespace Vulicy.Domain;

public class OsmFeatureEntity
{
    public int? FeatureId { get; set; }
    public FeatureEntity? Feature { get; set; }

    public long OsmId { get; set; }
    public OsmType Type { get; set; }

    // todo geometry
    // todo properties

    public bool IsDeleted { get; set; }
}

public class OsmFeatureImportEntity : OsmFeatureEntity
{
    public Guid ImportId { get; set; }
}

public class OsmFeatureHistoricEntity : OsmFeatureEntity
{
    public DateTime ChangeDateTime { get; set; }
}

public enum OsmType
{
    Node = 1,
    Way = 2,
    Relation = 3,
}