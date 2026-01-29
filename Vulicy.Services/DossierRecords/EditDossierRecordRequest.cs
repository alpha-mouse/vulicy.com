using FluentValidation;
using Vulicy.Domain;

namespace Vulicy.Services;

public class EditDossierRecordRequest
{
    public string NameBeTarask { get; set; } = null!;
    public string NameBeNark { get; set; } = null!;
    public string? NameRu { get; set; }
    public string? DescriptionBe { get; set; }
    public string? DescriptionRu { get; set; }
    public ClassificationGrade Classification { get; set; }
}

public class EditDossierRecordRequestValidator : AbstractValidator<EditDossierRecordRequest>
{
    public EditDossierRecordRequestValidator()
    {
        RuleFor(x => x.NameBeTarask).NotEmpty();
        RuleFor(x => x.NameBeNark).NotEmpty();
        RuleFor(x => x.Classification).IsInEnum();
    }
}