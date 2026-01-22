using System.Text.Json.Serialization;
using Vulicy.Domain;

namespace Vulicy.Services;

[JsonSerializable(typeof(List<NamingCategoryDto>))]
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(List<FeatureSearchResult>))]
[JsonSerializable(typeof(List<DossierRecordSearchResult>))]
[JsonSerializable(typeof(FeatureEditRequest))]
public partial class VulicyServicesSerializerContext : JsonSerializerContext
{

}