using System.Security.Claims;
using Amazon.DynamoDBv2.Model;
using Microsoft.IO;

namespace Vulicy.Web.Infrastructure;

public class AuditMiddleware(
    IAuditQueue auditQueue,
    RequestDelegate next)
{
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager = new();

    public Task InvokeAsync(HttpContext context)
    {
        var isInterestingMethod = HttpMethods.IsPost(context.Request.Method)
                           || HttpMethods.IsPut(context.Request.Method)
                           || HttpMethods.IsPatch(context.Request.Method)
                           || HttpMethods.IsDelete(context.Request.Method);
        if (!isInterestingMethod)
        {
            return next(context);
        }

        var isInterestingPath = context.Request.Path.StartsWithSegments("/api");
        if (!isInterestingPath)
        {
            return next(context);
        }

        return Audit(context);
    }

    private async Task Audit(HttpContext context)
    {
        string? requestBody = null;
        var isJson = context.Request.ContentType?.Contains("application/json") ?? false;
        var isForms = context.Request.ContentType?.Contains("multipart/form-data") ?? false;
        const long bufferLimit = 16384L;
        if ((isJson || isForms) && context.Request.ContentLength < bufferLimit)
        {
            context.Request.EnableBuffering(bufferLimit: bufferLimit);

            using (var streamReader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8, leaveOpen: true))
                requestBody = await streamReader.ReadToEndAsync();

            context.Request.Body.Seek(0, SeekOrigin.Begin);
        }

        var time = DateTime.UtcNow;
        var method = context.Request.Method;
        var path = context.Request.Path.ToString();
        var query = string.Join(",", context.Request.Query);
        var requestContentType = context.Request.ContentType;
        var requestContentLength = context.Request.ContentLength;
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int responseCode = 0;
        string? exceptionMessage = null;
        string? responseBody = null;

        var originalBody = context.Response.Body;
        try
        {
            await using var responseStream = _recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseStream;
            await next(context);
            responseStream.Seek(0, SeekOrigin.Begin);
            using (var streamReader = new StreamReader(responseStream, System.Text.Encoding.UTF8, leaveOpen: true))
                responseBody = await streamReader.ReadToEndAsync();
            responseStream.Seek(0, SeekOrigin.Begin);
            await responseStream.CopyToAsync(originalBody);
        }
        catch (Exception e)
        {
            // Filters (CommonExceptionFilter) are run upstream of this.
            // So if an exception occurs the status code will be a lie. It will be 200 OK.
            // So by the presence of the error message we'll be able to judge that something went wrong
            exceptionMessage = e.Message;
            throw;
        }
        finally
        {
            context.Response.Body = originalBody;
            responseCode = context.Response.StatusCode;

            auditQueue.Enqueue(new Dictionary<string, AttributeValue>
            {
                { "Month", new AttributeValue(time.ToString("yyyy-MM")) },
                { "DateTime", new AttributeValue(time.ToString("O")) },
                { "Method", new AttributeValue(method) },
                { "Path", new AttributeValue(path) },
                { "Query", new AttributeValue(query) },
                { "RequestContentType", new AttributeValue(requestContentType ?? string.Empty) },
                { "RequestContentLength", new AttributeValue(requestContentLength?.ToString() ?? string.Empty) },
                { "RequestBody", new AttributeValue(requestBody ?? string.Empty) },
                { "UserId", new AttributeValue(userId ?? string.Empty) },
                { "ResponseCode", new AttributeValue(responseCode.ToString()) },
                { "ResponseBody", new AttributeValue(responseBody ?? string.Empty) },
                { "ExceptionMessage", new AttributeValue(exceptionMessage ?? string.Empty) },
            });
        }
    }
}