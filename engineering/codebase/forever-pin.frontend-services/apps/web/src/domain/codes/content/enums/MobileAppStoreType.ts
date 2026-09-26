/** Defines which app store a mobile-app link points at. */
export const MobileAppStoreType = {
  /** Refers to the Apple App Store. */
  AppStore: "appStore",

  /** Refers to Google Play. */
  PlayStore: "playStore",

  /** Refers to another store or a direct download page. */
  Other: "other",
} as const;

export type MobileAppStoreType = (typeof MobileAppStoreType)[keyof typeof MobileAppStoreType];
