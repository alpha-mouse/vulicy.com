namespace Vulicy.Domain;

public interface ICadastreFeatureImportRepository : IRepository<CadastreFeatureImportEntity, string>
{
    Task MarkImport(int importId);
    Task StoreImportHistorySnapshot(int importId, DateTime now);
    Task UpdateFromImport(int importId, DateTime now);
    Task Truncate();
}