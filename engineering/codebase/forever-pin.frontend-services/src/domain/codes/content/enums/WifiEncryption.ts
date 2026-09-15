/** Defines the Wi-Fi authentication scheme. */
export const WifiEncryption = {
  /** Refers to WPA, WPA2, or WPA3 personal. */
  Wpa: "wpa",

  /** Refers to legacy WEP. */
  Wep: "wep",

  /** Refers to an open network without a password. */
  None: "none",
} as const;

export type WifiEncryption = (typeof WifiEncryption)[keyof typeof WifiEncryption];
