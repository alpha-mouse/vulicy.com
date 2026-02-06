using FluentValidation;
using Vulicy.Domain;

namespace Vulicy.Services;

public record OsmId(long Id, OsmType Type);

public class OsmIdRequestValidator : AbstractValidator<OsmId>
{
    public OsmIdRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
    }
}