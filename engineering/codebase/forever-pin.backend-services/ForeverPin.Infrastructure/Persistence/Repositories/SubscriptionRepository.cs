using Microsoft.EntityFrameworkCore;
using ForeverPin.Application.Billing.Core.Services;
using ForeverPin.Domain.Billing.Entities;
using ForeverPin.Persistence.DataContexts;

namespace ForeverPin.Infrastructure.Persistence.Repositories;

/// <summary>Fetches and persists subscriptions via EF Core.</summary>
public sealed class SubscriptionRepository(AppDbContext db) : ISubscriptionRepository
{
    /// <inheritdoc />
    public Task<SubscriptionEntity?> GetByUserAsync(Guid userId, CancellationToken ct) =>
        db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId, ct);

    /// <inheritdoc />
    public Task<SubscriptionEntity?> GetByStripeSubscriptionIdAsync(
        string stripeSubscriptionId,
        CancellationToken ct) =>
        db.Subscriptions.FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId, ct);

    /// <inheritdoc />
    public async Task<SubscriptionEntity> UpsertByUserAsync(SubscriptionEntity entity, CancellationToken ct)
    {
        // Upsert the single subscription row identified by user_id.
        var existing = await db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == entity.UserId, ct);

        if (existing is null)
        {
            db.Subscriptions.Add(entity);
            await db.SaveChangesAsync(ct);
            return entity;
        }

        existing.Plan = entity.Plan;
        existing.Status = entity.Status;
        existing.StripeCustomerId = entity.StripeCustomerId;
        existing.StripeSubscriptionId = entity.StripeSubscriptionId;
        existing.CurrentPeriodEnd = entity.CurrentPeriodEnd;
        await db.SaveChangesAsync(ct);
        return existing;
    }
}
