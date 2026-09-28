<script setup lang="ts">
import { Button, CopyButton } from '@wow-two-beta/ui-vue/presentation/actions';
import { Text } from '@wow-two-beta/ui-vue/presentation/display';
import { SurfaceLayout } from '@wow-two-beta/ui-vue/presentation/layout';
import { computed } from 'vue';

import { codeImageUrl } from '@/application/codes';
import { ContentMode, ImageFormat, type CodeDto } from '@/domain/codes';

/** Defines the props of {@link SavedCodePanel}. */
export interface SavedCodePanelProps {
  /** The code the last save returned. */
  readonly code: CodeDto;
  /** Whether the builder edits an existing code. */
  readonly isEdit: boolean;
  /** Whether the values changed since this save; the downloads are then the saved version. */
  readonly hasUnsavedEdits: boolean;
  /** The save count, so the image reloads after each save. */
  readonly revision: number;
}

/** Defines the events of {@link SavedCodePanel}. */
export interface SavedCodePanelEmits {
  /** Asks to leave the builder (edit). */
  (event: 'done'): void;
  /** Asks to start another code from these values (create). */
  (event: 'start-another'): void;
}

/** Renders the saved code itself — its image, short link and downloads — apart from the draft preview. */
defineOptions({ name: 'SavedCodePanel' });
const props = defineProps<SavedCodePanelProps>();
const emit = defineEmits<SavedCodePanelEmits>();

const image = computed(() => codeImageUrl(props.code, ImageFormat.Svg, props.revision));
</script>

<template>
  <SurfaceLayout variant="soft" tone="neutral" radius="lg" padding="md" class="w-full">
    <div class="flex items-start gap-3">
      <img :src="image" alt="The saved code" class="size-20 shrink-0 rounded-md bg-white object-contain p-1" />
      <div class="min-w-0 flex-1">
        <Text size="sm" weight="medium" role="status">{{ isEdit ? 'Changes saved ✓' : 'Code created ✓' }}</Text>
        <Text
          v-if="code.mode === ContentMode.Dynamic && code.shortUrl"
          size="sm"
          color="muted"
          is-truncated
          class="mt-1 font-mono"
          :title="code.shortUrl"
        >
          {{ code.shortUrl }}
        </Text>
        <Text v-else size="sm" color="muted" class="mt-1">
          The content is baked into the symbol — it works offline, with no redirect.
        </Text>
        <Text v-if="hasUnsavedEdits" size="xs" color="warning" class="mt-1">
          You have unsaved edits — the image and downloads are the saved version.
        </Text>
      </div>
    </div>
    <div class="mt-3 flex flex-wrap items-center gap-2">
      <CopyButton
        v-if="code.mode === ContentMode.Dynamic && code.shortUrl"
        size="sm"
        :text="code.shortUrl"
        aria-label="Copy short URL"
      >
        Copy link
      </CopyButton>
      <Button as-child size="sm" variant="outline" tone="neutral">
        <a :href="codeImageUrl(code, ImageFormat.Svg, revision)" target="_blank" rel="noreferrer">SVG</a>
      </Button>
      <Button as-child size="sm" variant="outline" tone="neutral">
        <a :href="codeImageUrl(code, ImageFormat.Png, revision)" target="_blank" rel="noreferrer">PNG</a>
      </Button>
      <Button v-if="isEdit" size="sm" variant="ghost" tone="neutral" @click="emit('done')">Done</Button>
      <Button v-else size="sm" variant="ghost" tone="neutral" @click="emit('start-another')">Create another</Button>
    </div>
  </SurfaceLayout>
</template>
