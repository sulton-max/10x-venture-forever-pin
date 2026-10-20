<script setup lang="ts">
import { Button } from '@wow-two-beta/fe/presentation/actions';
import {
  AlertModal,
  AlertModalContent,
  ModalDescription,
  ModalFooter,
  ModalHeader,
  ModalTitle,
} from '@wow-two-beta/fe/presentation/overlays';
import { computed } from 'vue';

import { ContentMode, type CodeDto } from '@/domain/codes';

/** Defines the props of {@link DeleteCodeModal}. */
export interface DeleteCodeModalProps {
  /** The code awaiting confirmation, or `null` when closed. */
  readonly code: CodeDto | null;
  /** Whether the deletion is running. */
  readonly isDeleting: boolean;
}

/** Defines the events of {@link DeleteCodeModal}. */
export interface DeleteCodeModalEmits {
  /** Confirms the deletion. */
  (event: 'confirm'): void;
  /** Dismisses the confirmation. */
  (event: 'cancel'): void;
}

/** Confirms a deletion and states what happens to printed copies, which differs by mode. */
defineOptions({ name: 'DeleteCodeModal' });
const props = defineProps<DeleteCodeModalProps>();
const emit = defineEmits<DeleteCodeModalEmits>();

const consequence = computed(() =>
  props.code?.mode === ContentMode.Dynamic
    ? 'Its short link stops resolving, so printed copies stop working.'
    : 'Printed copies keep working — the content is in the symbol — but you can no longer edit or download it here.',
);
</script>

<template>
  <AlertModal :open="code !== null" @update:open="(open) => !open && emit('cancel')">
    <AlertModalContent>
      <ModalHeader>
        <ModalTitle>Delete this code?</ModalTitle>
        <ModalDescription>
          “{{ code?.name }}” and its routing rules will be permanently deleted. {{ consequence }} This can’t be undone.
        </ModalDescription>
      </ModalHeader>
      <ModalFooter>
        <Button tone="neutral" variant="outline" :is-disabled="isDeleting" @click="emit('cancel')">Cancel</Button>
        <Button tone="danger" :is-loading="isDeleting" loading-text="Deleting…" @click="emit('confirm')">Delete</Button>
      </ModalFooter>
    </AlertModalContent>
  </AlertModal>
</template>
