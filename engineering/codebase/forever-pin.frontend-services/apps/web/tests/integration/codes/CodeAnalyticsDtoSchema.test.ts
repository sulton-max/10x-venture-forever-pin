import { LosslessJson } from '@wow-two-beta/fe/foundation/json';
import { Temporal } from 'temporal-polyfill';
import { describe, expect, it } from 'vitest';

import { CodeAnalyticsDtoSchema } from '@/integration/codes/models/CodeAnalyticsDtoSchema';

const wireAnalytics = {
  from: '2026-09-22',
  to: '2026-09-28',
  maxDays: 366,
  totalScans: 40,
  rangeScans: 3,
  daily: [
    { date: '2026-09-27', scans: 1 },
    { date: '2026-09-28', scans: 2 },
  ],
  devices: [{ key: 'ios', scans: 3 }],
  operatingSystems: [{ key: 'iOS', scans: 3 }],
  countries: [
    { key: 'AU', scans: 2 },
    { key: null, scans: 1 },
  ],
};

/** Parses wire JSON the way the codes API reads it. */
function readLossless(text: string): unknown {
  const parsed = LosslessJson.parse(text);
  if (!parsed.ok) throw new Error('Expected lossless JSON.');
  return parsed.value;
}

describe('CodeAnalyticsDtoSchema', () => {
  it('revives calendar days and reads an unknown attribute value as absent', () => {
    const analytics = CodeAnalyticsDtoSchema.parse(readLossless(JSON.stringify(wireAnalytics)));

    expect(analytics.from.equals(Temporal.PlainDate.from('2026-09-22'))).toBe(true);
    expect(analytics.daily.map((day) => day.scans)).toEqual([1, 2]);
    expect(analytics.countries[1]).toEqual({ key: undefined, scans: 1 });
  });

  it('keeps the Int64 totals exact on a lossless read', () => {
    const text = JSON.stringify(wireAnalytics).replace('"totalScans":40', '"totalScans":9007199254740993');

    const analytics = CodeAnalyticsDtoSchema.parse(readLossless(text));

    expect(analytics.totalScans).toBe(9007199254740993n);
    expect(analytics.rangeScans).toBe(3n);
  });

  it('rejects a fractional day count and an invalid day', () => {
    expect(
      CodeAnalyticsDtoSchema.safeParse({ ...wireAnalytics, daily: [{ date: '2026-09-28', scans: 1.5 }] }).success,
    ).toBe(false);
    expect(CodeAnalyticsDtoSchema.safeParse({ ...wireAnalytics, from: '2026-02-30' }).success).toBe(false);
  });
});
