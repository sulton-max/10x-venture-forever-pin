namespace ForeverPin.Application.Billing.Core.Models;

/// <summary>Represents a billing plan's usage limits.</summary>
public sealed record LimitsDto
{
    /// <summary>Gets the maximum number of owned codes; <c>-1</c> means unlimited.</summary>
    public required int MaxCodes { get; init; }
}
