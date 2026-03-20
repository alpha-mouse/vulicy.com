using System.Text.Json.Serialization;
using Vulicy.Domain;

namespace Vulicy.Services;

[JsonSerializable(typeof(UserDto))]

[JsonSerializable(typeof(List<NamingCategory>))]

[JsonSerializable(typeof(List<FeatureSearchResult>))]
[JsonSerializable(typeof(List<OsmFeatureSearchResult>))]
[JsonSerializable(typeof(List<CadastreFeatureSearchResult>))]

[JsonSerializable(typeof(OsmId))]

[JsonSerializable(typeof(GetFeaturePreviewRequest))]
[JsonSerializable(typeof(FeatureCreateFromSourcesRequest))]
[JsonSerializable(typeof(FeatureEditRequest))]
[JsonSerializable(typeof(FeatureTileMinimalDetails))]

[JsonSerializable(typeof(List<DossierRecordSearchResult>))]
[JsonSerializable(typeof(EditDossierRecordRequest))]
[JsonSerializable(typeof(DossierRecordMergeSuggestion))]
[JsonSerializable(typeof(MergeDossierRecordRequest))]

[JsonSerializable(typeof(List<Administrative>))]
public partial class VulicyServicesSerializerContext : JsonSerializerContext
{
}