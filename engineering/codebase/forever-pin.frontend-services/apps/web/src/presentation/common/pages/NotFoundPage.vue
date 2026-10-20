<script setup lang="ts">
import { Button } from '@wow-two-beta/fe/presentation/actions';
import { Heading, Text } from '@wow-two-beta/fe/presentation/display';
import { RouterLink } from 'vue-router';

import { AppPaths } from '../AppPaths';

/** Renders the not-found place: an unknown address, or an article that does not exist. */
defineOptions({ name: 'NotFoundPage' });
</script>

<template>
  <section class="mx-auto w-full max-w-6xl px-6 py-16 sm:py-20">
    <div class="mx-auto max-w-md text-center">
      <Text as="span" size="sm" weight="semibold" color="brand">404</Text>
      <Heading :level="1" size="2xl" weight="bold" class="mt-2">This page wandered off</Heading>
      <Text color="muted" class="mt-3">
        The page you're looking for doesn't exist — but your codes are still safe, and still working.
      </Text>
      <div class="mt-7 flex flex-wrap justify-center gap-3">
        <Button as-child tone="primary">
          <RouterLink :to="AppPaths.home()">Back home</RouterLink>
        </Button>
        <Button as-child tone="neutral" variant="outline">
          <RouterLink :to="AppPaths.codes()">Open the app</RouterLink>
        </Button>
      </div>
    </div>
  </section>
</template>
