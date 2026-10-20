import { ExactNumber } from '@wow-two-beta/fe/foundation/numbers';
import { z } from 'zod';

/** Decodes an Int64 count exactly: a lossless read keeps every digit; a native read is exact up to 2^53. */
export const exactCount = z.unknown().transform((value, context) => {
  if (ExactNumber.isExactNumber(value)) {
    const integer = value.toBigInt();
    if (integer.ok) return integer.value;
  } else if (typeof value === 'number' && Number.isInteger(value)) {
    return BigInt(value);
  }
  context.addIssue({ code: 'custom', message: 'Expected an integer count.' });
  return z.NEVER;
});

/** Decodes a count that stays far below 2^53, such as one day's scans, into a number for arithmetic. */
export const bucketCount = exactCount.transform((value, context) => {
  if (value <= BigInt(Number.MAX_SAFE_INTEGER)) return Number(value);
  context.addIssue({ code: 'custom', message: 'Expected a count below 2^53.' });
  return z.NEVER;
});
