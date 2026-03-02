using Vulicy.Domain;

namespace Vulicy.DB.Repositories;

public class DossierRecordHistoricRepository(VulicyDbContext dbContext)
    : HistoricRepositoryBase<DossierRecordHistoricEntity, int>(dbContext)
        , IDossierRecordHistoricRepository
{
}
