import { AuthStatus } from '@wow-two-beta/ui-vue/auth';
import { NoticeTone, notify } from '@wow-two-beta/ui-vue/feedback';
import { provideGoogleIdentity } from '@wow-two-beta/ui-vue/foundation/oauth';
import type { AppError } from '@wow-two-beta/ui-vue/foundation/results';
import { computed, ref, shallowRef } from 'vue';

import { FailureMessages, useRuntimeConfig } from '@/application/common';

import { SessionPhase } from '../models/SessionPhase';

import { useSession } from './UseSession';

/**
 * Owns the app shell's session lifecycle: checks the session on entry, offers retry after a failed check,
 * provisions guests, exchanges Google credentials and signs out — each failure stays visible.
 * Provides the Google identity owner, so call it once, from the app layout.
 */
export function useSessionGate() {
  const session = useSession();
  const { googleClientId } = useRuntimeConfig();
  const checkFailure = shallowRef<AppError | null>(null);
  const signInFailure = ref<string | null>(null);
  const isChecking = ref(false);
  const isStartingGuest = ref(false);
  const isSigningOut = ref(false);

  const phase = computed<SessionPhase>(() => {
    if (checkFailure.value) return SessionPhase.Failed;
    if (session.auth.isPending) return SessionPhase.Checking;
    return session.auth.status === AuthStatus.Authenticated ? SessionPhase.Ready : SessionPhase.Gate;
  });

  const google = provideGoogleIdentity({
    clientId: () => googleClientId.value,
    onCredential: (credential) => void signInWithGoogle(credential),
    onError: () => reportSignInFailure('Google sign-in could not load. Try again, or continue as a guest.'),
  });

  /** Checks the session again; a failure keeps the visitor's cookie and offers another retry. */
  async function check(): Promise<void> {
    isChecking.value = true;
    const result = await session.auth.refresh();
    isChecking.value = false;
    checkFailure.value = result.ok ? null : result.failure;
  }

  /** Provisions a guest; the new session renders the routed page at the same address. */
  async function continueAsGuest(): Promise<void> {
    isStartingGuest.value = true;
    signInFailure.value = null;
    const result = await session.continueAsGuest();
    isStartingGuest.value = false;
    if (!result.ok)
      reportSignInFailure(FailureMessages.describe(result.failure, 'We could not start a guest session.'));
  }

  /** Exchanges a Google ID token; a guest's codes move to the account. */
  async function signInWithGoogle(idToken: string): Promise<void> {
    signInFailure.value = null;
    const result = await session.signInWithGoogle(idToken);
    if (!result.ok) reportSignInFailure(FailureMessages.describe(result.failure, 'Google sign-in failed.'));
  }

  /** Signs out; the gate shows even when the server call fails, and the failure is reported. */
  async function signOut(): Promise<void> {
    isSigningOut.value = true;
    google.disableAutoSelect();
    const result = await session.signOut();
    isSigningOut.value = false;
    if (!result.ok)
      notify({
        tone: NoticeTone.Danger,
        title: 'Sign-out did not reach ForeverPin',
        description: 'Your session may still be active on this device. Try signing out again.',
      });
  }

  /** Shows a sign-in failure inline at the gate, or as a notice once the app is open. */
  function reportSignInFailure(message: string): void {
    if (phase.value === SessionPhase.Ready)
      notify({ tone: NoticeTone.Danger, title: 'Sign-in failed', description: message });
    else signInFailure.value = message;
  }

  // Entering the app checks an unexamined or signed-out session; a signed-in or resolving one is reused.
  if (session.auth.status === AuthStatus.Unknown || session.auth.status === AuthStatus.Anonymous) void check();

  return {
    phase,
    /** The signed-in identity, or `null`. */
    me: session.me,
    /** Whether the visitor is a guest. */
    isGuest: session.isGuest,
    /** The registered account, or `null` for a guest. */
    account: session.account,
    /** Whether Google sign-in is configured. */
    isGoogleEnabled: computed(() => Boolean(googleClientId.value)),
    /** The failed session check's message, or an empty string. */
    checkFailureMessage: computed(() =>
      checkFailure.value ? FailureMessages.describe(checkFailure.value, 'Your session could not be checked.') : '',
    ),
    signInFailure,
    isChecking,
    isStartingGuest,
    isSigningOut,
    check,
    continueAsGuest,
    signOut,
  };
}
