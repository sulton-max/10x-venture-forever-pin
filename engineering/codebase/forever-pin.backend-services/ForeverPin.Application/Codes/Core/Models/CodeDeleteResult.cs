namespace ForeverPin.Application.Codes.Core.Models;

/// <summary>Represents the outcome of deleting a code.</summary>
public abstract record CodeDeleteResult
{
    private CodeDeleteResult() { }

    /// <summary>Represents a deleted code.</summary>
    public sealed record Success : CodeDeleteResult;
}
