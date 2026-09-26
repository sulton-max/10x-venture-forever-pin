import type { UserKind } from "./enums/UserKind";

/** Represents a registered user's profile. */
export interface UserSummary {
  /** The stable user id. */
  id: string;

  /** The display name. */
  name: string;

  /** The primary email address. */
  email: string;
}

/** Represents the current identity state. */
export interface Me {
  /** How the visitor is identified. */
  kind: UserKind;

  /** The registered user's profile, or null for a guest or anonymous visitor. */
  user: UserSummary | null;
}
