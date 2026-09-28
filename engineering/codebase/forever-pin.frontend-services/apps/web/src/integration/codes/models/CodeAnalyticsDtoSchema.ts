import { Temporal } from 'temporal-polyfill';
import { z } from 'zod';

import type { CodeAnalyticsDto } from '@/domain/codes';

import { bucketCount, exactCount } from './CountSchemas';

/** Decodes an ISO calendar date into a `Temporal.PlainDate`. */
const plainDate = z.string().transform((value, context) => {
  try {
    return Temporal.PlainDate.from(value);
  } catch {
    context.addIssue({ code: 'custom', message: 'Invalid calendar date.' });
    return z.NEVER;
  }
});

/** Validates one attribute share; a null key is an unknown value and decodes as absent. */
const ScanShareDtoSchema = z.object({
  key: z
    .string()
    .nullish()
    .transform((value) => value ?? undefined),
  scans: bucketCount,
});

/** Validates a code's scan window, reviving its days and keeping the totals exact. */
export const CodeAnalyticsDtoSchema: z.ZodType<CodeAnalyticsDto> = z.object({
  from: plainDate,
  to: plainDate,
  totalScans: exactCount,
  rangeScans: exactCount,
  daily: z.array(z.object({ date: plainDate, scans: bucketCount })),
  devices: z.array(ScanShareDtoSchema),
  operatingSystems: z.array(ScanShareDtoSchema),
  countries: z.array(ScanShareDtoSchema),
});
