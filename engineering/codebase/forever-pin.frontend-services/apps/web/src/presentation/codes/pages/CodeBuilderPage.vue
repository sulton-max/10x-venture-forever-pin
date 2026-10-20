<script setup lang="ts">
import { ArrowLeft, RotateCw } from 'lucide-vue-next';
import { BlockerState, useNavigationBlocker } from '@wow-two-beta/fe/router';
import { Button } from '@wow-two-beta/fe/presentation/actions';
import { Card, Heading, Text } from '@wow-two-beta/fe/presentation/display';
import { Alert, Spinner } from '@wow-two-beta/fe/presentation/feedback';
import { ToggleGroup, ToggleInput } from '@wow-two-beta/fe/presentation/forms';
import {
  AlertModal,
  AlertModalContent,
  ModalDescription,
  ModalFooter,
  ModalHeader,
  ModalTitle,
} from '@wow-two-beta/fe/presentation/overlays';
import { computed, ref } from 'vue';

import { CodeBuilderPhase, useCodeBuilder } from '@/application/codes';
import { FailureMessages } from '@/application/common';
import { BarcodeFormat, ContentMode } from '@/domain/codes';

import ContentPanel from '../builder/ContentPanel.vue';
import DesignPanel from '../builder/DesignPanel.vue';
import { ContentModeDisplays } from '../content/ContentModeDisplays';
import ContrastCallout from '../design/ContrastCallout.vue';
import CodePreview from '../preview/CodePreview.vue';
import DownloadCodeModal from '../list/DownloadCodeModal.vue';
import SavedCodePanel from '../preview/SavedCodePanel.vue';

/** Defines the props of {@link CodeBuilderPage}. */
export interface CodeBuilderPageProps {
  /** The code to edit; absent for a new code. */
  readonly codeId?: string;
  /** The code a new one is copied from. */
  readonly copyFromId?: string;
  /** The mode a copy is created in. */
  readonly copyMode?: ContentMode;
  /** Returns to the codes list. */
  readonly onBack: () => void;
}

/** Defines the builder's sections. */
const BuilderTab = { Content: 'content', Design: 'design' } as const;

/** Renders the code builder for a new code, a copy or an edit, with the draft preview beside it. */
defineOptions({ name: 'CodeBuilderPage' });
const props = defineProps<CodeBuilderPageProps>();

const builder = useCodeBuilder(() => ({
  codeId: props.codeId,
  copyFromId: props.copyFromId,
  copyMode: props.copyMode,
}));
const { form } = builder;
const tab = ref<(typeof BuilderTab)[keyof typeof BuilderTab]>(BuilderTab.Content);
const isDownloading = ref(false);

const isDirty = computed(() => form.state.isDirty);
const blocker = useNavigationBlocker(() => isDirty.value && !form.state.isSubmitting);

const mode = computed(() => (builder.existing.value ? builder.existing.value.mode : form.values.mode));
const heading = computed(() =>
  builder.isEdit.value ? 'Edit code' : builder.copiedInto.value ? 'Copy code' : 'Create a code',
);
const subheading = computed(() => {
  if (builder.isEdit.value)
    return mode.value === ContentMode.Dynamic
      ? 'Changes reach printed copies right away — the short link stays the same.'
      : 'The content is in the symbol — saving produces a different image, and printed copies keep the old content.';
  if (builder.copiedInto.value)
    return `A new ${ContentModeDisplays[builder.copiedInto.value].label.toLowerCase()} code with the same content — the original keeps working.`;
  return 'One code, many destinations — and it never expires.';
});
const submitLabel = computed(() => {
  if (builder.isEdit.value) return 'Save changes';
  return builder.saved.value ? 'Create another code' : 'Create code';
});
const previewModel = computed(() => ({
  barcodeFormat: form.values.barcodeFormat,
  mode: form.values.mode,
  rules: form.values.rules,
  style: form.values.style,
}));
const previewNote = computed(() =>
  form.values.mode === ContentMode.Dynamic
    ? 'Style sample — the saved code encodes its own short link, shown after saving.'
    : 'Live preview of the draft, rendered by the server exactly as it downloads.',
);
const loadFailure = computed(() =>
  FailureMessages.describe(builder.loadFailure.value, 'The code could not be loaded.'),
);
</script>

<template>
  <div class="flex flex-col gap-6">
    <div class="flex flex-col gap-2">
      <Button variant="ghost" tone="neutral" size="sm" class="-ml-2 self-start" @click="props.onBack">
        <template #leading><ArrowLeft :size="16" aria-hidden="true" /></template>
        Back to codes
      </Button>
      <div>
        <Heading :level="1" size="xl" weight="bold">{{ heading }}</Heading>
        <Text color="muted">{{ subheading }}</Text>
      </div>
    </div>

    <div v-if="builder.phase.value === CodeBuilderPhase.Loading" class="flex min-h-[50vh] items-center justify-center">
      <Spinner size="lg" label="Loading code" />
    </div>

    <div
      v-else-if="builder.phase.value === CodeBuilderPhase.Failed"
      role="alert"
      class="flex flex-col items-start gap-3"
    >
      <Alert severity="danger" :description="loadFailure" />
      <div class="flex gap-2">
        <Button tone="primary" variant="outline" @click="builder.retryLoad">
          <template #leading><RotateCw :size="16" aria-hidden="true" /></template>
          Try again
        </Button>
        <Button tone="neutral" variant="ghost" @click="props.onBack">Back to codes</Button>
      </div>
    </div>

    <div v-else class="grid grid-cols-1 gap-6 lg:grid-cols-[minmax(0,1.25fr)_minmax(18rem,0.8fr)]">
      <form novalidate class="contents" @submit.prevent="form.handleSubmit">
        <Card class="surface-soft flex min-w-0 flex-col gap-5 p-6">
          <ToggleGroup
            variant="segmented"
            type="single"
            :model-value="tab"
            aria-label="Builder section"
            equal-width
            @update:model-value="(next) => (next === BuilderTab.Content || next === BuilderTab.Design) && (tab = next)"
          >
            <ToggleInput variant="soft" :value="BuilderTab.Content" class="flex-1">Content</ToggleInput>
            <ToggleInput variant="soft" :value="BuilderTab.Design" class="flex-1">Design</ToggleInput>
          </ToggleGroup>

          <!-- Both sections stay mounted, so drafts such as a gradient survive a switch. -->
          <div v-show="tab === BuilderTab.Content" class="motion-safe:animate-fade-in">
            <ContentPanel :form="form" :is-edit="builder.isEdit.value" :existing="builder.existing.value" />
          </div>
          <div v-show="tab === BuilderTab.Design" class="motion-safe:animate-fade-in">
            <DesignPanel :form="form" />
          </div>

          <div class="flex flex-col gap-2">
            <Button
              type="submit"
              tone="primary"
              is-full-width
              :is-loading="form.state.isSubmitting"
              :loading-text="builder.isEdit.value ? 'Saving…' : 'Creating…'"
            >
              {{ submitLabel }}
            </Button>
            <Text v-if="isDirty && builder.saved.value" size="xs" color="muted" align="center">Unsaved changes</Text>
          </div>

          <Alert v-if="form.state.submitError" severity="danger" :description="form.state.submitError.message" />
          <Alert v-if="builder.warnings.value.length > 0" severity="warning" title="Saved — check the destination">
            <ul class="list-disc pl-4">
              <li v-for="warning in builder.warnings.value" :key="`${warning.property}:${warning.code}`">
                {{ warning.message }}
              </li>
            </ul>
          </Alert>
        </Card>
      </form>

      <Card class="surface-soft flex flex-col items-center gap-4 p-6 lg:sticky lg:top-6 lg:self-start">
        <CodePreview :model="previewModel" />
        <Text size="xs" color="muted" align="center">{{ previewNote }}</Text>
        <div v-if="form.values.barcodeFormat === BarcodeFormat.QrCode" class="w-full">
          <ContrastCallout :style="form.values.style" />
        </div>
        <SavedCodePanel
          v-if="builder.saved.value"
          :code="builder.saved.value"
          :is-edit="builder.isEdit.value"
          :has-unsaved-edits="isDirty"
          :revision="builder.savedRevision.value"
          @done="props.onBack"
          @start-another="builder.startAnother"
          @download="isDownloading = true"
        />
        <DownloadCodeModal :code="isDownloading ? builder.saved.value : null" @close="isDownloading = false" />
      </Card>
    </div>

    <AlertModal :open="blocker.state === BlockerState.Blocked" @update:open="(open) => !open && blocker.reset()">
      <AlertModalContent>
        <ModalHeader>
          <ModalTitle>Discard unsaved changes?</ModalTitle>
          <ModalDescription>Your edits to this code have not been saved.</ModalDescription>
        </ModalHeader>
        <ModalFooter>
          <Button tone="neutral" variant="outline" @click="blocker.reset">Keep editing</Button>
          <Button tone="danger" @click="blocker.proceed">Discard changes</Button>
        </ModalFooter>
      </AlertModalContent>
    </AlertModal>
  </div>
</template>
