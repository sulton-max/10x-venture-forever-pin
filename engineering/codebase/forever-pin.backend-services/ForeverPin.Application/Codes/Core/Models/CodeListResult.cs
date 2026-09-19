namespace ForeverPin.Application.Codes.Core.Models;

/// <summary>Represents the outcome of listing an owner's codes.</summary>
public abstract record CodeListResult
{
    private CodeListResult() { }

    /// <summary>Represents the listed codes.</summary>
    /// <param name="Codes">The listed codes.</param>
    public sealed record Success(IReadOnlyList<CodeDto> Codes) : CodeListResult;
}
