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


public class FeatureEditRequestValidator : AbstractValidator<FeatureEditRequest>
{
    public FeatureEditRequestValidator()
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