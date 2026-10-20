import { useAppQuery } from '@wow-two-beta/fe/query';
import { computed } from 'vue';

import { systemApi } from '@/integration/common';

/** Holds the cache key of the API's status. */
const SystemStatusKey = ['systemStatus'] as const;

/** Reads the API's running version for the account menu; a failure leaves it unknown and stays quiet. */
export function useSystemStatus() {
  const query = useAppQuery({
    key: SystemStatusKey,
    queryFn: ({ signal }) => systemApi.status(signal),
    meta: { suppressGlobalError: true },
  });
  return {
    /** The API's informational version (`X.Y.Z+<commit>`), or `null` until it answers. */
    apiVersion: computed(() => query.data.value?.version ?? null),
  };
}
