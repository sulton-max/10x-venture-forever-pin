using ForeverPin.Application.Billing.Core;
using ForeverPin.Application.Billing.Core.Models;
using ForeverPin.Application.Billing.Core.Queries;
using ForeverPin.Application.Billing.Core.Services;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Domain.Billing.Enums;
using WoW.Two.Sdk.Beta.Be.Mediator.Cqrs;
using WoW.Two.Sdk.Beta.Be.Mediator.Result;

namespace ForeverPin.Infrastructure.Billing.QueryHandlers;

/// <summary>Handles <see cref="BillingMeQuery"/>.</summary>
public sealed class BillingMeQueryHandler(
    ISubscriptionRepository subscriptions,
    ICodeRepository codes)
    : IQueryHandler<BillingMeQuery, AppResult<BillingMeResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<BillingMeResult.Success>> HandleAsync(
        BillingMeQuery request, CancellationToken ct)
    {
        var subscription = await subscriptions.GetByUserAsync(request.UserId, ct);

        // No row ⇒ Free / active (the synthesized default; no row is ever required to use the app). A lapsed
        // subscription reports its own plan and status, while its limits fall back to Free's.
        var plan = subscription?.Plan ?? Plan.Free;
        var entitled = PlanLimits.EffectivePlan(subscription);
        var status = subscription is null ? "active" : subscription.Status.ToString().ToLowerInvariant();

        var dynamicCodeCount = await codes.CountDynamicByUserAsync(request.UserId, ct);

        var dto = new BillingStatusModel
        {
            Plan = plan,
            Status = status,
            Limits = new LimitsModel
            {
                MaxDynamicCodes = PlanLimits.MaxDynamicCodesForApi(entitled),
                AnalyticsDays = PlanLimits.AnalyticsDays(entitled),
            },
            Usage = new UsageModel { DynamicCodeCount = dynamicCodeCount },
        };

        return AppResult<BillingMeResult.Success>.Ok(new BillingMeResult.Success(dto));
    }
}
