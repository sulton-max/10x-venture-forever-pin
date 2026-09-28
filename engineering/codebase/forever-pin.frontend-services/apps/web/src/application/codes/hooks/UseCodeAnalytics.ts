import { useAppQuery } from '@wow-two-beta/ui-vue/query';
import { toValue, type MaybeRefOrGetter } from 'vue';

import { codesApi } from '@/integration/codes';

import { CodeQueryKeys } from '../CodeQueryKeys';

/** Reads one owned code's scans over its most recent `days` UTC days; idle while no id is given. */
export function useCodeAnalytics(id: MaybeRefOrGetter<string | undefined>, days: MaybeRefOrGetter<number>) {
  return useAppQuery({
    key: () => CodeQueryKeys.analytics(toValue(id) ?? '', toValue(days)),
    queryFn: ({ signal }) => codesApi.analytics(toValue(id) ?? '', toValue(days), signal),
    enabled: () => Boolean(toValue(id)),
  });
}
