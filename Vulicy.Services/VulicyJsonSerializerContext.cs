using System.Text.Json.Serialization;
using Vulicy.Domain;

namespace Vulicy.Services;

[JsonSerializable(typeof(List<NamingCategory>))]
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(List<FeatureSearchResult>))]
[JsonSerializable(typeof(List<DossierRecordSearchResult>))]
[JsonSerializable(typeof(FeatureEditRequest))]
[JsonSerializable(typeof(MergeDossierRecordRequest))]
[JsonSerializable(typeof(EditDossierRecordRequest))]
[JsonSerializable(typeof(DossierRecordMergeSuggestion))]
[JsonSerializable(typeof(DiscourseWebhookResult))]
public partial class VulicyServicesSerializerContext : JsonSerializerContext
{

}