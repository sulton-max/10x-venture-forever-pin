using FluentValidation;
using ForeverPin.Application.Codes.Core.Commands;
using ForeverPin.Application.Codes.Rules.Models;
using ForeverPin.Application.Codes.Rules.Validators;

namespace ForeverPin.Application.Codes.Core.Validators;

// Update has no mode field, so it cannot reuse the create validator.
/// <summary>Validates update-code input.</summary>
/// <seealso cref="CodeRuleValidator"/>
/// <seealso cref="CodeRuleSetValidator"/>
public sealed class CodeUpdateCommandValidator : AbstractValidator<CodeUpdateCommand>
{
    /// <summary>Builds the update-code rules.</summary>
    public CodeUpdateCommandValidator()
    {
        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must be 200 characters or fewer.");

        RuleForEach(command => command.Rules).SetValidator(new CodeRuleValidator());

        RuleFor(command => new CodeRuleSet(null, command.ContentType, command.Rules))
            .SetValidator(new CodeRuleSetValidator());
    }
}
