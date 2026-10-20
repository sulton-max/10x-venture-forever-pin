<script setup lang="ts">
import { ColorPicker, ControlGroupField, ToggleGroup, ToggleInput } from '@wow-two-beta/fe/presentation/forms';

import type { CodeStyleDto } from '@/domain/codes';
import { InfoPopover } from '@/presentation/common';

import { BuilderHelp } from '../builder/BuilderHelp';

/** Defines the props of {@link BackgroundControls}. */
export interface BackgroundControlsProps {
  /** The style whose background is edited. */
  readonly style: CodeStyleDto;
}

/** Defines the events of {@link BackgroundControls}. */
export interface BackgroundControlsEmits {
  /** Emits the style with the next background. */
  (event: 'change', style: CodeStyleDto): void;
}

/** Renders the background fill choice and, beside it, an independent color swatch kept while transparent. */
defineOptions({ name: 'BackgroundControls' });
const props = defineProps<BackgroundControlsProps>();
const emit = defineEmits<BackgroundControlsEmits>();

function setFill(next: unknown): void {
  if (next === 'color' || next === 'transparent')
    emit('change', { ...props.style, transparentBackground: next === 'transparent' });
}
</script>

<template>
  <ControlGroupField label="Background" class="border-t border-border">
    <template #label>
      <span class="inline-flex items-center gap-1"
        >Background <InfoPopover topic="the background" :text="BuilderHelp.background"
      /></span>
    </template>
    <div class="flex items-center gap-2">
      <ColorPicker
        trigger-variant="swatch"
        trigger-size="sm"
        :model-value="style.backgroundColor"
        :is-disabled="style.transparentBackground"
        aria-label="Background color"
        @update:model-value="(hex) => hex && emit('change', { ...style, backgroundColor: hex })"
      />
      <ToggleGroup
        variant="segmented"
        type="single"
        :model-value="style.transparentBackground ? 'transparent' : 'color'"
        aria-label="Background fill"
        @update:model-value="setFill"
      >
        <ToggleInput variant="soft" value="color" size="sm">Color</ToggleInput>
        <ToggleInput variant="soft" value="transparent" size="sm">Transparent</ToggleInput>
      </ToggleGroup>
    </div>
  </ControlGroupField>
</template>
