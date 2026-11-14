using System.Text.Json.Serialization;

namespace Vulicy.Services;

[JsonSerializable(typeof(DiscourseCreatePostRequest))]
[JsonSerializable(typeof(DiscoursePostResponse))]
[JsonSerializable(typeof(DiscourseWebhookPayload))]
[JsonSerializable(typeof(DiscourseTopicResponse))]
[JsonSerializable(typeof(DiscoursePost))]
[JsonSerializable(typeof(DiscourseWebhookResult))]
public partial class DiscourseJsonSerializerContext : JsonSerializerContext
{
}
