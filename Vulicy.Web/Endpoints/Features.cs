using Vulicy.Domain;
using Vulicy.Services;

namespace Vulicy.Web.Endpoints;

public static class Features
{
    public static void MapFeatures(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/features");
        group.MapGet("/search", Search);
    }

    private static Task<List<FeatureSearchResult>> Search(string query, double? lat, double? lng, IFeatureService featureService)
    {
        return featureService.SearchByName(query, lat, lng);
    }
}
