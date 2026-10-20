namespace ForeverPin.Tests.E2E.Support;

/// <summary>Represents the wire shape of the content.</summary>
public sealed record ContentDtoModel
{
    public string Type { get; init; } = "";
    public string? Url { get; init; }
    public string? Text { get; init; }
    public string? Ssid { get; init; }
    public string? Password { get; init; }
    public string? FirstName { get; init; }
    public string? Email { get; init; }
}
