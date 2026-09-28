import type { Plan } from '@/domain/billing';
import { apiClient, DecoderExtensions } from '@/integration/common';

import { BillingStatusDtoSchema, SessionUrlDtoSchema } from './models/BillingStatusDto';

/** Holds the decoders of billing payloads. */
const decodeStatus = DecoderExtensions.from(BillingStatusDtoSchema);
const decodeSession = DecoderExtensions.from(SessionUrlDtoSchema);

/** Provides the owner-scoped billing endpoints. */
export const billingApi = {
  /** `GET /api/billing/me` — plan, subscription status, limits and usage; free when there is no subscription. */
  getStatus(signal?: AbortSignal) {
    return apiClient.get('/api/billing/me', { signal, decode: decodeStatus });
  },

  /** `POST /api/billing/checkout` — a hosted Checkout address for a paid plan. */
  createCheckout(plan: Plan, signal?: AbortSignal) {
    return apiClient.post('/api/billing/checkout', { body: { plan }, signal, decode: decodeSession });
  },

  /** `POST /api/billing/portal` — a Customer Portal address; fails before the first subscription. */
  createPortal(signal?: AbortSignal) {
    return apiClient.post('/api/billing/portal', { signal, decode: decodeSession });
  },
} as const;
