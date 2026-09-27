using ForeverPin.Application.Billing.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Foundation.Results;

namespace ForeverPin.Application.Billing.Core.Services;

/// <summary>Defines the Stripe hosted billing flows.</summary>
public interface IBillingBroker
{
    /// <summary>Creates a hosted Checkout session for <paramref name="userId"/> and returns its URL.</summary>
    Task<string> CreateCheckoutSessionAsync(
        Guid userId,
        string priceId,
        string successUrl,
        string cancelUrl,
        CancellationToken ct);

    /// <summary>Creates a Customer Portal session for the given Stripe customer and returns the hosted URL.</summary>
    Task<string> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl, CancellationToken ct);

    /// <summary>Verifies the signature and asynchronously resolves the event; invalid signatures return a failure.</summary>
    Task<Result<BillingWebhookEvent>> ParseWebhookEventAsync(
        string rawBody, string stripeSignatureHeader, CancellationToken ct);
}
