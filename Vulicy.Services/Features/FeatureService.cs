using Vulicy.Domain;

namespace Vulicy.Services;

public interface IFeatureService
{
    Task<List<FeatureSearchResult>> SearchByName(string query, double? lat = null, double? lng = null);
    Task<List<FeatureSearchResult>> GetByDossierRecord(int dossierRecordId);
    Task EditFeature(int id, FeatureEditRequest featureEditRequest, int userId);
}

public class FeatureService(IFeatureRepository featureRepository, IFeatureHistoricRepository featureHistoricRepository) : IFeatureService
{
    public Task<List<FeatureSearchResult>> SearchByName(string query, double? lat = null, double? lng = null)
    {
        return featureRepository.SearchByName(query, lat, lng);
    }

    public Task<List<FeatureSearchResult>> GetByDossierRecord(int dossierRecordId)
    {
        return featureRepository.GetByDossierRecord(dossierRecordId);
    }

    public async Task EditFeature(int id, FeatureEditRequest featureEditRequest, int userId)
    {
        await using var transaction = await featureRepository.BeginTransaction();
        var feature = await featureRepository.GetByIdTracked(id);
        if (feature != null)
        {
            var history = FeatureHistoricEntity.FromBase(feature);
            history.ChangeDateTime = DateTime.UtcNow;
            history.InHistoryById = userId;
            featureHistoricRepository.Add(history);

            feature.NameBeTarask = featureEditRequest.NameBeTarask;
            feature.NameBeNark = featureEditRequest.NameBeNark;
            feature.NameRu = featureEditRequest.NameRu;
            feature.Classification = featureEditRequest.Classification;
            feature.Type = featureEditRequest.Type;
            feature.RenamingReason = featureEditRequest.RenamingReason;
            feature.HistoricNames = featureEditRequest.HistoricNames;
            feature.Comment = featureEditRequest.Comment;
            feature.HistoricPossible = featureEditRequest.HistoricPossible;
            feature.YearNamed = featureEditRequest.YearNamed;
            feature.NamingCategoryId = featureEditRequest.NamingCategoryId;
            feature.DossierRecordId = featureEditRequest.DossierRecordId;

            feature.LastModifiedById = userId;
            await featureRepository.SaveChanges();
            await transaction.Commit();
        }
    }
}