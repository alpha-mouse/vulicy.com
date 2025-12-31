namespace Vulicy.Domain;

public class CadastreFeatureEntity
{
    public int? FeatureId { get; set; }
    public FeatureEntity? Feature { get; set; }

    public string CadastreId { get; set; }
    public string? Geometry { get; set; }
    public string? BoundingBox { get; set; }

    public int IdIae { get; set; }
    public string? ParentAte { get; set; }
    public int? Region { get; set; }
    public int? District { get; set; }
    public int? VillageCouncil { get; set; }
    public int Ate { get; set; }
    public string? RegionName { get; set; }
    public string? DistrictName { get; set; }
    public string? VillageCouncilName { get; set; }
    public string AteName { get; set; }
    public string? RegionNameBel { get; set; }
    public string? DistrictNameBel { get; set; }
    public string? VillageCouncilNameBel { get; set; }
    public string? AteNameBel { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryNameShort { get; set; }
    public string? CategoryNameBel { get; set; }
    public string? CategoryNameShortBel { get; set; }
    public int ElementType { get; set; }
    public string ElementTypeName { get; set; }
    public string ElementTypeNameBel { get; set; }
    public string? ElementTypeShortName { get; set; }
    public string? ElementTypeShortNameBel { get; set; }
    public string ElementName { get; set; }
    public string? ElementNameBel { get; set; }
    public string? ShortInfo { get; set; }
    public int? ObjectNumber { get; set; }

    public bool IsDeleted { get; set; }
}

public class CadastreFeatureImportEntity : CadastreFeatureEntity
{
    public Guid ImportId { get; set; }
}

public class CadastreFeatureHistoricEntity : CadastreFeatureEntity
{
    public DateTime ChangeDateTime { get; set; }
}

public class InitialCadastreFeatureImportEntity : CadastreFeatureImportEntity
{
    public int? Classification { get; set; }
    public string? Reason { get; set; }
    public string? HistoricName { get; set; }
    public string? NameCategory { get; set; }
    public string? Comment { get; set; }
    public bool HistoricPossible { get; set; }
    public string? YearNamed { get; set; }
}