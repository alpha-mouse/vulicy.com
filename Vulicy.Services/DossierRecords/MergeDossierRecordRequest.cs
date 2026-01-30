using FluentValidation;
using Vulicy.Domain;

namespace Vulicy.Services;

public class MergeDossierRecordRequest
{
    public int OtherId { get; set; }
    public string NameBeTarask { get; set; } = null!;
    public string NameBeNark { get; set; } = null!;
    public string? NameRu { get; set; }
    public string? DescriptionBe { get; set; }
    public string? DescriptionRu { get; set; }
    public ClassificationGrade Classification { get; set; }
    public int? NamingCategoryId { get; set; }
}

public class MergeDossierRecordRequestValidator : AbstractValidator<MergeDossierRecordRequest>
{
    public MergeDossierRecordRequestValidator()
    {
        RuleFor(x => x.NameBeTarask).NotEmpty();
        RuleFor(x => x.NameBeNark).NotEmpty();
        RuleFor(x => x.OtherId).NotEmpty();
        RuleFor(x => x.Classification).IsInEnum();
    }
}