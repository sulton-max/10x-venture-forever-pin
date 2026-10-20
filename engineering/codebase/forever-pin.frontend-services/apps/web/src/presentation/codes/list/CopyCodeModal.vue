<script setup lang="ts">
import { Button } from '@wow-two-beta/fe/presentation/actions';
import { ChoiceCard, RadioGroup } from '@wow-two-beta/fe/presentation/forms';
import {
  Modal,
  ModalBody,
  ModalContent,
  ModalDescription,
  ModalFooter,
  ModalHeader,
  ModalTitle,
} from '@wow-two-beta/fe/presentation/overlays';
import { computed, ref, watch } from 'vue';

import { oppositeMode } from '@/application/codes';
import { ContentMode, type CodeDto } from '@/domain/codes';

import { ContentModeDisplays } from '../content/ContentModeDisplays';

/** Defines the props of {@link CopyCodeModal}. */
export interface CopyCodeModalProps {
  /** The code to copy, or `null` when closed. */
  readonly code: CodeDto | null;
}

/** Defines the events of {@link CopyCodeModal}. */
export interface CopyCodeModalEmits {
  /** Confirms the copy in the chosen mode. */
  (event: 'confirm', mode: ContentMode): void;
  /** Dismisses the modal. */
  (event: 'cancel'): void;
}

/**
 * Asks which mode a copy is created in. A static copy bakes one payload into its symbol, so it is offered only for a
 * single-rule code; the modal says why when it is not.
 */
defineOptions({ name: 'CopyCodeModal' });
const props = defineProps<CopyCodeModalProps>();
const emit = defineEmits<CopyCodeModalEmits>();

const canCopyStatic = computed(() => (props.code?.rules.length ?? 0) === 1);
const mode = ref<ContentMode>(ContentMode.Dynamic);

watch(
  () => props.code,
  (code) => {
    if (!code) return;
    const opposite = oppositeMode(code.mode);
    mode.value = opposite === ContentMode.Static && !canCopyStatic.value ? ContentMode.Dynamic : opposite;
  },
  { immediate: true },
);

function select(next: unknown): void {
  if (next === ContentMode.Dynamic || (next === ContentMode.Static && canCopyStatic.value)) mode.value = next;
}
</script>

<template>
  <Modal :open="code !== null" @update:open="(open) => !open && emit('cancel')">
    <ModalContent>
      <ModalHeader>
        <ModalTitle>Copy “{{ code?.name }}”</ModalTitle>
        <ModalDescription
          >The copy is a new code with the same content and style; the original keeps working.</ModalDescription
        >
      </ModalHeader>
      <ModalBody>
        <RadioGroup legend="How the copy resolves" :model-value="mode" @update:model-value="select">
          <ChoiceCard
            v-for="option in Object.values(ContentMode)"
            :key="option"
            :value="option"
            :label="ContentModeDisplays[option].label"
            :description="ContentModeDisplays[option].note"
            :is-disabled="option === ContentMode.Static && !canCopyStatic"
          />
        </RadioGroup>
        <p v-if="!canCopyStatic" class="mt-3 text-xs text-muted-foreground">
          A static copy holds one rule; this code has {{ code?.rules.length }}, so it copies as dynamic.
        </p>
      </ModalBody>
      <ModalFooter>
        <Button tone="neutral" variant="outline" @click="emit('cancel')">Cancel</Button>
        <Button tone="primary" @click="emit('confirm', mode)"
          >Copy as {{ ContentModeDisplays[mode].label.toLowerCase() }}</Button
        >
      </ModalFooter>
    </ModalContent>
  </Modal>
</template>
