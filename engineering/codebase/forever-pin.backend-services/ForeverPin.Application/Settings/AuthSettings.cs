namespace ForeverPin.Application.Settings;

/// <summary>Represents the authentication settings.</summary>
public sealed record AuthSettings
{
    /// <summary>Gets the Google OAuth settings.</summary>
    public AuthGoogleSettings Google { get; init; } = new();
}
