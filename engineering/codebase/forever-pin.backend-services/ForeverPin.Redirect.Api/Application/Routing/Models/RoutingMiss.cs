namespace ForeverPin.Redirect.Api.Application.Routing.Models;

/// <summary>Defines why a scan of an existing code has no destination; the scanner sees a page saying so.</summary>
public enum RoutingMiss
{
    /// <summary>Refers to a scan no rule matched, on a code without a catch-all.</summary>
    Unmatched,

    /// <summary>Refers to a code its owner paused.</summary>
    Paused,

    /// <summary>Refers to content a browser redirect cannot carry yet, such as text, WiFi or a contact card.</summary>
    Undeliverable,
}
