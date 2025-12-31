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
}