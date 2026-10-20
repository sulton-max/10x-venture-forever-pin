import { useAppQuery } from '@wow-two-beta/fe/query';
import { toValue, type MaybeRefOrGetter } from 'vue';

import { codesApi } from '@/integration/codes';

import { CodeQueryKeys } from '../CodeQueryKeys';

/**
 * Reads one owned code's scans over its most recent `days` UTC days; idle while no id is given. Switching the window
 * keeps the last window's figures on screen until the new one answers.
 */
export function useCodeAnalytics(id: MaybeRefOrGetter<string | undefined>, days: MaybeRefOrGetter<number>) {
  return useAppQuery({
    key: () => CodeQueryKeys.analytics(toValue(id) ?? '', toValue(days)),
    queryFn: ({ signal }) => codesApi.analytics(toValue(id) ?? '', toValue(days), signal),
    enabled: () => Boolean(toValue(id)),
    keepPreviousData: true,
  });
}
