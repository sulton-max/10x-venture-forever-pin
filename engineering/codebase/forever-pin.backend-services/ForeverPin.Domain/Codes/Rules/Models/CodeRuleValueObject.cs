using ForeverPin.Common.Domain.Serialization;
using ForeverPin.Domain.Codes.Rules.Enums;

namespace ForeverPin.Domain.Codes.Rules.Models;

/// <summary>Represents one routing rule of a code.</summary>
/// <remarks>Add a default rule, or an unmatched scan does not resolve.</remarks>
public abstract record CodeRuleValueObject
{
    /// <summary>Gets the closed set of rule variants, each bound to its wire discriminator.</summary>
    public static readonly SubtypeRegistry<CodeRuleValueObject, CodeRuleType> Subtypes = new(
        (CodeRuleType.Conditional, typeof(ConditionalRuleValueObject)),
        (CodeRuleType.Default, typeof(DefaultRuleValueObject)),
        (CodeRuleType.DefaultPointer, typeof(DefaultPointerRuleValueObject)));
}
