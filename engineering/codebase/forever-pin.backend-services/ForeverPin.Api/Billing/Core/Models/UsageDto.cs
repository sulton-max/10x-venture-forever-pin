using ForeverPin.Application.Billing.Core.Models;

namespace ForeverPin.Api.Billing.Core.Models;

/// <summary>Represents an account's code usage.</summary>
public sealed record UsageDto
{
    /// <summary>Gets the number of owned codes.</summary>
    public required int CodeCount { get; init; }
}

/// <summary>Maps the application UsageModel into its HTTP projection.</summary>
public static class UsageDtoExtensions
{
    /// <summary>Maps the application fields without changing the wire contract.</summary>
    public static UsageDto ToDto(this UsageModel model) => new()
    {
        CodeCount = model.CodeCount,
    };
}
