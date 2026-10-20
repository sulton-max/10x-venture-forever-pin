<script setup lang="ts">
import { Plus } from 'lucide-vue-next';
import { Button } from '@wow-two-beta/fe/presentation/actions';
import { Heading, Text } from '@wow-two-beta/fe/presentation/display';
import { DataRegion, InlineSpinner, SkeletonState } from '@wow-two-beta/fe/presentation/feedback';
import { useDebouncedValue } from '@wow-two-beta/fe/foundation/async';
import type { AppError, Result } from '@wow-two-beta/fe/foundation/results';
import { SearchInput } from '@wow-two-beta/fe/presentation/forms';
import { reactive, ref, shallowRef } from 'vue';

import { useCodeCommands, useCodes } from '@/application/codes';
import type { CodeDto, ContentMode } from '@/domain/codes';

import CodeRowCard from '../list/CodeRowCard.vue';
import CopyCodeModal from '../list/CopyCodeModal.vue';
import DeleteCodeModal from '../list/DeleteCodeModal.vue';
import DownloadCodeModal from '../list/DownloadCodeModal.vue';

/** Defines the props of {@link CodesListPage}. */
export interface CodesListPageProps {
  /** Opens the empty builder. */
  readonly onCreate: () => void;
  /** Opens a code in the builder. */
  readonly onEdit: (id: string) => void;
  /** Opens the builder prefilled from a code, in the chosen mode. */
  readonly onCopy: (id: string, mode: ContentMode) => void;
  /** Opens a dynamic code's scan analytics. */
  readonly onAnalytics: (id: string) => void;
}

/** Holds how long typing must pause before a search runs. */
const SearchSettleMs = 250;

/** Renders the owner's searchable code dashboard; each row's commands track their own progress, failures toast. */
defineOptions({ name: 'CodesListPage' });
const props = defineProps<CodesListPageProps>();

const search = ref('');
const settledSearch = useDebouncedValue(search, SearchSettleMs);
const list = useCodes(settledSearch);
const commands = useCodeCommands();
const busyIds = reactive(new Set<string>());
const pendingDelete = shallowRef<CodeDto | null>(null);
const pendingCopy = shallowRef<CodeDto | null>(null);
const pendingDownload = shallowRef<CodeDto | null>(null);

/** Runs one row command, keeping that row busy until its own reply lands; a failure reaches the toast host. */
async function runForRow(id: string, command: () => Promise<Result<unknown, AppError>>) {
  busyIds.add(id);
  const result = await command();
  busyIds.delete(id);
  return result.ok;
}

function toggleActive(code: CodeDto): void {
  void runForRow(code.id, () => commands.setActive.mutateAsync({ id: code.id, isActive: !code.isActive }));
}

async function confirmDelete(): Promise<void> {
  const target = pendingDelete.value;
  if (!target) return;
  if (await runForRow(target.id, () => commands.remove.mutateAsync(target.id))) pendingDelete.value = null;
}
</script>

<template>
  <div class="flex flex-col gap-6">
    <div class="flex flex-wrap items-end justify-between gap-4">
      <div>
        <Heading :level="1" size="xl" weight="bold">Your codes</Heading>
        <Text color="muted">Manage destinations and routing — nothing ever expires.</Text>
      </div>
      <Button tone="primary" @click="props.onCreate">
        <template #leading><Plus :size="16" aria-hidden="true" /></template>
        Create new
      </Button>
    </div>

    <div class="flex max-w-md items-center gap-3">
      <SearchInput
        v-model="search"
        is-clearable
        placeholder="Search by name or destination…"
        aria-label="Search codes"
      />
      <InlineSpinner v-if="list.isRefreshing.value">Searching</InlineSpinner>
    </div>

    <DataRegion
      class="flex flex-col gap-3"
      :is-first-load="list.isFirstLoad.value"
      :is-refreshing="list.isRefreshing.value"
      :error="list.error.value"
      :has-data="Boolean(list.codes.value)"
      :is-empty="list.codes.value?.length === 0"
      :empty-title="settledSearch ? 'No matching codes' : 'No codes yet'"
      :empty-description="
        settledSearch
          ? 'Try a different search term.'
          : 'Create your first programmable code — one code, many destinations.'
      "
      unavailable-title="Your codes aren't available right now"
      loading-label="Loading codes"
      @retry="list.refetch"
    >
      <template #skeleton>
        <SkeletonState v-for="index in 3" :key="index" class="h-24 w-full rounded-xl" />
      </template>
      <template v-if="!settledSearch" #emptyActions>
        <Button tone="primary" @click="props.onCreate">
          <template #leading><Plus :size="16" aria-hidden="true" /></template>
          Create new
        </Button>
      </template>
      <CodeRowCard
        v-for="code in list.codes.value"
        :key="code.id"
        :code="code"
        :is-busy="busyIds.has(code.id)"
        @edit="props.onEdit(code.id)"
        @analytics="props.onAnalytics(code.id)"
        @download="pendingDownload = code"
        @copy="pendingCopy = code"
        @toggle-active="toggleActive(code)"
        @delete="pendingDelete = code"
      />
    </DataRegion>

    <CopyCodeModal
      :code="pendingCopy"
      @confirm="(mode) => pendingCopy && props.onCopy(pendingCopy.id, mode)"
      @cancel="pendingCopy = null"
    />
    <DownloadCodeModal :code="pendingDownload" @close="pendingDownload = null" />
    <DeleteCodeModal
      :code="pendingDelete"
      :is-deleting="pendingDelete !== null && busyIds.has(pendingDelete.id)"
      @confirm="confirmDelete"
      @cancel="pendingDelete = null"
    />
  </div>
</template>
