<script setup lang="ts">
import { Alert } from '@wow-two-beta/ui-vue/presentation/feedback';
import { computed } from 'vue';

import { ContrastExtensions, ContrastVerdict, type CodeStyleDto } from '@/domain/codes';

/** Defines the props of {@link ContrastCallout}. */
export interface ContrastCalloutProps {
  /** The style whose colors are checked. */
  readonly style: CodeStyleDto;
}

/** Renders advisory scanning guidance for low contrast, inverted colors or a transparent background. */
defineOptions({ name: 'ContrastCallout' });
const props = defineProps<ContrastCalloutProps>();

const advice = computed(() => {
  const { verdict, ratio } = ContrastExtensions.assess(props.style);
  const shown = ratio.toFixed(1);
  switch (verdict) {
    case ContrastVerdict.Transparent:
      return {
        severity: 'info' as const,
        text: 'Transparent background — make sure the code sits on a light, plain surface so it scans.',
      };
    case ContrastVerdict.Inverted:
      return {
        severity: 'warning' as const,
        text: `Light-on-dark inverts the code (${shown}:1) — many scanners fail. Darken the foreground or lighten the background.`,
      };
    case ContrastVerdict.Low:
      return {
        severity: 'warning' as const,
        text: `Very low contrast (${shown}:1) — the code likely won't scan. Darken the foreground or lighten the background.`,
      };
    case ContrastVerdict.Marginal:
      return {
        severity: 'warning' as const,
        text: `Contrast is a little low (${shown}:1) — test a scan before printing.`,
      };
    default:
      return null;
  }
});
</script>

<template>
  <Alert v-if="advice" :severity="advice.severity" :description="advice.text" />
</template>
