<script lang="ts">
import { CreditCard, QrCode } from 'lucide-vue-next';

import { AppPaths } from '@/presentation/common';

/** Holds the product's primary destinations, in order: the bar's links and the phone's bottom bar. */
const Destinations = [
  { label: 'Codes', path: AppPaths.codes(), icon: QrCode },
  { label: 'Billing', path: AppPaths.billing(), icon: CreditCard },
] as const;

/** Names the destination a product path belongs to: every code page counts as Codes. */
function destinationOf(path: string): string {
  return path === AppPaths.billing() || path.startsWith(`${AppPaths.billing()}/`)
    ? AppPaths.billing()
    : AppPaths.codes();
}
</script>

<script setup lang="ts">
import { GoogleSignInButton } from '@wow-two-beta/fe/presentation/actions';
import { SplashScreen } from '@wow-two-beta/fe/presentation/feedback';
import { AppShell, AppShellContent, AppShellHeader, AppShellMain, Navbar } from '@wow-two-beta/fe/presentation/layout';
import { BottomNavMenu, BottomNavMenuItem, NavItem } from '@wow-two-beta/fe/presentation/nav';
import { computed, useTemplateRef, watch } from 'vue';
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router';

import { useSystemStatus } from '@/application/common';
import { SessionPhase, useSessionGate } from '@/application/identity';
import { AccountMenu, GuestKeepBanner, SessionFailureState, SignInGate } from '@/presentation/identity';
import { BrandGlyph } from '@/presentation/marketing';

/**
 * Renders the product: a splash while the session is checked, a retry state or the sign-in gate outside the frame,
 * then the frame — the bar with the brand, the version, the destinations and the account menu, the scrolling main
 * region, and the phone's bottom bar. The gate renders on the product address, so a deep link survives sign-in.
 */
defineOptions({ name: 'AppLayout' });
const gate = useSessionGate();
const system = useSystemStatus();
const route = useRoute();
const router = useRouter();
const main = useTemplateRef<{ el: HTMLElement | null }>('main');
const appVersion = __APP_VERSION__;
/** A numbered build shows as `v0.11.3`; a local build shows `dev`. */
const versionLabel = /^\d/.test(appVersion) ? `v${appVersion}` : appVersion;
/** The destination the current page belongs to. */
const current = computed(() => destinationOf(route.path));
/** The name the account menu shows. */
const displayName = computed(() => gate.account.value?.name || 'Guest');

/* The main region is the scroll container, so a new page starts at its top; a query change keeps its place. */
watch(
  () => route.path,
  () => main.value?.el?.scrollTo({ top: 0 }),
);
</script>

<template>
  <SplashScreen v-if="gate.phase.value === SessionPhase.Checking" label="Loading ForeverPin">
    <template #logo><BrandGlyph class="scale-150" /></template>
  </SplashScreen>
  <div
    v-else-if="gate.phase.value !== SessionPhase.Ready"
    class="surface-ambient flex min-h-dvh flex-col items-center justify-center bg-background px-4 py-10 text-foreground"
  >
    <RouterLink :to="AppPaths.home()" aria-label="ForeverPin home" class="mb-8"><BrandGlyph /></RouterLink>
    <main class="w-full">
      <SessionFailureState
        v-if="gate.phase.value === SessionPhase.Failed"
        :message="gate.checkFailureMessage.value"
        :is-retrying="gate.isChecking.value"
        @retry="gate.check"
      />
      <SignInGate
        v-else
        :is-google-enabled="gate.isGoogleEnabled.value"
        :is-starting-guest="gate.isStartingGuest.value"
        :failure="gate.signInFailure.value"
        @continue-as-guest="gate.continueAsGuest"
      />
    </main>
  </div>
  <AppShell v-else class="app-shell surface-ambient">
    <AppShellHeader height="auto" variant="glass" class="border-b border-(--fp-glass-edge)">
      <Navbar container-size="lg" container-class="gap-3 px-4 sm:px-6">
        <template #start>
          <div class="flex min-w-0 items-center gap-2">
            <RouterLink :to="AppPaths.codes()" aria-label="ForeverPin — your codes"><BrandGlyph /></RouterLink>
            <span
              class="rounded-md bg-muted px-1.5 py-0.5 font-mono text-[11px] font-medium text-muted-foreground"
              :title="`ForeverPin ${appVersion}`"
              >{{ versionLabel }}</span
            >
          </div>
          <nav aria-label="Primary" class="ms-4 hidden items-center gap-1 md:flex">
            <NavItem v-for="place in Destinations" :key="place.path" as-child :is-active="current === place.path">
              <RouterLink :to="place.path">{{ place.label }}</RouterLink>
            </NavItem>
          </nav>
        </template>
        <template #end>
          <div class="flex items-center gap-2">
            <GoogleSignInButton
              v-if="gate.isGuest.value && gate.isGoogleEnabled.value"
              size="medium"
              class="hidden sm:block"
            />
            <AccountMenu
              :display-name="displayName"
              :email="gate.account.value?.email"
              :api-version="system.apiVersion.value"
              @open-site="router.push(AppPaths.home())"
              @sign-out="gate.signOut"
            />
          </div>
        </template>
      </Navbar>
    </AppShellHeader>
    <AppShellMain ref="main">
      <AppShellContent gutter="lg" max-width="lg">
        <GuestKeepBanner v-if="gate.isGuest.value" :is-google-enabled="gate.isGoogleEnabled.value" />
        <RouterView />
      </AppShellContent>
    </AppShellMain>
    <BottomNavMenu :is-fixed="false" aria-label="Primary" class="surface-glass border-t [grid-area:footer] md:hidden">
      <BottomNavMenuItem v-for="place in Destinations" :key="place.path" as-child :is-active="current === place.path">
        <template #icon><component :is="place.icon" :size="20" /></template>
        <RouterLink :to="place.path">{{ place.label }}</RouterLink>
      </BottomNavMenuItem>
    </BottomNavMenu>
  </AppShell>
</template>
