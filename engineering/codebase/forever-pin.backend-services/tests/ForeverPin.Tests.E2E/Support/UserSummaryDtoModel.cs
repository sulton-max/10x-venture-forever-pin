namespace ForeverPin.Tests.E2E.Support;

/// <summary>Represents the wire shape of <c>UserSummaryModel</c>.</summary>
public sealed record UserSummaryDtoModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
}
