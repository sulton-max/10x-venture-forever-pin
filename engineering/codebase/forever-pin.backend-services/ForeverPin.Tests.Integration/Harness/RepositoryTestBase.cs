using ForeverPin.Persistence.DataContexts;

namespace ForeverPin.Tests.Integration.Harness;

/// <summary>Defines the xUnit collection sharing one test database across the repository tests.</summary>
[CollectionDefinition(Name)]
public sealed class RepositoryTestCollection : ICollectionFixture<ForeverPinTestDb>
{
    /// <summary>Holds the collection name every repository test class joins.</summary>
    public const string Name = "ForeverPin repository tests";
}

/// <summary>Provides repository tests with a shared database emptied before each test.</summary>
[Collection(RepositoryTestCollection.Name)]
public abstract class RepositoryTestBase(ForeverPinTestDb db) : IAsyncLifetime
{
    /// <summary>Gets the shared provider-switchable test database.</summary>
    protected ForeverPinTestDb Db { get; } = db;

    /// <summary>Creates a context on the active test database.</summary>
    protected AppDbContext NewContext() => Db.NewContext();

    /// <summary>Resets the shared database to empty before each test.</summary>
    public async Task InitializeAsync() => await Db.ResetAsync();

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;
}
