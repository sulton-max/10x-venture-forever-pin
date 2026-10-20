import { decodeWarned } from '@wow-two-beta/fe/foundation/http';
import { LosslessJson } from '@wow-two-beta/fe/foundation/json';
import { ResultExtensions } from '@wow-two-beta/fe/foundation/results';
import { Temporal } from 'temporal-polyfill';
import { describe, expect, it } from 'vitest';

import { DecoderExtensions } from '@/integration/common';
import { CodeDtoSchema } from '@/integration/codes/models/CodeDtoSchema';

const wireCode = {
  id: 'c1',
  slug: 'aB3x9',
  shortUrl: 'https://go.foreverpin.com/aB3x9',
  name: 'Menu',
  barcodeFormat: 'qrCode',
  mode: 'dynamic',
  contentType: 'calendar',
  isActive: true,
  scanCount: 12,
  createdAt: '2026-09-27T10:00:00Z',
  rules: [
    {
      type: 'default',
      content: { type: 'calendar', title: 'Launch', start: '2026-10-01T18:30:00', end: null, location: null },
    },
  ],
  style: {
    foregroundColor: '#000000',
    backgroundColor: '#ffffff',
    transparentBackground: false,
    eccLevel: 'q',
    quietZoneModules: 2,
    logo: null,
    moduleShape: 'rounded',
    finderShape: 'rounded',
    finderDotShape: 'rounded',
    gradient: null,
    emoji: null,
  },
};

describe('CodeDtoSchema', () => {
  it('revives Temporal values and drops null optionals', () => {
    const code = CodeDtoSchema.parse(wireCode);
    const rule = code.rules[0];
    expect(code.createdAt).toBeInstanceOf(Temporal.Instant);
    if (rule?.type !== 'default' || rule.content.type !== 'calendar') throw new Error('Expected a calendar default.');
    expect(rule.content.start.equals(Temporal.PlainDateTime.from('2026-10-01T18:30'))).toBe(true);
    expect(rule.content).not.toHaveProperty('end', null);
    expect(code.style.gradient).toBeUndefined();
  });

  it('keeps an Int64 scan count exact and reads measurements as doubles on a lossless read', () => {
    const text = JSON.stringify({ ...wireCode, scanCount: 0 }).replace('"scanCount":0', '"scanCount":9007199254740993');
    const parsed = LosslessJson.parse(text);
    if (!parsed.ok) throw new Error('Expected lossless JSON.');

    const code = CodeDtoSchema.parse(parsed.value);

    expect(code.scanCount).toBe(9007199254740993n);
    expect(code.style.quietZoneModules).toBe(2);
  });

  it('rejects a fractional scan count', () => {
    expect(CodeDtoSchema.safeParse({ ...wireCode, scanCount: 1.5 }).success).toBe(false);
  });

  it('reads save warnings beside the data, defaulting to none', () => {
    const decode = decodeWarned(DecoderExtensions.from(CodeDtoSchema));
    const warned = decode({
      data: wireCode,
      warnings: [
        { property: 'rules[0].content.url', message: 'Insecure', code: 'RedirectTargetInsecure', severity: 'warning' },
      ],
    });
    const bare = decode({ data: wireCode });
    expect(ResultExtensions.isOk(warned) && warned.value.warnings).toHaveLength(1);
    expect(ResultExtensions.isOk(bare) && bare.value.warnings).toEqual([]);
    expect(ResultExtensions.isOk(decode({ data: { ...wireCode, mode: 'bogus' } }))).toBe(false);
  });
});
