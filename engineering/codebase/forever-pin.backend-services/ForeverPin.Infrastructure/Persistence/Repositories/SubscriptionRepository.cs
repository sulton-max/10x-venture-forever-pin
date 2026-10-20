using Microsoft.EntityFrameworkCore;
using ForeverPin.Application.Billing.Core.Services;
using ForeverPin.Domain.Billing.Entities;
using ForeverPin.Persistence.DataContexts;
using WoW.Two.Sdk.Beta.Be.Data.EntityFrameworkCore.Locks.Extensions;

namespace ForeverPin.Infrastructure.Persistence.Repositories;

/// <summary>Fetches and persists subscriptions via EF Core.</summary>
public sealed class SubscriptionRepository(AppDbContext db) : ISubscriptionRepository
{
    /// <inheritdoc />
    public Task<SubscriptionEntity?> GetByUserAsync(Guid userId, CancellationToken ct) =>
        db.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId, ct);

    /// <inheritdoc />
    public Task<SubscriptionEntity?> GetByStripeSubscriptionIdAsync(
        string stripeSubscriptionId,
        CancellationToken ct) =>
        db.Subscriptions.FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId, ct);

    /// <inheritdoc />
    public async Task<SubscriptionEntity> UpsertByUserAsync(SubscriptionEntity entity, CancellationToken ct)
    {
        await using var ownedTransaction = db.Database.CurrentTransaction is null
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        await db.Database.AcquireTransactionLocksAsync("owner", [entity.UserId], ct);

        // Upsert the single subscription row identified by user_id.
        var existing = await db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == entity.UserId, ct);

        if (existing is null)
        {
            db.Subscriptions.Add(entity);
            await db.SaveChangesAsync(ct);
            if (ownedTransaction is not null)
                await ownedTransaction.CommitAsync(ct);
            return entity;
        }

        // Stripe delivers out of order: an event older than the last one applied changes nothing.
        // Same-second events apply in arrival order; the webhook inbox already drops redeliveries.
        if (entity.LastEventAt is { } eventAt && existing.LastEventAt > eventAt)
            return existing;

        existing.Plan = entity.Plan;
        existing.Status = entity.Status;
        existing.StripeCustomerId = entity.StripeCustomerId;
        existing.StripeSubscriptionId = entity.StripeSubscriptionId;
        existing.CurrentPeriodEnd = entity.CurrentPeriodEnd;
        existing.LastEventAt = entity.LastEventAt ?? existing.LastEventAt;
        await db.SaveChangesAsync(ct);
        if (ownedTransaction is not null)
            await ownedTransaction.CommitAsync(ct);
        return existing;
    }
}
