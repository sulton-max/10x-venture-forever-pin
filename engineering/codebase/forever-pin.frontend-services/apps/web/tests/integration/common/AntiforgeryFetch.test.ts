import { afterEach, describe, expect, it, vi } from 'vitest';

import { createAntiforgeryFetch } from '@/integration/common/AntiforgeryFetch';

function headerOf(init: RequestInit | undefined, name: string): string | null {
  return new Headers(init?.headers).get(name);
}

function setCookie(value: string): void {
  Object.defineProperty(document, 'cookie', { value, configurable: true, writable: true });
}

describe('createAntiforgeryFetch', () => {
  afterEach(() => setCookie(''));

  it('echoes the decoded token on writes and never on reads', async () => {
    setCookie('theme=dark; XSRF-TOKEN=abc%2Bdef%3D');
    const base = vi.fn<typeof fetch>(async () => new Response('{}'));
    const send = createAntiforgeryFetch('/api/runtime-config', base);

    await send('/api/codes', { method: 'POST', body: '{}' });
    await send('/api/codes');

    expect(headerOf(base.mock.calls[0]?.[1], 'X-XSRF-TOKEN')).toBe('abc+def=');
    expect(headerOf(base.mock.calls[1]?.[1], 'X-XSRF-TOKEN')).toBeNull();
  });

  it('refreshes a stale token once through a read and retries the write', async () => {
    setCookie('XSRF-TOKEN=before-sign-in');
    const base = vi
      .fn<typeof fetch>()
      .mockImplementationOnce(
        async () => new Response(JSON.stringify({ code: 'AntiforgeryValidationFailed' }), { status: 400 }),
      )
      .mockImplementationOnce(async () => {
        setCookie('XSRF-TOKEN=after-sign-in');
        return new Response('{}');
      })
      .mockImplementationOnce(async () => new Response('{}'));
    const send = createAntiforgeryFetch('/api/runtime-config', base);

    const response = await send('/api/codes', { method: 'PUT', body: '{}' });

    expect(response.status).toBe(200);
    expect(base).toHaveBeenCalledTimes(3);
    expect(base.mock.calls[1]?.[0]).toBe('/api/runtime-config');
    expect(headerOf(base.mock.calls[2]?.[1], 'X-XSRF-TOKEN')).toBe('after-sign-in');
  });

  it('returns other failures unchanged and readable', async () => {
    setCookie('XSRF-TOKEN=token');
    const base = vi.fn<typeof fetch>(
      async () => new Response(JSON.stringify({ title: 'Name is required.' }), { status: 400 }),
    );
    const send = createAntiforgeryFetch('/api/runtime-config', base);

    const response = await send('/api/codes', { method: 'POST', body: '{}' });

    expect(base).toHaveBeenCalledTimes(1);
    expect(await response.json()).toEqual({ title: 'Name is required.' });
  });
});
