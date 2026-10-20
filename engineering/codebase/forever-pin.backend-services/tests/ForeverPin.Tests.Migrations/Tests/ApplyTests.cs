using AwesomeAssertions;
using ForeverPin.Persistence.DataContexts;
using ForeverPin.Tests.Migrations.Harness;
using Microsoft.EntityFrameworkCore;
using WoW.Two.Sdk.Beta.Be.Data.Migrations.Bespoke;
using WoW.Two.Sdk.Beta.Be.Foundation.Results;
using WoW.Two.Sdk.Beta.Be.Testing.Data.Migrations;

namespace ForeverPin.Tests.Migrations.Tests;

/// <summary>Verifies the embedded product migrations against the current persistence model.</summary>
[Collection(MigratorCollection.Name)]
public sealed class ApplyTests(MigratorPostgresFixture fixture) : IAsyncLifetime
{
    [Fact]
    public async Task Apply_ShouldCreateProductSchema_WhenEmbeddedMigrationsRunOnEmptyDatabase()
    {
        await using var migrator = MigratorHarness.CreatePostgres(
            fixture.ConnectionString, typeof(AppDbContext).Assembly);

        var applied = await migrator.Runner.ApplyPendingAsync("product-test");
        var labels = applied.Should().BeOfType<Result<IReadOnlyList<string>>.Success>().Subject.Value;
        var embeddedLabels = typeof(AppDbContext).Assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith("Migrations/", StringComparison.Ordinal)
                && name.EndsWith("/Apply.sql", StringComparison.Ordinal))
            .Select(name => name.Split('/')[1]);
        labels.Should().BeEquivalentTo(embeddedLabels);
        labels.Should().Contain("010-rules-jsonb");

        foreach (var table in new[] { "codes", "scan_events", "subscriptions", "users", "inbox_messages" })
            (await migrator.HasTableAsync(table)).Should().BeTrue();
        (await migrator.HasTableAsync("routing_rules")).Should().BeFalse();
        (await migrator.HasIndexAsync("ix_codes_slug")).Should().BeTrue();
        (await migrator.HasIndexAsync("ix_subscriptions_user_id")).Should().BeTrue();
        (await migrator.HasIndexAsync("ix_users_google_subject")).Should().BeTrue();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(fixture.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using var context = new AppDbContext(options);
        // Materializing each mapped table checks columns and converters against the actual SQL resources.
        (await context.Codes.ToListAsync()).Should().BeEmpty();
        (await context.ScanEvents.ToListAsync()).Should().BeEmpty();
        (await context.Subscriptions.ToListAsync()).Should().BeEmpty();
        (await context.Users.ToListAsync()).Should().BeEmpty();

        var status = (await migrator.Runner.GetStatusAsync()).Should()
            .BeOfType<Result<MigrationStatus>.Success>().Subject.Value;
        status.Applied.Should().HaveCount(labels.Count);
        status.Pending.Should().BeEmpty();
        status.Drifted.Should().BeEmpty();
        status.Orphaned.Should().BeEmpty();
        (await migrator.ReadHistoryAsync()).Should().OnlyContain(row => row.AppliedBy == "product-test");

        var repeated = await migrator.Runner.ApplyPendingAsync("second-host");
        repeated.Should().BeOfType<Result<IReadOnlyList<string>>.Success>().Subject.Value.Should().BeEmpty();
    }

    /// <inheritdoc />
    public Task InitializeAsync() => fixture.ResetAsync().AsTask();

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;
}
