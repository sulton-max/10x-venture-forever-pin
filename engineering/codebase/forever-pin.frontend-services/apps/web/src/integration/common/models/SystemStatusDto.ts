import { z } from 'zod';

/** Validates the API's status: its service name, liveness and running version. */
export const SystemStatusDtoSchema = z.object({
  service: z.string(),
  status: z.string(),
  version: z.string(),
});

/** Represents the API's status. */
export type SystemStatusDto = z.output<typeof SystemStatusDtoSchema>;
