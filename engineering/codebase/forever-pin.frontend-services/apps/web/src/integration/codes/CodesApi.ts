import type { CodeDto, ImageFormat } from '@/domain/codes';
import { apiClient, DecoderExtensions, warnedEnvelopeSchema } from '@/integration/common';

import type { CodeCreateUpdateApiRequest, CodeUpdateApiRequest } from './models/CodeCreateUpdateApiRequest';
import { CodeDtoSchema } from './models/CodeDtoSchema';
import type { CodePreviewApiRequest } from './models/CodePreviewApiRequest';

/** Holds the decoders of code payloads. */
const decodeCode = DecoderExtensions.from(CodeDtoSchema);
const decodeCodes = DecoderExtensions.from(CodeDtoSchema.array());
const decodeSavedCode = DecoderExtensions.from(warnedEnvelopeSchema(CodeDtoSchema));

/** Provides the owner-scoped code endpoints and the server-side renderer. */
export const codesApi = {
  /** `GET /api/codes` — the owner's codes, newest first, filtered case-insensitively by `q`. */
  list(search: string, signal?: AbortSignal) {
    const q = search.trim();
    return apiClient.get<CodeDto[]>('/api/codes', { query: q ? { q } : undefined, signal, decode: decodeCodes });
  },

  /** `GET /api/codes/{id}` — one owned code; 404 when missing or owned by someone else. */
  get(id: string, signal?: AbortSignal) {
    return apiClient.get<CodeDto>(`/api/codes/${encodeURIComponent(id)}`, { signal, decode: decodeCode });
  },

  /** `POST /api/codes` — creates a code; the answer carries destination warnings that did not block it. */
  create(request: CodeCreateUpdateApiRequest, signal?: AbortSignal) {
    return apiClient.post('/api/codes', { body: request, signal, unwrap: false, decode: decodeSavedCode });
  },

  /** `PUT /api/codes/{id}` — replaces editable fields; slug, scans, creation time and mode stay. */
  update(id: string, request: CodeUpdateApiRequest, signal?: AbortSignal) {
    return apiClient.put(`/api/codes/${encodeURIComponent(id)}`, {
      body: request,
      signal,
      unwrap: false,
      decode: decodeSavedCode,
    });
  },

  /** `PATCH /api/codes/{id}/active` — pauses or resumes a code. */
  setActive(id: string, isActive: boolean, signal?: AbortSignal) {
    return apiClient.patch<CodeDto>(`/api/codes/${encodeURIComponent(id)}/active`, {
      body: { isActive },
      signal,
      decode: decodeCode,
    });
  },

  /** `DELETE /api/codes/{id}` — deletes a code and its rules. */
  delete(id: string, signal?: AbortSignal) {
    return apiClient.delete(`/api/codes/${encodeURIComponent(id)}`, { signal, response: 'empty' });
  },

  /** `POST /api/codes/preview` — renders an unsaved code as SVG markup. */
  preview(request: CodePreviewApiRequest, signal?: AbortSignal) {
    return apiClient.post('/api/codes/preview', {
      body: request,
      signal,
      headers: { Accept: 'image/svg+xml' },
      response: 'text',
    });
  },

  /** Builds the download address of a saved code's rendered image. */
  imageUrl(id: string, format: ImageFormat): string {
    return `/api/codes/${encodeURIComponent(id)}/image?format=${format}`;
  },
} as const;
