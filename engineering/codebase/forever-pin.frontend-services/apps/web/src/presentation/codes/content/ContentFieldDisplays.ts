import { ContentType, MobileAppStoreType, WifiEncryption } from '@/domain/codes';

/** Defines how a content field is edited. */
export const ContentFieldKind = {
  /** Refers to one line of text. */
  Text: 'text',
  /** Refers to several lines of text. */
  TextArea: 'textArea',
  /** Refers to a web address. */
  Url: 'url',
  /** Refers to an email address. */
  Email: 'email',
  /** Refers to a phone number. */
  Tel: 'tel',
  /** Refers to a decimal number; a cleared value stays empty until it validates. */
  Number: 'number',
  /** Refers to one choice from a fixed list. */
  Select: 'select',
  /** Refers to an on/off value. */
  Switch: 'switch',
  /** Refers to a local date and time. */
  DateTime: 'dateTime',
} as const;

export type ContentFieldKind = (typeof ContentFieldKind)[keyof typeof ContentFieldKind];

/** Defines one editable leaf of a content type. */
export interface ContentFieldDisplay {
  /** The content key the field binds, one level under the rule's content. */
  readonly key: string;
  /** The label. */
  readonly label: string;
  /** How the field is edited. */
  readonly kind: ContentFieldKind;
  /** Whether the field may stay empty; an emptied optional field is removed, not stored as `''`. */
  readonly isOptional?: boolean;
  /** The placeholder, or one derived from the other content values. */
  readonly placeholder?: string | ((content: Readonly<Record<string, unknown>>) => string);
  /** The choices of a select. */
  readonly options?: ReadonlyArray<{ readonly value: string; readonly label: string }>;
  /** The bounds and step of a number. */
  readonly range?: { readonly min: number; readonly max: number; readonly step: number };
}

/** Holds the store labels and each store's URL placeholder. */
const StoreDisplays: Readonly<Record<MobileAppStoreType, { readonly label: string; readonly placeholder: string }>> = {
  [MobileAppStoreType.AppStore]: { label: 'App Store (iOS)', placeholder: 'https://apps.apple.com/app/…' },
  [MobileAppStoreType.PlayStore]: { label: 'Google Play', placeholder: 'https://play.google.com/store/apps/…' },
  [MobileAppStoreType.Other]: { label: 'Other', placeholder: 'https://yourapp.com or another store' },
};

/** Maps each content type to its fields, in display order. */
export const ContentFieldDisplays: Readonly<Record<ContentType, ReadonlyArray<ContentFieldDisplay>>> = {
  [ContentType.Url]: [
    { key: 'url', label: 'Destination URL', kind: ContentFieldKind.Url, placeholder: 'https://example.com' },
  ],
  [ContentType.MobileApp]: [
    {
      key: 'store',
      label: 'Store',
      kind: ContentFieldKind.Select,
      options: Object.values(MobileAppStoreType).map((value) => ({ value, label: StoreDisplays[value].label })),
    },
    {
      key: 'url',
      label: 'Store URL',
      kind: ContentFieldKind.Url,
      placeholder: (content) =>
        StoreDisplays[content.store as MobileAppStoreType]?.placeholder ?? StoreDisplays.other.placeholder,
    },
  ],
  [ContentType.Text]: [{ key: 'text', label: 'Text', kind: ContentFieldKind.TextArea }],
  [ContentType.Email]: [
    { key: 'to', label: 'To', kind: ContentFieldKind.Email, placeholder: 'name@example.com' },
    { key: 'subject', label: 'Subject', kind: ContentFieldKind.Text, isOptional: true },
    { key: 'body', label: 'Body', kind: ContentFieldKind.TextArea, isOptional: true },
  ],
  [ContentType.Sms]: [
    { key: 'phone', label: 'Phone', kind: ContentFieldKind.Tel },
    { key: 'message', label: 'Message', kind: ContentFieldKind.TextArea, isOptional: true },
  ],
  [ContentType.Phone]: [{ key: 'phone', label: 'Phone', kind: ContentFieldKind.Tel, placeholder: '+1 555 0100' }],
  [ContentType.Geo]: [
    { key: 'latitude', label: 'Latitude', kind: ContentFieldKind.Number, range: { min: -90, max: 90, step: 0.000001 } },
    {
      key: 'longitude',
      label: 'Longitude',
      kind: ContentFieldKind.Number,
      range: { min: -180, max: 180, step: 0.000001 },
    },
  ],
  [ContentType.Wifi]: [
    { key: 'ssid', label: 'Network name (SSID)', kind: ContentFieldKind.Text },
    { key: 'password', label: 'Password', kind: ContentFieldKind.Text, isOptional: true },
    {
      key: 'encryption',
      label: 'Security',
      kind: ContentFieldKind.Select,
      options: [
        { value: WifiEncryption.Wpa, label: 'WPA/WPA2' },
        { value: WifiEncryption.Wep, label: 'WEP' },
        { value: WifiEncryption.None, label: 'None' },
      ],
    },
    { key: 'hidden', label: 'Hidden network', kind: ContentFieldKind.Switch },
  ],
  [ContentType.VCard]: [
    { key: 'firstName', label: 'First name', kind: ContentFieldKind.Text },
    { key: 'lastName', label: 'Last name', kind: ContentFieldKind.Text, isOptional: true },
    { key: 'org', label: 'Company', kind: ContentFieldKind.Text, isOptional: true },
    { key: 'title', label: 'Title', kind: ContentFieldKind.Text, isOptional: true },
    { key: 'phone', label: 'Phone', kind: ContentFieldKind.Tel, isOptional: true },
    { key: 'email', label: 'Email', kind: ContentFieldKind.Email, isOptional: true },
    { key: 'url', label: 'Website', kind: ContentFieldKind.Url, isOptional: true },
    { key: 'address', label: 'Address', kind: ContentFieldKind.Text, isOptional: true },
    { key: 'note', label: 'Note', kind: ContentFieldKind.TextArea, isOptional: true },
  ],
  [ContentType.Calendar]: [
    { key: 'title', label: 'Title', kind: ContentFieldKind.Text },
    { key: 'start', label: 'Starts', kind: ContentFieldKind.DateTime },
    { key: 'end', label: 'Ends', kind: ContentFieldKind.DateTime, isOptional: true },
    { key: 'location', label: 'Location', kind: ContentFieldKind.Text, isOptional: true },
    { key: 'description', label: 'Description', kind: ContentFieldKind.TextArea, isOptional: true },
  ],
};
