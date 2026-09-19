namespace ForeverPin.Application.Codes.Core.Models;

/// <summary>Represents the outcome of updating a code.</summary>
public abstract record CodeUpdateResult
{
    private CodeUpdateResult() { }

    /// <summary>Represents an updated code.</summary>
    /// <param name="Code">The updated code.</param>
    public sealed record Success(CodeDto Code) : CodeUpdateResult;
}
