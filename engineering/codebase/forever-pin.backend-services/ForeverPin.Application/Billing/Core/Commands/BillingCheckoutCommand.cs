using ForeverPin.Application.Billing.Core.Models;
using ForeverPin.Domain.Billing.Enums;
using WoW.Two.Sdk.Beta.Be.Mediator.Cqrs;
using WoW.Two.Sdk.Beta.Be.Mediator.Result;

namespace ForeverPin.Application.Billing.Core.Commands;

/// <summary>Represents a command to start a paid-plan checkout.</summary>
public sealed record BillingCheckoutCommand
    : ICommand<AppResult<BillingCheckoutResult.Success>>
{
    /// <summary>Gets the id of the user starting checkout.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Gets the plan to subscribe to.</summary>
    /// <remarks>Pass a paid plan; Free is rejected.</remarks>
    public required Plan Plan { get; init; }
}
