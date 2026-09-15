namespace ForeverPin.Application.Billing.Core.Models;

/// <summary>Represents a hosted Checkout session.</summary>
public sealed record CheckoutSessionDto
{
    /// <summary>Gets the hosted checkout URL.</summary>
    public required string Url { get; init; }
}
