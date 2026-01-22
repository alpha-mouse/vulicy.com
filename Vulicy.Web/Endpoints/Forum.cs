using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vulicy.Domain;
using Vulicy.Services;
using Vulicy.Web.Infrastructure;

namespace Vulicy.Web.Endpoints;

public record CreateTopicRequest(int FeatureId);

public record CreateTopicResponse(string ForumRelativeLink);

public static partial class Forum
{
    [GeneratedRegex(@"featureId=(\d+)")]
    private static partial Regex FeatureIdRegex();

    public static void MapForum(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/forum");
        group.MapPost("/create-topic", CreateTopic).RequireAuthorization();
        group.MapPost("/discourse-webhook", DiscourseWebhook);
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
        DiscourseConfig config,
        IFeatureRepository featureRepository)
    {
        // Read the raw body
        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();

        // Verify signature
        if (!context.Request.Headers.TryGetValue("X-Discourse-Event-Signature", out var signatureHeader))
        {
            return Results.Unauthorized();
        }

        var signature = signatureHeader.ToString();
        if (signature.StartsWith("sha256="))
        {
            signature = signature[7..];
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(config.AuthSecret));
        var expectedSignature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(body))).ToLower();

        if (signature != expectedSignature)
        {
            return Results.Unauthorized();
        }

        // Check event type
        if (!context.Request.Headers.TryGetValue("X-Discourse-Event", out var eventType) ||
            eventType != "topic_created")
        {
            return Results.Ok(); // Ignore non-topic_created events
        }

        // Parse the body to get the topic content
        try
        {
            var payload = JsonSerializer.Deserialize(body, DiscourseJsonSerializerContext.Default.DiscourseWebhookPayload);
            if (payload?.Topic != null)
            {
                var rawContent = payload.Post?.Raw;

                if (!string.IsNullOrEmpty(rawContent))
                {
                    var match = FeatureIdRegex().Match(rawContent);
                    if (match.Success && int.TryParse(match.Groups[1].Value, out var featureId))
                    {
                        var forumRelativeLink = $"/t/{payload.Topic.Slug}/{payload.Topic.Id}";
                        await featureRepository.SetForumLinkIfEmpty(featureId, forumRelativeLink, 0);
                    }
                }
            }
        }
        catch
        {
            // Log and ignore parsing errors
        }

        return Results.Ok();
    }
}
