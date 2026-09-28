<script setup lang="ts">
import StaticQrPreview from '../StaticQrPreview.vue';

/** Renders the hero's illustrative code card — a client-side sample, not a saved asset. */
defineOptions({ name: 'HeroCodePreview' });
</script>

<template>
  <div class="relative">
    <div class="absolute -inset-6 rounded-[2.5rem] bg-primary-soft/60 blur-2xl" aria-hidden="true" />
    <div class="relative rounded-3xl border border-border bg-card p-6 shadow-xl shadow-primary/5">
      <StaticQrPreview value="https://foreverpin.com/menu" foreground="#6d28d9" background="#ffffff" :size="220" />
      <div class="mt-4 flex items-center justify-between gap-2 text-xs">
        <span class="font-mono font-medium text-muted-foreground">foreverpin.com/menu</span>
        <span class="rounded-full bg-success-soft px-2 py-0.5 font-medium text-success">routes 4 ways</span>
      </div>
    </div>
  </div>
</template>
