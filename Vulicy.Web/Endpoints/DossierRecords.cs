using Vulicy.Domain;
using Vulicy.Services;
using Vulicy.Web.Infrastructure;

namespace Vulicy.Web.Endpoints;

public static class DossierRecords
{
    public static void MapDossierRecords(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/dossier-records");
        group.MapGet("/search", Search);
        group.MapGet("/{id:int}/features", GetFeatures);
        group.MapPost("", CreateRecord).RequireAuthorization().Validate<EditDossierRecordRequest>();
        group.MapPut("/{id:int}", EditRecord).RequireAuthorization().Validate<EditDossierRecordRequest>();
        group.MapPut("/{id:int}/merge-other", MergeOtherRecord).RequireAuthorization().Validate<MergeDossierRecordRequest>();
        group.MapDelete("/{id:int}", DeleteRecord).RequireAuthorization();
        group.MapGet("/merge-suggestions/next", GetNextMergeSuggestion);
        group.MapDelete("/merge-suggestions/{id:int}", IgnoreMergeSuggestion).RequireAuthorization();
    }

    private static Task<List<DossierRecordSearchResult>> Search(string? query, int? skip, int? take, IDossierRecordService dossierRecordService)
    {
        return dossierRecordService.SearchByName(query, skip, take);
    }

    private static Task<List<FeatureSearchResult>> GetFeatures(int id, IFeatureService featureService)
    {
        return featureService.GetByDossierRecord(id);
    }

    private static async Task<IResult> CreateRecord(EditDossierRecordRequest request, IDossierRecordService dossierRecordService, HttpContext context)
    {
        var userId = context.User.GetUserId();
        var newId = await dossierRecordService.CreateRecord(request, userId);
        return Results.Created($"/api/dossier-records/{newId}", new IdResponse(newId));
    }

    private static Task EditRecord(int id, EditDossierRecordRequest request, IDossierRecordService dossierRecordService, HttpContext context)
    {
        var userId = context.User.GetUserId();
        return dossierRecordService.EditRecord(id, request, userId);
    }

    private static Task MergeOtherRecord(int id, MergeDossierRecordRequest request, IDossierRecordService dossierRecordService, HttpContext context)
    {
        var userId = context.User.GetUserId();
        return dossierRecordService.MergeDossierRecord(id, request, userId);
    }

    private static Task DeleteRecord(int id, IDossierRecordService dossierRecordService, HttpContext context)
    {
        var userId = context.User.GetUserId();
        return dossierRecordService.DeleteDossierRecord(id, userId);
    }

    private static Task<DossierRecordMergeSuggestion?> GetNextMergeSuggestion(IDossierMergesService dossierMergesService)
    {
        return dossierMergesService.GetNextMergeSuggestion();
    }

    private static Task IgnoreMergeSuggestion(int id, IDossierMergesService dossierMergesService)
    {
        return dossierMergesService.IgnoreMergeSuggestion(id);
    }
}
