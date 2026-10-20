namespace ForeverPin.Tests.E2E.Support;

/// <summary>Represents the wire shape of the checkout and portal session DTOs.</summary>
public sealed record SessionUrlDtoModel
{
    public string Url { get; init; } = "";
}
