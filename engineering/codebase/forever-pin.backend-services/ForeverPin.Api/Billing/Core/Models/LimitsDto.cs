using ForeverPin.Application.Billing.Core.Models;

namespace ForeverPin.Api.Billing.Core.Models;

/// <summary>Represents a billing plan's usage limits.</summary>
public sealed record LimitsDto
{
    /// <summary>Gets the maximum number of owned dynamic codes; <c>-1</c> means unlimited. Static codes are unlimited.</summary>
    public required int MaxDynamicCodes { get; init; }

    /// <summary>Gets how many days back the plan's analytics reach.</summary>
    public required int AnalyticsDays { get; init; }
}

/// <summary>Maps the application LimitsModel into its HTTP projection.</summary>
public static class LimitsDtoExtensions
{
    /// <summary>Maps the application fields without changing the wire contract.</summary>
    public static LimitsDto ToDto(this LimitsModel model) => new()
    {
        MaxDynamicCodes = model.MaxDynamicCodes,
        AnalyticsDays = model.AnalyticsDays,
    };
}
