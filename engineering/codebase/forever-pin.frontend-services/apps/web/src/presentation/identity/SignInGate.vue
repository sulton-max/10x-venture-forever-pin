<script setup lang="ts">
import { Button, GoogleSignInButton } from '@wow-two-beta/fe/presentation/actions';
import { Card, Heading, Text } from '@wow-two-beta/fe/presentation/display';
import { Alert } from '@wow-two-beta/fe/presentation/feedback';
import { DividerLayout } from '@wow-two-beta/fe/presentation/layout';

/** Defines the props of {@link SignInGate}. */
export interface SignInGateProps {
  /** Whether Google sign-in is configured; otherwise the gate offers guest access only. */
  readonly isGoogleEnabled: boolean;
  /** Whether a guest session is being created. */
  readonly isStartingGuest: boolean;
  /** The last sign-in failure to show, or `null`. */
  readonly failure: string | null;
}

/** Defines the events of {@link SignInGate}. */
export interface SignInGateEmits {
  /** Asks to continue as a guest whose codes stay on this device until sign-in. */
  (event: 'continue-as-guest'): void;
}

/** Renders the sign-in gate in front of the app: Google, or continue as a guest. */
defineOptions({ name: 'SignInGate' });
defineProps<SignInGateProps>();
const emit = defineEmits<SignInGateEmits>();
</script>

<template>
  <div class="flex min-h-[60vh] items-center justify-center">
    <Card class="surface-soft w-full max-w-sm p-8">
      <div class="flex flex-col gap-5">
        <div class="text-center">
          <Heading :level="1" size="xl" weight="bold">Welcome to ForeverPin</Heading>
          <Text size="sm" color="muted" class="mt-1">Programmable codes that never expire.</Text>
        </div>

        <div v-if="isGoogleEnabled" class="flex justify-center">
          <GoogleSignInButton text="continue_with" />
        </div>
        <Text v-else size="xs" color="muted" align="center">Google sign-in isn't configured yet.</Text>

        <DividerLayout label="or" />

        <Button
          tone="primary"
          is-full-width
          :is-loading="isStartingGuest"
          loading-text="Setting up…"
          @click="emit('continue-as-guest')"
        >
          Continue as guest
        </Button>

        <Alert v-if="failure" severity="danger" :description="failure" />
      </div>
    </Card>
  </div>
</template>
