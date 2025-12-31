using Vulicy.Domain;

namespace Vulicy.Services;

public class ImportingService(
    IImportRepository importRepository,
    IInitialCadastreFeatureImportRepository initialCadastreFeatureImportRepository,
    IImportPipeline importPipeline
    ) : IImportingService
{
    public Task<int> StartImportOsm(string pbfDownloadUrl, CancellationToken cancellationToken)
    {
        return CreateAndRunImport(pbfDownloadUrl, ImportType.OpenStreetMap, cancellationToken);
    }

    public Task<int> StartImportCadastre(string geojsonDownloadUrl, CancellationToken cancellationToken)
    {
        return CreateAndRunImport(geojsonDownloadUrl, ImportType.Cadastre, cancellationToken);
    }

    public async Task<int> StartImportCadastreInitial(string geojsonDownloadUrl, CancellationToken cancellationToken)
    {
        if (await initialCadastreFeatureImportRepository.HasAny())
            throw new InvalidOperationException("Initial cadastre import has already happened");
        return await CreateAndRunImport(geojsonDownloadUrl, ImportType.CadastreInitial, cancellationToken);
    }

    private async Task<int> CreateAndRunImport(string pbfDownloadUrl, ImportType importType, CancellationToken cancellationToken)
    {
        var import = new ImportEntity
        {
            Type = importType,
            DownloadUrl = pbfDownloadUrl,
            LocalPath = Path.GetTempFileName(),
            Status = ImportStatus.Pending,
        };

        importRepository.Add(import);
        await importRepository.SaveChanges();
        var importId = import.Id;

        importPipeline.StartRunning(importId, import.Type, cancellationToken);

        return importId;
    }
}