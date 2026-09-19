namespace ForeverPin.Application.Identity.Core.Models;

/// <summary>Represents the outcome of a Google sign-in.</summary>
public abstract record GoogleSignInResult
{
    private GoogleSignInResult() { }

    /// <summary>Represents a signed-in user.</summary>
    /// <param name="User">The signed-in user's profile.</param>
    public sealed record Success(UserSummaryDto User) : GoogleSignInResult;
}
