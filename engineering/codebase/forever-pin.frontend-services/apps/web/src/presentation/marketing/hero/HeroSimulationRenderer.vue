<script setup lang="ts">
import { onBeforeUnmount, onMounted, useTemplateRef } from 'vue';

import { DEFAULT_PARAMS, HeroMode, startHeroSimulation } from './HeroSimulation';

/** Renders the landing hero's floating-code simulation behind scrims that keep the headline legible. */
defineOptions({ name: 'HeroSimulationRenderer' });

const canvas = useTemplateRef<HTMLCanvasElement>('canvas');
let stop: (() => void) | undefined;

onMounted(() => {
  if (canvas.value)
    stop = startHeroSimulation(canvas.value, { mode: () => HeroMode.Drift, params: () => DEFAULT_PARAMS });
});
onBeforeUnmount(() => stop?.());
</script>

<template>
  <canvas ref="canvas" class="absolute inset-0 h-full w-full" aria-hidden="true" />
  <!-- The headline sits on the left, so the simulation fades out toward it. -->
  <div
    aria-hidden="true"
    class="pointer-events-none absolute inset-0 z-[1] bg-gradient-to-r from-background via-background/85 to-background/40"
  />
  <div
    aria-hidden="true"
    class="pointer-events-none absolute inset-x-0 bottom-0 z-[1] h-24 bg-gradient-to-t from-background to-transparent"
  />
</template>
