namespace ForeverPin.Application.Settings;

/// <summary>Represents the management API settings.</summary>
public sealed record ApiSettings
{
    /// <summary>Gets the base URL of the redirect service.</summary>
    public string RedirectBaseUrl { get; init; } = "https://localhost:7022";
}
