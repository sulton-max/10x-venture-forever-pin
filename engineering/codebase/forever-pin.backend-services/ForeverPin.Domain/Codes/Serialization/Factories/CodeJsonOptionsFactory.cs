using System.Text.Json;
using System.Text.Json.Serialization;
using ForeverPin.Domain.Codes.Content;
using ForeverPin.Domain.Codes.Rules.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models;
using WoW.Two.Sdk.Backend.Beta.Foundation.Serialization;

namespace ForeverPin.Domain.Codes.Serialization.Factories;

/// <summary>Creates immutable stored profiles for the product unions and legacy style tokens.</summary>
public static class CodeJsonOptionsFactory
{
    private static readonly JsonSerializerOptions Content = StoredJsonOptionsFactory.Create(CodeContentValueObject.Subtypes.ToJsonModifier());
    private static readonly JsonSerializerOptions Rules = StoredJsonOptionsFactory.Create(
        CodeRuleValueObject.Subtypes.ToJsonModifier(), CodeContentValueObject.Subtypes.ToJsonModifier());
    private static readonly JsonSerializerOptions Style = BuildStyle();

    /// <summary>Gets the stored content union profile.</summary>
    public static JsonSerializerOptions CreateContent() => Content;

    /// <summary>Gets the stored rule document profile.</summary>
    public static JsonSerializerOptions CreateRules() => Rules;

    /// <summary>Gets the stored style profile, retaining uppercase ECC tokens.</summary>
    public static JsonSerializerOptions CreateStyle() => Style;

    private static JsonSerializerOptions BuildStyle()
    {
        var options = new JsonSerializerOptions(StoredJsonOptionsFactory.Create());
        options.Converters.Insert(0, new JsonStringEnumConverter<EccLevel>());
        options.MakeReadOnly();
        return options;
    }
}
