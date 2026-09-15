using ForeverPin.Domain.Codes.Core.Enums;

namespace ForeverPin.Redirect.Api.Application.Analytics.Models;

/// <summary>Represents a single scan to record.</summary>
public sealed record ScanRecord
{
    /// <summary>Gets the id of the code that was scanned.</summary>
    public required Guid CodeId { get; init; }

    /// <summary>Gets the moment the scan was resolved.</summary>
    public required DateTimeOffset ScannedAt { get; init; }

    /// <summary>Gets the device class.</summary>
    public DeviceType Device { get; init; }

    /// <summary>Gets the ISO country code, or null when unknown.</summary>
    public string? CountryCode { get; init; }

    /// <summary>Gets the operating-system family, or null when unknown.</summary>
    public string? Os { get; init; }

    /// <summary>Gets the HTTP referrer, or null when absent.</summary>
    public string? Referrer { get; init; }

    /// <summary>Gets the hash of the scan's User-Agent.</summary>
    public string? UserAgentHash { get; init; }

    /// <summary>Gets the matched routing rule's order, or null when no rule matched.</summary>
    public int? MatchedRuleOrder { get; init; }

    /// <summary>Gets the resolved destination.</summary>
    public required string DestinationUrl { get; init; }
}
