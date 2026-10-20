import { createApiClient } from '@wow-two-beta/fe/foundation/http';

import { authBridge } from './AuthBridge';

/** Holds the path whose read issues a fresh antiforgery token. */
const TokenRefreshPath = '/api/runtime-config';

/**
 * Holds the same-origin client for the ForeverPin API, unwrapping the `{ data }` success envelope. Cookie-carrying
 * writes echo the antiforgery token; a token issued before sign-in or sign-out is refreshed once through a read.
 */
export const apiClient = createApiClient({
  credentials: 'same-origin',
  scope: authBridge.scope,
  onUnauthorized: authBridge.onUnauthorized,
  antiforgery: { refreshPath: TokenRefreshPath },
});
