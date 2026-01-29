using Vulicy.Domain;

namespace Vulicy.DB;

public class DossierRecordHistoricRepository(VulicyDbContext dbContext)
    : HistoricRepositoryBase<DossierRecordHistoricEntity, int>(dbContext)
        , IDossierRecordHistoricRepository
{
}
