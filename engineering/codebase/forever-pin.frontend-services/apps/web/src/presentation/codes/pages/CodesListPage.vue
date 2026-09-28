<script setup lang="ts">
import { Plus, QrCode } from '@lucide/vue';
import { Button } from '@wow-two-beta/ui-vue/presentation/actions';
import { Card, EmptyState, Heading, Text } from '@wow-two-beta/ui-vue/presentation/display';
import { Alert, Spinner } from '@wow-two-beta/ui-vue/presentation/feedback';
import type { AppError, Result } from '@wow-two-beta/ui-vue/foundation/results';
import { SearchInput } from '@wow-two-beta/ui-vue/presentation/forms';
import { computed, reactive, ref, shallowRef } from 'vue';

import { useCodeCommands, useCodes } from '@/application/codes';
import { FailureMessages, useSettledValue } from '@/application/common';
import type { CodeDto, ContentMode } from '@/domain/codes';

import CodeRowCard from '../list/CodeRowCard.vue';
import CopyCodeModal from '../list/CopyCodeModal.vue';
import DeleteCodeModal from '../list/DeleteCodeModal.vue';

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

/** Renders the owner's searchable code dashboard; each row's commands track their own progress. */
defineOptions({ name: 'CodesListPage' });
const props = defineProps<CodesListPageProps>();

const search = ref('');
const settledSearch = useSettledValue(search, SearchSettleMs);
const list = useCodes(settledSearch);
const commands = useCodeCommands();
const busyIds = reactive(new Set<string>());
const pendingDelete = shallowRef<CodeDto | null>(null);
const pendingCopy = shallowRef<CodeDto | null>(null);
const commandFailure = ref<string | null>(null);

const listFailure = computed(() =>
  list.error.value ? FailureMessages.describe(list.error.value, 'Your codes could not be loaded.') : null,
);

/** Runs one row command, keeping that row busy until its own reply lands. */
async function runForRow(id: string, command: () => Promise<Result<unknown, AppError>>, fallback: string) {
  busyIds.add(id);
  commandFailure.value = null;
  const result = await command();
  busyIds.delete(id);
  if (!result.ok) commandFailure.value = FailureMessages.describe(result.failure, fallback);
  return result.ok;
}

function toggleActive(code: CodeDto): void {
  void runForRow(
    code.id,
    () => commands.setActive.mutateAsync({ id: code.id, isActive: !code.isActive }),
    'The code could not be updated.',
  );
}

async function confirmDelete(): Promise<void> {
  const target = pendingDelete.value;
  if (!target) return;
  if (await runForRow(target.id, () => commands.remove.mutateAsync(target.id), 'The code could not be deleted.'))
    pendingDelete.value = null;
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
      <Spinner v-if="list.isRefreshing.value" size="sm" label="Searching" />
    </div>

    <Alert v-if="listFailure" severity="danger" :description="listFailure" />
    <Alert v-if="commandFailure" severity="danger" :description="commandFailure" />

    <div v-if="list.isLoading.value" class="flex min-h-[40vh] items-center justify-center">
      <Spinner size="lg" label="Loading codes" />
    </div>
    <Card v-else-if="list.codes.value && list.codes.value.length === 0" class="surface-soft p-6">
      <EmptyState
        :title="settledSearch ? 'No matching codes' : 'No codes yet'"
        :description="
          settledSearch
            ? 'Try a different search term.'
            : 'Create your first programmable code — one code, many destinations.'
        "
      >
        <template #icon><QrCode :size="32" /></template>
        <template v-if="!settledSearch" #actions>
          <Button tone="primary" @click="props.onCreate">
            <template #leading><Plus :size="16" aria-hidden="true" /></template>
            Create new
          </Button>
        </template>
      </EmptyState>
    </Card>
    <div v-else-if="list.codes.value" class="flex flex-col gap-3" :aria-busy="list.isRefreshing.value">
      <CodeRowCard
        v-for="code in list.codes.value"
        :key="code.id"
        :code="code"
        :is-busy="busyIds.has(code.id)"
        @edit="props.onEdit(code.id)"
        @analytics="props.onAnalytics(code.id)"
        @copy="pendingCopy = code"
        @toggle-active="toggleActive(code)"
        @delete="pendingDelete = code"
      />
    </div>

    <CopyCodeModal
      :code="pendingCopy"
      @confirm="(mode) => pendingCopy && props.onCopy(pendingCopy.id, mode)"
      @cancel="pendingCopy = null"
    />
    <DeleteCodeModal
      :code="pendingDelete"
      :is-deleting="pendingDelete !== null && busyIds.has(pendingDelete.id)"
      @confirm="confirmDelete"
      @cancel="pendingDelete = null"
    />
  </div>
</template>
