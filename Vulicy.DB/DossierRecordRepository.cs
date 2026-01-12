using Microsoft.EntityFrameworkCore;
using Vulicy.Domain;

namespace Vulicy.DB;

public class DossierRecordRepository(VulicyDbContext dbContext)
    : RepositoryBase<DossierRecordEntity, int>(dbContext)
        , IDossierRecordRepository
{
    public Task<bool> HasAny()
    {
        return Entities.AnyAsync();
    }

    public Task<List<DossierRecordEntity>> GetRangeTracked(int skip, int take)
    {
        return Entities
            .AsTracking()
            .OrderBy(e => e.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
}