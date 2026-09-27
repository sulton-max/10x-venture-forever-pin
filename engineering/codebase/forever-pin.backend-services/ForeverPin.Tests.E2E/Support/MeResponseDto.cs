namespace ForeverPin.Tests.E2E.Support;

/// <summary>Represents the wire shape of <c>MeResponse</c>.</summary>
public sealed record MeResponseDto
{
    /// <summary>Gets the caller's identity kind (Anonymous, Guest, or User).</summary>
    public string Kind { get; init; } = "";
}
