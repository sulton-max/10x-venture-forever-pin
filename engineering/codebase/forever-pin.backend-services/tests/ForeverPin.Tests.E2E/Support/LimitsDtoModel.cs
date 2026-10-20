namespace ForeverPin.Tests.E2E.Support;

/// <summary>Represents the wire shape of <c>LimitsModel</c>.</summary>
public sealed record LimitsDtoModel
{
    /// <summary>Gets the maximum dynamic-code count, or <c>-1</c> when unlimited.</summary>
    public int MaxDynamicCodes { get; init; }

    /// <summary>Gets how many days back the plan's analytics reach.</summary>
    public int AnalyticsDays { get; init; }
}
