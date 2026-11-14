namespace Vulicy.Services;

/// <summary>Requested object doesn't exist, answered with 404 by ExceptionMiddleware and not reported to Sentry.</summary>
public class NotFoundException(string objectType)
    : Exception((objectType ?? throw new ArgumentNullException(nameof(objectType))) + " not found");
