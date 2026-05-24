using Microsoft.AspNetCore.Mvc;
using Vulicy.Domain;
using Vulicy.Services;
using Vulicy.Web.Infrastructure;

namespace Vulicy.Web.Endpoints;

public static class Import
{
    public static void MapImport(this IEndpointRouteBuilder builder, bool withAdminAuth)
    {
        // These endpoints are registered anonymous in Development and admin authenticated in Production.
        var group = builder.MapGroup("/api/import");
        group = withAdminAuth ? group.RequireAdmin() : group.AllowAnonymous();
        group.MapPost("/osm", ImportOsm);
        group.MapPost("/cadastre", ImportCadastre);
        group.MapPost("/cadastre-initial", ImportCadastreInitial);
        group.MapPost("/1-initialize-naming-categories", InitializeNamingCategories);
        group.MapPost("/2-create-dossier-records-from-sql", CreateDossierRecordsFromSql);
        group.MapPost("/3-initialize-dossier-records", InitializeDossierRecords);
        group.MapPost("/4-create-missing-administrative", CreateMissingAdministrative);
        group.MapPost("/5-match-osm-cadastre", MatchOsmCadastre);
        group.MapPost("/6-initialize-features-dossier-categories-references", InitializeFeaturesDossierCategoriesReferences);
        group.MapPost("/7-map-fields-from-initial-cadastre-import", MapFieldsFromInitialCadastreImport);
        group.MapPost("/8-set-administrative-on-features", SetAdministrativeOnFeatures);
        group.MapPost("/9-compute-administrative-boundaries", ComputeAdministrativeBoundaries);
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

    private static IResult CreateDossierRecordsFromSql()
    {
        return Results.BadRequest("FROM SQL!!1! Run SQL script manually.");
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

    private static Task CreateMissingAdministrative(IAdministrativeRepository administrativeRepository)
    {
        return administrativeRepository.CreateMissingAdministrativeFromCadastre();
    }

    private static Task SetAdministrativeOnFeatures(IAdministrativeRepository administrativeRepository)
    {
        return administrativeRepository.SetAdministrativeOnFeatures();
    }

    private static Task ComputeAdministrativeBoundaries(IImportPipeline importPipeline, IHostApplicationLifetime hostApplicationLifetime)
    {
        return importPipeline.ComputeAdministrativeBoundaries(hostApplicationLifetime.ApplicationStopping);
    }
}