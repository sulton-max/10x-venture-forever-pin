using ForeverPin.Application.Codes.Core.Models;
using FluentValidation;
using ForeverPin.Application.Codes.Core.Commands;
using ForeverPin.Application.Codes.Rules.Models;
using ForeverPin.Application.Codes.Rules.Validators;

namespace ForeverPin.Application.Codes.Core.Validators;

// This validator runs only after ownership and the persisted mode have been resolved.
/// <summary>Validates update-code input.</summary>
/// <seealso cref="CodeRuleValidator"/>
/// <seealso cref="CodeRuleSetModelValidator"/>
public sealed class CodeUpdateModelValidator : AbstractValidator<CodeUpdateModel>
{
    /// <summary>Builds the update-code rules.</summary>
    public CodeUpdateModelValidator()
    {
        RuleFor(model => model.Command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must be 200 characters or fewer.")
            .OverridePropertyName(nameof(CodeUpdateCommand.Name));

        RuleForEach(model => model.Command.Rules).NotNull().SetValidator(new CodeRuleValidator())
            .OverridePropertyName(nameof(CodeUpdateCommand.Rules));

        RuleFor(model => new CodeRuleSetModel(model.Mode, model.Command.ContentType, model.Command.Rules))
            .SetValidator(new CodeRuleSetModelValidator());

        RuleFor(model => model.Command.BarcodeFormat).IsInEnum().OverridePropertyName(nameof(CodeUpdateCommand.BarcodeFormat));

        RuleFor(model => model.Command.Style).NotNull().SetValidator(new WoW.Two.Sdk.Backend.Beta.Codes.Validators.StyleSpecValidator())
            .OverridePropertyName(nameof(CodeUpdateCommand.Style));
    }
}
