using ForeverPin.Application.Codes.Analytics.Models;

namespace ForeverPin.Application.Codes.Analytics.Services;

/// <summary>Defines aggregate reads over a code's recorded scans.</summary>
public interface IScanAnalyticsRepository
{
    /// <summary>Groups the code's scans resolved from <paramref name="since"/> until before <paramref name="until"/>.</summary>
    /// <remarks>Days are UTC calendar days.</remarks>
    Task<ScanTallyModel> TallyAsync(Guid codeId, DateTimeOffset since, DateTimeOffset until, CancellationToken ct);
}
