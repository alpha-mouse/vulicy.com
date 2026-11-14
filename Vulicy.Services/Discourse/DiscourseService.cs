using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Vulicy.Domain;

namespace Vulicy.Services;

public partial class DiscourseService(
    DiscourseConfig discourseConfig,
    ILinksService linksService,
    IFeatureRepository featureRepository,
    IDossierRecordRepository dossierRecordRepository,
    IHttpClientFactory httpClientFactory,
    ILogger<DiscourseService> logger
    ) : IDiscourseService
{
    private const int MaxLoggedResponseLength = 4000;

    public async Task<string> CreateFeatureTopic(int featureId, int userId)
    {
        var data = await featureRepository.GetCreateForumTopicData(featureId)
                   ?? throw new NotFoundException("Feature");

        if (data.ForumRelativeLink != null)
            return data.ForumRelativeLink;

        var backLink = linksService.CreateFeatureLink(featureId, data);

        var topicBody = $"[Глядзець вуліцу на мапе]({backLink})";

        var label = NameHelpers.GetLabel(data.Type);
        var title = string.IsNullOrWhiteSpace(label) ? data.Name : $"{label} {data.Name}";

        var forumRelativeLink = await PostTopic("feature", featureId, title, topicBody, discourseConfig.StreetCategoryId);

        await featureRepository.UpdateForumLink(featureId, forumRelativeLink, userId);

        return forumRelativeLink;
    }

    public async Task<string> CreateDossierRecordTopic(int dossierRecordId, int userId)
    {
        var dossierRecord = await dossierRecordRepository.GetById(dossierRecordId)
                            ?? throw new NotFoundException("Dossier record");

        if (dossierRecord.ForumRelativeLink != null)
            return dossierRecord.ForumRelativeLink;

        var backLink = linksService.CreateDossierRecordLink(dossierRecordId);

        var topicBody = $"[Глядзець на мапе]({backLink})" + '\n' + (dossierRecord.DescriptionBe ?? dossierRecord.DescriptionRu);

        var title = dossierRecord.NameBeTarask;

        var forumRelativeLink = await PostTopic("dossier record", dossierRecordId, title, topicBody, discourseConfig.DossierRecordCategoryId);

        await dossierRecordRepository.UpdateForumLink(dossierRecordId, forumRelativeLink, userId);

        return forumRelativeLink;
    }

    /// <summary>
    /// Creates a topic and returns its relative link. Any Discourse-side failure is logged at Error level with
    /// the full Discourse response (picked up by Sentry) and surfaced as <see cref="ExternalServiceException"/>.
    /// </summary>
    private async Task<string> PostTopic(string objectType, int objectId, string? title, string raw, int categoryId)
    {
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Api-Key", discourseConfig.ApiKey);
        client.DefaultRequestHeaders.Add("Api-Username", "system");

        var requestBody = new DiscourseCreatePostRequest(title, raw, categoryId);

        using var response = await client.PostAsJsonAsync(
            $"{discourseConfig.BaseUrl}/posts.json",
            requestBody,
            DiscourseJsonSerializerContext.Default.DiscourseCreatePostRequest);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            LogTopicCreationRejected(objectType, objectId, (int)response.StatusCode, response.ReasonPhrase, title, categoryId, Truncate(responseBody));
            throw new ExternalServiceException("Failed to create forum topic");
        }

        DiscoursePostResponse? postResponse;
        try
        {
            postResponse = JsonSerializer.Deserialize(responseBody, DiscourseJsonSerializerContext.Default.DiscoursePostResponse);
        }
        catch (JsonException e)
        {
            LogTopicCreationUnexpectedResponse(e, objectType, objectId, (int)response.StatusCode, Truncate(responseBody));
            throw new ExternalServiceException("Failed to create forum topic");
        }

        if (postResponse == null || string.IsNullOrEmpty(postResponse.TopicSlug))
        {
            LogTopicCreationUnexpectedResponse(null, objectType, objectId, (int)response.StatusCode, Truncate(responseBody));
            throw new ExternalServiceException("Failed to create forum topic");
        }

        return $"/t/{postResponse.TopicSlug}/{postResponse.TopicId}";
    }

    private static string Truncate(string value) =>
        value.Length <= MaxLoggedResponseLength ? value : value[..MaxLoggedResponseLength] + "…(truncated)";

    public async Task<DiscourseWebhookResult> ProcessWebhook(string? signatureHeader, string? eventTypeHeader, ArraySegment<byte> body)
    {
        if (string.IsNullOrEmpty(signatureHeader))
            return DiscourseWebhookResult.InvalidSignature;

        var signature = signatureHeader.StartsWith("sha256=")
            ? signatureHeader.AsSpan()[7..]
            : signatureHeader;

        byte[] providedSignature;
        try
        {
            providedSignature = Convert.FromHexString(signature);
        }
        catch (FormatException)
        {
            return DiscourseWebhookResult.InvalidSignature;
        }

        var expectedSignature = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(discourseConfig.AuthSecret),
            body);

        if (!CryptographicOperations.FixedTimeEquals(expectedSignature, providedSignature))
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

        if (payload.Topic.CategoryId != discourseConfig.StreetCategoryId && payload.Topic.CategoryId != discourseConfig.DossierRecordCategoryId)
            return DiscourseWebhookResult.IgnoredEvent;

        var firstPost = await FetchFirstPost(payload.Topic.Id);
        if (firstPost == null)
            return DiscourseWebhookResult.Success;

        if (payload.Topic.CategoryId == discourseConfig.StreetCategoryId && linksService.TryFindFeatureLinkAndParseId(firstPost.Raw, out var featureId))
        {
            var forumRelativeLink = $"/t/{payload.Topic.Slug}/{payload.Topic.Id}";
            await featureRepository.SetForumLinkIfEmpty(featureId, forumRelativeLink, 0);
        }

        if (payload.Topic.CategoryId == discourseConfig.DossierRecordCategoryId && linksService.TryFindDossierRecordLinkAndParseId(firstPost.Raw, out var dossierRecordId))
        {
            var forumRelativeLink = $"/t/{payload.Topic.Slug}/{payload.Topic.Id}";
            await dossierRecordRepository.SetForumLinkIfEmpty(dossierRecordId, forumRelativeLink, 0);
        }

        return DiscourseWebhookResult.Success;
    }

    private async Task<DiscourseWebhookResult> HandlePostEditedEvent(ArraySegment<byte> body)
    {
        var payload = ParsePayload(body);

        if (payload?.Post == null)
            return DiscourseWebhookResult.UnexpectedBody;

        if (payload.Post.PostNumber != 1 || payload.Post.CategoryId != discourseConfig.StreetCategoryId && payload.Post.CategoryId != discourseConfig.DossierRecordCategoryId)
            return DiscourseWebhookResult.IgnoredEvent;

        if (payload.Post.CategoryId == discourseConfig.StreetCategoryId && linksService.TryFindFeatureLinkAndParseId(payload.Post.Raw, out var featureId))
        {
            var forumRelativeLink = $"/t/{payload.Post.TopicSlug}/{payload.Post.TopicId}";
            await featureRepository.SetForumLinkIfEmpty(featureId, forumRelativeLink, 0);
        }

        if (payload.Post.CategoryId == discourseConfig.DossierRecordCategoryId && linksService.TryFindDossierRecordLinkAndParseId(payload.Post.Raw, out var dossierRecordId))
        {
            var forumRelativeLink = $"/t/{payload.Post.TopicSlug}/{payload.Post.TopicId}";
            await dossierRecordRepository.SetForumLinkIfEmpty(dossierRecordId, forumRelativeLink, 0);
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

    [LoggerMessage(LogLevel.Error, "Discourse rejected topic creation for {ObjectType} {ObjectId}: {StatusCode} {ReasonPhrase}. Title: {Title}, category: {CategoryId}. Response: {ResponseBody}")]
    private partial void LogTopicCreationRejected(string objectType, int objectId, int statusCode, string? reasonPhrase, string? title, int categoryId, string responseBody);

    [LoggerMessage(LogLevel.Error, "Unexpected Discourse response on topic creation for {ObjectType} {ObjectId}: {StatusCode}. Response: {ResponseBody}")]
    private partial void LogTopicCreationUnexpectedResponse(Exception? e, string objectType, int objectId, int statusCode, string responseBody);

    [LoggerMessage(LogLevel.Error, "Failed to parse Discourse event payload")]
    private partial void LogFailedParseWebhook(Exception e);

    [LoggerMessage(LogLevel.Error, "Failed to fetch first post for topic {TopicId}")]
    private partial void LogFailedFetchPost(int topicId, Exception e);
}
