using NetTopologySuite.Geometries;
using Vulicy.Domain;
using Vulicy.Services;
using Vulicy.Web.Infrastructure;

namespace Vulicy.Web.Endpoints;

public static class Features
{
    public static void MapFeatures(this IEndpointRouteBuilder builder)
    {
        var features = builder.MapGroup("/api/features");
        features.MapGet("/search", Search);
        features.MapPost("/from-sources", CreateFeatureFromSources).RequireAdmin().Validate<FeatureCreateFromSourcesRequest>();
        features.MapPut("/{id:int}", EditFeature).RequireAdmin().Validate<FeatureEditRequest>();
        features.MapPost("/{id:int}/link-osm", LinkOsmFeature).RequireAdmin().Validate<OsmId>();
        features.MapPut("/{id:int}/recompute-geometry", RecomputeGeometry).RequireAdmin();
        features.MapPost("/preview", GetFeaturePreview).RequireAdmin().Validate<GetFeaturePreviewRequest>();

        var osmFeatures = builder.MapGroup("/api/osm-features").RequireAdmin();
        osmFeatures.MapGet("/search-unmatched", OsmSearch);

        var cadastreFeatures = builder.MapGroup("/api/cadastre-features").RequireAdmin();
        cadastreFeatures.MapGet("/search-unmatched", CadastreSearch);
    }

    private static Task<List<FeatureSearchResult>> Search(string query, double? lat, double? lng, IFeatureRepository featureRepository)
    {
        return featureRepository.SearchByName(query, lat, lng);
    }

    private static Task<FeatureSearchResult> CreateFeatureFromSources(FeatureCreateFromSourcesRequest feature, IFeatureService featureService, HttpContext context)
    {
        var userId = context.User.GetUserId();
        return featureService.CreateFeatureFromSources(feature, userId);
    }

    private static Task EditFeature(int id, FeatureEditRequest feature, IFeatureService featureService, HttpContext context)
    {
        var userId = context.User.GetUserId();
        return featureService.EditFeature(id, feature, userId);
    }

    private static Task<Geometry> LinkOsmFeature(int id, OsmId feature, IFeatureService featureService, HttpContext context)
    {
        var userId = context.User.GetUserId();
        return featureService.LinkOsmFeature(id, feature, userId);
    }

    private static Task RecomputeGeometry(int id, IFeatureService featureService)
    {
        return featureService.RecomputeGeometry(id);
    }

    private static Task<FeatureTileMinimalDetails> GetFeaturePreview(GetFeaturePreviewRequest request, IFeatureService featureService)
    {
        return featureService.GetFeaturePreview(request);
    }

    private static Task<List<OsmFeatureSearchResult>> OsmSearch(string query, double? lat, double? lng, IOsmFeatureRepository osmFeatureRepository)
    {
        return osmFeatureRepository.SearchUnmatchedByName(query, lat, lng);
    }

    private static Task<List<CadastreFeatureSearchResult>> CadastreSearch(string query, double? lat, double? lng, ICadastreFeatureRepository cadastreFeatureRepository)
    {
        return cadastreFeatureRepository.SearchUnmatchedByName(query, lat, lng);
    }
}
