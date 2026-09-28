<script setup lang="ts">
import { Check, X } from '@lucide/vue';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeaderCell,
  TableRow,
} from '@wow-two-beta/ui-vue/presentation/display';

import { ComparisonRows } from './MarketingContent';

/**
 * Renders the comparison with a typical incumbent; it scrolls sideways on narrow screens. The table owns its
 * scroll region, focusable and named, so keyboard users can scroll it too.
 */
defineOptions({ name: 'ComparisonTable' });
</script>

<template>
  <div
    role="region"
    aria-label="ForeverPin compared with a typical QR vendor"
    tabindex="0"
    class="relative w-full overflow-x-auto rounded-2xl border border-border focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring"
  >
    <Table is-bare density="roomy" class="min-w-[34rem]">
      <TableHead head-variant="plain">
        <TableRow>
          <TableHeaderCell class="font-medium text-muted-foreground"
            ><span class="sr-only">Topic</span></TableHeaderCell
          >
          <TableHeaderCell class="text-primary">ForeverPin</TableHeaderCell>
          <TableHeaderCell class="font-medium text-muted-foreground">Typical incumbent</TableHeaderCell>
        </TableRow>
      </TableHead>
      <TableBody>
        <TableRow v-for="row in ComparisonRows" :key="row.dimension">
          <TableCell class="font-medium">{{ row.dimension }}</TableCell>
          <TableCell>
            <span class="inline-flex items-start gap-2">
              <Check :size="16" class="mt-0.5 shrink-0 text-primary" aria-hidden="true" />
              <span>{{ row.foreverPin }}</span>
            </span>
          </TableCell>
          <TableCell class="text-muted-foreground">
            <span class="inline-flex items-start gap-2">
              <X :size="16" class="mt-0.5 shrink-0" aria-hidden="true" />
              <span>{{ row.incumbent }}</span>
            </span>
          </TableCell>
        </TableRow>
      </TableBody>
    </Table>
  </div>
</template>
