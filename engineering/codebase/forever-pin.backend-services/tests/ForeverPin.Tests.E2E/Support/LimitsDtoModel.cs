namespace ForeverPin.Tests.E2E.Support;

/// <summary>Represents the wire shape of <c>LimitsModel</c>.</summary>
public sealed record LimitsDtoModel
{
    /// <summary>Gets the maximum code count, or <c>-1</c> when unlimited.</summary>
    public int MaxCodes { get; init; }
}
