using Vulicy.Domain;

namespace Vulicy.DB;

public class FeatureHistoricRepository(VulicyDbContext dbContext)
    : HistoricRepositoryBase<FeatureHistoricEntity, int>(dbContext)
        , IFeatureHistoricRepository
{
}