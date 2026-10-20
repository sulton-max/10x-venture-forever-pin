using Stripe;
using WoW.Two.Sdk.Beta.Be.Foundation.Errors;
using WoW.Two.Sdk.Beta.Be.Foundation.Results;
using Stripe.Checkout;
using ForeverPin.Application.Billing.Core.Models;
using ForeverPin.Application.Billing.Core.Services;
using BillingSettings = ForeverPin.Application.Settings.BillingSettings;
using StripeCheckoutSessionService = Stripe.Checkout.SessionService;
using StripePortalSessionService = Stripe.BillingPortal.SessionService;
using StripePortalSessionOptions = Stripe.BillingPortal.SessionCreateOptions;

namespace ForeverPin.Infrastructure.Billing.Services;

/// <summary>Wraps the Stripe API for checkout, portal, and webhook handling.</summary>
public sealed class StripeBillingBroker(BillingSettings settings) : IBillingBroker
{
    private RequestOptions Request => new() { ApiKey = settings.SecretKey };

    /// <inheritdoc />
    public async Task<string> CreateCheckoutSessionAsync(
        Guid userId, string priceId, string successUrl, string cancelUrl, CancellationToken ct)
    {
        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            ClientReferenceId = userId.ToString(),
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            LineItems = [new SessionLineItemOptions { Price = priceId, Quantity = 1 }],
        };

        var session = await new StripeCheckoutSessionService().CreateAsync(options, Request, ct);
        return session.Url;
    }

    /// <inheritdoc />
    public async Task<string> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl, CancellationToken ct)
    {
        var options = new StripePortalSessionOptions { Customer = stripeCustomerId, ReturnUrl = returnUrl };
        var session = await new StripePortalSessionService().CreateAsync(options, Request, ct);
        return session.Url;
    }

    /// <inheritdoc />
    public async Task<Result<BillingWebhookEvent>> ParseWebhookEventAsync(
        string rawBody, string stripeSignatureHeader, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(rawBody, stripeSignatureHeader, settings.WebhookSecret);
        }
        catch (Exception ex) when (ex is StripeException or Newtonsoft.Json.JsonException)
        {
            return Result<BillingWebhookEvent>.Fail(AppError.Of(
                AppErrorType.Validation, "The webhook payload or signature is invalid."));
        }

        var parsed = stripeEvent.Type switch
        {
            EventTypes.CheckoutSessionCompleted => await FromCheckoutAsync(stripeEvent, (Session)stripeEvent.Data.Object, ct),
            EventTypes.CustomerSubscriptionUpdated => FromSubscription(
                stripeEvent,
                (Subscription)stripeEvent.Data.Object,
                BillingWebhookEventType.SubscriptionUpdated),
            EventTypes.CustomerSubscriptionDeleted => FromSubscription(
                stripeEvent,
                (Subscription)stripeEvent.Data.Object,
                BillingWebhookEventType.SubscriptionDeleted),
            _ => new BillingWebhookEvent
            {
                Type = BillingWebhookEventType.Ignored,
                EventId = stripeEvent.Id,
                CreatedAt = CreatedAt(stripeEvent),
            },
        };
        return Result<BillingWebhookEvent>.Ok(parsed);
    }

    /// <summary>Flattens a completed Checkout session, expanding the subscription for price and period end.</summary>
    private async Task<BillingWebhookEvent> FromCheckoutAsync(Event stripeEvent, Session session, CancellationToken ct)
    {
        string? priceId = null;
        DateTimeOffset? periodEnd = null;

        if (!string.IsNullOrWhiteSpace(session.SubscriptionId))
        {
            var subscription = await new SubscriptionService().GetAsync(
                session.SubscriptionId, requestOptions: Request, cancellationToken: ct);
            var item = subscription.Items?.Data?.FirstOrDefault();
            priceId = item?.Price?.Id;
            if (item is not null)
                periodEnd = new DateTimeOffset(DateTime.SpecifyKind(item.CurrentPeriodEnd, DateTimeKind.Utc));
        }

        return new BillingWebhookEvent
        {
            Type = BillingWebhookEventType.CheckoutSessionCompleted,
            EventId = stripeEvent.Id,
            CreatedAt = CreatedAt(stripeEvent),
            UserId = Guid.TryParse(session.ClientReferenceId, out var id) ? id : null,
            StripeCustomerId = session.CustomerId,
            StripeSubscriptionId = session.SubscriptionId,
            PriceId = priceId,
            CurrentPeriodEnd = periodEnd,
        };
    }

    /// <summary>Flattens a <c>customer.subscription.*</c> event.</summary>
    private static BillingWebhookEvent FromSubscription(
        Event stripeEvent, Subscription subscription, BillingWebhookEventType type)
    {
        var item = subscription.Items?.Data?.FirstOrDefault();

        return new BillingWebhookEvent
        {
            Type = type,
            EventId = stripeEvent.Id,
            CreatedAt = CreatedAt(stripeEvent),
            StripeCustomerId = subscription.CustomerId,
            StripeSubscriptionId = subscription.Id,
            PriceId = item?.Price?.Id,
            CurrentPeriodEnd = item is null
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(item.CurrentPeriodEnd, DateTimeKind.Utc)),
        };
    }

    /// <summary>Reads the event's creation time, which Stripe reports in UTC.</summary>
    private static DateTimeOffset CreatedAt(Event stripeEvent) =>
        new(DateTime.SpecifyKind(stripeEvent.Created, DateTimeKind.Utc));
}
