namespace ForeverPin.Application.Billing.Core.Models;

/// <summary>Represents an account's code usage.</summary>
public sealed record UsageModel
{
    /// <summary>Gets the number of owned dynamic codes, the ones the plan caps.</summary>
    public required int DynamicCodeCount { get; init; }
}
