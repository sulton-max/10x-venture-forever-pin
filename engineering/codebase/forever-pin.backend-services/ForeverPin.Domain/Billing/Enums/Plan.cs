namespace ForeverPin.Domain.Billing.Enums;

/// <summary>Defines the available subscription tiers.</summary>
public enum Plan
{
    /// <summary>Represents the free tier.</summary>
    Free,

    /// <summary>Represents the Basic paid tier, the one on sale.</summary>
    Basic,

    /// <summary>Represents the Pro paid tier; not on sale until its features ship.</summary>
    Pro,

    /// <summary>Represents the Agency paid tier; not on sale until its features ship.</summary>
    Agency,
}
