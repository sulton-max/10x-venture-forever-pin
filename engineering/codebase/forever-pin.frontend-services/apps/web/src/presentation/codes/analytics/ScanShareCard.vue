<script setup lang="ts">
import { Card, Heading, Text } from '@wow-two-beta/fe/presentation/display';
import { ProgressBar } from '@wow-two-beta/fe/presentation/feedback';
import { computed } from 'vue';

import type { ScanShareDto } from '@/domain/codes';

import { AnalyticsDisplays } from './AnalyticsDisplays';

/** Defines the props of {@link ScanShareCard}. */
export interface ScanShareCardProps {
  /** The card heading. */
  readonly title: string;
  /** The shares to list, most first. */
  readonly shares: ReadonlyArray<ScanShareDto>;
  /** Names one share's value. */
  readonly describe: (key?: string) => string;
}

/** Holds how many shares are listed before the rest fold into one row. */
const VisibleShares = 6;

/** Renders one scan attribute's shares as labelled bars, folding the long tail into one row. */
defineOptions({ name: 'ScanShareCard' });
const props = defineProps<ScanShareCardProps>();

const total = computed(() => props.shares.reduce((sum, share) => sum + share.scans, 0));
const rows = computed(() => {
  const shown = props.shares.slice(0, VisibleShares).map((share) => ({
    label: props.describe(share.key),
    scans: share.scans,
  }));
  const rest = props.shares.slice(VisibleShares);
  if (rest.length > 0)
    shown.push({ label: `${rest.length} more`, scans: rest.reduce((sum, share) => sum + share.scans, 0) });
  return shown;
});
</script>

<template>
  <Card class="surface-glass flex flex-col gap-4 p-5">
    <Heading :level="2" size="sm" weight="semibold">{{ title }}</Heading>
    <Text v-if="rows.length === 0" size="sm" color="muted">No scans in this window.</Text>
    <ul v-else class="flex flex-col gap-3">
      <li v-for="row in rows" :key="row.label" class="flex flex-col gap-1.5">
        <div class="flex items-baseline justify-between gap-3 text-sm">
          <span class="truncate" :title="row.label">{{ row.label }}</span>
          <span class="shrink-0 tabular-nums text-muted-foreground">
            {{ AnalyticsDisplays.count(row.scans) }} · {{ AnalyticsDisplays.percent(row.scans, total) }}
          </span>
        </div>
        <ProgressBar
          size="sm"
          tone="brand"
          :value="total > 0 ? (row.scans / total) * 100 : 0"
          :label="`${row.label}: ${AnalyticsDisplays.percent(row.scans, total)} of scans`"
        />
      </li>
    </ul>
    <slot name="footer" />
  </Card>
</template>
