import { createAuthBridge } from '@wow-two-beta/fe/auth';

/** Describes what the bridge compares between sessions: the identity kind and, for an account, its id. */
export interface SessionIdentity {
  /** How the caller is identified. */
  readonly kind: string;
  /** The account, or `null` for a guest. */
  readonly user: { readonly id: string } | null;
}

/** Holds the session bridge — API 401s flip the session; a new guest or account clears private queries. */
export const authBridge = createAuthBridge<SessionIdentity>({
  getIdentity: (identity) => `${identity.kind}:${identity.user?.id ?? ''}`,
});
