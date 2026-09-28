<script setup lang="ts">
import { Text } from '@wow-two-beta/ui-vue/presentation/display';

import CallToActionSection from '../CallToActionSection.vue';
import ComparisonTable from '../ComparisonTable.vue';
import FaqGroup from '../FaqGroup.vue';
import { PricingFaqs } from '../MarketingContent';
import MarketingHeading from '../MarketingHeading.vue';
import MarketingSection from '../MarketingSection.vue';
import PricingTierGroup from '../PricingTierGroup.vue';

/** Renders the pricing page: plans, the comparison and pricing questions. */
defineOptions({ name: 'PricingPage' });
</script>

<template>
  <MarketingSection>
    <MarketingHeading
      eyebrow="Pricing"
      title="Flat pricing. Unlimited scans. Codes that never expire."
      description="Every plan includes unlimited scans and the never-expire promise. Pay for more codes and routing power — never for the scans themselves."
      :level="1"
    />
    <div class="mt-12">
      <PricingTierGroup />
    </div>
    <Text size="sm" color="muted" align="center" class="mt-6">
      All plans: unlimited scans · codes never deactivate on downgrade · export &amp; delete anytime.
    </Text>
  </MarketingSection>

  <MarketingSection is-muted>
    <MarketingHeading
      title="How we compare"
      description="The same three things people actually want — smart routing, a custom domain, and un-capped scans — without the $30+ paywall."
    />
    <div class="mt-12 overflow-x-auto">
      <ComparisonTable />
    </div>
  </MarketingSection>

  <MarketingSection>
    <MarketingHeading title="Pricing questions" />
    <div class="mt-12">
      <FaqGroup :items="PricingFaqs" />
    </div>
  </MarketingSection>

  <CallToActionSection />
</template>
