using System.Net.Http.Json;
using System.Text.Json;
using Vulicy.Domain;

namespace Vulicy.Services;

public class ForumService(AppConfig appConfig, DiscourseConfig discourseConfig, IHttpClientFactory httpClientFactory, IFeatureRepository featureRepository) : IForumService
{
    public async Task<string?> CreateTopic(int featureId, int userId)
    {
        var data = await featureRepository.GetCreateForumTopicData(featureId);
        if (data == null)
            return null;

        var backLink = $"{appConfig.BaseUrl}/?featureId={featureId}&lat={data.Lat:F4}&lng={data.Lng:F4}&z={appConfig.DefaultZoom}";

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
            category = discourseConfig.ForumCategoryId.Value
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
}
