using Microsoft.EntityFrameworkCore;
using Vulicy.Domain;

namespace Vulicy.DB;

public class CadastreFeatureRepository(VulicyDbContext context)
    : RepositoryBase<CadastreFeatureEntity, string>(context)
        , ICadastreFeatureRepository
{
    public Task<List<int>> GetUnmatchedAtes()
    {
        return Entities
            .Where(x => x.FeatureId == null && !x.IsDeleted)
            .Select(x => x.Ate)
            .Distinct()
            .ToListAsync();
    }

    public Task<List<CadastreFeatureEntity>> GetUnmatchedByAteTracking(int ate)
    {
        return Entities
            .AsTracking()
            .Where(x => x.FeatureId == null && !x.IsDeleted && x.Ate == ate)
            .ToListAsync();
    }

    public Task<List<int>> GetAllAtes()
    {
        return Entities
            .Where(x => x.Feature != null)
            .Select(x => x.Ate)
            .Distinct()
            .ToListAsync();
    }
}