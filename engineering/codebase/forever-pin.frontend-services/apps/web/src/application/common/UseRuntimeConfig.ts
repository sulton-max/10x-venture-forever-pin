import { useAppQuery } from '@wow-two-beta/fe/query';
import { computed } from 'vue';

import { runtimeConfigApi } from '@/integration/common';

/** Holds the cache key of the public runtime settings. */
const RuntimeConfigKey = ['runtimeConfig'] as const;

/** Reads the API's public runtime settings; the build-time Google id fills in while they load. */
export function useRuntimeConfig() {
  const query = useAppQuery({ key: RuntimeConfigKey, queryFn: ({ signal }) => runtimeConfigApi.get(signal) });
  return {
    /** The Google OAuth web client id; empty keeps sign-in hidden. */
    googleClientId: computed(() => query.data.value?.googleClientId ?? import.meta.env.VITE_GOOGLE_CLIENT_ID ?? ''),
    /** The public origin a dynamic code's short link starts with. */
    redirectBaseUrl: computed(() => query.data.value?.redirectBaseUrl ?? ''),
    /** Whether the settings failed to load. */
    error: query.error,
  };
}
