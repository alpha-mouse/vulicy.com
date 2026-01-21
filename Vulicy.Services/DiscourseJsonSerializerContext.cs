using System.Text.Json.Serialization;

namespace Vulicy.Services;

[JsonSerializable(typeof(DiscoursePostResponse))]
[JsonSerializable(typeof(DiscourseWebhookPayload))]
public partial class DiscourseJsonSerializerContext : JsonSerializerContext
{
}
