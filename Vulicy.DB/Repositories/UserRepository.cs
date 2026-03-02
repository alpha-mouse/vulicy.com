using Microsoft.EntityFrameworkCore;
using Vulicy.Domain;

namespace Vulicy.DB.Repositories;

public class UserRepository(VulicyDbContext dbContext)
    : RepositoryBase<UserEntity, int>(dbContext)
        , IUserRepository
{
    public Task<UserEntity?> GetByExternalIdTracking(int externalId)
    {
        return Entities
            .AsTracking()
            .FirstOrDefaultAsync(x => x.ExternalId == externalId);
    }
}