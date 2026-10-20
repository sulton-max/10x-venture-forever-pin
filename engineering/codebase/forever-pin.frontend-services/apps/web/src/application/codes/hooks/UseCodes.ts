import { useAppQuery } from '@wow-two-beta/fe/query';
import { computed, shallowRef, toValue, watch, type MaybeRefOrGetter } from 'vue';

import type { CodeDto } from '@/domain/codes';
import { codesApi } from '@/integration/codes';

import { CodeQueryKeys } from '../CodeQueryKeys';

/**
 * Reads the owner's codes for a search. Each search has its own cache entry, so a slow older reply never
 * replaces a newer one; the last shown list stays visible while another search loads.
 */
export function useCodes(search: MaybeRefOrGetter<string>) {
  const query = useAppQuery({
    key: () => CodeQueryKeys.list(toValue(search)),
    queryFn: ({ signal }) => codesApi.list(toValue(search), signal),
  });
  const lastShown = shallowRef<ReadonlyArray<CodeDto>>();
  watch(query.data, (codes) => {
    if (codes) lastShown.value = codes;
  });

  return {
    /** The codes to show: the current search's, or the last shown list while it loads. */
    codes: computed(() => query.data.value ?? lastShown.value),
    /** Whether nothing has loaded yet. */
    isLoading: computed(() => query.loading.value && !lastShown.value),
    /** Whether a newer search is loading behind the shown list. */
    isRefreshing: computed(() => query.loading.value && Boolean(lastShown.value)),
    /** The failure, or `null`. */
    error: query.error,
    /** Reloads the current search. */
    refetch: query.refetch,
  };
}
