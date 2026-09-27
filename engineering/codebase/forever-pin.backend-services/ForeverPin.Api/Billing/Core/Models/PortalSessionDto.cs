using ForeverPin.Application.Billing.Core.Models;

namespace ForeverPin.Api.Billing.Core.Models;

/// <summary>Represents a hosted Customer Portal session.</summary>
public sealed record PortalSessionDto
{
    /// <summary>Gets the hosted billing-portal URL.</summary>
    public required string Url { get; init; }
}

/// <summary>Maps the application PortalSessionModel into its HTTP projection.</summary>
public static class PortalSessionDtoExtensions
{
    /// <summary>Maps the application fields without changing the wire contract.</summary>
    public static PortalSessionDto ToDto(this PortalSessionModel model) => new()
    {
        Url = model.Url,
    };
}
