namespace ForeverPin.Application.Billing.Core.Models;

/// <summary>Represents an account's code usage.</summary>
public sealed record UsageModel
{
    /// <summary>Gets the number of owned codes.</summary>
    public required int CodeCount { get; init; }
}
