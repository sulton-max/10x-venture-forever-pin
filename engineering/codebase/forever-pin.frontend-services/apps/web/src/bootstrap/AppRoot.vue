<script setup lang="ts">
import { AuthProvider } from '@wow-two-beta/fe/auth';
import { ColorModeProvider } from '@wow-two-beta/fe/foundation/primitives';
import { FeedbackToastHost } from '@wow-two-beta/fe/presentation/feedback';
import { QueryProgressBridge } from '@wow-two-beta/fe/query';
import {
  AppErrorBoundary,
  NavigationProgress,
  NavigationProgressMode,
  ProgressProvider,
  RouteAnnouncer,
} from '@wow-two-beta/fe/router';
import { RouterView } from 'vue-router';

import { createIdentityStrategy } from '@/application/identity';
import { authBridge } from '@/integration/common';

import { ColorModeStorageKey } from './ColorMode';

/**
 * Renders the app root — remembered color mode, the session (checked only when the product opens),
 * navigation progress, announcements, notices and routes.
 */
defineOptions({ name: 'AppRoot' });
const identityStrategy = createIdentityStrategy();
</script>

<template>
  <ColorModeProvider default-mode="system" :storage-key="ColorModeStorageKey">
    <AuthProvider :strategy="identityStrategy" :bridge="authBridge" :can-resolve-on-mount="false">
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
