namespace ForeverPin.Application.Settings;

/// <summary>Represents the paid plans' Stripe price ids.</summary>
public sealed record BillingPricesSettings
{
    /// <summary>Gets the Stripe price id for the Basic plan: a yearly price, shown as its monthly share.</summary>
    public string Basic { get; init; } = "";

    /// <summary>Gets the Stripe price id for the Pro plan; empty while the plan is not on sale.</summary>
    public string Pro { get; init; } = "";

    /// <summary>Gets the Stripe price id for the Agency plan; empty while the plan is not on sale.</summary>
    public string Agency { get; init; } = "";
}
