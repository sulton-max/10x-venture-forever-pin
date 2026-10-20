<script setup lang="ts">
import { Info } from 'lucide-vue-next';
import { Popover, PopoverContent, PopoverTrigger } from '@wow-two-beta/fe/presentation/overlays';

/** Defines the props of {@link InfoPopover}. */
export interface InfoPopoverProps {
  /** Names what the note explains; the trigger reads "About {topic}". */
  readonly topic: string;
  /** The explanation. */
  readonly text: string;
}

/**
 * Renders a small info button that opens a short explanation — by click or tap, so it also works on touch. The
 * popover renders no element of its own, so classes and attributes land on the button.
 */
defineOptions({ name: 'InfoPopover', inheritAttrs: false });
defineProps<InfoPopoverProps>();
</script>

<template>
  <Popover placement="top">
    <PopoverTrigger
      v-bind="$attrs"
      class="inline-grid size-5 shrink-0 place-items-center rounded-full align-middle text-subtle-foreground transition-colors hover:text-foreground focus-visible:outline-2 focus-visible:outline-offset-1 focus-visible:outline-ring"
      :aria-label="`About ${topic}`"
    >
      <Info :size="14" aria-hidden="true" />
    </PopoverTrigger>
    <PopoverContent padding="sm" class="max-w-72 text-sm leading-snug">{{ text }}</PopoverContent>
  </Popover>
</template>
