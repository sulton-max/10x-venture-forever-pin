namespace ForeverPin.Application.Codes.Analytics.Models;

/// <summary>Represents the scans of one UTC calendar day.</summary>
/// <param name="Date">The day.</param>
/// <param name="Scans">The scans resolved that day.</param>
public sealed record ScanDayModel(DateOnly Date, long Scans);
