<script setup lang="ts">
/** Defines the props of {@link MarketingSection}. */
export interface MarketingSectionProps {
  /** The in-page anchor id. */
  readonly id?: string;
  /** Whether the band carries the muted tint. */
  readonly isMuted?: boolean;
}

/** Renders a full-bleed content band whose inner column owns the site's `max-w-6xl` width. */
defineOptions({ name: 'MarketingSection' });
defineProps<MarketingSectionProps>();
</script>

<template>
  <section :id="id" :class="isMuted ? 'bg-muted/40' : undefined">
    <div class="mx-auto w-full max-w-6xl px-6 py-16 sm:py-20">
      <slot />
    </div>
  </section>
</template>
