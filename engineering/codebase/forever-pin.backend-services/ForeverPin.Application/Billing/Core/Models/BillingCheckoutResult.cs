namespace ForeverPin.Application.Billing.Core.Models;

/// <summary>Represents the outcome of starting a hosted Checkout session.</summary>
public abstract record BillingCheckoutResult
{
    private BillingCheckoutResult() { }

    /// <summary>Represents a created Checkout session.</summary>
    /// <param name="Session">The created Checkout session.</param>
    public sealed record Success(CheckoutSessionDto Session) : BillingCheckoutResult;
}
