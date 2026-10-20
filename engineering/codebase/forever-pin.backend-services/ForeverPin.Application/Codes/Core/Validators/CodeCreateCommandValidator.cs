using FluentValidation;
using ForeverPin.Application.Codes.Core.Commands;
using ForeverPin.Application.Codes.Rules.Models;
using ForeverPin.Application.Codes.Rules.Validators;

namespace ForeverPin.Application.Codes.Core.Validators;

/// <summary>Validates create-code input.</summary>
/// <seealso cref="CodeRuleValidator"/>
/// <seealso cref="CodeRuleSetModelValidator"/>
public sealed class CodeCreateCommandValidator : AbstractValidator<CodeCreateCommand>
{
    /// <summary>Builds the create-code rules.</summary>
    public CodeCreateCommandValidator()
    {
        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must be 200 characters or fewer.");

        RuleForEach(command => command.Rules).NotNull().SetValidator(new CodeRuleValidator());

        RuleFor(command => new CodeRuleSetModel(command.Mode, command.ContentType, command.Rules))
            .SetValidator(new CodeRuleSetModelValidator());

        RuleFor(command => command.BarcodeFormat).IsInEnum();

        RuleFor(command => command.Style).NotNull().SetValidator(new WoW.Two.Sdk.Beta.Be.Codes.Validators.StyleSpecValidator());
    }
}
