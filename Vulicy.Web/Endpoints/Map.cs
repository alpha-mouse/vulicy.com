using Vulicy.Domain;
using Vulicy.Services;

namespace Vulicy.Web.Endpoints;

public static class Map
{
    public static void MapMap(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/map");
        group.MapGet("/tile/{z}/{x}/{y}.mvt", GetTile);
        group.MapGet("/naming-categories", GetNamingCategories);

        group.MapGet("/tile-details/{z}/{x}/{y}.mvt", GetTileDetails).RequireAuthorization(policy => policy.RequireRole(Auth.AdminRole));
    }

    private static async Task<IResult> GetTile(int z, int x, int y, IFeatureRepository featureRepository)
    {
        var result = await featureRepository.GetTile(z, x, y);

        if (result is byte[] { Length: > 0 } bytes)
        {
            return Results.File(bytes, "application/x-protobuf");
        }

        return Results.NoContent();
    }

    private static async Task<IResult> GetTileDetails(int z, int x, int y, IFeatureRepository featureRepository)
    {
        var result = await featureRepository.GetTileDetails(z, x, y);

        if (result is byte[] { Length: > 0 } bytes)
        {
            return Results.File(bytes, "application/x-protobuf");
        }

        return Results.NoContent();
    }

    private static Task<List<NamingCategoryDto>> GetNamingCategories(INamingCategoryService namingCategoryService)
    {
        return namingCategoryService.GetAll();
    }
}