using ForeverPin.Application.Billing.Core;
using ForeverPin.Application.Billing.Core.Models;
using ForeverPin.Application.Billing.Core.Queries;
using ForeverPin.Application.Billing.Core.Services;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Domain.Billing.Enums;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

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

        // No row ⇒ Free / active (the synthesized default; no row is ever required to use the app).
        var plan = subscription?.Plan ?? Plan.Free;
        var status = subscription is null ? "active" : subscription.Status.ToString().ToLowerInvariant();

        var codeCount = await codes.CountByUserAsync(request.UserId, ct);

        var dto = new BillingStatusModel
        {
            Plan = plan,
            Status = status,
            Limits = new LimitsModel { MaxCodes = PlanLimitsConstants.MaxCodesForApi(plan) },
            Usage = new UsageModel { CodeCount = codeCount },
        };

        return AppResult<BillingMeResult.Success>.Ok(new BillingMeResult.Success(dto));
    }
}
