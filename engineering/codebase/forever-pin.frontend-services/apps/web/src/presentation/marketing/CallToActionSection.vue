<script setup lang="ts">
import { Button } from '@wow-two-beta/fe/presentation/actions';
import { Heading, Text } from '@wow-two-beta/fe/presentation/display';
import { SurfaceLayout } from '@wow-two-beta/fe/presentation/layout';
import { RouterLink } from 'vue-router';

import { AppPaths } from '@/presentation/common';

/** Renders the closing call to action; callers add top space when it follows a tinted band. */
defineOptions({ name: 'CallToActionSection' });
</script>

<template>
  <div class="mx-auto w-full max-w-6xl px-6 pb-20">
    <SurfaceLayout variant="soft" tone="primary" class="rounded-3xl px-6 py-12 text-center sm:py-16">
      <Heading :level="2" size="xl" weight="bold" class="sm:text-3xl">Your codes, forever.</Heading>
      <Text color="muted" class="mx-auto mt-3 max-w-xl">
        Create a programmable code in under a minute. No account needed, unlimited scans, and it never expires on you.
      </Text>
      <div class="mt-7 flex flex-wrap justify-center gap-3">
        <Button as-child tone="primary">
          <RouterLink :to="AppPaths.newCode()">Create your first code</RouterLink>
        </Button>
        <Button as-child tone="neutral" variant="outline">
          <RouterLink :to="AppPaths.pricing()">See pricing</RouterLink>
        </Button>
      </div>
    </SurfaceLayout>
  </div>
</template>
