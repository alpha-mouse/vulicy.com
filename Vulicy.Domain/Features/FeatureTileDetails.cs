using NetTopologySuite.Geometries;

namespace Vulicy.Domain;

public record FeatureTileMinimalDetails(
    Geometry Geometry,
    string NameBeTarask,
    string NameBeNark,
    string NameRu,
    ClassificationGrade Classification,
    FeatureType Type,
    string RenamingReason,
    string HistoricNames,
    bool HistoricPossible,
    string YearNamed,
    string Comment,
    int? NamingCategoryId,
    int? DossierRecordId,
    string DossierRecordNameBeTarask
    );