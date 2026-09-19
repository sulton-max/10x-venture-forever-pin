using ForeverPin.Domain.Billing.Enums;

namespace ForeverPin.Application.Billing.Core.Models;

/// <summary>Represents an account's billing snapshot.</summary>
public sealed record BillingStatusDto
{
    /// <summary>Gets the billing plan; Free when there is no subscription.</summary>
    public required Plan Plan { get; init; }

    /// <summary>Gets the subscription status, lower-cased to mirror Stripe; Free synthesizes <c>active</c>.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the plan's limits.</summary>
    public required LimitsDto Limits { get; init; }

    /// <summary>Gets the account's code usage.</summary>
    public required UsageDto Usage { get; init; }
}
