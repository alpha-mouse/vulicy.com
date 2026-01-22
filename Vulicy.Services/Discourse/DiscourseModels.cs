using System.Text.Json.Serialization;

namespace Vulicy.Services;

public record DiscoursePostResponse(
    [property: JsonPropertyName("topic_id")] int TopicId,
    [property: JsonPropertyName("topic_slug")] string TopicSlug
);

public record DiscourseWebhookPayload(
    [property: JsonPropertyName("topic")] DiscourseTopic? Topic,
    [property: JsonPropertyName("post")] DiscoursePost? Post
);

public record DiscourseTopic(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("slug")] string Slug
);

public record DiscoursePost(
    [property: JsonPropertyName("raw")] string Raw
);
