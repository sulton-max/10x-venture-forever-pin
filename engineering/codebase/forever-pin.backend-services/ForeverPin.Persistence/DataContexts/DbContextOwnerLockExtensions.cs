using System.Buffers.Binary;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace ForeverPin.Persistence.DataContexts;

/// <summary>Serializes writes affecting an owner's codes or subscription across database clients.</summary>
public static class DbContextOwnerLockExtensions
{
    /// <summary>Locks owners in a stable order for the lifetime of the caller's transaction.</summary>
    public static async Task AcquireOwnerLocksAsync(
        this AppDbContext db, IEnumerable<Guid> ownerIds, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Owner writes require an explicit database transaction.");

        if (db.Database.IsSqlite())
            return;

        if (!db.Database.IsNpgsql())
            throw new NotSupportedException("Owner write locks require PostgreSQL or SQLite.");

        // Hash collisions only serialize unrelated owners; they cannot weaken exclusion.
        var keys = ownerIds
            .Select(id => BinaryPrimitives.ReadInt64LittleEndian(SHA256.HashData(id.ToByteArray())))
            .Distinct()
            .Order();

        foreach (var key in keys)
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({key})", ct);
    }
}
