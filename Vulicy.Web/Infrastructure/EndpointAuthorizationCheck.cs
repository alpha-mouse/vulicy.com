namespace Vulicy.Web.Infrastructure;

/// <summary>
/// Startup check to ensure all non-GET endpoints either have RequireAuthorization() or AllowAnonymous() explicitly set.
/// This prevents accidentally exposing mutating endpoints without authorization.
/// </summary>
public static class EndpointAuthorizationCheck
{
    /// <summary>
    /// Validates that all non-GET endpoints have either authorization requirements or are explicitly marked as anonymous.
    /// Call this after all endpoints are mapped but before the app starts serving requests.
    /// </summary>
    public static void CheckNonGetEndpointsRequireAuthorization(this IServiceProvider services)
    {
        var allowedMethods = new List<string> { "GET", "HEAD", "OPTIONS" };
        var endpointDataSource = services.GetRequiredService<EndpointDataSource>();
        var violations = new List<string>();

        foreach (var endpoint in endpointDataSource.Endpoints)
        {
            if (endpoint is not RouteEndpoint routeEndpoint)
                continue;

            var httpMethodMetadata = endpoint.Metadata.GetMetadata<HttpMethodMetadata>();
            if (httpMethodMetadata == null)
                continue;

            var methods = httpMethodMetadata.HttpMethods;
            if (methods.All(x => allowedMethods.Contains(x, StringComparer.OrdinalIgnoreCase)))
                continue;

            var hasAuthorization = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAuthorizeData>() != null;
            var hasAllowAnonymous = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null;

            if (!hasAuthorization && !hasAllowAnonymous)
            {
                var pattern = routeEndpoint.RoutePattern.RawText ?? routeEndpoint.DisplayName;
                var methodsStr = string.Join(", ", methods);
                violations.Add($"{methodsStr} {pattern}");
            }
        }

        if (violations.Count > 0)
        {
            throw new InvalidOperationException(
                $"The following non-GET endpoints are missing RequireAuthorization() or AllowAnonymous():\n" +
                string.Join("\n", violations.Select(v => $"  - {v}")));
        }
    }
}
