using System.Text.Json.Serialization;

namespace Vulicy.Services;

[JsonSerializable(typeof(DiscoursePostResponse))]
[JsonSerializable(typeof(DiscourseWebhookPayload))]
[JsonSerializable(typeof(DiscourseTopicResponse))]
[JsonSerializable(typeof(DiscoursePost))]
public partial class DiscourseJsonSerializerContext : JsonSerializerContext
{
}
