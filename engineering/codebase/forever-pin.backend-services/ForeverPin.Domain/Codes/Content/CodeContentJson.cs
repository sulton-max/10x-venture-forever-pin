using System.Text.Json;
using ForeverPin.Common.Domain.Serialization.Json;

namespace ForeverPin.Domain.Codes.Content;

/// <summary>Serializes and deserializes content JSON.</summary>
public static class CodeContentJson
{
    /// <summary>Gets the JSON options for content subtype binding.</summary>
    public static readonly JsonSerializerOptions Options =
        JsonbOptions.Create(CodeContentValueObject.Subtypes.ToJsonModifier());

    /// <summary>Serializes content to its jsonb string form, emitting the <c>type</c> discriminator.</summary>
    public static string Serialize(CodeContentValueObject content) => JsonSerializer.Serialize(content, Options);

    /// <summary>Deserializes content JSON, returning null for null or blank input.</summary>
    public static CodeContentValueObject? Deserialize(string? json) =>
        string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<CodeContentValueObject>(json, Options);
}
