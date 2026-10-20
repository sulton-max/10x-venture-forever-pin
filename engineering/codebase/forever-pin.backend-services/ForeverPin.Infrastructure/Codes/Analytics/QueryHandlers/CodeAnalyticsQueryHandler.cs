using ForeverPin.Application.Billing.Core;
using ForeverPin.Application.Billing.Core.Services;
using ForeverPin.Application.Codes.Analytics.Models;
using ForeverPin.Application.Codes.Analytics.Queries;
using ForeverPin.Application.Codes.Analytics.Services;
using ForeverPin.Application.Codes.Core.Services;
using WoW.Two.Sdk.Beta.Be.Foundation.Errors;
using WoW.Two.Sdk.Beta.Be.Mediator.Cqrs;
using WoW.Two.Sdk.Beta.Be.Mediator.Result;

namespace ForeverPin.Infrastructure.Codes.Analytics.QueryHandlers;

/// <summary>Handles <see cref="CodeAnalyticsQuery"/>.</summary>
public sealed class CodeAnalyticsQueryHandler(
    ICodeRepository codes,
    IScanAnalyticsRepository scans,
    ISubscriptionRepository subscriptions,
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

        // A window longer than the plan reaches is cut to it; the answer names the reach so the page can say so.
        var maxDays = PlanLimits.AnalyticsDays(PlanLimits.EffectivePlan(await subscriptions.GetByUserAsync(request.UserId, ct)));
        var days = Math.Min(request.Days, maxDays);

        // The window ends with today; per-code time zones do not exist yet, so days are UTC.
        var to = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        var from = to.AddDays(1 - days);
        var tally = await scans.TallyAsync(code.Id, Midnight(from), Midnight(to.AddDays(1)), ct);

        var scansByDay = tally.Days.ToDictionary(day => day.Date, day => day.Scans);
        var daily = Enumerable.Range(0, days)
            .Select(offset => from.AddDays(offset))
            .Select(date => new ScanDayModel(date, scansByDay.GetValueOrDefault(date)))
            .ToArray();

        return AppResult<CodeAnalyticsResult.Success>.Ok(new CodeAnalyticsResult.Success(new CodeAnalyticsModel
        {
            From = from,
            To = to,
            MaxDays = maxDays,
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
