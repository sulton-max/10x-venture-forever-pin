using ForeverPin.Domain.Billing.Enums;
using WoW.Two.Sdk.Backend.Beta.Data.Abstractions;

namespace ForeverPin.Domain.Billing.Entities;

/// <summary>Represents a user's Stripe subscription.</summary>
public sealed record SubscriptionEntity : IKeyedEntity<Guid>, IHasTableName, IAuditable
{
    /// <summary>Gets the storage table name.</summary>
    public static string TableName => "subscriptions";

    /// <summary>Gets or sets the primary key.</summary>
    public required Guid Id { get; set; }

    // Carried through Stripe Checkout as client_reference_id.
    /// <summary>Gets or sets the id of the user who owns this subscription.</summary>
    public required Guid UserId { get; set; }

    /// <summary>Gets or sets the subscription tier.</summary>
    public required Plan Plan { get; set; }

    /// <summary>Gets or sets the lifecycle status.</summary>
    public required SubscriptionStatus Status { get; set; }

    /// <summary>Gets or sets the Stripe customer id.</summary>
    public required string StripeCustomerId { get; set; }

    /// <summary>Gets or sets the Stripe subscription id.</summary>
    public required string StripeSubscriptionId { get; set; }

    /// <summary>Gets or sets the current billing period's end, or null when unknown.</summary>
    public DateTimeOffset? CurrentPeriodEnd { get; set; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the last-update timestamp.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
