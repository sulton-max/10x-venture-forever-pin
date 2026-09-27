namespace ForeverPin.Tests.Integration.Harness;

/// <summary>Defines the xUnit collection sharing one test database across the repository tests.</summary>
[CollectionDefinition(Name)]
public sealed class RepositoryTestCollection : ICollectionFixture<ForeverPinTestDb>
{
    /// <summary>Holds the collection name every repository test class joins.</summary>
    public const string Name = "ForeverPin repository tests";
}
