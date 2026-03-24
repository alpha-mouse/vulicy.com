using FluentValidation;
using Vulicy.Domain;

namespace Vulicy.Services;

public record FeatureEditRequest(
    string NameBeTarask,
    string NameBeNark,
    string NameRu,
    ClassificationGrade Classification,
    FeatureType Type,
    string? RenamingReason,
    string? HistoricNames,
    string? Comment,
    bool HistoricPossible,
    string? YearNamed,
    int? NamingCategoryId,
    int? DossierRecordId);

public record FeatureCreateFromSourcesRequest(
    string NameBeTarask,
    string NameBeNark,
    string NameRu,
    ClassificationGrade Classification,
    FeatureType Type,
    string? RenamingReason,
    string? HistoricNames,
    string? Comment,
    bool HistoricPossible,
    string? YearNamed,
    int? NamingCategoryId,
    int? DossierRecordId,
    int OsmId,
    OsmType OsmType,
    string? CadastreId
    ) : FeatureEditRequest(NameBeTarask, NameBeNark, NameRu, Classification, Type, RenamingReason, HistoricNames, Comment, HistoricPossible, YearNamed, NamingCategoryId, DossierRecordId);

public abstract class FeatureEditRequestBaseValidator<T> : AbstractValidator<T>
    where T: FeatureEditRequest
{
    protected FeatureEditRequestBaseValidator()
    {
        RuleFor(x => x.NameBeTarask).NotEmpty().MaximumLength(128);
        RuleFor(x => x.NameBeNark).NotEmpty().MaximumLength(128);
        RuleFor(x => x.NameRu).MaximumLength(128);
        RuleFor(x => x.RenamingReason).MaximumLength(1024);
        RuleFor(x => x.HistoricNames).MaximumLength(256);
        RuleFor(x => x.Comment).MaximumLength(512);
        RuleFor(x => x.YearNamed).MaximumLength(64);
        RuleFor(x => x.Classification).IsInEnum();
        RuleFor(x => x.Type).IsInEnum();
    }
}

public class FeatureEditRequestValidator : FeatureEditRequestBaseValidator<FeatureEditRequest>;

public class FeatureCreateFromSourcesRequestValidator : FeatureEditRequestBaseValidator<FeatureCreateFromSourcesRequest>
{
    public FeatureCreateFromSourcesRequestValidator()
    {
        RuleFor(x => x.OsmId).NotEmpty();
        RuleFor(x => x.OsmType).IsInEnum();
    }
}