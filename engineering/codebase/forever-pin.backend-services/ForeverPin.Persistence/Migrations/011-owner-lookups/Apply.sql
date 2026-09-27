CREATE INDEX ix_codes_user_id_created_at ON codes (user_id, created_at);
CREATE INDEX ix_subscriptions_stripe_subscription_id ON subscriptions (stripe_subscription_id);
