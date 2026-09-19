namespace ForeverPin.Application.Codes.Core.Models;

/// <summary>Represents the outcome of enabling or disabling a code.</summary>
public abstract record CodeSetActiveResult
{
    private CodeSetActiveResult() { }

    /// <summary>Represents a code with its updated active state.</summary>
    /// <param name="Code">The updated code.</param>
    public sealed record Success(CodeDto Code) : CodeSetActiveResult;
}
