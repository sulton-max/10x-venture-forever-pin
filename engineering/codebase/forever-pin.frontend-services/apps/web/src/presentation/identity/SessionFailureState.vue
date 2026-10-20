<script setup lang="ts">
import { RotateCw } from 'lucide-vue-next';
import { Button } from '@wow-two-beta/fe/presentation/actions';
import { Heading, Text } from '@wow-two-beta/fe/presentation/display';

/** Defines the props of {@link SessionFailureState}. */
export interface SessionFailureStateProps {
  /** What went wrong, in words a person can act on. */
  readonly message: string;
  /** Whether a retry is running. */
  readonly isRetrying: boolean;
}

/** Defines the events of {@link SessionFailureState}. */
export interface SessionFailureStateEmits {
  /** Asks to check the session again; no new guest is created. */
  (event: 'retry'): void;
}

/** Renders the state shown when the session could not be checked — retry, never a new guest. */
defineOptions({ name: 'SessionFailureState' });
defineProps<SessionFailureStateProps>();
const emit = defineEmits<SessionFailureStateEmits>();
</script>

<template>
  <div class="flex min-h-[60vh] items-center justify-center" role="alert">
    <div class="max-w-sm text-center">
      <Heading :level="1" size="lg" weight="semibold">We couldn't reach ForeverPin</Heading>
      <Text size="sm" color="muted" class="mt-2">{{ message }}</Text>
      <Button
        tone="primary"
        variant="outline"
        class="mt-5"
        :is-loading="isRetrying"
        loading-text="Checking…"
        @click="emit('retry')"
      >
        <RotateCw :size="16" aria-hidden="true" />
        Try again
      </Button>
    </div>
  </div>
</template>
