using Microsoft.EntityFrameworkCore;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Persistence.Constants;
using ForeverPin.Persistence.DataContexts;
using ForeverPin.Application.Codes.Core.Enums;
using ForeverPin.Common.Domain.Codes.Core.Enums;
using WoW.Two.Sdk.Beta.Be.Caching.Invalidation.Extensions;
using WoW.Two.Sdk.Beta.Be.Data.EntityFrameworkCore.Locks.Extensions;
using WoW.Two.Sdk.Beta.Be.Data.EntityFrameworkCore.Uniqueness.Extensions;

namespace ForeverPin.Infrastructure.Persistence.Repositories;

/// <summary>Fetches and persists codes via EF Core.</summary>
public sealed class CodeRepository(AppDbContext db) : ICodeRepository
{
    /// <inheritdoc />
    public async Task<CodeInsertStatus> AddWithinLimitAsync(CodeEntity code, int limit, CancellationToken ct)
    {
        await using var ownedTransaction = db.Database.CurrentTransaction is null
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        await db.Database.AcquireTransactionLocksAsync("owner", [code.UserId], ct);

        // Only dynamic codes count against the plan; a static code is always added.
        if (code.Mode is ContentMode.Dynamic
            && await db.Codes.CountAsync(c => c.UserId == code.UserId && c.Mode == ContentMode.Dynamic, ct) >= limit)
            return CodeInsertStatus.PlanLimitReached;

        // A taken slug rolls back to a savepoint and detaches the insert; the caller's transaction stays usable.
        db.Codes.Add(code);
        if (!await db.TrySaveChangesUniqueAsync("ix_codes_slug", ct))
            return CodeInsertStatus.SlugConflict;

        if (ownedTransaction is not null)
            await ownedTransaction.CommitAsync(ct);
        return CodeInsertStatus.Added;
    }

    /// <inheritdoc />
    public async Task<CodeEntity> AddAsync(CodeEntity code, CancellationToken ct)
    {
        db.Codes.Add(code);
        await db.SaveChangesAsync(ct);
        return code;
    }

    /// <inheritdoc />
    public Task<CodeEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Codes.FirstOrDefaultAsync(c => c.Id == id, ct);

    /// <inheritdoc />
    public Task<CodeEntity?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct) =>
        db.Codes.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<CodeEntity>> ListByUserAsync(Guid userId, string? q, CancellationToken ct)
    {
        var query = db.Codes
            .Where(c => c.UserId == userId);

        if (!string.IsNullOrWhiteSpace(q))
        {
            // Normalize both operands for case-insensitive matching on Postgres and SQLite.
            var term = q.Trim().ToLowerInvariant();
            query = query.Where(c => c.Name.ToLower().Contains(term));
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<CodeEntity> UpdateAsync(CodeEntity code, CancellationToken ct)
    {
        if (db.Entry(code).State == EntityState.Detached)
            throw new InvalidOperationException("Load and mutate the tracked code before updating it.");

        // The replacement rule set is saved in the code's jsonb column.
        await SaveAndEvictAsync(code.Slug, ct);
        return code;
    }

    /// <inheritdoc />
    public async Task<CodeEntity?> SetActiveAsync(Guid id, Guid userId, bool isActive, CancellationToken ct)
    {
        var code = await db.Codes
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

        if (code is null)
            return null;

        code.IsActive = isActive;
        await SaveAndEvictAsync(code.Slug, ct);
        return code;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct)
    {
        await using var ownedTransaction = db.Database.CurrentTransaction is null
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        var owned = db.Codes.Where(c => c.Id == id && c.UserId == userId);
        var slug = await owned.Select(c => c.Slug).FirstOrDefaultAsync(ct);

        // Owner-scoped hard delete; the rules ride the row, so there is nothing to cascade. No-op if not theirs.
        var removed = await owned.ExecuteDeleteAsync(ct);
        if (removed > 0 && slug is not null)
            await db.Database.PublishCacheKeyInvalidationAsync(CodeCacheConstants.KeyPrefix + slug, cancellationToken: ct);

        if (ownedTransaction is not null)
            await ownedTransaction.CommitAsync(ct);
        return removed > 0;
    }

    /// <inheritdoc />
    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct) =>
        db.Codes.AnyAsync(c => c.Slug == slug, ct);

    /// <inheritdoc />
    public Task<int> CountDynamicByUserAsync(Guid userId, CancellationToken ct) =>
        db.Codes.CountAsync(c => c.UserId == userId && c.Mode == ContentMode.Dynamic, ct);

    /// <summary>Saves the pending change and evicts the code from every redirect host once the change commits.</summary>
    /// <remarks>Joins the caller's transaction when present; a static code has no slug and no cached route.</remarks>
    private async Task SaveAndEvictAsync(string? slug, CancellationToken ct)
    {
        await using var ownedTransaction = db.Database.CurrentTransaction is null
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        await db.SaveChangesAsync(ct);
        if (slug is not null)
            await db.Database.PublishCacheKeyInvalidationAsync(CodeCacheConstants.KeyPrefix + slug, cancellationToken: ct);

        if (ownedTransaction is not null)
            await ownedTransaction.CommitAsync(ct);
    }

    /// <inheritdoc />
    public async Task<int> ReassignOwnerAsync(Guid fromUserId, Guid toUserId, CancellationToken ct)
    {
        await db.Database.AcquireTransactionLocksAsync("owner", [fromUserId, toUserId], ct);
        return await db.Codes
            .Where(c => c.UserId == fromUserId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.UserId, toUserId), ct);
    }
}
