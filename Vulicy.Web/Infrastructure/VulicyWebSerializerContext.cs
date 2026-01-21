using System.Text.Json.Serialization;
using Vulicy.Web.Endpoints;

namespace Vulicy.Web.Infrastructure;

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(CreateTopicRequest))]
[JsonSerializable(typeof(CreateTopicResponse))]
internal partial class VulicyWebSerializerContext : JsonSerializerContext
{
}
