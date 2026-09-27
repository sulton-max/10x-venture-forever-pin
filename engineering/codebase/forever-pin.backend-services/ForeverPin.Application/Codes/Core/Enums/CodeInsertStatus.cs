namespace ForeverPin.Application.Codes.Core.Enums;

/// <summary>Identifies the outcome of an owner-scoped code insert.</summary>
public enum CodeInsertStatus
{
    /// <summary>The code was inserted; an existing caller transaction still owns its commit.</summary>
    Added,

    /// <summary>The owner's plan does not allow another code.</summary>
    PlanLimitReached,

    /// <summary>The proposed public slug was claimed by another code.</summary>
    SlugConflict,
}
