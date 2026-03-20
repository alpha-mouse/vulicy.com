using System.Globalization;
using Vulicy.Domain;

namespace Vulicy.Services;

public record Administrative(int Id, string NameBeTarask, AdministrativeType Type, ICollection<Administrative>? ChildAdministratives);

public interface IAdministrativeService
{
    Task<List<Administrative>> GetAdministratives();
}

public class AdministrativeService(IAdministrativeRepository administrativeRepository) : IAdministrativeService
{
    public async Task<List<Administrative>> GetAdministratives()
    {
        var allAdministratives = await administrativeRepository.GetAll();
        var beByStringComparer = CultureProvider.BeByCultureInfo.CompareInfo.GetStringComparer(CompareOptions.None);
        var districtSubordinate = allAdministratives
            .Where(x => x.ParentDistrictId != null)
            .OrderBy(x => x.NameBeTarask, beByStringComparer)
            .ToLookup(x => x.ParentDistrictId, MapToAdministrative);
        var regionSubordinate = allAdministratives
            .Where(x => x is { ParentRegionId: not null, ParentDistrictId: null })
            .OrderBy(x => x.NameBeTarask, beByStringComparer)
            .ToLookup(x => x.ParentRegionId, x => MapToAdministrative(x, districtSubordinate[x.Id]));
        return allAdministratives
            .Where(x => x.ParentRegionId == null)
            .OrderBy(x => x.NameBeTarask, beByStringComparer)
            .Select(x => MapToAdministrative(x, regionSubordinate[x.Id]))
            .ToList();
    }

    private Administrative MapToAdministrative(AdministrativeEntity administrativeEntity)
        => new(administrativeEntity.Id, administrativeEntity.NameBeTarask, administrativeEntity.Type, null);

    private Administrative MapToAdministrative(AdministrativeEntity administrativeEntity, IEnumerable<Administrative> children)
        => new(administrativeEntity.Id, administrativeEntity.NameBeTarask, administrativeEntity.Type, children.ToList());
}