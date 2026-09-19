namespace ForeverPin.Application.Billing.Core.Models;

/// <summary>Represents the outcome of processing a Stripe webhook event.</summary>
public abstract record BillingWebhookResult
{
    private BillingWebhookResult() { }

    /// <summary>Represents a processed or ignored webhook event.</summary>
    public sealed record Success : BillingWebhookResult;
}
