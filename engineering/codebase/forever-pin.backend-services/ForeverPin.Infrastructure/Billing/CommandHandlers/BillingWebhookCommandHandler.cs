using Microsoft.Extensions.Logging;
using ForeverPin.Application.Billing.Core;
using ForeverPin.Application.Billing.Core.Commands;
using ForeverPin.Application.Billing.Core.Models;
using ForeverPin.Application.Billing.Core.Services;
using ForeverPin.Domain.Billing.Entities;
using ForeverPin.Domain.Billing.Enums;
using WoW.Two.Sdk.Beta.Be.Foundation.Results;
using WoW.Two.Sdk.Beta.Be.Mediator.Cqrs;
using WoW.Two.Sdk.Beta.Be.Mediator.Result;
using WoW.Two.Sdk.Beta.Be.Messaging.Reliability;
using BillingSettings = ForeverPin.Application.Settings.BillingSettings;

namespace ForeverPin.Infrastructure.Billing.CommandHandlers;

/// <summary>Handles <see cref="BillingWebhookCommand"/>.</summary>
public sealed class BillingWebhookCommandHandler(
    ISubscriptionRepository subscriptions,
    IBillingBroker gateway,
    IInboxProcessor inbox,
    BillingSettings settings,
    ILogger<BillingWebhookCommandHandler> logger)
    : ICommandHandler<BillingWebhookCommand, AppResult<BillingWebhookResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<BillingWebhookResult.Success>> HandleAsync(
        BillingWebhookCommand request, CancellationToken ct)
    {
        var parsed = await gateway.ParseWebhookEventAsync(request.RawBody, request.StripeSignature, ct);
        if (parsed is Result<BillingWebhookEvent>.Failure failure)
            return AppResult<BillingWebhookResult.Success>.Fail(failure.Error);

        // Acknowledge unhandled events with 200 so Stripe doesn't retry them.
        var webhookEvent = ((Result<BillingWebhookEvent>.Success)parsed).Value;
        if (webhookEvent.Type is BillingWebhookEventType.Ignored)
            return AppResult<BillingWebhookResult.Success>.Ok(new BillingWebhookResult.Success());

        // A redelivered event changes nothing; its dedupe mark commits with the subscription change.
        if (!await inbox.ProcessOnceAsync($"stripe:{webhookEvent.EventId}", token => ApplyAsync(webhookEvent, token), ct))
            logger.LogInformation("Stripe event {EventId} was already applied; acknowledging", webhookEvent.EventId);

        return AppResult<BillingWebhookResult.Success>.Ok(new BillingWebhookResult.Success());
    }

    /// <summary>Applies one verified event's subscription change.</summary>
    private async ValueTask ApplyAsync(BillingWebhookEvent webhookEvent, CancellationToken ct)
    {
        switch (webhookEvent.Type)
        {
            case BillingWebhookEventType.CheckoutSessionCompleted:
                await UpsertFromCheckoutAsync(webhookEvent, ct);
                break;

            case BillingWebhookEventType.SubscriptionUpdated:
                await RefreshFromSubscriptionAsync(webhookEvent, SubscriptionStatus.Active, ct);
                break;

            case BillingWebhookEventType.SubscriptionDeleted:
                await RefreshFromSubscriptionAsync(webhookEvent, SubscriptionStatus.Canceled, ct);
                break;
        }
    }

    /// <summary>Upserts the subscription row from a completed Checkout session.</summary>
    private async Task UpsertFromCheckoutAsync(BillingWebhookEvent e, CancellationToken ct)
    {
        if (e.UserId is not { } userId)
        {
            logger.LogWarning("checkout.session.completed had no client_reference_id; skipping upsert");
            return;
        }

        var plan = PlanPriceMapper.PlanFor(settings, e.PriceId);

        await subscriptions.UpsertByUserAsync(new SubscriptionEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Plan = plan,
            Status = SubscriptionStatus.Active,
            StripeCustomerId = e.StripeCustomerId ?? "",
            StripeSubscriptionId = e.StripeSubscriptionId ?? "",
            CurrentPeriodEnd = e.CurrentPeriodEnd,
            LastEventAt = e.CreatedAt,
        }, ct);
    }

    /// <summary>Refreshes the row for a subscription id.</summary>
    private async Task RefreshFromSubscriptionAsync(
        BillingWebhookEvent e,
        SubscriptionStatus status,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(e.StripeSubscriptionId))
        {
            logger.LogWarning("subscription event had no subscription id; skipping");
            return;
        }

        var existing = await subscriptions.GetByStripeSubscriptionIdAsync(e.StripeSubscriptionId, ct);
        if (existing is null)
        {
            logger.LogWarning(
                "No subscription row for {SubscriptionId}; skipping {EventType}",
                e.StripeSubscriptionId,
                e.Type);
            return;
        }

        await subscriptions.UpsertByUserAsync(new SubscriptionEntity
        {
            Id = existing.Id,
            UserId = existing.UserId,
            Plan = e.PriceId is null ? existing.Plan : PlanPriceMapper.PlanFor(settings, e.PriceId),
            Status = status,
            StripeCustomerId = e.StripeCustomerId ?? existing.StripeCustomerId,
            StripeSubscriptionId = e.StripeSubscriptionId,
            CurrentPeriodEnd = e.CurrentPeriodEnd ?? existing.CurrentPeriodEnd,
            LastEventAt = e.CreatedAt,
        }, ct);
    }
}
