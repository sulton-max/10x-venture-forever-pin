namespace ForeverPin.Domain.Codes.Core.Enums;

/// <summary>Defines the signal a routing rule matches on.</summary>
public enum RuleConditionType
{
    /// <summary>Represents a device-class match.</summary>
    Device,

    /// <summary>Represents an ISO country-code match.</summary>
    Country,

    /// <summary>Represents a primary-language match.</summary>
    Language,

    /// <summary>Represents a match on a daily time window <c>HH:mm-HH:mm</c>.</summary>
    TimeOfDay,
}
