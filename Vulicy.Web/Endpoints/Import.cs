using Microsoft.AspNetCore.Mvc;
using Vulicy.Services;

namespace Vulicy.Web.Endpoints;

public static class Import
{
    public static void MapImport(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/import");
        group.MapPost("/osm", ImportOsm);
        group.MapPost("/cadastre", ImportCadastre);
        group.MapPost("/cadastre-initial", ImportCadastreInitial);
        group.MapPost("/match-osm-cadastre", MatchOsmCadastre);
        group.MapPost("/1-initialize-naming-categories", InitializeNamingCategories);
        group.MapPost("/2-initialize-dossier-records", InitializeDossierRecords);
        group.MapPost("/3-initialize-features-dossier-categories-references", InitializeFeaturesDossierCategoriesReferences);
        group.MapPost("/4-map-fields-from-initial-cadastre-import", MapFieldsFromInitialCadastreImport);
    }

    private static Task<int> ImportOsm([FromBody] string pbfDownloadUrl, IImportingService importingService, IHostApplicationLifetime hostApplicationLifetime)
    {
        return importingService.StartImportOsm(pbfDownloadUrl, hostApplicationLifetime.ApplicationStopping);
    }

    private static Task<int> ImportCadastre([FromBody] string geojsonDownloadUrl, IImportingService importingService, IHostApplicationLifetime hostApplicationLifetime)
    {
        return importingService.StartImportCadastre(geojsonDownloadUrl, hostApplicationLifetime.ApplicationStopping);
    }

    private static Task<int> ImportCadastreInitial([FromBody] string geojsonDownloadUrl, IImportingService importingService, IHostApplicationLifetime hostApplicationLifetime)
    {
        return importingService.StartImportCadastreInitial(geojsonDownloadUrl, hostApplicationLifetime.ApplicationStopping);
    }

    private static Task MatchOsmCadastre(IImportPipeline importPipeline, IHostApplicationLifetime hostApplicationLifetime)
    {
        return importPipeline.MatchMissingOsmCadastre(hostApplicationLifetime.ApplicationStopping);
    }

    private static Task InitializeNamingCategories(IImportPipeline importingService)
    {
        return importingService.InitializeNamingCategories();
    }

    private static Task InitializeDossierRecords(IImportPipeline importPipeline)
    {
        return importPipeline.InitializeDossierRecords();
    }

    private static Task InitializeFeaturesDossierCategoriesReferences(IImportPipeline importPipeline, IHostApplicationLifetime hostApplicationLifetime)
    {
        return importPipeline.InitializeFeaturesDossierCategoriesReferences(hostApplicationLifetime.ApplicationStopping);
    }

    private static Task MapFieldsFromInitialCadastreImport(IImportPipeline importPipeline, IHostApplicationLifetime hostApplicationLifetime)
    {
        return importPipeline.MapFieldsFromInitialCadastreImport(hostApplicationLifetime.ApplicationStopping);
    }
}