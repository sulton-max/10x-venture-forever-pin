<script setup lang="ts">
import { ArrowLeft } from '@lucide/vue';
import { Button, GoogleSignInButton } from '@wow-two-beta/ui-vue/presentation/actions';
import { Text } from '@wow-two-beta/ui-vue/presentation/display';
import { Spinner } from '@wow-two-beta/ui-vue/presentation/feedback';
import { RouterLink, RouterView } from 'vue-router';

import { SessionPhase, useSessionGate } from '@/application/identity';
import { AppPaths, ColorModeButton } from '@/presentation/common';
import { SessionFailureState, SignInGate } from '@/presentation/identity';
import { BrandGlyph } from '@/presentation/marketing';

/**
 * Renders the product shell: the account bar and, by session phase, a spinner, a retry state, the sign-in gate
 * or the routed page. The gate renders in place, so a deep link survives sign-in.
 */
defineOptions({ name: 'AppLayout' });
const gate = useSessionGate();
</script>

<template>
  <div class="app-shell flex min-h-screen flex-col bg-background text-foreground">
    <header class="border-b border-border bg-card">
      <div
        class="mx-auto flex min-h-16 w-full max-w-5xl flex-wrap items-center justify-between gap-x-4 gap-y-3 px-4 py-3 sm:px-6"
      >
        <RouterLink :to="AppPaths.home()" aria-label="ForeverPin home" class="shrink-0">
          <BrandGlyph />
        </RouterLink>
        <nav
          aria-label="Account navigation"
          class="flex min-w-0 flex-1 basis-full flex-wrap items-center gap-3 text-sm sm:basis-auto sm:justify-end"
        >
          <ColorModeButton />
          <RouterLink
            v-if="gate.phase.value === SessionPhase.Ready"
            :to="AppPaths.billing()"
            class="whitespace-nowrap text-muted-foreground transition-colors hover:text-foreground"
          >
            Billing
          </RouterLink>
          <template v-if="gate.phase.value === SessionPhase.Ready && gate.isGuest.value">
            <Text as="span" size="sm" color="muted">Guest</Text>
            <GoogleSignInButton v-if="gate.isGoogleEnabled.value" size="medium" />
            <Button tone="neutral" variant="outline" :is-loading="gate.isSigningOut.value" @click="gate.signOut">
              Sign out
            </Button>
          </template>
          <template v-else-if="gate.phase.value === SessionPhase.Ready && gate.account.value">
            <Text
              as="span"
              size="sm"
              color="muted"
              class="min-w-0 max-w-full truncate sm:max-w-48"
              :title="gate.account.value.name"
            >
              {{ gate.account.value.name }}
            </Text>
            <Button tone="neutral" variant="outline" :is-loading="gate.isSigningOut.value" @click="gate.signOut">
              Log out
            </Button>
          </template>
          <Button as-child variant="ghost" tone="neutral">
            <RouterLink :to="AppPaths.home()" class="inline-flex items-center gap-1.5 whitespace-nowrap">
              <ArrowLeft :size="16" aria-hidden="true" />
              Back to site
            </RouterLink>
          </Button>
        </nav>
      </div>
    </header>

    <main class="mx-auto w-full min-w-0 max-w-5xl flex-1 px-4 py-6 sm:px-6 sm:py-8">
      <div v-if="gate.phase.value === SessionPhase.Checking" class="flex min-h-[60vh] items-center justify-center">
        <Spinner size="lg" label="Loading" />
      </div>
      <SessionFailureState
        v-else-if="gate.phase.value === SessionPhase.Failed"
        :message="gate.checkFailureMessage.value"
        :is-retrying="gate.isChecking.value"
        @retry="gate.check"
      />
      <SignInGate
        v-else-if="gate.phase.value === SessionPhase.Gate"
        :is-google-enabled="gate.isGoogleEnabled.value"
        :is-starting-guest="gate.isStartingGuest.value"
        :failure="gate.signInFailure.value"
        @continue-as-guest="gate.continueAsGuest"
      />
      <RouterView v-else />
    </main>
  </div>
</template>
