using FluentValidation;
using ForeverPin.Application.Codes.Content.Validators;
using ForeverPin.Domain.Codes.Rules.Models;

namespace ForeverPin.Application.Codes.Rules.Validators;

/// <summary>Validates a conditional rule.</summary>
/// <seealso cref="CodeContentValidator"/>
public sealed class ConditionalRuleValidator : AbstractValidator<ConditionalRuleValueObject>
{
    /// <summary>Builds the conditional-rule rules.</summary>
    public ConditionalRuleValidator()
    {
        RuleFor(rule => rule.Order)
            .GreaterThan(0).WithMessage("Rule order must be greater than zero.");

        RuleFor(rule => rule.ConditionValue)
            .NotEmpty().WithMessage("Rule condition value is required.");

        RuleFor(rule => rule.Content)
            .SetValidator(new CodeContentValidator());
    }
}
