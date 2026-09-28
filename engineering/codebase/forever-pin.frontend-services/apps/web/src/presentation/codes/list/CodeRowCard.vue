<script setup lang="ts">
import { CopyPlus, Pencil, Trash2 } from '@lucide/vue';
import { Button, CopyButton } from '@wow-two-beta/ui-vue/presentation/actions';
import { Badge, Card, Text } from '@wow-two-beta/ui-vue/presentation/display';
import { computed } from 'vue';

import { oppositeMode } from '@/application/codes';
import { CodeRuleType, ContentMode, ContentType, type CodeDto } from '@/domain/codes';

import { ContentModeDisplays } from '../content/ContentModeDisplays';

/** Defines the props of {@link CodeRowCard}. */
export interface CodeRowCardProps {
  /** The code to show. */
  readonly code: CodeDto;
  /** Whether one of this code's own commands is running. */
  readonly isBusy: boolean;
}

/** Defines the events of {@link CodeRowCard}. */
export interface CodeRowCardEmits {
  /** Asks to open the code in the builder. */
  (event: 'edit'): void;
  /** Asks to copy the code into the opposite mode. */
  (event: 'copy', mode: ContentMode): void;
  /** Asks to pause or resume the code. */
  (event: 'toggle-active'): void;
  /** Asks to delete the code. */
  (event: 'delete'): void;
}

/** Holds the scan count format. */
const ScanFormat = new Intl.NumberFormat();

/** Renders one code: its name, mode, state, short link, destination, scans and commands. */
defineOptions({ name: 'CodeRowCard' });
const props = defineProps<CodeRowCardProps>();
const emit = defineEmits<CodeRowCardEmits>();

const mode = computed(() => ContentModeDisplays[props.code.mode]);
const copyMode = computed(() => oppositeMode(props.code.mode));
// A static copy holds one rule, so only single-rule codes can become static.
const canCopy = computed(() => copyMode.value === ContentMode.Dynamic || props.code.rules.length === 1);
const destination = computed(() => {
  for (const rule of props.code.rules)
    if (rule.type !== CodeRuleType.DefaultPointer && rule.content.type === ContentType.Url) return rule.content.url;
  return undefined;
});
</script>

<template>
  <Card class="surface-soft flex flex-col gap-4 p-4 sm:flex-row sm:items-center">
    <div class="min-w-0 flex-1">
      <div class="flex items-center gap-2">
        <span class="truncate font-medium" :title="code.name">{{ code.name }}</span>
        <Badge :variant="mode.badge" size="xs" class="shrink-0">{{ mode.label }}</Badge>
        <span
          class="inline-flex shrink-0 items-center gap-1.5 rounded-full bg-muted px-2 py-0.5 text-xs font-medium text-muted-foreground"
        >
          <span class="size-1.5 rounded-full" :class="code.isActive ? 'bg-accent' : 'bg-subtle-foreground'" />
          {{ code.isActive ? 'Active' : 'Inactive' }}
        </span>
      </div>
      <div v-if="code.shortUrl" class="mt-1 flex items-center gap-2">
        <a
          :href="code.shortUrl"
          target="_blank"
          rel="noreferrer"
          class="truncate font-mono text-sm text-muted-foreground hover:text-foreground hover:underline"
          :title="code.shortUrl"
        >
          {{ code.shortUrl }}
        </a>
        <CopyButton size="xs" variant="ghost" tone="neutral" :text="code.shortUrl" aria-label="Copy short URL" />
      </div>
      <Text v-if="destination" size="sm" color="muted" is-truncated class="mt-1" :title="destination">
        → {{ destination }}
      </Text>
    </div>

    <div class="flex shrink-0 items-center gap-4 sm:gap-6">
      <div class="text-right">
        <div class="text-lg font-semibold tabular-nums">{{ ScanFormat.format(code.scanCount) }}</div>
        <Text size="xs" color="muted">scans</Text>
      </div>

      <div class="flex flex-wrap items-center justify-end gap-1">
        <Button
          v-if="canCopy"
          size="sm"
          variant="ghost"
          tone="neutral"
          :is-disabled="isBusy"
          @click="emit('copy', copyMode)"
        >
          <template #leading><CopyPlus :size="15" aria-hidden="true" /></template>
          Copy as {{ ContentModeDisplays[copyMode].label.toLowerCase() }}
        </Button>
        <Button size="sm" variant="ghost" tone="neutral" :is-disabled="isBusy" @click="emit('edit')">
          <template #leading><Pencil :size="15" aria-hidden="true" /></template>
          Edit
        </Button>
        <Button size="sm" variant="soft" tone="neutral" :is-loading="isBusy" @click="emit('toggle-active')">
          {{ code.isActive ? 'Disable' : 'Enable' }}
        </Button>
        <Button
          size="sm"
          variant="ghost"
          tone="danger"
          shape="square"
          aria-label="Delete code"
          :is-disabled="isBusy"
          @click="emit('delete')"
        >
          <Trash2 :size="16" />
        </Button>
      </div>
    </div>
  </Card>
</template>
