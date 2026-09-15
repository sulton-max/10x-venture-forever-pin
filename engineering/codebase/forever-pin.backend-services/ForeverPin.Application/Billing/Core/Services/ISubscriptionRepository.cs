using ForeverPin.Domain.Billing.Entities;

namespace ForeverPin.Application.Billing.Core.Services;

/// <summary>Defines reading and persisting subscriptions.</summary>
public interface ISubscriptionRepository
{
    /// <summary>Loads the user's subscription, or null when there is none.</summary>
    Task<SubscriptionEntity?> GetByUserAsync(Guid userId, CancellationToken ct);

    /// <summary>Loads a subscription by its Stripe subscription id (<c>sub_…</c>), or null.</summary>
    Task<SubscriptionEntity?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken ct);

    /// <summary>Inserts the user's subscription row, or overwrites the existing row's billing fields.</summary>
    Task<SubscriptionEntity> UpsertByUserAsync(SubscriptionEntity entity, CancellationToken ct);
}
