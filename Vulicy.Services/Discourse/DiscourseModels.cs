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
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("category_id")] int CategoryId
);

public record DiscoursePost(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("raw")] string Raw,
    [property: JsonPropertyName("post_number")] int PostNumber,
    [property: JsonPropertyName("topic_id")] int TopicId,
    [property: JsonPropertyName("topic_slug")] string TopicSlug,
    [property: JsonPropertyName("category_id")] int CategoryId
);

public record DiscourseTopicResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("category_id")] int CategoryId,
    [property: JsonPropertyName("post_stream")] DiscoursePostsStream? PostStream
);

public record DiscoursePostsStream(
    [property: JsonPropertyName("posts")] DiscoursePost[]? Posts
);
