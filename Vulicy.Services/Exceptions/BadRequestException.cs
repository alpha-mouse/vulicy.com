namespace Vulicy.Services;

/// <summary>Client error, answered with 400 by ExceptionMiddleware and not reported to Sentry.</summary>
public class BadRequestException(string message) : Exception(message);
