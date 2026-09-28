namespace ForeverPin.Tests.E2E.Support;

/// <summary>Represents the wire shape of <c>CodeAnalyticsDto</c>.</summary>
public sealed record CodeAnalyticsDtoModel
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public long TotalScans { get; init; }
    public long RangeScans { get; init; }
    public IReadOnlyList<ScanDayDtoModel> Daily { get; init; } = [];
    public IReadOnlyList<ScanShareDtoModel> Devices { get; init; } = [];
    public IReadOnlyList<ScanShareDtoModel> OperatingSystems { get; init; } = [];
    public IReadOnlyList<ScanShareDtoModel> Countries { get; init; } = [];
}

/// <summary>Represents the wire shape of one analytics day.</summary>
public sealed record ScanDayDtoModel(DateOnly Date, long Scans);

/// <summary>Represents the wire shape of one attribute share; a null key is an unknown value.</summary>
public sealed record ScanShareDtoModel(string? Key, long Scans);
