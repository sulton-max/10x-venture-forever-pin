namespace ForeverPin.Application.Settings;

/// <summary>Represents the paid plans' Stripe price ids.</summary>
public sealed record BillingPricesSettings
{
    /// <summary>Gets the Stripe price id for the Solo plan.</summary>
    public string Solo { get; init; } = "";

    /// <summary>Gets the Stripe price id for the Pro plan.</summary>
    public string Pro { get; init; } = "";

    /// <summary>Gets the Stripe price id for the Agency plan.</summary>
    public string Agency { get; init; } = "";
}
