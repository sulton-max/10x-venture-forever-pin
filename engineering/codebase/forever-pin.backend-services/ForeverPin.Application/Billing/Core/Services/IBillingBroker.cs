using ForeverPin.Application.Billing.Core.Models;

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

    /// <summary>Verifies the <c>Stripe-Signature</c> header; throws on an invalid signature.</summary>
    BillingWebhookEvent ParseWebhookEvent(string rawBody, string stripeSignatureHeader);
}
