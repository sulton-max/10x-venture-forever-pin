using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ForeverPin.Domain.Billing.Entities;

namespace ForeverPin.Persistence.Configurations;

/// <summary>Configures the subscriptions table mapping.</summary>
public sealed class SubscriptionEntityConfiguration : IEntityTypeConfiguration<SubscriptionEntity>
{
    public void Configure(EntityTypeBuilder<SubscriptionEntity> builder)
    {
        builder.ToTable(SubscriptionEntity.TableName);

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.StripeSubscriptionId)
            .HasDatabaseName("ix_subscriptions_stripe_subscription_id");

        // One live subscription per user — UserId is the lookup key and Stripe Checkout client_reference_id.
        builder
            .HasIndex(e => e.UserId)
            .IsUnique();
    }
}
