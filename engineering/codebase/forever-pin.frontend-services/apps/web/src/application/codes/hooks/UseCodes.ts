import { useAppQuery } from '@wow-two-beta/fe/query';
import { toValue, type MaybeRefOrGetter } from 'vue';

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
    keepPreviousData: true,
  });

  return {
    /** The codes to show: the current search's, or the last shown list while it loads. */
    codes: query.data,
    /** Whether nothing has loaded yet. */
    isFirstLoad: query.isFirstLoad,
    /** Whether a newer search, or a refetch, is loading behind the shown list. */
    isRefreshing: query.isRefreshing,
    /** The failure, or `null`. */
    error: query.error,
    /** Reloads the current search. */
    refetch: query.refetch,
  };
}
