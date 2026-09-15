using FluentValidation;
using ForeverPin.Application.Codes.Content.Validators;
using ForeverPin.Domain.Codes.Rules.Models;

namespace ForeverPin.Application.Codes.Rules.Validators;

/// <summary>Validates the catch-all rule.</summary>
/// <seealso cref="CodeContentValidator"/>
public sealed class DefaultRuleValidator : AbstractValidator<DefaultRuleValueObject>
{
    /// <summary>Builds the catch-all rule's rules.</summary>
    public DefaultRuleValidator() =>
        RuleFor(rule => rule.Content)
            .SetValidator(new CodeContentValidator());
}
