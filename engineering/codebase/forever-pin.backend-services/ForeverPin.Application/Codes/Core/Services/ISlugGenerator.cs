namespace ForeverPin.Application.Codes.Core.Services;

/// <summary>Defines generating short, URL-safe, unguessable slugs.</summary>
public interface ISlugGenerator
{
    /// <summary>Returns a new random slug.</summary>
    /// <remarks>The database unique index arbitrates collisions; callers retry a bounded number of times.</remarks>
    string Next();
}
