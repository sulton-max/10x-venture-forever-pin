using ForeverPin.Application.Billing.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Foundation.Results;
using ForeverPin.Application.Billing.Core.Services;

namespace ForeverPin.Tests.E2E.Harness;

/// <summary>Provides an in-memory billing broker with staged responses.</summary>
public sealed class FakeBillingBroker : IBillingBroker
{
    /// <summary>Gets the checkout URL the broker returns.</summary>
    public string CheckoutUrl { get; init; } = "https://checkout.stripe.com/c/pay/cs_test_fake";

    /// <summary>Gets the portal URL the broker returns.</summary>
    public string PortalUrl { get; init; } = "https://billing.stripe.com/p/session/test_fake";

    /// <summary>Gets or sets the staged webhook event; null yields an <c>Ignored</c> event.</summary>
    public BillingWebhookEvent? NextEvent { get; set; }

    /// <summary>Holds the creation time staged events share unless a test orders them.</summary>
    public static readonly DateTimeOffset EventTime = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>Creates a unique Stripe-style event id, so the webhook inbox treats each staged event as new.</summary>
    public static string NewEventId() => $"evt_{Guid.NewGuid():N}";

    /// <summary>Gets or sets a marker simulating a failed signature check.</summary>
    public Exception? SignatureError { get; set; }

    /// <summary>Gets or sets an unexpected provider failure.</summary>
    public Exception? WebhookError { get; set; }

    /// <summary>Gets the arguments captured from the last checkout call.</summary>
    public (Guid UserId, string PriceId, string SuccessUrl, string CancelUrl)? LastCheckout { get; private set; }

    /// <summary>Gets the Stripe customer id captured from the last portal call.</summary>
    public string? LastPortalCustomerId { get; private set; }

    /// <inheritdoc />
    public Task<string> CreateCheckoutSessionAsync(
        Guid userId,
        string priceId,
        string successUrl,
        string cancelUrl,
        CancellationToken ct)
    {
        LastCheckout = (userId, priceId, successUrl, cancelUrl);
        return Task.FromResult(CheckoutUrl);
    }

    /// <inheritdoc />
    public Task<string> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl, CancellationToken ct)
    {
        LastPortalCustomerId = stripeCustomerId;
        return Task.FromResult(PortalUrl);
    }

    /// <inheritdoc />
    public Task<Result<BillingWebhookEvent>> ParseWebhookEventAsync(
        string rawBody, string stripeSignatureHeader, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (WebhookError is not null)
            throw WebhookError;
        if (SignatureError is not null)
            return Task.FromResult(Result<BillingWebhookEvent>.Fail(AppError.Of(
                AppErrorType.Validation, "The webhook payload or signature is invalid.")));

        return Task.FromResult(Result<BillingWebhookEvent>.Ok(
            NextEvent ?? new BillingWebhookEvent
            {
                Type = BillingWebhookEventType.Ignored,
                EventId = NewEventId(),
                CreatedAt = EventTime,
            }));
    }

    /// <summary>Clears the staged state and captured calls.</summary>
    public void Reset()
    {
        NextEvent = null;
        SignatureError = null;
        WebhookError = null;
        LastCheckout = null;
        LastPortalCustomerId = null;
    }
}
