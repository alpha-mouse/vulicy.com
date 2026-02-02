using Vulicy.Services;
using Vulicy.Web.Infrastructure;

namespace Vulicy.Web.Endpoints;

public record CreateTopicRequest(int FeatureId);

public record CreateTopicResponse(string ForumRelativeLink);

public static class Forum
{
    public static void MapForum(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/forum");
        group.MapPost("/create-topic", CreateTopic).RequireAdmin();
        // AllowAnonymous: This webhook uses HMAC signature verification instead of cookie auth
        group.MapPost("/discourse-webhook", DiscourseWebhook).AllowAnonymous();
    }

    private static async Task<IResult> CreateTopic(
        CreateTopicRequest request,
        IDiscourseService discourseService,
        HttpContext context)
    {
        var userId = context.User.GetUserId();

        var forumRelativeLink = await discourseService.CreateTopic(request.FeatureId, userId);
        if (forumRelativeLink == null)
        {
            return Results.Problem("Failed to create forum topic. Check Discourse configuration.");
        }

        return Results.Ok(new CreateTopicResponse(forumRelativeLink));
    }

    private static async Task<IResult> DiscourseWebhook(
        HttpContext context,
        IDiscourseService discourseService)
    {
        using var ms = new MemoryStream();
        await context.Request.Body.CopyToAsync(ms);

        var signatureHeader = context.Request.Headers["X-Discourse-Event-Signature"].ToString();
        var eventTypeHeader = context.Request.Headers["X-Discourse-Event"].ToString();

        DiscourseWebhookResult result;
        if (ms.TryGetBuffer(out var buffer))
            result = await discourseService.ProcessWebhook(signatureHeader, eventTypeHeader, buffer);
        else
            throw new InvalidOperationException("MemoryStream betrayed us");

        return result == DiscourseWebhookResult.InvalidSignature
            ? Results.Unauthorized()
            : Results.Ok(result);
    }
}
