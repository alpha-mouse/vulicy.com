using System.Text.Json.Serialization;
using Vulicy.Domain;

namespace Vulicy.Services;

[JsonSerializable(typeof(NamingCategoryDto))]
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(List<Vulicy.Domain.FeatureSearchResult>))]
public partial class VulicyServicesSerializerContext : JsonSerializerContext
{

}