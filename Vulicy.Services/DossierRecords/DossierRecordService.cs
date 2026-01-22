using Vulicy.Domain;

namespace Vulicy.Services;

public interface IDossierRecordService
{
    Task<List<DossierRecordSearchResult>> SearchByName(string? query, int? skip = null, int? take = null);
}

public class DossierRecordService(IDossierRecordRepository dossierRecordRepository) : IDossierRecordService
{
    public Task<List<DossierRecordSearchResult>> SearchByName(string? query, int? skip = null, int? take = null)
    {
        if (skip < 0 || take < 0 || 1000 < take) throw new InvalidOperationException("Bad paging parameters");
        return dossierRecordRepository.SearchByName(query, skip ?? 0, take ?? 100);
    }
}