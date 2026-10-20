<script lang="ts">
/** Holds the per-browser key remembering that the guest dismissed the banner. */
const DismissedKey = 'foreverpin.guest-keep-banner.dismissed';
</script>

<script setup lang="ts">
import { localStorageStorageBroker } from '@wow-two-beta/fe/foundation/storage';
import { Banner } from '@wow-two-beta/fe/presentation/feedback';
import { ref } from 'vue';

/** Defines the props of {@link GuestKeepBanner}. */
export interface GuestKeepBannerProps {
  /** Whether Google sign-in is configured, so the banner can point at it. */
  readonly isGoogleEnabled: boolean;
}

/**
 * Tells a guest that their codes live in this browser's cookie, so clearing site data or switching devices loses
 * them until they sign in. Dismissing it is remembered in this browser only.
 */
defineOptions({ name: 'GuestKeepBanner' });
defineProps<GuestKeepBannerProps>();

const isDismissed = ref(localStorageStorageBroker.read<boolean>(DismissedKey) === true);

function dismiss(): void {
  isDismissed.value = true;
  localStorageStorageBroker.write(DismissedKey, true);
}
</script>

<template>
  <Banner
    v-if="!isDismissed"
    severity="info"
    title="Your codes are saved in this browser"
    :description="
      isGoogleEnabled
        ? 'Sign in with Google to keep them on every device — clearing this browser’s data would otherwise lose them.'
        : 'Clearing this browser’s data would lose them; account sign-in is coming soon.'
    "
    class="mb-6"
    @close="dismiss"
  />
</template>
