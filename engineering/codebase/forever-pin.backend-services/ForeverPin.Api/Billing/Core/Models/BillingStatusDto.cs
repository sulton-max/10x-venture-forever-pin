using ForeverPin.Application.Billing.Core.Models;
using ForeverPin.Domain.Billing.Enums;

namespace ForeverPin.Api.Billing.Core.Models;

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

/// <summary>Maps the application BillingStatusModel into its HTTP projection.</summary>
public static class BillingStatusDtoExtensions
{
    /// <summary>Maps the application fields without changing the wire contract.</summary>
    public static BillingStatusDto ToDto(this BillingStatusModel model) => new()
    {
        Plan = model.Plan,
        Status = model.Status,
        Limits = model.Limits.ToDto(),
        Usage = model.Usage.ToDto(),
    };
}
