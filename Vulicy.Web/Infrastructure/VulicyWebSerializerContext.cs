using System.Text.Json.Serialization;
using Vulicy.Web.Endpoints;

namespace Vulicy.Web.Infrastructure;

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(FrontConfig))]
[JsonSerializable(typeof(CreateTopicRequest))]
[JsonSerializable(typeof(CreateTopicResponse))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(IdResponse))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
internal partial class VulicyWebSerializerContext : JsonSerializerContext
{
}
