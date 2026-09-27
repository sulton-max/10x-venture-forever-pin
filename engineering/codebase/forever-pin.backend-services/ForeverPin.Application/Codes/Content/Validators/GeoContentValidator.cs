using FluentValidation;
using ForeverPin.Domain.Codes.Content.Geo.Models;

namespace ForeverPin.Application.Codes.Content.Validators;

/// <summary>Validates a geographic location.</summary>
public sealed class GeoContentValidator : AbstractValidator<GeoContentValueObject>
{
    /// <summary>Builds the geo-content rules.</summary>
    public GeoContentValidator()
    {
        RuleFor(content => content.Latitude)
            .Must(double.IsFinite)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");

        RuleFor(content => content.Longitude)
            .Must(double.IsFinite)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
    }
}
