namespace ForeverPin.Application.Billing.Core.Models;

/// <summary>Represents a verified Stripe billing event.</summary>
public sealed record BillingWebhookEvent
{
    /// <summary>Gets the kind of event.</summary>
    public required BillingWebhookEventType Type { get; init; }

    /// <summary>Gets the owning user id, set only on a completed Checkout session.</summary>
    public Guid? UserId { get; init; }

    /// <summary>Gets the Stripe customer id, or null when absent.</summary>
    public string? StripeCustomerId { get; init; }

    /// <summary>Gets the Stripe subscription id, or null when absent.</summary>
    public string? StripeSubscriptionId { get; init; }

    /// <summary>Gets the subscribed item's Stripe price id.</summary>
    public string? PriceId { get; init; }

    /// <summary>Gets the end of the current billing period when present on the event.</summary>
    public DateTimeOffset? CurrentPeriodEnd { get; init; }
}
