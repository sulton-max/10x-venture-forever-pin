using FluentValidation;
using ForeverPin.Application.Codes.Analytics.Queries;

namespace ForeverPin.Application.Codes.Analytics.Validators;

/// <summary>Validates analytics input.</summary>
public sealed class CodeAnalyticsQueryValidator : AbstractValidator<CodeAnalyticsQuery>
{
    /// <summary>Holds the longest window a read may span.</summary>
    public const int MaxDays = 366;

    /// <summary>Builds the analytics rules.</summary>
    public CodeAnalyticsQueryValidator()
    {
        RuleFor(query => query.Days)
            .InclusiveBetween(1, MaxDays).WithMessage($"Days must be between 1 and {MaxDays}.");
    }
}
