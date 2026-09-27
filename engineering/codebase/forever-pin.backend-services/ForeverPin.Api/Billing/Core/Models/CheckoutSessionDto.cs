using ForeverPin.Application.Billing.Core.Models;

namespace ForeverPin.Api.Billing.Core.Models;

/// <summary>Represents a hosted Checkout session.</summary>
public sealed record CheckoutSessionDto
{
    /// <summary>Gets the hosted checkout URL.</summary>
    public required string Url { get; init; }
}

/// <summary>Maps the application CheckoutSessionModel into its HTTP projection.</summary>
public static class CheckoutSessionDtoExtensions
{
    /// <summary>Maps the application fields without changing the wire contract.</summary>
    public static CheckoutSessionDto ToDto(this CheckoutSessionModel model) => new()
    {
        Url = model.Url,
    };
}
