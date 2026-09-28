<script setup lang="ts">
import { computed } from 'vue';

import type { CodeBuilderForm } from '@/application/codes';
import { contentType, type CodeContent } from '@/domain/codes';

import ContentLeafField from './ContentLeafField.vue';
import { ContentFieldDisplays } from './ContentFieldDisplays';

/** Defines the props of {@link ContentFieldControls}. */
export interface ContentFieldControlsProps {
  /** The builder form. */
  readonly form: CodeBuilderForm;
  /** The content object's path, such as `rules[0].content`. */
  readonly contentPath: string;
  /** The current content. */
  readonly content: CodeContent;
}

/** Renders the content type's note and one bound field per content leaf. */
defineOptions({ name: 'ContentFieldControls' });
const props = defineProps<ContentFieldControlsProps>();

const note = computed(() => contentType(props.content.type).note);
const values = computed(() => props.content as unknown as Readonly<Record<string, unknown>>);
</script>

<template>
  <div class="flex flex-col gap-4">
    <p v-if="note" class="text-sm text-muted-foreground">{{ note }}</p>
    <ContentLeafField
      v-for="field in ContentFieldDisplays[content.type]"
      :key="field.key"
      :form="form"
      :content-path="contentPath"
      :content="values"
      :field="field"
    />
  </div>
</template>
