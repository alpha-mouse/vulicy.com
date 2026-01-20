using Vulicy.Web.Infrastructure;

namespace Vulicy.Web.Endpoints;

public static class Config
{
    public static void MapConfig(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/config");
        group.MapGet("", GetConfig);
    }

    private static FrontConfig GetConfig(FrontConfig config)
        => config;

}
