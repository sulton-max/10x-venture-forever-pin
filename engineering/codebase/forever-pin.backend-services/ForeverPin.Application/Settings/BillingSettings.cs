namespace ForeverPin.Application.Settings;

/// <summary>Represents the Stripe billing settings.</summary>
public sealed record BillingSettings
{
    /// <summary>Gets the Stripe secret API key.</summary>
    /// <remarks>Set via environment variable or user-secrets, never in appsettings.</remarks>
    public string SecretKey { get; init; } = "";

    /// <summary>Gets the Stripe webhook signing secret.</summary>
    public string WebhookSecret { get; init; } = "";

    /// <summary>Gets the Stripe price ids per paid plan.</summary>
    public BillingPricesSettings Prices { get; init; } = new();

    /// <summary>Gets the hosted Checkout success-redirect URL.</summary>
    public string SuccessUrl { get; init; } = "http://localhost:7020/billing/success";

    /// <summary>Gets the hosted Checkout cancel-redirect URL, reused as the Portal return URL.</summary>
    public string CancelUrl { get; init; } = "http://localhost:7020/billing/cancel";
}
