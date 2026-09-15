using System.Text.Json;
using ForeverPin.Common.Domain.Serialization.Json;
using ForeverPin.Domain.Codes.Content;
using ForeverPin.Domain.Codes.Rules.Models;

namespace ForeverPin.Domain.Codes.Rules;

/// <summary>Serializes and deserializes a code's routing-rule JSON.</summary>
public static class CodeRuleJson
{
    /// <summary>Holds the serializer options for a code's rule document.</summary>
    public static readonly JsonSerializerOptions Options = JsonbOptions.Create(
        CodeRuleValueObject.Subtypes.ToJsonModifier(),
        CodeContentValueObject.Subtypes.ToJsonModifier());

    /// <summary>Serializes a code's rules to their jsonb document form.</summary>
    public static string Serialize(IReadOnlyList<CodeRuleValueObject> rules) =>
        JsonSerializer.Serialize(rules, Options);

    /// <summary>Deserializes a stored <c>rules</c> document; returns an empty list for a null/blank column.</summary>
    public static List<CodeRuleValueObject> Deserialize(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<CodeRuleValueObject>>(json, Options) ?? [];
}
