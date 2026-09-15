using WoW.Two.Sdk.Backend.Beta.Data.Abstractions;

namespace ForeverPin.Domain.Identity.Entities;

/// <summary>Represents a registered account.</summary>
public sealed record UserEntity : IKeyedEntity<Guid>, IHasTableName, IAuditable
{
    /// <summary>Gets the storage table name.</summary>
    public static string TableName => "users";

    /// <summary>Gets or sets the primary key.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the stable Google subject identifier.</summary>
    public required string GoogleSubject { get; set; }

    /// <summary>Gets or sets the primary email address.</summary>
    public required string Email { get; set; }

    /// <summary>Gets or sets the display name.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the avatar URL, or null when absent.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the last-update timestamp.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
