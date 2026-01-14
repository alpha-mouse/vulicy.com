using System.Text.Json.Serialization;

namespace Vulicy.Services;

[JsonSerializable(typeof(NamingCategoryDto))]
public partial class VulicyServicesSerializerContext : JsonSerializerContext
{

}