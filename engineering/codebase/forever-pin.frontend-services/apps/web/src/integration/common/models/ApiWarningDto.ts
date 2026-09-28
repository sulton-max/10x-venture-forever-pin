import { z } from 'zod';

/** Validates an advisory finding a successful write returns beside its data. */
export const ApiWarningDtoSchema = z.object({
  property: z.string(),
  message: z.string(),
  code: z.string(),
  severity: z.string(),
});

/** Represents an advisory finding a successful write returns beside its data. */
export type ApiWarningDto = z.output<typeof ApiWarningDtoSchema>;

/** Builds the schema of a success envelope whose data `data` validates, with any advisory warnings. */
export function warnedEnvelopeSchema<TData extends z.ZodType>(data: TData) {
  return z
    .object({ data: z.unknown(), warnings: z.array(ApiWarningDtoSchema).default([]) })
    .transform((envelope, context) => {
      const parsed = data.safeParse(envelope.data);
      if (parsed.success) return { data: parsed.data, warnings: envelope.warnings };
      context.addIssue({ code: 'custom', message: 'The envelope data is invalid.', path: ['data'] });
      return z.NEVER;
    });
}
