import { createApiClient, wowTwoEnvelope } from '@wow-two-beta/ui-vue/foundation/http';

import { createAntiforgeryFetch } from './AntiforgeryFetch';
import { authBridge } from './AuthBridge';

/** Holds the path whose read issues a fresh antiforgery token. */
const TokenRefreshPath = '/api/runtime-config';

/** Holds the same-origin client for the ForeverPin API, unwrapping the `{ data }` success envelope. */
export const apiClient = createApiClient({
  envelope: wowTwoEnvelope,
  credentials: 'same-origin',
  scope: authBridge.scope,
  onUnauthorized: authBridge.onUnauthorized,
  fetch: createAntiforgeryFetch(TokenRefreshPath),
});
