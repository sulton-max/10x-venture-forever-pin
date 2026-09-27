namespace ForeverPin.Redirect.Api.Settings;

/// <summary>Represents the redirect-service settings.</summary>
public sealed record RedirectSettings
{
    /// <summary>Gets the in-memory code-cache lifetime, in seconds.</summary>
    public int ConfigCacheSeconds { get; init; } = 30;
}
