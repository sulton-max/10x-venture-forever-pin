using FluentValidation;
using ForeverPin.Common.Domain.Codes.Content.Wifi.Enums;
using ForeverPin.Domain.Codes.Content.Wifi.Models;

namespace ForeverPin.Application.Codes.Content.Validators;

/// <summary>Validates Wi-Fi join credentials.</summary>
public sealed class WifiContentValidator : AbstractValidator<WifiContentValueObject>
{
    /// <summary>Builds the wifi-content rules.</summary>
    public WifiContentValidator()
    {
        RuleFor(content => content.Ssid).NotEmpty().WithMessage("Network name is required.");

        // An open network carries no password; every other scheme needs one to be joinable.
        RuleFor(content => content.Password)
            .NotEmpty().WithMessage("Password is required on a secured network.")
            .When(content => content.Encryption is not WifiEncryption.None);
    }
}
