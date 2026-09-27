using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using ForeverPin.Persistence.DataContexts;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Audit;
using WoW.Two.Sdk.Backend.Beta.Testing.Data.EntityFrameworkCore;

namespace ForeverPin.Tests.Integration.Harness;

/// <summary>Provides the product's provider-owned test database and audit conventions.</summary>
public sealed class ForeverPinTestDb : RelationalTestDb<AppDbContext>
{
    /// <summary>Gets the controlled audit clock shared by this fixture's contexts.</summary>
    public FakeTimeProvider Clock { get; } = new();

    /// <inheritdoc />
    protected override void ApplyConventions(DbContextOptionsBuilder builder) =>
        builder.UseSnakeCaseNamingConvention()
            .AddInterceptors(new AuditInterceptor(Clock));

    /// <inheritdoc />
    protected override AppDbContext CreateContext(DbContextOptionsBuilder<AppDbContext> builder) =>
        new(builder.Options);
}
