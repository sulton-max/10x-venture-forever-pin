namespace ForeverPin.Domain.Codes.Content.Phone.Models;

/// <summary>Represents a telephone number to dial.</summary>
public sealed record PhoneContentValueObject : CodeContentValueObject
{
    /// <summary>Gets the number to dial.</summary>
    public required string Phone { get; init; }

}
