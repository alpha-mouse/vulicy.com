using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Vulicy.Domain;

namespace Vulicy.Services;

public partial class DiscourseService(
    DiscourseConfig discourseConfig,
    ILinksService linksService,
    IFeatureRepository featureRepository,
    IHttpClientFactory httpClientFactory,
    ILogger<DiscourseService> logger
    ) : IDiscourseService
{
    public async Task<string?> CreateTopic(int featureId, int userId)
    {
        var data = await featureRepository.GetCreateForumTopicData(featureId);
        if (data == null)
            return null;

        var backLink = linksService.CreateFeatureLink(featureId, data);

        var topicBody = $"[Глядзець вуліцу на мапе]({backLink})";

        var label = NameHelpers.GetLabel(data.Type);
        var title = string.IsNullOrWhiteSpace(label) ? data.Name : $"{label} {data.Name}";

        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Api-Key", discourseConfig.ApiKey);
        client.DefaultRequestHeaders.Add("Api-Username", "system");

        var requestBody = new
        {
            title = title,
            raw = topicBody,
            category = discourseConfig.StreetCategoryId
        };

        var response = await client.PostAsJsonAsync(
            $"{discourseConfig.BaseUrl}/posts.json",
            requestBody);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var postResponse = await response.Content.ReadFromJsonAsync(DiscourseJsonSerializerContext.Default.DiscoursePostResponse);

        if (postResponse != null)
        {
            var forumRelativeLink = $"/t/{postResponse.TopicSlug}/{postResponse.TopicId}";

            await featureRepository.UpdateForumLink(featureId, forumRelativeLink, userId);

            return forumRelativeLink;
        }

        return null;
    }

    public async Task<DiscourseWebhookResult> ProcessWebhook(string? signatureHeader, string? eventTypeHeader, ArraySegment<byte> body)
    {
        if (string.IsNullOrEmpty(signatureHeader))
            return DiscourseWebhookResult.InvalidSignature;

        var signature = signatureHeader.StartsWith("sha256=")
            ? signatureHeader.AsSpan()[7..]
            : signatureHeader;

        var expectedSignature = Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(discourseConfig.AuthSecret),
            body));

        if (!signature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase))
            return DiscourseWebhookResult.InvalidSignature;

        return eventTypeHeader switch
        {
            "topic_created" or "topic_edited" => await HandleTopicEvent(body),
            "post_edited" => await HandlePostEditedEvent(body),
            _ => DiscourseWebhookResult.IgnoredEvent
        };
    }

    private async Task<DiscourseWebhookResult> HandleTopicEvent(ArraySegment<byte> body)
    {
        var payload = ParsePayload(body);

        if (payload?.Topic == null)
            return DiscourseWebhookResult.UnexpectedBody;

        if (payload.Topic.CategoryId != discourseConfig.StreetCategoryId)
            return DiscourseWebhookResult.IgnoredEvent;

        var firstPost = await FetchFirstPost(payload.Topic.Id);
        if (firstPost == null)
            return DiscourseWebhookResult.Success;

        if (linksService.TryFindLinkAndParseFeatureId(firstPost.Raw, out var featureId))
        {
            var forumRelativeLink = $"/t/{payload.Topic.Slug}/{payload.Topic.Id}";
            await featureRepository.SetForumLinkIfEmpty(featureId, forumRelativeLink, 0);
        }

        return DiscourseWebhookResult.Success;
    }

    private async Task<DiscourseWebhookResult> HandlePostEditedEvent(ArraySegment<byte> body)
    {
        var payload = ParsePayload(body);

        if (payload?.Post == null)
            return DiscourseWebhookResult.UnexpectedBody;

        if (payload.Post.CategoryId != discourseConfig.StreetCategoryId || payload.Post.PostNumber != 1)
            return DiscourseWebhookResult.IgnoredEvent;

        if (linksService.TryFindLinkAndParseFeatureId(payload.Post.Raw, out var featureId))
        {
            var forumRelativeLink = $"/t/{payload.Post.TopicSlug}/{payload.Post.TopicId}";
            await featureRepository.SetForumLinkIfEmpty(featureId, forumRelativeLink, 0);
        }

        return DiscourseWebhookResult.Success;
    }

    private DiscourseWebhookPayload? ParsePayload(ArraySegment<byte> body)
    {
        try
        {
            return JsonSerializer.Deserialize(body, DiscourseJsonSerializerContext.Default.DiscourseWebhookPayload);
        }
        catch (JsonException e)
        {
            LogFailedParseWebhook(e);
            return null;
        }
    }

    private async Task<DiscoursePost?> FetchFirstPost(int topicId)
    {
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Api-Key", discourseConfig.ApiKey);
        client.DefaultRequestHeaders.Add("Api-Username", "system");

        try
        {
            // The topic endpoint returns the first 20 posts by default, which includes the first post (PostNumber: 1).
            // We use include_raw=true to avoid a second API call to fetch the post content.
            var topicResponse = await client.GetFromJsonAsync(
                $"{discourseConfig.BaseUrl}/t/{topicId}/1.json?include_raw=true&track_visit=false",
                DiscourseJsonSerializerContext.Default.DiscourseTopicResponse);

            return topicResponse?.PostStream?.Posts?.FirstOrDefault(p => p.PostNumber == 1);
        }
        catch (HttpRequestException e)
        {
            LogFailedFetchPost(topicId, e);
            return null;
        }
    }

    [LoggerMessage(LogLevel.Error, "Failed to parse Discourse event payload")]
    private partial void LogFailedParseWebhook(Exception e);

    [LoggerMessage(LogLevel.Error, "Failed to fetch first post for topic {TopicId}")]
    private partial void LogFailedFetchPost(int topicId, Exception e);
}
