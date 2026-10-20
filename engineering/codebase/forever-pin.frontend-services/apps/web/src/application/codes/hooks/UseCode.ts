import { useAppQuery } from '@wow-two-beta/fe/query';
import { toValue, type MaybeRefOrGetter } from 'vue';

import { codesApi } from '@/integration/codes';

import { CodeQueryKeys } from '../CodeQueryKeys';

/** Reads one owned code; idle while no id is given. */
export function useCode(id: MaybeRefOrGetter<string | undefined>) {
  return useAppQuery({
    key: () => CodeQueryKeys.detail(toValue(id) ?? ''),
    queryFn: ({ signal }) => codesApi.get(toValue(id) ?? '', signal),
    enabled: () => Boolean(toValue(id)),
  });
}
