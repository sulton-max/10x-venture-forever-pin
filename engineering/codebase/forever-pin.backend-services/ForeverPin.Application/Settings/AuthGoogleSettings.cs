namespace ForeverPin.Application.Settings;

/// <summary>Represents the Google OAuth settings.</summary>
public sealed record AuthGoogleSettings
{
    /// <summary>Gets the Google Cloud OAuth 2.0 Web client id.</summary>
    public string ClientId { get; init; } = "";
}
