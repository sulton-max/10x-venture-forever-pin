/** Defines the kind of content a code carries. */
export const ContentType = {
  /** Refers to a destination URL. */
  Url: "url",

  /** Refers to an app-store link. */
  MobileApp: "mobileApp",

  /** Refers to free-form text. */
  Text: "text",

  /** Refers to an email recipient and draft. */
  Email: "email",

  /** Refers to an SMS recipient and message. */
  Sms: "sms",

  /** Refers to a telephone number. */
  Phone: "phone",

  /** Refers to a geographic location. */
  Geo: "geo",

  /** Refers to Wi-Fi network credentials. */
  Wifi: "wifi",

  /** Refers to a contact card. */
  VCard: "vCard",

  /** Refers to a calendar event. */
  Calendar: "calendar",
} as const;

export type ContentType = (typeof ContentType)[keyof typeof ContentType];
