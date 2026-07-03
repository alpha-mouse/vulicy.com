using Vulicy.Domain;
using Vulicy.Services;
using Vulicy.Web.Infrastructure;
using Microsoft.Extensions.Caching.Memory;


namespace Vulicy.Web.Endpoints;

public static class Map
{
    public static void MapMap(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/map");
        group.MapGet("/tile/{z}/{x}/{y}.mvt", GetTile);
        group.MapGet("/osm-unmatched-tile/{z}/{x}/{y}.mvt", GetOsmUnmatchedTile).RequireAdmin();
        group.MapGet("/cadastre-unmatched-tile/{z}/{x}/{y}.mvt", GetCadastreUnmatchedTile).RequireAdmin();
        group.MapGet("/naming-categories", GetNamingCategories);

        group.MapGet("/tile-details/{z}/{x}/{y}.mvt", GetTileDetails).RequireAdmin();

        group.MapGet("/explicitly-categorized-tile/{z}/{x}/{y}.mvt", GetExplicitlyCategorizedTile).RequireAdmin();
    }

    private static async Task<IResult> GetTile(int z, int x, int y, IFeatureRepository featureRepository, IMemoryCache cache)
    {
        var cacheKey = $"tile-{z}-{x}-{y}";
        if (cache.TryGetValue(cacheKey, out byte[]? bytes))
        {
            return Results.File(bytes!, "application/x-protobuf");
        }

        var result = await featureRepository.GetTile(z, x, y);

        if (result is byte[] { Length: > 0 } newBytes)
        {
            cache.Set(cacheKey, newBytes, TimeSpan.FromMinutes(10));
            return Results.File(newBytes, "application/x-protobuf");
        }

        return Results.NoContent();
    }

    private static async Task<IResult> GetOsmUnmatchedTile(int z, int x, int y, IOsmFeatureRepository osmFeatureRepository)
    {
        var result = await osmFeatureRepository.GetUnmatchedTile(z, x, y);

        if (result is byte[] { Length: > 0 } newBytes)
        {
            return Results.File(newBytes, "application/x-protobuf");
        }

        return Results.NoContent();
    }

    private static async Task<IResult> GetCadastreUnmatchedTile(int z, int x, int y, ICadastreFeatureRepository cadastreFeatureRepository)
    {
        var result = await cadastreFeatureRepository.GetUnmatchedTile(z, x, y);

        if (result is byte[] { Length: > 0 } newBytes)
        {
            return Results.File(newBytes, "application/x-protobuf");
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

    private static async Task<IResult> GetExplicitlyCategorizedTile(int z, int x, int y, IFeatureRepository featureRepository)
    {
        var result = await featureRepository.GetExplicitlyCategorizedTileDetails(z, x, y);

        if (result is byte[] { Length: > 0 } bytes)
        {
            return Results.File(bytes, "application/x-protobuf");
        }

        return Results.NoContent();
    }

    private static Task<List<NamingCategory>> GetNamingCategories(INamingCategoryService namingCategoryService)
    {
        return namingCategoryService.GetAll();
    }
}