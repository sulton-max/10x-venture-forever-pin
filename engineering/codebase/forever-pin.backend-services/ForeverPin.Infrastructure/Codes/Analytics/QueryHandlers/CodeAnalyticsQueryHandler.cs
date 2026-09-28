using ForeverPin.Application.Codes.Analytics.Models;
using ForeverPin.Application.Codes.Analytics.Queries;
using ForeverPin.Application.Codes.Analytics.Services;
using ForeverPin.Application.Codes.Core.Services;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace ForeverPin.Infrastructure.Codes.Analytics.QueryHandlers;

/// <summary>Handles <see cref="CodeAnalyticsQuery"/>.</summary>
public sealed class CodeAnalyticsQueryHandler(
    ICodeRepository codes,
    IScanAnalyticsRepository scans,
    TimeProvider clock)
    : IQueryHandler<CodeAnalyticsQuery, AppResult<CodeAnalyticsResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeAnalyticsResult.Success>> HandleAsync(
        CodeAnalyticsQuery request, CancellationToken ct)
    {
        var code = await codes.GetByIdForUserAsync(request.Id, request.UserId, ct);
        if (code is null)
            return AppResult<CodeAnalyticsResult.Success>.Fail(AppError.Of(AppErrorType.NotFound, "Code not found"));

        // The window ends with today; per-code time zones do not exist yet, so days are UTC.
        var to = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        var from = to.AddDays(1 - request.Days);
        var tally = await scans.TallyAsync(code.Id, Midnight(from), Midnight(to.AddDays(1)), ct);

        var scansByDay = tally.Days.ToDictionary(day => day.Date, day => day.Scans);
        var daily = Enumerable.Range(0, request.Days)
            .Select(offset => from.AddDays(offset))
            .Select(date => new ScanDayModel(date, scansByDay.GetValueOrDefault(date)))
            .ToArray();

        return AppResult<CodeAnalyticsResult.Success>.Ok(new CodeAnalyticsResult.Success(new CodeAnalyticsModel
        {
            From = from,
            To = to,
            TotalScans = code.ScanCount,
            RangeScans = daily.Sum(day => day.Scans),
            Daily = daily,
            Devices = tally.Devices,
            OperatingSystems = tally.OperatingSystems,
            Countries = tally.Countries,
        }));
    }

    private static DateTimeOffset Midnight(DateOnly date) => new(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
}
