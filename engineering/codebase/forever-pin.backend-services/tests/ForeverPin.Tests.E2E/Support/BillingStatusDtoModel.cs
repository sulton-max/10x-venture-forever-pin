namespace ForeverPin.Tests.E2E.Support;

/// <summary>Represents the wire shape of <c>BillingStatusModel</c>.</summary>
public sealed record BillingStatusDtoModel
{
    /// <summary>Gets the caller's plan name (e.g. <c>Free</c>, <c>Pro</c>).</summary>
    public string Plan { get; init; } = "";

    /// <summary>Gets the subscription status (e.g. <c>active</c>, <c>canceled</c>).</summary>
    public string Status { get; init; } = "";

    /// <summary>Gets the plan's limits.</summary>
    public LimitsDtoModel Limits { get; init; } = new();

    /// <summary>Gets the caller's current usage.</summary>
    public UsageDtoModel Usage { get; init; } = new();
}
