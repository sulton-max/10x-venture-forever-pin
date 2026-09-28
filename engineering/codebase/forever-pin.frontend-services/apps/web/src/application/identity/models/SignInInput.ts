/** Defines how a visitor establishes a session. */
export type SignInInput =
  | {
      /** Provisions a guest that owns codes on this device. */
      readonly kind: 'guest';
    }
  | {
      /** Exchanges a verified Google ID token; the guest's codes move to the account. */
      readonly kind: 'google';
      /** The Google ID token. */
      readonly idToken: string;
    };
