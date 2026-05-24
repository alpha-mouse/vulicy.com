using System.Globalization;
using CsvHelper;
using NetTopologySuite.Geometries;
using Vulicy.Domain;
using Vulicy.Services;
using Vulicy.Web.Infrastructure;

namespace Vulicy.Web.Endpoints;

public static class Features
{
    public static void MapFeatures(this IEndpointRouteBuilder builder)
    {
        var features = builder.MapGroup("/api/features");
        features.MapGet("/search", Search);
        features.MapPost("/from-sources", CreateFeatureFromSources).RequireAdmin().Validate<FeatureCreateFromSourcesRequest>();
        features.MapPut("/{id:int}", EditFeature).RequireAdmin().Validate<FeatureEditRequest>();
        features.MapPost("/{id:int}/link-osm", LinkOsmFeature).RequireAdmin().Validate<OsmId>();
        features.MapPut("/{id:int}/recompute-geometry", RecomputeGeometry).RequireAdmin();
        features.MapPost("/preview", GetFeaturePreview).RequireAdmin().Validate<GetFeaturePreviewRequest>();
        features.MapGet("/export", ExportFeatures).RequireAdmin();
        features.MapGet("/export/by-administrative/{administrativeId:int}", ExportFeaturesByAdministrative).RequireAdmin();

        var osmFeatures = builder.MapGroup("/api/osm-features").RequireAdmin();
        osmFeatures.MapGet("/search-unmatched", OsmSearch);

        var cadastreFeatures = builder.MapGroup("/api/cadastre-features").RequireAdmin();
        cadastreFeatures.MapGet("/search-unmatched", CadastreSearch);
    }

    private static Task<List<FeatureSearchResult>> Search(string query, double? lat, double? lng, IFeatureRepository featureRepository)
    {
        return featureRepository.SearchByName(query, lat, lng);
    }

    private static Task<FeatureSearchResult> CreateFeatureFromSources(FeatureCreateFromSourcesRequest feature, IFeatureService featureService, HttpContext context)
    {
        var userId = context.User.GetUserId();
        return featureService.CreateFeatureFromSources(feature, userId);
    }

    private static Task EditFeature(int id, FeatureEditRequest feature, IFeatureService featureService, HttpContext context)
    {
        var userId = context.User.GetUserId();
        return featureService.EditFeature(id, feature, userId);
    }

    private static Task<Geometry> LinkOsmFeature(int id, OsmId feature, IFeatureService featureService, HttpContext context)
    {
        var userId = context.User.GetUserId();
        return featureService.LinkOsmFeature(id, feature, userId);
    }

    private static Task RecomputeGeometry(int id, IFeatureService featureService)
    {
        return featureService.RecomputeGeometry(id);
    }

    private static Task<FeatureTileMinimalDetails> GetFeaturePreview(GetFeaturePreviewRequest request, IFeatureService featureService)
    {
        return featureService.GetFeaturePreview(request);
    }

    private static Task<List<OsmFeatureSearchResult>> OsmSearch(string query, double? lat, double? lng, IOsmFeatureRepository osmFeatureRepository)
    {
        return osmFeatureRepository.SearchUnmatched(query, lat, lng);
    }

    private static Task<List<CadastreFeatureSearchResult>> CadastreSearch(string query, double? lat, double? lng, ICadastreFeatureRepository cadastreFeatureRepository)
    {
        return cadastreFeatureRepository.SearchUnmatched(query, lat, lng);
    }

    private static async Task<IResult> ExportFeatures(IFeatureRepository featureRepository, HttpContext context)
    {
        var features = await featureRepository.GetForExport();
        return WriteCsvResponse(features, "export.csv", context);
    }

    private static async Task<IResult> ExportFeaturesByAdministrative(int administrativeId, IFeatureRepository featureRepository, HttpContext context)
    {
        var features = await featureRepository.GetForExportByAdministrative(administrativeId);
        return WriteCsvResponse(features, $"export-{administrativeId}.csv", context);
    }

    private static IResult WriteCsvResponse(List<FeatureEntity> features, string fileName, HttpContext context)
    {
        features = features
            .OrderBy(x => x.Administrative?.ParentRegion?.NameBeTarask, CultureProvider.BeByStringComparer)
            .ThenBy(x => x.Administrative?.ParentDistrict?.NameBeTarask, CultureProvider.BeByStringComparer)
            .ThenBy(x => x.Administrative?.ParentVillageCouncil?.NameBeTarask, CultureProvider.BeByStringComparer)
            .ThenBy(x => x.Administrative?.NameBeTarask, CultureProvider.BeByStringComparer)
            .ThenBy(x => x.NameBeNark, CultureProvider.BeByStringComparer)
            .ThenBy(x => x.Type)
            .ToList();

        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(true));
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        // Write header
        csv.WriteField("Id");
        csv.WriteField("Тып тапанімічнага аб'екту");
        csv.WriteField("Афіцыйная цяперашняя назва");
        csv.WriteField("Патрэба перайменаваньня");
        csv.WriteField("Абгрунтаваньне патрэбы перайменаваньня");
        csv.WriteField("Гістарычная назва");
        csv.WriteField("Тэматычная катэгорыя");
        csv.WriteField("Камэнтар");
        csv.WriteField("Патрэба вяртаньня гістарычнай назвы (падлік)");
        csv.WriteField("Год цяперашняга найменаваньня");
        csv.WriteField("Этымалёгія назвы");
        csv.WriteField("Вобласьць");
        csv.WriteField("Раён");
        csv.WriteField("Сельсавет");
        csv.WriteField("Населены пункт");
        csv.NextRecord();

        foreach (var f in features)
        {
            var classification = f.Classification != ClassificationGrade.None
                ? f.Classification
                : f.DossierRecord?.Classification ?? ClassificationGrade.None;

            csv.WriteField(f.Id);
            csv.WriteField(GetFeatureTypeName(f.Type));
            csv.WriteField(f.NameBeNark ?? f.NameRu);
            csv.WriteField(GetClassificationDescription(classification));
            csv.WriteField(f.RenamingReason);
            csv.WriteField(f.HistoricNames);
            csv.WriteField(f.NamingCategory?.Name ?? f.DossierRecord?.NamingCategory?.Name);
            csv.WriteField(f.Comment);
            csv.WriteField(f.HistoricPossible ? "1" : "");
            csv.WriteField(f.YearNamed);
            csv.WriteField(f.DossierRecord?.NameBeTarask);
            csv.WriteField(f.Administrative?.ParentRegion?.NameBeTarask);
            csv.WriteField(f.Administrative?.ParentDistrict?.NameBeTarask);
            csv.WriteField(f.Administrative?.ParentVillageCouncil?.NameBeTarask);
            csv.WriteField(f.Administrative?.NameBeTarask);
            csv.NextRecord();
        }

        writer.Flush();
        var bytes = memoryStream.ToArray();
        return Results.File(bytes, "text/csv", fileName);
    }

    private static string GetFeatureTypeName(FeatureType type) => type switch
    {
        FeatureType.Street => "вуліца",
        FeatureType.Avenue => "праспэкт",
        FeatureType.Square => "плошча",
        FeatureType.Boulevard => "бульвар",
        FeatureType.HighRoad => "тракт",
        FeatureType.Riverside => "набярэжная",
        FeatureType.Highway => "шаша",
        FeatureType.Roundabout => "кальцо",
        FeatureType.Alley => "завулак",
        FeatureType.Driveway => "праезд",
        FeatureType.DeadEnd => "тупік",
        FeatureType.Descent => "спуск",
        FeatureType.Entryway => "заезд",
        FeatureType.Park => "парк",
        FeatureType.PublicGarden => "сквэр",
        _ => ""
    };

    private static string GetClassificationDescription(ClassificationGrade grade) => grade switch
    {
        ClassificationGrade.Priority => "1. Перайменаваньне неабходнае ў прыярытэтным парадку",
        ClassificationGrade.Required => "2. Перайменаваньне неабходнае",
        ClassificationGrade.Suggested => "3. Перайменаваньне пажаданае",
        ClassificationGrade.Possible => "4. Перайменаваньне магчымае",
        ClassificationGrade.Unneeded => "5. Перайменаваньне не патрэбнае",
        _ => ""
    };
}
