using System.Collections;

namespace Vulicy.Domain;

public interface IDossierRecordRepository : IRepository<DossierRecordEntity, int>
{
    Task<bool> HasAny();
    Task<List<DossierRecordEntity>> GetRangeTracked(int skip, int take);
    Task<List<DossierRecordSearchResult>> SearchByName(string? query, int skip, int take);
}