using Vulicy.Domain;
using Vulicy.Services;

namespace Vulicy.Web.Endpoints;

public static class DossierRecords
{
    public static void MapDossierRecords(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/dossier-records");
        group.MapGet("/search", Search);
    }

    private static Task<List<DossierRecordSearchResult>> Search(string? query, int? skip, int? take, IDossierRecordService dossierRecordService)
    {
        return dossierRecordService.SearchByName(query, skip, take);
    }
}
