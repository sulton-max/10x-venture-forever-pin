using FluentValidation;
using ForeverPin.Domain.Codes.Content.Sms.Models;

namespace ForeverPin.Application.Codes.Content.Validators;

/// <summary>Validates a pre-filled SMS.</summary>
public sealed class SmsContentValidator : AbstractValidator<SmsContentValueObject>
{
    /// <summary>Builds the sms-content rules.</summary>
    public SmsContentValidator() =>
        RuleFor(content => content.Phone).NotEmpty().WithMessage("Phone number is required.");
}
