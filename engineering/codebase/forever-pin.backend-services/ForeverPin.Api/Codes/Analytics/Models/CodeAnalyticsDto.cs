using ForeverPin.Application.Codes.Analytics.Models;

namespace ForeverPin.Api.Codes.Analytics.Models;

/// <summary>Represents a code's scans over a window of UTC calendar days.</summary>
public sealed record CodeAnalyticsDto
{
    /// <summary>Gets the first day of the window.</summary>
    public required DateOnly From { get; init; }

    /// <summary>Gets the last day of the window, today.</summary>
    public required DateOnly To { get; init; }

    /// <summary>Gets every scan the code has resolved.</summary>
    public required long TotalScans { get; init; }

    /// <summary>Gets the scans inside the window.</summary>
    public required long RangeScans { get; init; }

    /// <summary>Gets one entry per day of the window, oldest first.</summary>
    public required IReadOnlyList<ScanDayDto> Daily { get; init; }

    /// <summary>Gets the window's scans per device class, most first.</summary>
    public required IReadOnlyList<ScanShareDto> Devices { get; init; }

    /// <summary>Gets the window's scans per operating system, most first.</summary>
    public required IReadOnlyList<ScanShareDto> OperatingSystems { get; init; }

    /// <summary>Gets the window's scans per ISO country code, most first.</summary>
    public required IReadOnlyList<ScanShareDto> Countries { get; init; }
}

/// <summary>Represents the scans of one UTC calendar day.</summary>
/// <param name="Date">The day.</param>
/// <param name="Scans">The scans resolved that day.</param>
public sealed record ScanDayDto(DateOnly Date, long Scans);

/// <summary>Represents the scans that share one value of a scan attribute.</summary>
/// <param name="Key">The attribute value, or null when it was unknown.</param>
/// <param name="Scans">The scans carrying that value.</param>
public sealed record ScanShareDto(string? Key, long Scans);

/// <summary>Maps the application analytics into their HTTP projection.</summary>
public static class CodeAnalyticsDtoExtensions
{
    /// <summary>Maps every window aggregate without changing its order.</summary>
    public static CodeAnalyticsDto ToDto(this CodeAnalyticsModel model) => new()
    {
        From = model.From,
        To = model.To,
        TotalScans = model.TotalScans,
        RangeScans = model.RangeScans,
        Daily = [.. model.Daily.Select(day => new ScanDayDto(day.Date, day.Scans))],
        Devices = [.. model.Devices.Select(ToDto)],
        OperatingSystems = [.. model.OperatingSystems.Select(ToDto)],
        Countries = [.. model.Countries.Select(ToDto)],
    };

    private static ScanShareDto ToDto(ScanShareModel share) => new(share.Key, share.Scans);
}
