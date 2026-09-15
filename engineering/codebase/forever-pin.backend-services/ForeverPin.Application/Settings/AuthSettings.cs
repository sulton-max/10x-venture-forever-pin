namespace ForeverPin.Application.Settings;

/// <summary>Represents the authentication settings.</summary>
public class AuthSettings
{
    /// <summary>Gets or sets the Google OAuth settings.</summary>
    public AuthGoogleSettings Google { get; set; } = new();
}

/// <summary>Represents the Google OAuth settings.</summary>
public class AuthGoogleSettings
{
    /// <summary>Gets or sets the Google Cloud OAuth 2.0 Web client id.</summary>
    public string ClientId { get; set; } = "";
}
