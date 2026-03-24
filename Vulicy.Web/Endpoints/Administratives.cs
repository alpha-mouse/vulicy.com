using Microsoft.Extensions.Caching.Memory;
using Vulicy.Services;

namespace Vulicy.Web.Endpoints;

public static class Administratives
{
    private static readonly object AdministrativesCacheKey = new();

    public static void MapAdministratives(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/administratives");
        group.MapGet("", GetAllAdministratives);
    }

    private static async Task<List<Services.Administrative>> GetAllAdministratives(IAdministrativeService administrativeService, IMemoryCache cache)
    {
        if (cache.TryGetValue(AdministrativesCacheKey, out List<Administrative>? administratives))
        {
            return administratives!;
        }

        administratives = await administrativeService.GetAdministratives();
        cache.Set(AdministrativesCacheKey, administratives, TimeSpan.FromMinutes(10));

        return administratives;
    }
}
