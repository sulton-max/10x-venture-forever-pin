<script setup lang="ts">
import { ControlGroupField, OptionTileGroupField, OptionTilePicker } from '@wow-two-beta/ui-vue/presentation/forms';

import { FinderShape, ModuleShape, type CodeStyleDto } from '@/domain/codes';

import { FinderShapeDisplays, ModuleShapeDisplays } from './ShapeDisplays';

/** Defines the props of {@link ShapeControls}. */
export interface ShapeControlsProps {
  /** The style whose shapes are edited. */
  readonly style: CodeStyleDto;
}

/** Defines the events of {@link ShapeControls}. */
export interface ShapeControlsEmits {
  /** Emits the style with the next shapes. */
  (event: 'change', style: CodeStyleDto): void;
}

/** Holds the eye parts in display order: the frame, then the pupil. */
const EyeParts = [
  { key: 'finderShape', label: 'External', isDot: false },
  { key: 'finderDotShape', label: 'Internal', isDot: true },
] as const;

/** Renders the body (module) and eye (finder frame and pupil) shape tiles; eyes stack on narrow screens. */
defineOptions({ name: 'ShapeControls' });
const props = defineProps<ShapeControlsProps>();
const emit = defineEmits<ShapeControlsEmits>();

function set(patch: Partial<CodeStyleDto>): void {
  emit('change', { ...props.style, ...patch });
}
</script>

<template>
  <div class="flex flex-col gap-2">
    <ControlGroupField label="Body" orientation="vertical" :divided="false">
      <OptionTileGroupField label="Body shape" wrap>
        <OptionTilePicker
          v-for="shape in Object.values(ModuleShape)"
          :key="shape"
          :selected="style.moduleShape === shape"
          :label="`Body shape: ${ModuleShapeDisplays[shape].label}`"
          @select="() => set({ moduleShape: shape })"
        >
          <component :is="ModuleShapeDisplays[shape].glyph" v-bind="ModuleShapeDisplays[shape].glyphProps" />
        </OptionTilePicker>
      </OptionTileGroupField>
    </ControlGroupField>

    <ControlGroupField label="Eyes" orientation="vertical" :divided="false" class="border-t border-border pt-2">
      <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <div v-for="part in EyeParts" :key="part.key" class="min-w-0">
          <div class="mb-1.5 text-xs text-muted-foreground">{{ part.label }}</div>
          <OptionTileGroupField :label="`${part.label} eye`" wrap>
            <OptionTilePicker
              v-for="shape in Object.values(FinderShape)"
              :key="shape"
              :selected="style[part.key] === shape"
              :label="`${part.label} eye: ${FinderShapeDisplays[shape].label}`"
              @select="() => set({ [part.key]: shape })"
            >
              <component
                :is="FinderShapeDisplays[shape].glyph"
                v-bind="FinderShapeDisplays[shape].glyphProps"
                :is-dot="part.isDot"
              />
            </OptionTilePicker>
          </OptionTileGroupField>
        </div>
      </div>
    </ControlGroupField>
  </div>
</template>
