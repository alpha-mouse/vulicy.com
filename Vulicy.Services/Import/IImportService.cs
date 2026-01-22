
namespace Vulicy.Services;

public interface IImportService
{
    Task MergeImport(int importId, CancellationToken cancellationToken);
    Task ClearImport();
}

public interface IOsmImportService : IImportService
{
    Task StageImport(int importId, string localPath, CancellationToken cancellationToken);
}

public interface ICadastreImportService : IImportService
{
    Task StageImport(int importId, string localPath, bool considerInitial, CancellationToken cancellationToken);
}