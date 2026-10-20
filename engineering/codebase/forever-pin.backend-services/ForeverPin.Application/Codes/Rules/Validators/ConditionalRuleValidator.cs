using System.Globalization;
using FluentValidation;
using ForeverPin.Application.Codes.Content.Validators;
using ForeverPin.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Rules.Models;
using WoW.Two.Sdk.Beta.Be.Foundation.Results;
using WoW.Two.Sdk.Beta.Be.Web.RequestContext.Models;
using WoW.Two.Sdk.Beta.Be.Web.RequestContext.Parsers;

namespace ForeverPin.Application.Codes.Rules.Validators;

/// <summary>Validates a conditional rule and the value of its declared signal.</summary>
/// <seealso cref="CodeContentValidator"/>
public sealed class ConditionalRuleValidator : AbstractValidator<ConditionalRuleValueObject>
{
    private static readonly HashSet<string> CountryCodes = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
        .Select(culture => new RegionInfo(culture.Name).TwoLetterISORegionName)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
    private readonly AcceptLanguageParser _languages = new();

    /// <summary>Builds the conditional-rule rules.</summary>
    public ConditionalRuleValidator()
    {
        RuleFor(rule => rule.Order).GreaterThan(0);

        RuleFor(rule => rule.Condition).IsInEnum();

        RuleFor(rule => rule.ConditionValue)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must((rule, value) => ValidCondition(rule.Condition, value))
            .WithMessage("Use a declared device, ISO country, language range, or UTC HH:mm-HH:mm window.");

        RuleFor(rule => rule.Content)
            .NotNull()
            .SetValidator(new CodeContentValidator());
    }

    private bool ValidCondition(RuleConditionType condition, string value) => condition switch
    {
        RuleConditionType.Device => Enum.GetNames<DeviceType>().Contains(value, StringComparer.OrdinalIgnoreCase),
        RuleConditionType.Country => value.Length == 2 && CountryCodes.Contains(value),
        RuleConditionType.Language => value == value.Trim() && _languages.Parse(value) is Result<IReadOnlyList<LanguagePreferenceModel>>.Success
            { Value.Count: 1 } parsed && parsed.Value[0].Range != "*" && !value.Contains(';'),
        RuleConditionType.TimeOfDay => value.Split('-') is [var from, var to]
            && TimeOnly.TryParseExact(from, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start)
            && TimeOnly.TryParseExact(to, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end)
            && start != end,
        _ => false,
    };
}
