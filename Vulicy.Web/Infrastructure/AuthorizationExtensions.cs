namespace Vulicy.Web.Infrastructure;

public static class AuthorizationExtensions
{
    public const string RequireAdminPolicy = nameof(RequireAdmin);

    public static TBuilder RequireAdmin<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(RequireAdminPolicy);
    }
}
