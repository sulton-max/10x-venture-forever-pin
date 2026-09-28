<script setup lang="ts">
import { Heading, HeadingSize, Text } from '@wow-two-beta/ui-vue/presentation/display';

/** Defines the props of {@link MarketingHeading}. */
export interface MarketingHeadingProps {
  /** The small label above the title. */
  readonly eyebrow?: string;
  /** The title. */
  readonly title: string;
  /** The supporting sentence. */
  readonly description?: string;
  /** Whether the block is centered. Default `true`. */
  readonly isCentered?: boolean;
  /** The title's semantic level; `1` when it is the page's top heading. The visual size stays fixed. */
  readonly level?: 1 | 2;
}

/** Renders a section's eyebrow, title and description block. */
defineOptions({ name: 'MarketingHeading' });
withDefaults(defineProps<MarketingHeadingProps>(), {
  eyebrow: undefined,
  description: undefined,
  isCentered: true,
  level: 2,
});
</script>

<template>
  <div class="max-w-2xl" :class="isCentered ? 'mx-auto text-center' : undefined">
    <Text v-if="eyebrow" as="span" size="sm" weight="semibold" color="brand">{{ eyebrow }}</Text>
    <Heading :level="level" :size="HeadingSize.Xxl" weight="bold" class="mt-2">{{ title }}</Heading>
    <Text v-if="description" color="muted" class="mt-3">{{ description }}</Text>
  </div>
</template>
