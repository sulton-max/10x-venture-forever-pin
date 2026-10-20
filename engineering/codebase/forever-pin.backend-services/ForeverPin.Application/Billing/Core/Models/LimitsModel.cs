namespace ForeverPin.Application.Billing.Core.Models;

/// <summary>Represents a billing plan's usage limits.</summary>
public sealed record LimitsModel
{
    /// <summary>Gets the maximum number of owned dynamic codes; <c>-1</c> means unlimited. Static codes are unlimited.</summary>
    public required int MaxDynamicCodes { get; init; }

    /// <summary>Gets how many days back the plan's analytics reach.</summary>
    public required int AnalyticsDays { get; init; }
}
