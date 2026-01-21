using Vulicy.Domain;

namespace Vulicy.Services;

public interface IFeatureService
{
    Task<List<FeatureSearchResult>> SearchByName(string query, double? lat = null, double? lng = null);
    Task<List<FeatureSearchResult>> GetByDossierRecord(int dossierRecordId);
}

public class FeatureService(IFeatureRepository featureRepository) : IFeatureService
{
    public Task<List<FeatureSearchResult>> SearchByName(string query, double? lat = null, double? lng = null)
    {
        return featureRepository.SearchByName(query, lat, lng);
    }

    public Task<List<FeatureSearchResult>> GetByDossierRecord(int dossierRecordId)
    {
        return featureRepository.GetByDossierRecord(dossierRecordId);
    }
}