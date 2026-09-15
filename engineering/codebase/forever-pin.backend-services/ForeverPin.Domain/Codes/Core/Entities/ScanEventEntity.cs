using ForeverPin.Domain.Codes.Core.Enums;
using WoW.Two.Sdk.Backend.Beta.Data.Abstractions;

namespace ForeverPin.Domain.Codes.Core.Entities;

/// <summary>Represents a scan or click event.</summary>
public sealed record ScanEventEntity : IKeyedEntity<Guid>, IHasTableName
{
    /// <summary>Gets the storage table name.</summary>
    public static string TableName => "scan_events";

    /// <summary>Gets or sets the primary key.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the id of the code that was scanned.</summary>
    public required Guid CodeId { get; set; }

    /// <summary>Gets or sets the moment the scan was resolved.</summary>
    public required DateTimeOffset ScannedAt { get; set; }

    /// <summary>Gets or sets the device class.</summary>
    public DeviceType Device { get; set; }

    /// <summary>Gets or sets the ISO country code, or null when unknown.</summary>
    public string? CountryCode { get; set; }

    /// <summary>Gets or sets the operating-system family, or null when unknown.</summary>
    public string? Os { get; set; }

    /// <summary>Gets or sets the HTTP referrer, or null when absent.</summary>
    public string? Referrer { get; set; }

    /// <summary>Gets or sets the hash of the scan's User-Agent.</summary>
    public string? UserAgentHash { get; set; }

    /// <summary>Gets or sets the matched routing rule's order, or null when no rule matched.</summary>
    public int? MatchedRuleOrder { get; set; }

    /// <summary>Gets or sets the resolved destination.</summary>
    public required string DestinationUrl { get; set; }
}
