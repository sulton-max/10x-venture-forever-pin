import type { Temporal } from 'temporal-polyfill';

/** Represents the scans of one UTC calendar day. */
export interface ScanDayDto {
  /** The day. */
  date: Temporal.PlainDate;

  /** The scans resolved that day. */
  scans: number;
}

/** Represents the scans that share one value of a scan attribute. */
export interface ScanShareDto {
  /** The attribute value; absent when it was unknown. */
  key?: string;

  /** The scans carrying that value. */
  scans: number;
}

/** Represents a code's scans over a window of UTC calendar days ending today. */
export interface CodeAnalyticsDto {
  /** The first day of the window. */
  from: Temporal.PlainDate;

  /** The last day of the window, today. */
  to: Temporal.PlainDate;

  /** How many days back the owner's plan lets the window reach. */
  maxDays: number;

  /** Every scan the code has resolved — an Int64 on the server, so it stays exact as a bigint. */
  totalScans: bigint;

  /** The scans inside the window. */
  rangeScans: bigint;

  /** One entry per day of the window, oldest first. */
  daily: ScanDayDto[];

  /** The window's scans per device class, most first. */
  devices: ScanShareDto[];

  /** The window's scans per operating system, most first. */
  operatingSystems: ScanShareDto[];

  /** The window's scans per ISO country code, most first. */
  countries: ScanShareDto[];
}
