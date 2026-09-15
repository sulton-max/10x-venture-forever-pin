using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using ForeverPin.Common.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Rules.Models;

namespace ForeverPin.Application.Codes.Core.Models;

/// <summary>Represents a QR code, barcode, or short link.</summary>
public sealed record CodeDto
{
    /// <summary>Gets the unique id.</summary>
    public required Guid Id { get; init; }

    /// <summary>Gets the URL-safe short-link slug, or null for a static code.</summary>
    public string? Slug { get; init; }

    /// <summary>Gets the short URL, or null for a static code.</summary>
    public string? ShortUrl { get; init; }

    /// <summary>Gets the display name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the rendering symbology.</summary>
    public required BarcodeFormat BarcodeFormat { get; init; }

    /// <summary>Gets the content resolution mode.</summary>
    public required ContentMode Mode { get; init; }

    /// <summary>Gets the kind of content every rule of the code carries.</summary>
    public required CodeContentType ContentType { get; init; }

    /// <summary>Gets whether the code currently resolves.</summary>
    public bool IsActive { get; init; }

    /// <summary>Gets the running scan total.</summary>
    public long ScanCount { get; init; }

    /// <summary>Gets the creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Gets the collection of content-bearing routing rules.</summary>
    public IReadOnlyList<CodeRuleValueObject> Rules { get; init; } = [];

    /// <summary>Gets the persisted visual style, or the render default when none was saved.</summary>
    public required StyleSpec Style { get; init; }
}
