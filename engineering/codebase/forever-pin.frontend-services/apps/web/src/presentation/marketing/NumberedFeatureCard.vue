<script setup lang="ts">
import { FeatureCard } from '@wow-two-beta/fe/presentation/display';
import { computed } from 'vue';

import type { HighlightModel } from './MarketingContent';

/** Defines the props of {@link NumberedFeatureCard}. */
export interface NumberedFeatureCardProps {
  /** The feature or step to show. */
  readonly highlight: HighlightModel;
  /** The zero-based position; the card shows it as a large background numeral. */
  readonly index: number;
}

/** Renders a feature or step as a card with its position as a faint background numeral. */
defineOptions({ name: 'NumberedFeatureCard' });
const props = defineProps<NumberedFeatureCardProps>();

const numeral = computed(() => String(props.index + 1).padStart(2, '0'));
</script>

<template>
  <FeatureCard :title="highlight.title" :description="highlight.body" :data-number="numeral" class="numbered-card">
    <template #icon>
      <component :is="highlight.icon" :size="22" />
    </template>
  </FeatureCard>
</template>
