using System.Net;
using Vulicy.Services;

namespace Vulicy.Web.Infrastructure;

public partial class ExceptionMiddleware(ILogger<ExceptionMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (BadRequestException e)
        {
            await Write(context, HttpStatusCode.BadRequest, e.Message);
        }
        catch (NotFoundException e)
        {
            await Write(context, HttpStatusCode.NotFound, e.Message);
        }
        catch (ExternalServiceException e)
        {
            // Details were already logged at Error level by the thrower.
            await Write(context, HttpStatusCode.BadGateway, e.Message);
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    private Task Write(HttpContext context, HttpStatusCode status, string message)
    {
        LogInfo((int)status, message);
        return context.Response.HasStarted
            ? Task.CompletedTask
            : ErrorResponse.Write(context.Response, status, message);
    }

    [LoggerMessage(LogLevel.Error, "Unhandled exception")]
    private partial void LogError(Exception e);

    [LoggerMessage(LogLevel.Information, "{StatusCode}: {Message}")]
    private partial void LogInfo(int statusCode, string message);
}
