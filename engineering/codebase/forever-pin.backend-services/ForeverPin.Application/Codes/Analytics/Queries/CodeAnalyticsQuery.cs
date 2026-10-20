using ForeverPin.Application.Codes.Analytics.Models;
using WoW.Two.Sdk.Beta.Be.Mediator.Cqrs;
using WoW.Two.Sdk.Beta.Be.Mediator.Result;

namespace ForeverPin.Application.Codes.Analytics.Queries;

/// <summary>Represents a query for a code's scan analytics over its most recent days.</summary>
public sealed record CodeAnalyticsQuery
    : IQuery<AppResult<CodeAnalyticsResult.Success>>
{
    /// <summary>Gets the id of the code.</summary>
    public required Guid Id { get; init; }

    /// <summary>Gets the user the code must belong to.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Gets how many UTC calendar days, ending today, the window spans.</summary>
    public required int Days { get; init; }
}
