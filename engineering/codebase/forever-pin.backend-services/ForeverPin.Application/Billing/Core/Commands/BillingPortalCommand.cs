using ForeverPin.Application.Billing.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace ForeverPin.Application.Billing.Core.Commands;

/// <summary>Represents a command to open the Stripe billing portal.</summary>
public sealed record BillingPortalCommand
    : ICommand<AppResult<BillingPortalResult.Success>>
{
    /// <summary>Gets the id of the user opening the billing portal.</summary>
    public required Guid UserId { get; init; }
}
