import { apiClient, DecoderExtensions } from '@/integration/common';

import { MeDtoSchema } from './models/MeDto';

/** Holds the decoder of the identity state. */
export const decodeMe = DecoderExtensions.from(MeDtoSchema);

/** Provides the identity endpoints: resolve, guest provisioning, Google sign-in and sign-out. */
export const identityApi = {
  /** `GET /api/identity/me` — the visitor's identity; anonymous when no session exists. */
  getMe(signal?: AbortSignal) {
    return apiClient.get('/api/identity/me', { signal, decode: decodeMe });
  },

  /** `POST /api/identity/guest` — provisions a guest session that owns its codes. */
  createGuest(signal?: AbortSignal) {
    return apiClient.post('/api/identity/guest', { signal, decode: decodeMe });
  },

  /** `POST /api/identity/sign-in` — exchanges a Google ID token for a session, claiming the guest's codes. */
  signInWithGoogle(idToken: string, signal?: AbortSignal) {
    return apiClient.post('/api/identity/sign-in', { body: { idToken }, signal, decode: decodeMe });
  },

  /** `POST /api/identity/sign-out` — drops the session and guest cookies. */
  signOut(signal?: AbortSignal) {
    return apiClient.post('/api/identity/sign-out', { signal, response: 'empty' });
  },
} as const;
