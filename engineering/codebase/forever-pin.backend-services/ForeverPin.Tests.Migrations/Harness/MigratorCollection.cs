using WoW.Two.Sdk.Backend.Beta.Testing.Data.Migrations;

namespace ForeverPin.Tests.Migrations.Harness;

/// <summary>Defines the xUnit collection sharing one drop-schema Postgres fixture across migrator tests.</summary>
[CollectionDefinition(Name)]
public sealed class MigratorCollection : ICollectionFixture<MigratorPostgresFixture>
{
    /// <summary>Holds the collection name every migrator test class joins.</summary>
    public const string Name = "forever-pin-migrator";
}
