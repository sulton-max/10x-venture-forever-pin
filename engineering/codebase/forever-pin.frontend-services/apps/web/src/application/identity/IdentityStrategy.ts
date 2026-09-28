import { createCookieStrategy } from '@wow-two-beta/ui-vue/auth';

import { UserKind, type Me } from '@/domain/identity';
import { apiClient } from '@/integration/common';
import { decodeMe, identityApi } from '@/integration/identity';

import type { SignInInput } from './models/SignInInput';

/**
 * Creates the cookie-session strategy: an anonymous visitor sees the guest gate; guests and
 * accounts are signed in, and a sign-in exchange returns the new identity.
 */
export function createIdentityStrategy() {
  return createCookieStrategy<Me, SignInInput>({
    client: apiClient,
    decodeUser: decodeMe,
    mePath: '/api/identity/me',
    signOutPath: '/api/auth/logout',
    isAnonymous: (me) => me.kind === UserKind.Anonymous,
    signIn: (input, context) =>
      input.kind === 'guest'
        ? identityApi.createGuest(context.signal)
        : identityApi.signInWithGoogle(input.idToken, context.signal),
  });
}
