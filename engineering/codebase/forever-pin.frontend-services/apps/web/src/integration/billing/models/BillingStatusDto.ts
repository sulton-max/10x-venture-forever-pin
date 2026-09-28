import { z } from 'zod';

import { Plan, type BillingStatus } from '@/domain/billing';

/** Validates an account's billing snapshot. */
export const BillingStatusDtoSchema: z.ZodType<BillingStatus> = z.object({
  plan: z.enum([Plan.Free, Plan.Solo, Plan.Pro, Plan.Agency]),
  status: z.string(),
  limits: z.object({ maxCodes: z.number() }),
  usage: z.object({ codeCount: z.number() }),
});

/** Validates a hosted billing-session address. */
export const SessionUrlDtoSchema = z.object({ url: z.string() });
