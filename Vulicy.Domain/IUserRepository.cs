namespace Vulicy.Domain;

public interface IUserRepository : IRepository<UserEntity, int>
{
    Task<UserEntity?> GetByExternalIdTracking(int externalId);
}