using Microsoft.EntityFrameworkCore;
using ForeverPin.Domain.Billing.Entities;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Domain.Identity.Entities;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Naming;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Sqlite;

namespace ForeverPin.Persistence.DataContexts;

/// <summary>Provides access to the ForeverPin database.</summary>
/// <remarks>Author schema changes in <c>Migrations/NNN-name/Apply.sql</c>, never through EF.</remarks>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : AppDbContextBase(options)
{
    /// <summary>Gets the code set.</summary>
    public DbSet<CodeEntity> Codes => Set<CodeEntity>();


    /// <summary>Gets the append-only scan/click events set.</summary>
    public DbSet<ScanEventEntity> ScanEvents => Set<ScanEventEntity>();

    /// <summary>Gets the Stripe subscriptions set.</summary>
    public DbSet<SubscriptionEntity> Subscriptions => Set<SubscriptionEntity>();

    /// <summary>Gets the registered-account set.</summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Base applies this assembly's IEntityTypeConfiguration<T> and SDK conventions first.
        base.OnModelCreating(modelBuilder);

        // Store every enum property (nullable and non-nullable) as snake_case text — bulk via the SDK helper.
        modelBuilder.ApplyEnumStringConversions();

        // Use the binary timestamp conversion only for SQLite, which lacks native DateTimeOffset support.
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            modelBuilder.ApplyDateTimeOffsetToBinaryConversion();
    }
}
