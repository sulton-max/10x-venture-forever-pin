using FluentValidation;
using ForeverPin.Application.Codes.Validators;
using ForeverPin.Domain.Codes.Content.Url.Models;

namespace ForeverPin.Application.Codes.Content.Validators;

/// <summary>Validates URL content.</summary>
public sealed class UrlContentValidator : AbstractValidator<UrlContentValueObject>
{
    /// <summary>Builds the URL-content rules.</summary>
    public UrlContentValidator() =>
        RuleFor(content => content.Url)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("URL is required.")
            .Must(CodeValidationRules.IsAbsoluteHttpUrl).WithMessage("URL must be an absolute http(s) URL.");
}
