namespace ForeverPin.Application.Codes.Core.Services;

/// <summary>Defines generating short, URL-safe, unguessable slugs.</summary>
public interface ISlugGenerator
{
    /// <summary>Returns a new random slug.</summary>
    /// <remarks>Check uniqueness against the store before use.</remarks>
    string Next();
}
