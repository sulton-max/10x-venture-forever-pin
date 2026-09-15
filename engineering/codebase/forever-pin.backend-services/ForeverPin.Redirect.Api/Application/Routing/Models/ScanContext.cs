using ForeverPin.Domain.Codes.Core.Enums;

namespace ForeverPin.Redirect.Api.Application.Routing.Models;

/// <summary>Represents the resolved context of a single scan.</summary>
public sealed record ScanContext
{
    /// <summary>Gets the scanned slug.</summary>
    public required string Slug { get; init; }

    /// <summary>Gets the device class.</summary>
    public required DeviceType Device { get; init; }

    /// <summary>Gets the ISO country code, or null when unknown.</summary>
    public string? CountryCode { get; init; }

    /// <summary>Gets the primary language tag from Accept-Language (e.g. <c>ru</c>).</summary>
    public string? Language { get; init; }

    /// <summary>Gets the moment the scan was resolved.</summary>
    public required DateTimeOffset NowUtc { get; init; }

    /// <summary>Gets the HTTP referrer, or null when absent.</summary>
    public string? Referrer { get; init; }

    /// <summary>Gets the raw User-Agent.</summary>
    public string? UserAgent { get; init; }

    /// <summary>Gets the caller's IP address.</summary>
    public string? IpAddress { get; init; }
}
