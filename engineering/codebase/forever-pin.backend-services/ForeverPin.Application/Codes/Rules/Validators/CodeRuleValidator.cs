using FluentValidation;
using ForeverPin.Domain.Codes.Rules.Models;

namespace ForeverPin.Application.Codes.Rules.Validators;

// Register each rule role with its validator.
/// <summary>Validates a routing rule.</summary>
public sealed class CodeRuleValidator : AbstractValidator<CodeRuleValueObject>
{
    /// <summary>Builds the per-role rule dispatch.</summary>
    public CodeRuleValidator() =>
        RuleFor(rule => rule).SetInheritanceValidator(v =>
        {
            v.Add(new ConditionalRuleValidator());
            v.Add(new DefaultRuleValidator());
            v.Add(new DefaultPointerRuleValidator());
        });
}
