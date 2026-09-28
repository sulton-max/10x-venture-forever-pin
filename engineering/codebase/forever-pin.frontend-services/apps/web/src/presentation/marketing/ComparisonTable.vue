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

/** Renders the comparison with a typical incumbent; it scrolls sideways on narrow screens. */
defineOptions({ name: 'ComparisonTable' });
</script>

<template>
  <Table radius="2xl" density="roomy" class="min-w-[34rem]">
    <TableHead head-variant="plain">
      <TableRow>
        <TableHeaderCell class="font-medium text-muted-foreground"><span class="sr-only">Topic</span></TableHeaderCell>
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
</template>
