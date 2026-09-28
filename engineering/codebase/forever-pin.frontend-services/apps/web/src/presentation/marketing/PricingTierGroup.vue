<script setup lang="ts">
import { PricingTiers } from './MarketingContent';
import PricingTierCard from './PricingTierCard.vue';

/** Renders the plan grid; marketing stays backend-independent and defers Checkout to in-app billing. */
defineOptions({ name: 'PricingTierGroup' });
</script>

<template>
  <div class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
    <PricingTierCard v-for="tier in PricingTiers" :key="tier.id" :tier="tier" />
  </div>
</template>
