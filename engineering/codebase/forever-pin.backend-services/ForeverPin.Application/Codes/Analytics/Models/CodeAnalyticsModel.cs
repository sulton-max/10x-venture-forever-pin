namespace ForeverPin.Application.Codes.Analytics.Models;

/// <summary>Represents a code's scans over a window of UTC calendar days.</summary>
public sealed record CodeAnalyticsModel
{
    /// <summary>Gets the first day of the window.</summary>
    public required DateOnly From { get; init; }

    /// <summary>Gets the last day of the window, today.</summary>
    public required DateOnly To { get; init; }

    /// <summary>Gets how many days back the owner's plan lets the window reach.</summary>
    public required int MaxDays { get; init; }

    /// <summary>Gets every scan the code has resolved.</summary>
    public required long TotalScans { get; init; }

    /// <summary>Gets the scans inside the window.</summary>
    public required long RangeScans { get; init; }

    /// <summary>Gets one entry per day of the window, oldest first, zero when nothing was scanned.</summary>
    public required IReadOnlyList<ScanDayModel> Daily { get; init; }

    /// <summary>Gets the window's scans per device class, most first.</summary>
    public required IReadOnlyList<ScanShareModel> Devices { get; init; }

    /// <summary>Gets the window's scans per operating system, most first.</summary>
    public required IReadOnlyList<ScanShareModel> OperatingSystems { get; init; }

    /// <summary>Gets the window's scans per ISO country code, most first.</summary>
    public required IReadOnlyList<ScanShareModel> Countries { get; init; }
}
