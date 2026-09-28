import { z } from 'zod';

import { UserKind, type Me } from '@/domain/identity';

/** Validates the current identity state. */
export const MeDtoSchema: z.ZodType<Me> = z.object({
  kind: z.enum([UserKind.Anonymous, UserKind.Guest, UserKind.User]),
  user: z
    .object({ id: z.string(), name: z.string(), email: z.string() })
    .nullable()
    .optional()
    .transform((user) => user ?? null),
});
