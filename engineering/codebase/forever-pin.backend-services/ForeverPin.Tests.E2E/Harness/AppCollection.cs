namespace ForeverPin.Tests.E2E.Harness;

/// <summary>Defines the shared two-host E2E collection.</summary>
[CollectionDefinition(Name)]
public sealed class AppCollection : ICollectionFixture<AppFixture>
{
    /// <summary>Gets the collection name used by product E2E tests.</summary>
    public const string Name = "forever-pin-e2e";
}
