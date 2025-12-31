using System.Text.Json.Serialization;

namespace Vulicy.Web.Infrastructure;

[JsonSerializable(typeof(Todo[]))]
internal partial class VulicyJsonSerializerContext : JsonSerializerContext
{

}