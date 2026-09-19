namespace ForeverPin.Application.Codes.Core.Models;

/// <summary>Represents the outcome of fetching a single code.</summary>
public abstract record CodeGetByIdResult
{
    private CodeGetByIdResult() { }

    /// <summary>Represents a fetched code.</summary>
    /// <param name="Code">The fetched code.</param>
    public sealed record Success(CodeDto Code) : CodeGetByIdResult;
}
