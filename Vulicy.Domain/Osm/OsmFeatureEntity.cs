using NetTopologySuite.Geometries;

namespace Vulicy.Domain;

public class OsmFeatureBaseEntity : Entity<long>
{
    public int? FeatureId { get; set; }
    public FeatureEntity? Feature { get; set; }

    public OsmType Type { get; set; }

    public Geometry Geometry { get; set; } = null!;
    public Dictionary<string, string> Tags { get; set; } = null!;

    public bool IsDeleted { get; set; }
}

public class OsmFeatureEntity : OsmFeatureBaseEntity
{
    public bool GeometryUpdatePending { get; set; }
}

public class OsmFeatureImportEntity : OsmFeatureBaseEntity
{
    public int ImportId { get; set; }
    public bool DoUpdate { get; set; }
}

public class OsmFeatureHistoricEntity : OsmFeatureBaseEntity, IHistoricEntity<long>
{
    public DateTime ChangeDateTime { get; set; }
}

public enum OsmType
{
    Node = 0,
    Way = 1,
    Relation = 2,
}