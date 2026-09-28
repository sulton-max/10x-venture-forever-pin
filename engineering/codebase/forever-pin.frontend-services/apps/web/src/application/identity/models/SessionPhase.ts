/** Defines what the app shell renders for the visitor's session. */
export const SessionPhase = {
  /** Refers to a session check in flight — render a spinner, not the gate. */
  Checking: 'checking',
  /** Refers to a session check that failed — offer a retry, never a new guest. */
  Failed: 'failed',
  /** Refers to an anonymous visitor — render the sign-in gate. */
  Gate: 'gate',
  /** Refers to a guest or an account — render the routed page. */
  Ready: 'ready',
} as const;

export type SessionPhase = (typeof SessionPhase)[keyof typeof SessionPhase];
