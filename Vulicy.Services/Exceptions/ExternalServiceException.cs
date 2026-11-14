namespace Vulicy.Services;

/// <summary>
/// An upstream service (e.g. Discourse) failed. Answered with 502 by ExceptionMiddleware.
/// The thrower is responsible for logging the failure details at Error level (which is what Sentry picks up);
/// the middleware only logs this at Information to avoid a duplicate Sentry event.
/// </summary>
public class ExternalServiceException(string message) : Exception(message);
