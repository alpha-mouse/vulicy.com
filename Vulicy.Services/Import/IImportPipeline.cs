using Vulicy.Domain;

namespace Vulicy.Services;

public interface IImportPipeline
{
    void StartRunning(int importId, ImportType importType, CancellationToken cancellationToken);
    Task MatchMissingOsmCadastre(CancellationToken cancellationToken);
    Task InitializeNamingCategories();
    Task InitializeDossierRecords();
    Task InitializeFeaturesDossierCategoriesReferences(CancellationToken cancellationToken);
    Task MapFieldsFromInitialCadastreImport(CancellationToken applicationStopping);
}