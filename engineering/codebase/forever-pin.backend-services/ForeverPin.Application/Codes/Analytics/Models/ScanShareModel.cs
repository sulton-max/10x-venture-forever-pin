namespace ForeverPin.Application.Codes.Analytics.Models;

/// <summary>Represents the scans that share one value of a scan attribute.</summary>
/// <param name="Key">The attribute value, or null when it was unknown.</param>
/// <param name="Scans">The scans carrying that value.</param>
public sealed record ScanShareModel(string? Key, long Scans);
