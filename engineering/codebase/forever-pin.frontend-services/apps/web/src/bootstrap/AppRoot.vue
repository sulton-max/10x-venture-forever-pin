<script lang="ts">
/** Holds the storage key of the remembered color mode — `index.html` reads the same key before first paint. */
const ColorModeStorageKey = 'color-mode';
</script>

<script setup lang="ts">
import { AuthProvider } from '@wow-two-beta/ui-vue/auth';
import { ColorModeProvider } from '@wow-two-beta/ui-vue/foundation/primitives';
import { FeedbackToastHost } from '@wow-two-beta/ui-vue/presentation/feedback';
import { QueryProgressBridge } from '@wow-two-beta/ui-vue/query';
import {
  AppErrorBoundary,
  NavigationProgress,
  NavigationProgressMode,
  ProgressProvider,
  RouteAnnouncer,
} from '@wow-two-beta/ui-vue/router';
import { RouterView } from 'vue-router';

import { createIdentityStrategy } from '@/application/identity';
import { authBridge } from '@/integration/common';

/**
 * Renders the app root — remembered color mode, the session (checked only when the product opens),
 * navigation progress, announcements, notices and routes.
 */
defineOptions({ name: 'AppRoot' });
const identityStrategy = createIdentityStrategy();
</script>

<template>
  <ColorModeProvider default-mode="system" :storage-key="ColorModeStorageKey">
    <AuthProvider :strategy="identityStrategy" :bridge="authBridge" :resolve-on-mount="false">
      <ProgressProvider>
        <QueryProgressBridge />
        <NavigationProgress :mode="NavigationProgressMode.Both" />
        <RouteAnnouncer />
        <AppErrorBoundary>
          <RouterView />
        </AppErrorBoundary>
        <FeedbackToastHost />
      </ProgressProvider>
    </AuthProvider>
  </ColorModeProvider>
</template>
