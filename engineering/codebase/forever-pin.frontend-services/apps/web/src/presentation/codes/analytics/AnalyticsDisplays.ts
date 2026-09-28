import type { Temporal } from 'temporal-polyfill';

/** Lists the windows the analytics page offers, shortest first. */
export const AnalyticsWindows = [
  { days: 7, label: '7 days' },
  { days: 30, label: '30 days' },
  { days: 90, label: '90 days' },
] as const;

/** Holds the window the analytics page opens with. */
export const DefaultAnalyticsDays = 30;

/** Labels each device class the redirect host records. */
const DeviceLabels: Readonly<Record<string, string>> = {
  ios: 'iPhone & iPad',
  android: 'Android',
  desktop: 'Desktop',
  bot: 'Link previews & bots',
  unknown: 'Other devices',
};

/** Holds the viewer-locale formats of the analytics page. */
const CountFormat = new Intl.NumberFormat();
const PercentFormat = new Intl.NumberFormat(undefined, { style: 'percent', maximumFractionDigits: 0 });
const CountryNames = new Intl.DisplayNames(undefined, { type: 'region' });

/** Formats analytics values for display in the viewer's locale. */
export const AnalyticsDisplays = {
  /** Formats a scan count with grouping. */
  count: (value: number | bigint): string => CountFormat.format(value),

  /** Formats a share of a whole as a rounded percentage. */
  percent: (part: number, whole: number): string => PercentFormat.format(whole > 0 ? part / whole : 0),

  /** Formats a UTC calendar day as a short month and day. */
  day: (date: Temporal.PlainDate): string => date.toLocaleString(undefined, { month: 'short', day: 'numeric' }),

  /** Names a recorded device class. */
  device: (key?: string): string => (key ? (DeviceLabels[key] ?? key) : 'Unknown'),

  /** Names a recorded operating system. */
  system: (key?: string): string => key ?? 'Unknown',

  /** Names an ISO country code in the viewer's language. */
  country: (key?: string): string => {
    if (!key) return 'Unknown';
    try {
      return CountryNames.of(key) ?? key;
    } catch {
      return key;
    }
  },
} as const;
