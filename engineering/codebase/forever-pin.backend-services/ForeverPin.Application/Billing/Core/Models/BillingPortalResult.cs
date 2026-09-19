namespace ForeverPin.Application.Billing.Core.Models;

/// <summary>Represents the outcome of opening a Customer Portal session.</summary>
public abstract record BillingPortalResult
{
    private BillingPortalResult() { }

    /// <summary>Represents a created Customer Portal session.</summary>
    /// <param name="Session">The created Customer Portal session.</param>
    public sealed record Success(PortalSessionDto Session) : BillingPortalResult;
}
