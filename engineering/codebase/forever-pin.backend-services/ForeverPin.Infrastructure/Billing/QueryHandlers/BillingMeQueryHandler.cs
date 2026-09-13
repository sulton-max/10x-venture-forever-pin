using Microsoft.Extensions.Logging;
using ForeverPin.Application.Billing.Core;
using ForeverPin.Application.Billing.Core.Models;
using ForeverPin.Application.Billing.Core.Queries;
using ForeverPin.Application.Billing.Core.Services;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Domain.Billing.Enums;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace ForeverPin.Infrastructure.Billing.QueryHandlers;

/// <summary>Handles <see cref="BillingMeQuery"/>.</summary>
public sealed class BillingMeQueryHandler(
    ISubscriptionRepository subscriptions,
    ICodeRepository codes,
    ILogger<BillingMeQueryHandler> logger)
    : IQueryHandler<BillingMeQuery, AppResult<BillingMeResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<BillingMeResult.Success>> HandleAsync(
        BillingMeQuery request, CancellationToken ct)
    {
        try
        {
            var subscription = await subscriptions.GetByUserAsync(request.UserId, ct);

            // No row ⇒ Free / active (the synthesized default; no row is ever required to use the app).
            var plan = subscription?.Plan ?? Plan.Free;
            var status = subscription is null ? "active" : subscription.Status.ToString().ToLowerInvariant();

            var codeCount = await codes.CountByUserAsync(request.UserId, ct);

            var dto = new BillingStatusDto
            {
                Plan = plan,
                Status = status,
                Limits = new LimitsDto { MaxCodes = PlanLimitsConstants.MaxCodesForApi(plan) },
                Usage = new UsageDto { CodeCount = codeCount },
            };

            return AppResult<BillingMeResult.Success>.Ok(new BillingMeResult.Success(dto));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "BillingMe failed for user {UserId}", request.UserId);
            return AppResult<BillingMeResult.Success>.Fail(AppError.Of(AppErrorType.Unexpected, ex.Message));
        }
    }
}
