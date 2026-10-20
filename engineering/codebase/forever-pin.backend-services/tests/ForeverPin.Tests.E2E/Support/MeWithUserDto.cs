namespace ForeverPin.Tests.E2E.Support;

/// <summary>Represents the wire shape of <c>CurrentUserModel</c>, including the user profile.</summary>
public sealed record MeWithUserDto
{
    /// <summary>Gets the caller's identity kind (Anonymous, Guest, or User).</summary>
    public string Kind { get; init; } = "";

    /// <summary>Gets the signed-in account profile; null for guest or anonymous.</summary>
    public UserSummaryDtoModel? User { get; init; }
}
