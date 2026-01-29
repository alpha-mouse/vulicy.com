using System.Text.Json.Serialization;
using Vulicy.Web.Endpoints;

namespace Vulicy.Web.Infrastructure;

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(FrontConfig))]
[JsonSerializable(typeof(CreateTopicRequest))]
[JsonSerializable(typeof(CreateTopicResponse))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(IdResponse))]
internal partial class VulicyWebSerializerContext : JsonSerializerContext
{
}
