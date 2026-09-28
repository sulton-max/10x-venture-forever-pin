import { apiClient } from './ApiClient';
import { DecoderExtensions } from './DecoderExtensions';
import { RuntimeConfigDtoSchema } from './models/RuntimeConfigDto';

/** Holds the decoder of the public runtime settings. */
const decodeRuntimeConfig = DecoderExtensions.from(RuntimeConfigDtoSchema);

/** Provides the API's public runtime settings, so one image serves every environment. */
export const runtimeConfigApi = {
  /** `GET /api/runtime-config` — the Google audience and the short-link origin; never secrets. */
  get(signal?: AbortSignal) {
    return apiClient.get('/api/runtime-config', { signal, unwrap: false, decode: decodeRuntimeConfig });
  },
} as const;
