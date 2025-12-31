namespace Vulicy.Domain;

public interface IFeatureRepository : IRepository<FeatureEntity, int>
{
    Task MirrorIsDeletedFromCadastre(DateTime now);
    Task<List<FeatureEntity>> GetByAteWithCadastreTracking(int ate);
    Task<List<FeatureEntity>> GetByAteWithImportsTracking(int ate);
    Task<List<FeatureEntity>> GetNextForGeometryUpdateTracking(int batchSize);
    Task MirrorFromInitialCadastre();
    Task AssignClassificationsFromInitialCadastre();
}