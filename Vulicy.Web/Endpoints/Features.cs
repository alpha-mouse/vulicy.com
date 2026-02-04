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
        features.MapPut("/{id:int}", EditFeature).RequireAdmin().Validate<FeatureEditRequest>();

        var osmFeatures = builder.MapGroup("/api/osm-features");
        osmFeatures.MapGet("/search", OsmSearch);

        var cadastreFeatures = builder.MapGroup("/api/cadastre-features");
        cadastreFeatures.MapGet("/search", CadastreSearch);
    }

    private static Task<List<FeatureSearchResult>> Search(string query, double? lat, double? lng, IFeatureService featureService)
    {
        return featureService.SearchByName(query, lat, lng);
    }

    private static Task EditFeature(int id, FeatureEditRequest feature, IFeatureService featureService, HttpContext context)
    {
        var userId = context.User.GetUserId();
        return featureService.EditFeature(id, feature, userId);
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
