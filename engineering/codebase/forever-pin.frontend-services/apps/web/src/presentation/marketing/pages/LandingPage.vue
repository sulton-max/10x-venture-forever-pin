<script setup lang="ts">
import { ArrowRight, Infinity as InfinityIcon } from '@lucide/vue';
import { Button } from '@wow-two-beta/ui-vue/presentation/actions';
import { Heading, Text } from '@wow-two-beta/ui-vue/presentation/display';
import { RouterLink } from 'vue-router';

import { AppPaths } from '@/presentation/common';

import CallToActionSection from '../CallToActionSection.vue';
import ComparisonTable from '../ComparisonTable.vue';
import FaqGroup from '../FaqGroup.vue';
import HeroCodePreview from '../hero/HeroCodePreview.vue';
import HeroSimulationRenderer from '../hero/HeroSimulationRenderer.vue';
import { Faqs, Features, Steps } from '../MarketingContent';
import MarketingHeading from '../MarketingHeading.vue';
import MarketingSection from '../MarketingSection.vue';
import NumberedFeatureCard from '../NumberedFeatureCard.vue';
import PricingTierGroup from '../PricingTierGroup.vue';
import RoutingDemoCard from '../RoutingDemoCard.vue';

/** Renders the landing page: hero, features, steps, routing demo, pricing, comparison and questions. */
defineOptions({ name: 'LandingPage' });
</script>

<template>
  <section class="relative overflow-hidden">
    <HeroSimulationRenderer />
    <div class="relative z-10 mx-auto grid w-full max-w-6xl items-center gap-12 px-6 py-16 sm:py-24 lg:grid-cols-2">
      <div>
        <span
          class="inline-flex items-center gap-2 rounded-full border border-border bg-card px-3 py-1 text-xs text-muted-foreground"
        >
          <InfinityIcon :size="14" class="text-primary" aria-hidden="true" />
          Codes never expire — on every plan
        </span>
        <Heading :level="1" size="3xl" weight="bold" class="mt-5 leading-[1.1] sm:text-5xl">
          A QR code smart enough to route every scan — and that you'll never reprint.
        </Heading>
        <Text size="lg" color="muted" class="mt-5">
          Programmable routing, every code type, one flat price. Print it once; reprogram it forever; it never expires
          on you.
        </Text>
        <div class="mt-8 flex flex-wrap gap-3">
          <Button as-child tone="primary">
            <RouterLink :to="AppPaths.newCode()">Get started free</RouterLink>
          </Button>
          <Button as-child tone="neutral" variant="outline">
            <RouterLink :to="AppPaths.pricing()">See pricing</RouterLink>
          </Button>
        </div>
        <Text size="xs" color="muted" class="mt-4">No account required · Unlimited scans · Free forever tier</Text>
      </div>
      <div class="flex justify-center lg:justify-end">
        <HeroCodePreview />
      </div>
    </div>
  </section>

  <MarketingSection id="features">
    <MarketingHeading
      eyebrow="Features"
      title="Everything a printed code should do"
      description="The genuinely useful parts of a QR platform — unbundled from the up-sell and made the default."
    />
    <div class="mt-12 grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
      <NumberedFeatureCard
        v-for="(feature, index) in Features"
        :key="feature.title"
        :highlight="feature"
        :index="index"
      />
    </div>
  </MarketingSection>

  <MarketingSection is-muted>
    <MarketingHeading
      eyebrow="How it works"
      title="Print once. Reprogram forever."
      description="Three steps from idea to a code you'll never have to reprint."
    />
    <div class="mt-12 grid grid-cols-1 gap-5 md:grid-cols-3">
      <NumberedFeatureCard v-for="(step, index) in Steps" :key="step.title" :highlight="step" :index="index" />
    </div>
  </MarketingSection>

  <MarketingSection>
    <MarketingHeading
      eyebrow="Smart routing"
      title="One code, many destinations"
      description="The headline feature — not a hidden premium toggle. Route every scan by who's scanning and when."
    />
    <div class="mt-12">
      <RoutingDemoCard />
    </div>
  </MarketingSection>

  <MarketingSection is-muted>
    <MarketingHeading
      eyebrow="Pricing"
      title="Flat plans. Unlimited scans on all of them."
      description="Smart routing and a custom domain from $5 — not buried in a $30+ tier."
    />
    <div class="mt-12">
      <PricingTierGroup />
    </div>
    <div class="mt-8 text-center">
      <Button as-child variant="ghost" tone="primary">
        <RouterLink :to="AppPaths.pricing()" class="inline-flex items-center gap-1">
          Compare plans in detail <ArrowRight :size="15" aria-hidden="true" />
        </RouterLink>
      </Button>
    </div>
  </MarketingSection>

  <MarketingSection>
    <MarketingHeading
      title="Why we're different"
      description="The calm cut of a crowded category. Here's what changes when codes are treated as infrastructure, not hostages."
    />
    <ComparisonTable class="mt-12" />
  </MarketingSection>

  <MarketingSection is-muted>
    <MarketingHeading title="Questions, answered" />
    <div class="mt-12">
      <FaqGroup :items="Faqs" />
    </div>
  </MarketingSection>

  <!-- The tinted questions band ends flush, so the closing call to action brings its own top space. -->
  <CallToActionSection class="pt-16 sm:pt-20" />
</template>
