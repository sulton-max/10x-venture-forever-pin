<script setup lang="ts">
import brandMarkUrl from './assets/foreverpin-navbar-logo@2x.png';
import brandWordmarkUrl from './assets/wordmark-ink.png';

/** Renders the brand mark and wordmark — shared by the marketing header, footer and the app shell. */
defineOptions({ name: 'BrandGlyph' });
</script>

<template>
  <span class="inline-flex shrink-0 items-center gap-2">
    <span
      class="size-7 shrink-0 bg-primary mask-alpha mask-contain mask-center mask-no-repeat"
      :style="{ maskImage: `url(${brandMarkUrl})`, WebkitMaskImage: `url(${brandMarkUrl})` }"
      aria-hidden="true"
    />
    <span
      class="h-5 w-[94px] shrink-0 bg-foreground mask-alpha mask-contain mask-center mask-no-repeat"
      :style="{
        maskImage: `url(${brandWordmarkUrl})`,
        WebkitMaskImage: `url(${brandWordmarkUrl})`,
      }"
      aria-hidden="true"
    />
    <span class="sr-only">ForeverPin</span>
  </span>
</template>
