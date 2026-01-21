using Vulicy.Domain;
using Vulicy.Services;

namespace Vulicy.Web.Endpoints;

public static class DossierRecords
{
    public static void MapDossierRecords(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/dossier-records");
        group.MapGet("/search", Search);
        group.MapGet("/{id:int}/features", GetFeatures);
    }

    private static Task<List<DossierRecordSearchResult>> Search(string? query, int? skip, int? take, IDossierRecordService dossierRecordService)
    {
        return dossierRecordService.SearchByName(query, skip, take);
    }

    private static Task<List<FeatureSearchResult>> GetFeatures(int id, IFeatureService featureService)
    {
        return featureService.GetByDossierRecord(id);
    }
}
