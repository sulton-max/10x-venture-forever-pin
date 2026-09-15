namespace ForeverPin.Redirect.Api.Settings;

/// <summary>Represents the redirect-service settings.</summary>
public class RedirectSettings
{
    /// <summary>Gets or sets the in-memory code-cache lifetime, in seconds.</summary>
    public int ConfigCacheSeconds { get; set; } = 30;
}
