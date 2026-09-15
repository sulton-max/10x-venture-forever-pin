using ForeverPin.Common.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Rules.Models;
using WoW.Two.Sdk.Backend.Beta.Data.Abstractions;

namespace ForeverPin.Domain.Codes.Core.Entities;

/// <summary>Represents a QR code, barcode, or short link.</summary>
public sealed record CodeEntity : IKeyedEntity<Guid>, IHasTableName, IAuditable
{
    /// <summary>Gets the storage table name.</summary>
    public static string TableName => "codes";

    /// <summary>Gets or sets the primary key.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the URL-safe short-link slug, or null for a static code.</summary>
    public string? Slug { get; set; }

    // Not an FK: a guest owner has no users row.
    /// <summary>Gets or sets the id of the user who owns this code.</summary>
    public required Guid UserId { get; set; }

    /// <summary>Gets or sets the display name.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the rendering symbology.</summary>
    public BarcodeFormat BarcodeFormat { get; set; }

    /// <summary>Gets or sets whether the code resolves.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets or sets the JSON style descriptor.</summary>
    /// <remarks>Store <c>"{}"</c> when unstyled.</remarks>
    public required string StyleJson { get; set; }

    /// <summary>Gets or sets the content resolution mode.</summary>
    /// <remarks>Set the mode at create; never change it.</remarks>
    public ContentMode Mode { get; set; }

    /// <summary>Gets or sets the kind of content every rule of this code carries.</summary>
    /// <remarks>Give every rule of the code this same content type.</remarks>
    public CodeContentType ContentType { get; set; }

    /// <summary>Gets or sets the running scan total.</summary>
    public long ScanCount { get; set; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the last-update timestamp.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Gets or sets the collection of content-bearing routing rules.</summary>
    /// <remarks>Give the code at most one default rule.</remarks>
    public List<CodeRuleValueObject> Rules { get; set; } = [];
}
