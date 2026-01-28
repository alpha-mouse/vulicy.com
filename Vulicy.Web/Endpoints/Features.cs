using Vulicy.Domain;
using Vulicy.Services;
using Vulicy.Web.Infrastructure;

namespace Vulicy.Web.Endpoints;

public static class Features
{
    public static void MapFeatures(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/features");
        group.MapGet("/search", Search);
        group.MapPut("/{id:int}", EditFeature).RequireAuthorization().Validate<FeatureEditRequest>();
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
}
