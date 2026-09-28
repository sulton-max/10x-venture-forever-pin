<script setup lang="ts">
import { EmojiCatalog, type EmojiCatalogEntry } from '@wow-two-beta/ui-vue/domain/emoji';
import { localStorageStorageBroker } from '@wow-two-beta/ui-vue/foundation/storage';
import { DefaultEmojiSize, EmojiPicker, EmojiSizePicker } from '@wow-two-beta/ui-vue/presentation/forms';
import { computed } from 'vue';

import type { CodeStyleDto } from '@/domain/codes';
import { InfoPopover } from '@/presentation/common';

import { BuilderHelp } from '../builder/BuilderHelp';

/** Defines the props of {@link EmojiControls}. */
export interface EmojiControlsProps {
  /** The style whose center emoji is edited. */
  readonly style: CodeStyleDto;
}

/** Defines the events of {@link EmojiControls}. */
export interface EmojiControlsEmits {
  /** Emits the style with the next center emoji. */
  (event: 'change', style: CodeStyleDto): void;
}

/** Holds the largest preview glyph in px that fits a size tile without clipping. */
const MaxPreviewGlyph = 24;

/** Renders the center-emoji picker and, once one is chosen, its size. */
defineOptions({ name: 'EmojiControls' });
const props = defineProps<EmojiControlsProps>();
const emit = defineEmits<EmojiControlsEmits>();

const selected = computed(() =>
  props.style.emoji ? (EmojiCatalog.all.find((entry) => entry.glyph === props.style.emoji?.glyph) ?? null) : null,
);

function pick(entry: EmojiCatalogEntry | null): void {
  emit('change', {
    ...props.style,
    emoji: entry ? { glyph: entry.glyph, sizeRatio: props.style.emoji?.sizeRatio ?? DefaultEmojiSize } : undefined,
  });
}
</script>

<template>
  <div class="relative flex flex-col gap-3">
    <InfoPopover topic="the center emoji" :text="BuilderHelp.emoji" class="absolute top-0 right-0 z-10" />
    <EmojiPicker
      :model-value="selected"
      :storage="localStorageStorageBroker"
      label="Center emoji"
      show-first-category-when-recents-empty
      :size="{ icon: 16 }"
      @update:model-value="pick"
    />
    <EmojiSizePicker
      v-if="style.emoji"
      :glyph="style.emoji.glyph"
      :model-value="style.emoji.sizeRatio"
      :max-preview-glyph="MaxPreviewGlyph"
      @update:model-value="
        (ratio) => style.emoji && emit('change', { ...style, emoji: { ...style.emoji, sizeRatio: ratio } })
      "
    />
  </div>
</template>
