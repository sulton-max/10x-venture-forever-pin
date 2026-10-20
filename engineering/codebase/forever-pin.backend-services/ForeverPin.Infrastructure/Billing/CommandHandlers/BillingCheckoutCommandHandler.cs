using ForeverPin.Application.Billing.Core;
using ForeverPin.Application.Billing.Core.Commands;
using ForeverPin.Application.Billing.Core.Models;
using ForeverPin.Application.Billing.Core.Services;
using ForeverPin.Domain.Billing.Enums;
using WoW.Two.Sdk.Beta.Be.Foundation.Errors;
using WoW.Two.Sdk.Beta.Be.Mediator.Cqrs;
using WoW.Two.Sdk.Beta.Be.Mediator.Result;
using BillingSettings = ForeverPin.Application.Settings.BillingSettings;

namespace ForeverPin.Infrastructure.Billing.CommandHandlers;

/// <summary>Handles <see cref="BillingCheckoutCommand"/>.</summary>
public sealed class BillingCheckoutCommandHandler(
    IBillingBroker gateway,
    BillingSettings settings)
    : ICommandHandler<BillingCheckoutCommand, AppResult<BillingCheckoutResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<BillingCheckoutResult.Success>> HandleAsync(
        BillingCheckoutCommand request, CancellationToken ct)
    {
        // Free has no price — checking out into it is a client error, not a server fault.
        if (request.Plan == Plan.Free)
            return Failure("The Free plan has no checkout — pick a paid plan.", AppErrorType.Validation);

        var priceId = PlanPriceMapper.PriceIdFor(settings, request.Plan);
        if (priceId is null)
            return Failure($"No price configured for plan '{request.Plan}'.", AppErrorType.Validation);

        var url = await gateway.CreateCheckoutSessionAsync(
            request.UserId, priceId, settings.SuccessUrl, settings.CancelUrl, ct);

        return AppResult<BillingCheckoutResult.Success>.Ok(
            new BillingCheckoutResult.Success(new CheckoutSessionModel { Url = url }));
    }

    private static AppResult<BillingCheckoutResult.Success> Failure(
        string message, AppErrorType type) =>
        AppResult<BillingCheckoutResult.Success>.Fail(AppError.Of(type, message));
}
