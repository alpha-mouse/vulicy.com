namespace Vulicy.Domain;

public interface IFeatureRepository : IRepository<FeatureEntity, int>
{
    Task<byte[]?> GetTile(int z, int x, int y);
    Task<List<FeatureSearchResult>> SearchByName(string query, double? lat = null, double? lng = null);
    Task<byte[]?> GetTileDetails(int z, int x, int y);
    Task<List<FeatureSearchResult>> GetByDossierRecord(int dossierRecordId);

    Task<ForumTopicData?> GetCreateForumTopicData(int id);
    Task UpdateForumLink(int featureId, string forumRelativeLink, int userId);
    Task SetForumLinkIfEmpty(int featureId, string forumRelativeLink, int userId);

    Task MirrorIsDeletedFromCadastre(DateTime now);
    Task<List<FeatureEntity>> GetByAteWithCadastreTracking(int ate);
    Task<List<FeatureEntity>> GetByAteWithImportsTracking(int ate);
    Task<List<FeatureEntity>> GetNextForGeometryUpdateTracking(int batchSize);
    Task MirrorFromInitialCadastre();
    Task AssignClassificationsFromInitialCadastre();
}