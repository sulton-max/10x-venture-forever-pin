import { useAppMutation, useAppQuery } from '@wow-two-beta/fe/query';

import type { Plan } from '@/domain/billing';
import { billingApi } from '@/integration/billing';

import { BillingQueryKeys } from './BillingQueryKeys';

/** Reads the owner's plan, subscription status, limits and usage. */
export function useBillingStatus() {
  return useAppQuery({ key: BillingQueryKeys.status, queryFn: ({ signal }) => billingApi.getStatus(signal) });
}

/** Provides the hosted billing sessions: Checkout for a paid plan and the Customer Portal. */
export function useBillingSessions() {
  return {
    /** Creates a Checkout session for a paid plan; the caller navigates to its address. */
    checkout: useAppMutation({
      mutationFn: (plan: Plan, { signal }) => billingApi.createCheckout(plan, signal),
    }),
    /** Creates a Customer Portal session; fails before the first subscription. */
    portal: useAppMutation({
      mutationFn: (_: void, { signal }) => billingApi.createPortal(signal),
    }),
  };
}
