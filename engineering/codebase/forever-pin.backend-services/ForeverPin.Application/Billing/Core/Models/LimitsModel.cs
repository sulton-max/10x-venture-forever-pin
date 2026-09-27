namespace ForeverPin.Application.Billing.Core.Models;

/// <summary>Represents a billing plan's usage limits.</summary>
public sealed record LimitsModel
{
    /// <summary>Gets the maximum number of owned codes; <c>-1</c> means unlimited.</summary>
    public required int MaxCodes { get; init; }
}
