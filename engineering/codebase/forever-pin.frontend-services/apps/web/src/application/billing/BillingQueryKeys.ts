/** Holds the cache keys of billing reads. */
export const BillingQueryKeys = {
  /** The key of the owner's billing snapshot. */
  status: ['billing', 'status'] as const,
} as const;
