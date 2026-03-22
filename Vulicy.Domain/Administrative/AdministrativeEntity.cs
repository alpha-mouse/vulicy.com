namespace Vulicy.Domain;

public class AdministrativeEntity : Entity<int>
{
    public string NameBeTarask { get; set; } = null!;
    public string NameBeNark { get; set; } = null!;
    public string? NameRu { get; set; }
    public AdministrativeType Type { get; set; }
    public int? ParentRegionId { get; set; }
    public int? ParentDistrictId { get; set; }
    public int? ParentVillageCouncilId { get; set; }
    public int? CadastreAte { get; set; }

    public AdministrativeEntity? ParentRegion { get; set; }
    public AdministrativeEntity? ParentDistrict { get; set; }
    public AdministrativeEntity? ParentVillageCouncil { get; set; }
}