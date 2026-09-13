using FluentValidation;
using ForeverPin.Application.Billing.Core.Commands;

namespace ForeverPin.Application.Billing.Core.Validators;

/// <summary>Validates checkout input.</summary>
public sealed class BillingCheckoutCommandValidator : AbstractValidator<BillingCheckoutCommand>
{
    /// <summary>Builds the checkout rules.</summary>
    public BillingCheckoutCommandValidator()
    {
        RuleFor(c => c.Plan)
            .IsInEnum().WithMessage("Plan must be a known plan.");
    }
}
