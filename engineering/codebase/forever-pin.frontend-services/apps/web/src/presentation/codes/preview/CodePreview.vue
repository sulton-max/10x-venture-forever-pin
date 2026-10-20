<script setup lang="ts">
import { Spinner } from '@wow-two-beta/fe/presentation/feedback';
import { computed } from 'vue';

import { useCodePreview, type CodePreviewModel } from '@/application/codes';

/** Defines the props of {@link CodePreview}. */
export interface CodePreviewProps {
  /** The draft values to render. */
  readonly model: CodePreviewModel;
  /** The rendered edge in px. Default `240`. */
  readonly size?: number;
}

/**
 * Renders the server's SVG of the draft as an image — never as inline markup — so a rendered payload cannot
 * run script. A stale image stays visible under a spinner until the newest render lands.
 */
defineOptions({ name: 'CodePreview' });
const props = withDefaults(defineProps<CodePreviewProps>(), { size: 240 });

const preview = useCodePreview(() => props.model);
const source = computed(() =>
  preview.svg.value ? `data:image/svg+xml;charset=utf-8,${encodeURIComponent(preview.svg.value)}` : null,
);
const frameBackground = computed(() =>
  props.model.style.transparentBackground ? 'transparent' : props.model.style.backgroundColor,
);
</script>

<template>
  <div
    class="relative inline-flex items-center justify-center rounded-xl p-4"
    :style="{ width: `${size}px`, height: `${size}px`, backgroundColor: frameBackground }"
    :aria-busy="preview.isStale.value"
  >
    <img v-if="source" :src="source" alt="Preview of the code" class="block h-full w-full object-contain" />
    <div
      v-if="preview.isStale.value"
      class="absolute inset-0 flex items-center justify-center rounded-xl bg-white/60 backdrop-blur-[1px] dark:bg-black/40"
    >
      <Spinner size="md" label="Rendering preview" />
    </div>
    <div
      v-else-if="preview.error.value"
      role="status"
      class="absolute inset-0 flex items-center justify-center rounded-xl bg-card/90 p-4 text-center text-xs text-muted-foreground"
    >
      {{ preview.error.value }}
    </div>
  </div>
</template>
