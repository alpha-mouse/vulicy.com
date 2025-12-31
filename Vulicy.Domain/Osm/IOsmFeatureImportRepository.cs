namespace Vulicy.Domain;

public interface IOsmFeatureImportRepository : IRepository<OsmFeatureImportEntity, long>
{
    Task MarkImport(int importId);
    Task StoreImportHistorySnapshot(int importId, DateTime now);
    Task UpdateFromImport(int importId, DateTime now);
    Task Truncate();
}