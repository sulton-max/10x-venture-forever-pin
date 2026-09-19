namespace ForeverPin.Redirect.Api.Application.Routing.Models;

/// <summary>Represents the outcome of routing a scan.</summary>
public abstract record RoutingResult
{
    private RoutingResult() { }

    /// <summary>Represents a routed scan with a destination.</summary>
    /// <param name="Destination">The resolved destination.</param>
    /// <param name="MatchedRuleOrder">The matched rule's order, or null when no rule matched.</param>
    public sealed record Redirect(string Destination, int? MatchedRuleOrder) : RoutingResult;

    /// <summary>Represents a scan with no destination.</summary>
    public sealed record NotFound : RoutingResult;
}
