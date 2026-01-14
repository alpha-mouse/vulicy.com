using Vulicy.Domain;

namespace Vulicy.Web.Endpoints;

public static class Map
{
    public static void MapMap(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/map");
        group.MapGet("/tile/{z}/{x}/{y}.mvt", GetTile);
        group.MapGet("/naming-categories", GetNamingCategories);
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

    private static Task<List<NamingCategoryEntity>> GetNamingCategories(INamingCategoryRepository namingCategoryRepository)
    {
        return namingCategoryRepository.GetAll();
    }
}