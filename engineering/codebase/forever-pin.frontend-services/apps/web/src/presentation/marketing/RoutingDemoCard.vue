<script setup lang="ts">
import { Clock, Globe, Languages, Smartphone } from 'lucide-vue-next';
import { Card, Text } from '@wow-two-beta/fe/presentation/display';

import StaticQrPreview from './StaticQrPreview.vue';

/** Holds the illustrated rules, in evaluation order. */
const DemoRules = [
  { icon: Smartphone, when: 'On an iPhone', then: '→ App Store' },
  { icon: Globe, when: 'In Germany', then: '→ German store' },
  { icon: Clock, when: 'Before 4pm', then: '→ Lunch menu' },
  { icon: Languages, when: 'Browser set to Russian', then: '→ RU landing page' },
] as const;

/** Renders a client-side QR code beside the rules it resolves through. */
defineOptions({ name: 'RoutingDemoCard' });
</script>

<template>
  <Card variant="outline" class="grid items-center gap-8 rounded-3xl bg-card p-8 lg:grid-cols-2">
    <div class="flex justify-center">
      <StaticQrPreview value="https://foreverpin.com/demo" foreground="#18181b" background="#ffffff" :size="200" />
    </div>
    <div class="flex flex-col gap-3">
      <Text size="sm" weight="medium" color="muted">One printed code resolves by context:</Text>
      <div
        v-for="rule in DemoRules"
        :key="rule.when"
        class="flex items-center gap-3 rounded-lg border border-border bg-background px-4 py-3"
      >
        <span class="grid size-9 shrink-0 place-items-center rounded-md bg-primary-soft text-primary">
          <component :is="rule.icon" :size="18" />
        </span>
        <span class="text-sm">
          <span class="font-medium">{{ rule.when }}</span> <span class="text-muted-foreground">{{ rule.then }}</span>
        </span>
      </div>
      <Text size="xs" color="muted">
        …otherwise → your fallback URL. Change any of this without reprinting the code.
      </Text>
    </div>
  </Card>
</template>
