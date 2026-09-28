namespace ForeverPin.Application.Codes.Analytics.Models;

/// <summary>Represents recorded scans grouped by day and by attribute.</summary>
public sealed record ScanTallyModel
{
    /// <summary>Gets the days that have scans, oldest first.</summary>
    public required IReadOnlyList<ScanDayModel> Days { get; init; }

    /// <summary>Gets the scans per device class, most first.</summary>
    public required IReadOnlyList<ScanShareModel> Devices { get; init; }

    /// <summary>Gets the scans per operating system, most first.</summary>
    public required IReadOnlyList<ScanShareModel> OperatingSystems { get; init; }

    /// <summary>Gets the scans per ISO country code, most first.</summary>
    public required IReadOnlyList<ScanShareModel> Countries { get; init; }
}
