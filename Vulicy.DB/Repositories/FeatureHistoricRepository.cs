using Vulicy.Domain;

namespace Vulicy.DB.Repositories;

public class FeatureHistoricRepository(VulicyDbContext dbContext)
    : HistoricRepositoryBase<FeatureHistoricEntity, int>(dbContext)
        , IFeatureHistoricRepository
{
}