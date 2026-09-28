<script setup lang="ts">
import {
  AccordionGroup,
  AccordionGroupContent,
  AccordionGroupItem,
  AccordionGroupTrigger,
} from '@wow-two-beta/ui-vue/presentation/display';

import type { FaqModel } from './MarketingContent';

/** Defines the props of {@link FaqGroup}. */
export interface FaqGroupProps {
  /** The questions, in display order. */
  readonly items: ReadonlyArray<FaqModel>;
}

/** Renders questions as a single-open accordion with the first answer open. */
defineOptions({ name: 'FaqGroup' });
defineProps<FaqGroupProps>();
</script>

<template>
  <AccordionGroup
    type="single"
    is-collapsible
    default-value="0"
    class="mx-auto max-w-2xl divide-y divide-border overflow-hidden rounded-2xl border border-border [&>*]:border-b-0"
  >
    <AccordionGroupItem v-for="(item, index) in items" :key="item.question" :value="String(index)" class="border-b-0">
      <AccordionGroupTrigger class="gap-4 px-5 py-4 text-base font-medium hover:bg-transparent [&>svg]:size-[18px]">
        {{ item.question }}
      </AccordionGroupTrigger>
      <AccordionGroupContent>
        <div class="-mt-1 px-5 pb-5 leading-relaxed text-muted-foreground">{{ item.answer }}</div>
      </AccordionGroupContent>
    </AccordionGroupItem>
  </AccordionGroup>
</template>
