using ForeverPin.Application.Codes.Core.Services;
using WoW.Two.Sdk.Beta.Be.Foundation.Identifiers.Generators;

namespace ForeverPin.Infrastructure.Codes.Core.Services;

/// <summary>Provides cryptographically random base62 slugs.</summary>
public sealed class SlugGenerator(IIdGenerator generator) : ISlugGenerator
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int Length = 7;

    /// <inheritdoc />
    public string Next() => generator.Generate(Length, Alphabet);
}
