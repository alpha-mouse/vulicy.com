using System.Text.Json.Serialization;

namespace Vulicy.Web.Infrastructure;

[JsonSerializable(typeof(string))]
internal partial class VulicyWebSerializerContext : JsonSerializerContext
{

}