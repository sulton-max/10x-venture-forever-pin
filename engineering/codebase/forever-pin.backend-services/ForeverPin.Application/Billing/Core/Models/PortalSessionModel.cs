namespace ForeverPin.Application.Billing.Core.Models;

/// <summary>Represents a hosted Customer Portal session.</summary>
public sealed record PortalSessionModel
{
    /// <summary>Gets the hosted billing-portal URL.</summary>
    public required string Url { get; init; }
}
