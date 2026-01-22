namespace Vulicy.Services;

public interface IImportingService
{
    Task<int> StartImportOsm(string pbfDownloadUrl, CancellationToken cancellationToken);
    Task<int> StartImportCadastre(string geojsonDownloadUrl, CancellationToken cancellationToken);
    Task<int> StartImportCadastreInitial(string geojsonDownloadUrl, CancellationToken cancellationToken);
}