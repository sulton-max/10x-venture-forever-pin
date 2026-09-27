namespace ForeverPin.Persistence.Constants;

/// <summary>Contains the cache names shared by the hosts that write codes and the hosts that cache them.</summary>
public static class CodeCacheConstants
{
    /// <summary>Holds the prefix of a cached code's key; the code's public slug follows it.</summary>
    public const string KeyPrefix = "code:";
}
