import { useAuth } from '@wow-two-beta/ui-vue/auth';
import { computed } from 'vue';

import { UserKind, type Me } from '@/domain/identity';

import type { SignInInput } from '../models/SignInInput';

/** Reads the visitor's session: guest or account, plus the sign-in and sign-out commands. */
export function useSession() {
  const auth = useAuth<Me, SignInInput>();
  return {
    auth,
    /** The signed-in identity, or `null` before a session exists. */
    me: computed(() => auth.user),
    /** Whether the visitor is a guest whose codes live on this device until they sign in. */
    isGuest: computed(() => auth.user?.kind === UserKind.Guest),
    /** The registered account, or `null` for a guest. */
    account: computed(() => (auth.user?.kind === UserKind.User ? auth.user.user : null)),
    /** Provisions a guest session. */
    continueAsGuest: () => auth.signIn({ kind: 'guest' }),
    /** Exchanges a Google ID token; a guest's codes move to the account. */
    signInWithGoogle: (idToken: string) => auth.signIn({ kind: 'google', idToken }),
    /** Signs out; the session becomes anonymous even when the server call fails. */
    signOut: () => auth.signOut(),
  };
}
