namespace ForeverPin.Application.Codes.Core.Models;

/// <summary>Represents the outcome of creating a code.</summary>
public abstract record CodeCreateResult
{
    private CodeCreateResult() { }

    /// <summary>Represents a created code.</summary>
    /// <param name="Code">The created code.</param>
    public sealed record Success(CodeDto Code) : CodeCreateResult;
}
