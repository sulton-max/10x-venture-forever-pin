<script setup lang="ts">
import { QrCode } from '@lucide/vue';

/** Renders the brand mark and wordmark — shared by the marketing header, footer and the app shell. */
defineOptions({ name: 'BrandGlyph' });
</script>

<template>
  <span class="inline-flex shrink-0 items-center gap-2 font-bold">
    <span class="grid size-7 shrink-0 place-items-center rounded-md bg-primary text-primary-foreground">
      <QrCode :size="18" />
    </span>
    <span class="text-lg tracking-tight whitespace-nowrap">ForeverPin</span>
  </span>
</template>
