<script setup lang="ts">
import { ArrowUpRight, Check, CreditCard, Infinity as InfinityIcon } from 'lucide-vue-next';
import { Button } from '@wow-two-beta/fe/presentation/actions';
import { Card, Heading, Text } from '@wow-two-beta/fe/presentation/display';
import { Alert, Banner, DataRegion, MeterBar, SkeletonState } from '@wow-two-beta/fe/presentation/feedback';
import { computed, ref } from 'vue';

import {
  CheckoutConfirmation,
  useBillingSessions,
  useBillingStatus,
  useCheckoutConfirmation,
} from '@/application/billing';
import { FailureMessages } from '@/application/common';
import { PAID_PLANS, Plan } from '@/domain/billing';

import { PlanDisplays, subscriptionStatusDisplay } from '../PlanDisplays';

/** Defines the props of {@link BillingPage}. */
export interface BillingPageProps {
  /** The Stripe Checkout return state from the address, if any. */
  readonly checkoutReturn?: 'success' | 'cancelled';
  /** Clears the return state from the address once dismissed. */
  readonly onClearCheckoutReturn: () => void;
}

/** Holds the backend's unlimited-codes sentinel. */
const Unlimited = -1;

/** Holds the count format. */
const CountFormat = new Intl.NumberFormat();

/**
 * Renders the plan, usage and upgrades. A Checkout return stays provisional until the server confirms it,
 * and a snapshot that failed to load settles into neutral copy with a retry — never a made-up Free plan.
 */
defineOptions({ name: 'BillingPage' });
const props = defineProps<BillingPageProps>();

const status = useBillingStatus();
const sessions = useBillingSessions();
const confirmation = useCheckoutConfirmation(
  () => props.checkoutReturn === 'success',
  status.data,
  () => status.refetch(),
);
const redirectingTo = ref<Plan | 'portal' | null>(null);
const sessionFailure = ref<string | null>(null);

const snapshot = computed(() => status.data.value);
const plan = computed(() => snapshot.value?.plan ?? Plan.Free);
const standing = computed(() => subscriptionStatusDisplay(snapshot.value?.status ?? ''));
const isUnlimited = computed(() => snapshot.value?.limits.maxCodes === Unlimited);
const upgrades = computed(() =>
  PAID_PLANS.filter((candidate) => PlanDisplays[candidate].rank > PlanDisplays[plan.value].rank),
);

/** Opens a hosted Stripe page in this window; the page stays usable if the session cannot be created. */
async function open(target: Plan | 'portal'): Promise<void> {
  redirectingTo.value = target;
  sessionFailure.value = null;
  const result =
    target === 'portal' ? await sessions.portal.mutateAsync() : await sessions.checkout.mutateAsync(target);
  if (result.ok) {
    window.location.assign(result.value.url);
    return;
  }
  redirectingTo.value = null;
  sessionFailure.value = FailureMessages.describe(
    result.failure,
    target === 'portal' ? 'The billing portal could not be opened.' : 'Checkout could not be started.',
  );
}
</script>

<template>
  <div class="flex flex-col gap-6">
    <div>
      <Heading :level="1" size="xl" weight="bold">Billing</Heading>
      <Text color="muted">Manage your plan, usage, and payment method.</Text>
    </div>

    <Banner
      v-if="confirmation === CheckoutConfirmation.Pending"
      severity="info"
      title="Confirming your subscription…"
      description="Stripe finished Checkout; your plan updates here as soon as the payment is confirmed."
    />
    <Banner
      v-else-if="confirmation === CheckoutConfirmation.Confirmed"
      severity="success"
      title="You're all set."
      description="Your subscription is confirmed. Your new plan and limits are shown below."
      @close="props.onClearCheckoutReturn"
    />
    <Banner
      v-else-if="confirmation === CheckoutConfirmation.Delayed"
      severity="warning"
      title="Still waiting for confirmation."
      description="This can take a minute. Refresh this page shortly — you won't be charged twice."
      @close="props.onClearCheckoutReturn"
    />
    <Banner
      v-if="checkoutReturn === 'cancelled'"
      severity="warning"
      title="Checkout cancelled."
      description="No changes were made — you can pick a plan again whenever you're ready."
      @close="props.onClearCheckoutReturn"
    />

    <Alert v-if="sessionFailure" severity="danger" :description="sessionFailure" />

    <DataRegion
      class="flex flex-col gap-6"
      :is-first-load="status.isFirstLoad.value"
      :is-refreshing="status.isRefreshing.value"
      :error="status.error.value"
      :has-data="Boolean(snapshot)"
      unavailable-title="Billing isn't available right now"
      loading-label="Loading billing"
      @retry="status.refetch"
    >
      <template #skeleton>
        <SkeletonState class="h-40 w-full rounded-xl" />
        <SkeletonState class="h-64 w-full rounded-xl" />
      </template>
      <template v-if="snapshot">
        <Card class="surface-soft flex flex-col gap-5 p-6">
          <div class="flex flex-wrap items-start justify-between gap-4">
            <div>
              <div class="flex items-center gap-2">
                <span class="text-lg font-semibold">{{ PlanDisplays[plan].name }}</span>
                <span
                  class="inline-flex shrink-0 items-center gap-1.5 rounded-full bg-muted px-2 py-0.5 text-xs font-medium text-muted-foreground"
                >
                  <span
                    class="size-1.5 rounded-full"
                    :class="
                      plan === Plan.Free ? 'bg-subtle-foreground' : standing.isHealthy ? 'bg-accent' : 'bg-warning'
                    "
                  />
                  {{ plan === Plan.Free ? 'Free plan' : standing.label }}
                </span>
              </div>
              <Text size="sm" color="muted" class="mt-1">{{ PlanDisplays[plan].tagline }}</Text>
            </div>
            <Button
              v-if="plan !== Plan.Free"
              variant="outline"
              tone="neutral"
              :is-loading="redirectingTo === 'portal'"
              loading-text="Opening…"
              :is-disabled="redirectingTo !== null"
              @click="open('portal')"
            >
              <template #leading><CreditCard :size="16" aria-hidden="true" /></template>
              Manage billing
            </Button>
          </div>

          <div>
            <div class="mb-1.5 flex items-center justify-between text-sm">
              <span class="font-medium">Codes used</span>
              <span class="text-muted-foreground tabular-nums">
                {{ CountFormat.format(snapshot.usage.codeCount) }} /
                <InfinityIcon
                  v-if="isUnlimited"
                  :size="15"
                  class="-mt-0.5 inline align-middle"
                  aria-label="unlimited"
                />
                <template v-else>{{ CountFormat.format(snapshot.limits.maxCodes) }}</template>
              </span>
            </div>
            <MeterBar v-if="isUnlimited" :value="1" :max="1" :thresholds="[1, 1]" aria-label="Unlimited codes" />
            <MeterBar
              v-else
              :value="snapshot.usage.codeCount"
              :max="Math.max(snapshot.limits.maxCodes, 1)"
              :label="`${snapshot.usage.codeCount} of ${snapshot.limits.maxCodes} codes used`"
            />
            <Text
              v-if="!isUnlimited && snapshot.usage.codeCount >= snapshot.limits.maxCodes"
              size="sm"
              color="warning"
              class="mt-2"
            >
              You've reached your plan's code limit. Upgrade to create more — your existing codes keep working
              regardless.
            </Text>
          </div>
        </Card>

        <div v-if="upgrades.length > 0" class="flex flex-col gap-3">
          <Heading :level="2" size="md" weight="semibold">{{
            plan === Plan.Free ? 'Choose a plan' : 'Upgrade'
          }}</Heading>
          <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            <Card v-for="candidate in upgrades" :key="candidate" class="surface-soft flex flex-col gap-4 p-5">
              <div>
                <div class="flex items-baseline justify-between gap-2">
                  <span class="text-base font-semibold">{{ PlanDisplays[candidate].name }}</span>
                  <span class="text-sm text-muted-foreground">
                    <span class="text-xl font-bold text-foreground">{{ PlanDisplays[candidate].price }}</span
                    >{{ PlanDisplays[candidate].cadence }}
                  </span>
                </div>
                <Text size="sm" color="muted" class="mt-1">{{ PlanDisplays[candidate].tagline }}</Text>
              </div>
              <ul class="flex flex-1 flex-col gap-2 text-sm">
                <li
                  v-for="highlight in PlanDisplays[candidate].highlights"
                  :key="highlight"
                  class="flex items-start gap-2"
                >
                  <Check :size="15" class="mt-0.5 shrink-0 text-primary" aria-hidden="true" />
                  <span>{{ highlight }}</span>
                </li>
              </ul>
              <Button
                tone="primary"
                is-full-width
                :is-loading="redirectingTo === candidate"
                loading-text="Redirecting…"
                :is-disabled="redirectingTo !== null"
                @click="open(candidate)"
              >
                Upgrade to {{ PlanDisplays[candidate].name }}
                <template #trailing><ArrowUpRight :size="16" aria-hidden="true" /></template>
              </Button>
            </Card>
          </div>
        </div>
        <Card v-else class="surface-soft p-6">
          <Text size="sm" color="muted">
            You're on the top plan — nothing more to upgrade to. Use
            <span class="font-medium text-foreground">Manage billing</span> to update your payment method or cancel.
          </Text>
        </Card>
      </template>
    </DataRegion>
  </div>
</template>
