namespace ForeverPin.Domain.Codes.Content.Text.Models;

/// <summary>Represents free-form text.</summary>
public sealed record TextContentValueObject : CodeContentValueObject
{
    /// <summary>Gets the free-form text.</summary>
    public required string Text { get; init; }

}
