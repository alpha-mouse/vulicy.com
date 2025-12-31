namespace Vulicy.Domain;

public interface IDossierRecordRepository : IRepository<DossierRecordEntity, int>
{
    Task<bool> HasAny();
}