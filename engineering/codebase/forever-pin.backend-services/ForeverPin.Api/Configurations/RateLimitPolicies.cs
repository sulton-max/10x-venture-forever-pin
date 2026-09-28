namespace ForeverPin.Api.Configurations;

/// <summary>
/// Names the management host's request budgets. Configuration declares each under <c>RateLimits:Policies</c>;
/// the host fails at startup when one is missing, so a renamed policy can never silently lift a limit.
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>Holds the budget for starting sessions: guest provisioning and Google sign-in, per IP.</summary>
    public const string Sessions = "sessions";

    /// <summary>Holds the budget for server-rendered previews, per caller.</summary>
    public const string Previews = "previews";

    /// <summary>Holds the budget for code writes — create, update, pause and delete — per caller.</summary>
    public const string Writes = "writes";

    /// <summary>Holds the budget for rendered image downloads, per caller.</summary>
    public const string Images = "images";

    /// <summary>Holds every named budget the controllers apply.</summary>
    public static readonly IReadOnlyList<string> All = [Sessions, Previews, Writes, Images];
}
