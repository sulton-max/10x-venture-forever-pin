import { ApiFailureFactory, type ApiDecoder } from '@wow-two-beta/fe/foundation/http';
import { ResultExtensions } from '@wow-two-beta/fe/foundation/results';
import type { z } from 'zod';

/** Extends the wire codecs with decoders built from schemas. */
export const DecoderExtensions = {
  /** Wraps a schema as an endpoint decoder; a payload it rejects is a protocol failure. */
  from<TSchema extends z.ZodType>(schema: TSchema): ApiDecoder<z.output<TSchema>> {
    return (value) => {
      const parsed = schema.safeParse(value);
      return parsed.success
        ? ResultExtensions.ok(parsed.data)
        : ResultExtensions.fail(ApiFailureFactory.create('protocol'));
    };
  },
} as const;
