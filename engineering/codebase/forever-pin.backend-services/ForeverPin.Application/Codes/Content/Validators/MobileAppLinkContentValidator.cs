using FluentValidation;
using ForeverPin.Application.Codes.Validators;
using ForeverPin.Domain.Codes.Content.MobileApp.Models;

namespace ForeverPin.Application.Codes.Content.Validators;

/// <summary>Validates one app-store link.</summary>
public sealed class MobileAppLinkContentValidator : AbstractValidator<MobileAppLinkContentValueObject>
{
    /// <summary>Builds the store-link rules.</summary>
    public MobileAppLinkContentValidator() =>
        RuleFor(content => content.Url)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Store URL is required.")
            .Must(CodeValidationRules.IsAbsoluteHttpUrl).WithMessage("Store URL must be an absolute http(s) URL.");
}
