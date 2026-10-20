using FluentValidation;
using ForeverPin.Application.Codes.Validators;
using ForeverPin.Domain.Codes.Content.Url.Models;
using WoW.Two.Sdk.Beta.Be.Web.Redirects.Extensions;

namespace ForeverPin.Application.Codes.Content.Validators;

/// <summary>Validates URL content.</summary>
public sealed class UrlContentValidator : AbstractValidator<UrlContentValueObject>
{
    /// <summary>Builds the URL-content rules.</summary>
    public UrlContentValidator()
    {
        RuleFor(content => content.Url)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("URL is required.")
            .Must(CodeValidationRules.IsAbsoluteHttpUrl).WithMessage("URL must be an absolute http(s) URL.");

        // Advisory only: an intranet or VPN-only destination may be intended, so the save still succeeds.
        RuleFor(content => content.Url).AdviseOnRedirectTarget();
    }
}
