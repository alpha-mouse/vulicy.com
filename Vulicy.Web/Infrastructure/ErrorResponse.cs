using System.Net;

namespace Vulicy.Web.Infrastructure;

public record ErrorResponse(int Code, string Message)
{
    /// <summary>Single writer for every error body the app emits outside the minimal-API <see cref="IResult"/> pipeline,
    /// so the middleware and the endpoint filters cannot drift into separate shapes.</summary>
    public static Task Write(HttpResponse response, HttpStatusCode status, string message)
    {
        response.StatusCode = (int)status;
        // Uses the app's HttpJsonOptions (camelCase + source-gen resolver chain), same as IResult responses.
        return response.WriteAsJsonAsync(new ErrorResponse((int)status, message));
    }
}
