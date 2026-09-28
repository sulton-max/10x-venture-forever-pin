import { computed, onScopeDispose, ref, toValue, watch, type MaybeRefOrGetter } from 'vue';

import { Plan, type BillingStatus } from '@/domain/billing';

/** Holds how often the snapshot is re-read while a completed Checkout awaits the server's confirmation. */
const PollIntervalMs = 2_000;

/** Holds how many re-reads run before the page stops waiting. */
const MaxPolls = 10;

/** Defines where a Checkout return stands. */
export const CheckoutConfirmation = {
  /** Refers to no Checkout return. */
  None: 'none',
  /** Refers to a completed Checkout the server has not confirmed yet. */
  Pending: 'pending',
  /** Refers to a subscription the server confirmed. */
  Confirmed: 'confirmed',
  /** Refers to a confirmation that did not arrive in time. */
  Delayed: 'delayed',
} as const;

export type CheckoutConfirmation = (typeof CheckoutConfirmation)[keyof typeof CheckoutConfirmation];

/**
 * Treats a Checkout return as provisional: the address only says the person finished Checkout, so the
 * snapshot is re-read on a bounded schedule until the server reports a paid plan.
 */
export function useCheckoutConfirmation(
  isReturning: MaybeRefOrGetter<boolean>,
  status: MaybeRefOrGetter<BillingStatus | undefined>,
  refetch: () => unknown,
) {
  const polls = ref(0);
  let timer: ReturnType<typeof setTimeout> | undefined;

  const isConfirmed = computed(() => {
    const snapshot = toValue(status);
    return Boolean(snapshot && snapshot.plan !== Plan.Free);
  });

  const state = computed<CheckoutConfirmation>(() => {
    if (!toValue(isReturning)) return CheckoutConfirmation.None;
    if (isConfirmed.value) return CheckoutConfirmation.Confirmed;
    return polls.value >= MaxPolls ? CheckoutConfirmation.Delayed : CheckoutConfirmation.Pending;
  });

  watch(
    [state, () => toValue(status)],
    () => {
      clearTimeout(timer);
      if (state.value !== CheckoutConfirmation.Pending || !toValue(status)) return;
      timer = setTimeout(() => {
        polls.value += 1;
        refetch();
      }, PollIntervalMs);
    },
    { immediate: true },
  );
  onScopeDispose(() => clearTimeout(timer));

  return state;
}
