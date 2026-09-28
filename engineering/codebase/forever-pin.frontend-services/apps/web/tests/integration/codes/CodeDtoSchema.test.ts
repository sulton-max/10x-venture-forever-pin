import { Temporal } from 'temporal-polyfill';
import { describe, expect, it } from 'vitest';

import { warnedEnvelopeSchema } from '@/integration/common';
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

  it('reads save warnings beside the data, defaulting to none', () => {
    const schema = warnedEnvelopeSchema(CodeDtoSchema);
    const warned = schema.parse({
      data: wireCode,
      warnings: [
        { property: 'rules[0].content.url', message: 'Insecure', code: 'RedirectTargetInsecure', severity: 'warning' },
      ],
    });
    expect(warned.warnings).toHaveLength(1);
    expect(schema.parse({ data: wireCode }).warnings).toEqual([]);
    expect(schema.safeParse({ data: { ...wireCode, mode: 'bogus' } }).success).toBe(false);
  });
});
