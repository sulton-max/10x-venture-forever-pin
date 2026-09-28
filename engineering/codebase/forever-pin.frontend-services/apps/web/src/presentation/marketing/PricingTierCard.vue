<script setup lang="ts">
import { Button } from '@wow-two-beta/ui-vue/presentation/actions';
import { PricingCard } from '@wow-two-beta/ui-vue/presentation/display';
import { RouterLink } from 'vue-router';

import { AppPaths } from '@/presentation/common';

import type { PricingTierModel } from './MarketingContent';

/** Defines the props of {@link PricingTierCard}. */
export interface PricingTierCardProps {
  /** The plan to show. */
  readonly tier: PricingTierModel;
}

/** Renders one plan; Free opens the guest builder and paid plans open in-app billing. */
defineOptions({ name: 'PricingTierCard' });
defineProps<PricingTierCardProps>();
</script>

<template>
  <PricingCard
    :name="tier.name"
    :price="tier.price"
    :cadence="tier.cadence"
    :tagline="tier.tagline"
    :features="tier.features"
    :featured="tier.featured"
  >
    <Button
      as-child
      :tone="tier.featured ? 'primary' : 'neutral'"
      :variant="tier.featured ? 'solid' : 'outline'"
      is-full-width
    >
      <RouterLink :to="tier.id === 'free' ? AppPaths.newCode() : AppPaths.billing()">{{ tier.cta }}</RouterLink>
    </Button>
  </PricingCard>
</template>
