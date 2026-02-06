using FluentValidation;
using Vulicy.Domain;

namespace Vulicy.Services;

public record GetFeaturePreviewRequest(long OsmId, OsmType OsmType, string CadastreId);

public class GetFeaturePreviewRequestValidator : AbstractValidator<GetFeaturePreviewRequest>
{
    public GetFeaturePreviewRequestValidator()
    {
        RuleFor(x => x.OsmId).NotEmpty();
        RuleFor(x => x.OsmType).IsInEnum();
        RuleFor(x => x.CadastreId).NotEmpty();
    }
}