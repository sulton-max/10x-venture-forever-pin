namespace ForeverPin.Application.Billing.Core.Models;

/// <summary>Represents the outcome of reading an account's billing snapshot.</summary>
public abstract record BillingMeResult
{
    private BillingMeResult() { }

    /// <summary>Represents a resolved billing snapshot.</summary>
    /// <param name="Status">The resolved billing snapshot.</param>
    public sealed record Success(BillingStatusDto Status) : BillingMeResult;
}
