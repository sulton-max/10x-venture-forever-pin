namespace ForeverPin.Application.Codes.Analytics.Models;

/// <summary>Represents the outcome of reading a code's scan analytics.</summary>
public abstract record CodeAnalyticsResult
{
    private CodeAnalyticsResult() { }

    /// <summary>Represents the read analytics.</summary>
    /// <param name="Analytics">The code's analytics over the requested window.</param>
    public sealed record Success(CodeAnalyticsModel Analytics) : CodeAnalyticsResult;
}
