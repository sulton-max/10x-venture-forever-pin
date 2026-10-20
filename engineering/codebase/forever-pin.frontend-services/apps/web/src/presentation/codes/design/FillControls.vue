<script setup lang="ts">
import { ArrowLeftRight, ArrowRight } from 'lucide-vue-next';
import { Gradient, GradientType } from '@wow-two-beta/fe/domain/color';
import { Button } from '@wow-two-beta/fe/presentation/actions';
import {
  ColorPicker,
  ControlGroupField,
  OptionTileGroupField,
  OptionTilePicker,
  ToggleGroup,
  ToggleInput,
} from '@wow-two-beta/fe/presentation/forms';
import { computed, shallowRef } from 'vue';

import { FillType, type CodeGradientDto, type CodeStyleDto } from '@/domain/codes';
import { InfoPopover } from '@/presentation/common';

import { BuilderHelp } from '../builder/BuilderHelp';

import { GradientPresetRows, PresetIconSize } from './GradientPresets';

/** Defines the props of {@link FillControls}. */
export interface FillControlsProps {
  /** The style whose foreground fill is edited. */
  readonly style: CodeStyleDto;
}

/** Defines the events of {@link FillControls}. */
export interface FillControlsEmits {
  /** Emits the style with the next foreground fill. */
  (event: 'change', style: CodeStyleDto): void;
}

/** Holds the default linear angle in degrees. */
const DefaultAngle = 45;

/** Holds the default radial extent. */
const DefaultRadius = 0.8;

/** Holds the end color a first gradient starts with. */
const DefaultGradientEnd = '#7c3aed';

/** Renders the solid or gradient foreground; switching to solid keeps the gradient as a draft to come back to. */
defineOptions({ name: 'FillControls' });
const props = defineProps<FillControlsProps>();
const emit = defineEmits<FillControlsEmits>();

const gradientDraft = shallowRef<CodeGradientDto | undefined>(props.style.gradient);
const fill = computed(() => (props.style.gradient ? FillType.Gradient : FillType.Solid));

function setGradient(gradient: Gradient | undefined): void {
  if (gradient) gradientDraft.value = gradient;
  emit('change', { ...props.style, gradient });
}

function setFill(next: unknown): void {
  if (next === FillType.Gradient && !props.style.gradient)
    setGradient(gradientDraft.value ?? Gradient.twoStop(props.style.foregroundColor, DefaultGradientEnd, DefaultAngle));
  else if (next === FillType.Solid && props.style.gradient) emit('change', { ...props.style, gradient: undefined });
}

function projectionOf(gradient: Gradient): number {
  return gradient.type === GradientType.Linear ? gradient.angle : gradient.radius;
}

function applyProjection(gradient: Gradient, value: number): Gradient {
  return gradient.type === GradientType.Linear
    ? Gradient.withAngle(gradient, value)
    : Gradient.withRadius(gradient, value);
}

function setType(next: unknown): void {
  const gradient = props.style.gradient;
  if (!gradient || (next !== GradientType.Linear && next !== GradientType.Radial) || gradient.type === next) return;
  setGradient(Gradient.withType(gradient, next, { angle: DefaultAngle, radius: DefaultRadius }));
}
</script>

<template>
  <div class="flex flex-col">
    <ControlGroupField label="Fill">
      <template #label>
        <span class="inline-flex items-center gap-1"
          >Fill <InfoPopover topic="the fill" :text="BuilderHelp.fill"
        /></span>
      </template>
      <ToggleGroup
        variant="segmented"
        type="single"
        :model-value="fill"
        aria-label="Foreground fill"
        @update:model-value="setFill"
      >
        <ToggleInput variant="soft" :value="FillType.Solid" size="sm">Solid</ToggleInput>
        <ToggleInput variant="soft" :value="FillType.Gradient" size="sm">Gradient</ToggleInput>
      </ToggleGroup>
    </ControlGroupField>

    <ControlGroupField label="Colors">
      <template #label>
        <span class="inline-flex items-center gap-1"
          >Colors <InfoPopover topic="the colors" :text="BuilderHelp.colors"
        /></span>
      </template>
      <div v-if="style.gradient" class="flex items-center gap-1">
        <ColorPicker
          trigger-variant="swatch"
          :model-value="style.gradient.stops[0]?.color ?? DefaultGradientEnd"
          aria-label="Gradient start color"
          @update:model-value="(hex) => hex && style.gradient && setGradient(Gradient.withStop(style.gradient, 0, hex))"
        />
        <Button
          variant="reveal"
          tone="neutral"
          shape="square"
          size="xs"
          aria-label="Reverse gradient colors"
          @click="style.gradient && setGradient(Gradient.reverseStops(style.gradient))"
        >
          <ArrowRight :size="PresetIconSize" />
          <template #hover><ArrowLeftRight :size="PresetIconSize" /></template>
        </Button>
        <ColorPicker
          trigger-variant="swatch"
          :model-value="style.gradient.stops[1]?.color ?? DefaultGradientEnd"
          aria-label="Gradient end color"
          @update:model-value="(hex) => hex && style.gradient && setGradient(Gradient.withStop(style.gradient, 1, hex))"
        />
      </div>
      <ColorPicker
        v-else
        trigger-variant="swatch"
        :model-value="style.foregroundColor"
        aria-label="Foreground color"
        @update:model-value="(hex) => hex && emit('change', { ...style, foregroundColor: hex })"
      />
    </ControlGroupField>

    <div v-if="style.gradient" class="flex flex-wrap items-stretch gap-3 py-2">
      <ToggleGroup
        variant="segmented"
        type="single"
        orientation="vertical"
        :model-value="style.gradient.type"
        aria-label="Gradient type"
        class="shrink-0 self-stretch"
        @update:model-value="setType"
      >
        <ToggleInput variant="soft" :value="GradientType.Linear" size="sm" class="flex-1">Linear</ToggleInput>
        <ToggleInput variant="soft" :value="GradientType.Radial" size="sm" class="flex-1">Radial</ToggleInput>
      </ToggleGroup>

      <div class="flex min-w-0 flex-1 flex-col divide-y divide-border">
        <OptionTileGroupField
          v-for="row in GradientPresetRows"
          :key="row.type"
          :label="row.label"
          :is-disabled="style.gradient.type !== row.type"
          align="end"
          can-wrap
          class="py-2 first:pt-0 last:pb-0"
        >
          <OptionTilePicker
            v-for="preset in row.presets"
            :key="preset.value"
            :selected="style.gradient.type === row.type && projectionOf(style.gradient) === preset.value"
            :label="preset.label"
            @select="() => style.gradient && setGradient(applyProjection(style.gradient, preset.value))"
          >
            <component :is="preset.glyph" v-bind="preset.glyphProps" />
          </OptionTilePicker>
        </OptionTileGroupField>
      </div>
    </div>
  </div>
</template>
