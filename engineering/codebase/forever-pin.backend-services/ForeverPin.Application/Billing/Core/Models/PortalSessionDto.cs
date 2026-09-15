namespace ForeverPin.Application.Billing.Core.Models;

/// <summary>Represents a hosted Customer Portal session.</summary>
public sealed record PortalSessionDto
{
    /// <summary>Gets the hosted billing-portal URL.</summary>
    public required string Url { get; init; }
}
