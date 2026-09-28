import { z } from 'zod';

/** Validates the API's public runtime settings. */
export const RuntimeConfigDtoSchema = z.object({
  googleClientId: z.string(),
  redirectBaseUrl: z.string(),
});

/** Represents the API's public runtime settings. */
export type RuntimeConfigDto = z.output<typeof RuntimeConfigDtoSchema>;
