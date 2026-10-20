-- The Solo tier is now Basic: the one paid plan on sale.
UPDATE subscriptions SET plan = 'basic' WHERE plan = 'solo';
