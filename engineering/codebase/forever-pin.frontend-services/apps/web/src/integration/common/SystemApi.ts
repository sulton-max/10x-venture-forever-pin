import { apiClient } from './ApiClient';
import { DecoderExtensions } from './DecoderExtensions';
import { SystemStatusDtoSchema } from './models/SystemStatusDto';

/** Holds the decoder of the API's status. */
const decodeSystemStatus = DecoderExtensions.from(SystemStatusDtoSchema);

/** Provides the API's anonymous status read. */
export const systemApi = {
  /** `GET /api/system/status` — the service name, liveness and the running version with its commit. */
  status(signal?: AbortSignal) {
    return apiClient.get('/api/system/status', { signal, decode: decodeSystemStatus });
  },
} as const;
