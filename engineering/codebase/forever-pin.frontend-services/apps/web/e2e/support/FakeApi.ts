import type { Page, Route } from '@playwright/test';

/** Represents a code as the fake API stores it — the wire shape the app reads. */
export interface FakeCode {
  readonly id: string;
  readonly slug: string;
  readonly shortUrl: string;
  name: string;
  barcodeFormat: string;
  mode: string;
  contentType: string;
  isActive: boolean;
  scanCount: number;
  createdAt: string;
  rules: unknown[];
  style: Record<string, unknown>;
}

/** Represents one attribute share as the fake API answers it. */
export interface FakeShare {
  readonly key: string | null;
  readonly scans: number;
}

/** Represents a code's recorded scans; `daily` ends today, oldest first. */
export interface FakeScans {
  readonly daily: number[];
  readonly devices?: FakeShare[];
  readonly operatingSystems?: FakeShare[];
  readonly countries?: FakeShare[];
}

/** Holds the day the fake API treats as today. */
const FakeToday = '2026-09-28';

/** Holds the options a scenario starts the fake API with. */
export interface FakeApiOptions {
  /** Whether the visitor already has a guest session. */
  readonly isGuest?: boolean;
  /** Fails the session check with this status, as an unreachable API would. */
  readonly failMe?: number;
  /** Seeds the guest's codes. */
  readonly codes?: FakeCode[];
  /** Seeds recorded scans per code id. */
  readonly scans?: Record<string, FakeScans>;
  /** Seeds rendered SVG markup per code id; others render a small sample. */
  readonly images?: Record<string, string>;
}

const PreviewSvg =
  '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 4 4"><rect width="4" height="4" fill="#fff"/><rect width="2" height="2" fill="#000"/></svg>';

/** Builds a stored code with sensible defaults. */
export function fakeCode(overrides: Partial<FakeCode> = {}): FakeCode {
  const id = overrides.id ?? crypto.randomUUID();
  const slug = overrides.slug ?? id.slice(0, 7);
  return {
    id,
    slug,
    shortUrl: `https://go.foreverpin.test/${slug}`,
    name: 'Menu',
    barcodeFormat: 'qrCode',
    mode: 'dynamic',
    contentType: 'url',
    isActive: true,
    scanCount: 0,
    createdAt: '2026-09-28T10:00:00Z',
    rules: [{ type: 'default', content: { type: 'url', url: 'https://example.com/menu' } }],
    style: {
      foregroundColor: '#000000',
      backgroundColor: '#ffffff',
      transparentBackground: false,
      eccLevel: 'q',
      quietZoneModules: 2,
      moduleShape: 'rounded',
      finderShape: 'rounded',
      finderDotShape: 'rounded',
    },
    ...overrides,
  };
}

/**
 * Serves the ForeverPin API from memory inside the page's network layer: identity, codes, preview and billing.
 * Writes check the antiforgery echo the way the real host does, so a client that forgets it fails here too.
 */
export class FakeApi {
  readonly codes: FakeCode[];
  readonly scans: Record<string, FakeScans>;
  readonly images: Record<string, string>;
  readonly analyticsReads: number[] = [];
  isGuest: boolean;
  failMe: number | undefined;

  constructor(options: FakeApiOptions = {}) {
    this.codes = options.codes ?? [];
    this.scans = options.scans ?? {};
    this.images = options.images ?? {};
    this.isGuest = options.isGuest ?? false;
    this.failMe = options.failMe;
  }

  /** Routes every `/api` request of `page` to this fake, with the readable token cookie the real host issues. */
  async install(page: Page, baseURL: string): Promise<void> {
    await page.context().addCookies([{ name: 'XSRF-TOKEN', value: 'fake-token', url: baseURL }]);
    await page.route('**/api/**', (route) => this.handle(route));
  }

  private async handle(route: Route): Promise<void> {
    const request = route.request();
    const url = new URL(request.url());
    const path = url.pathname;
    const method = request.method();

    if (method !== 'GET' && !request.headers()['x-xsrf-token'])
      return this.json(route, 400, { code: 'AntiforgeryValidationFailed' });

    if (path === '/api/runtime-config')
      return this.json(route, 200, { googleClientId: '', redirectBaseUrl: 'https://go.foreverpin.test' });
    if (path === '/api/system/status')
      return this.json(route, 200, { data: { service: 'forever-pin-api', status: 'ok', version: '0.11.0+abcdef0' } });
    if (path === '/api/identity/me') {
      if (this.failMe) return this.json(route, this.failMe, { title: 'Unavailable' });
      return this.json(route, 200, { data: { kind: this.isGuest ? 'guest' : 'anonymous', user: null } });
    }
    if (path === '/api/identity/guest' && method === 'POST') {
      this.isGuest = true;
      return this.json(route, 200, { data: { kind: 'guest', user: null } });
    }
    if (path === '/api/billing/me')
      return this.json(route, 200, {
        data: { plan: 'free', status: 'active', limits: { maxCodes: 3 }, usage: { codeCount: this.codes.length } },
      });
    if (path === '/api/codes/preview' && method === 'POST')
      return route.fulfill({ status: 200, contentType: 'image/svg+xml', body: PreviewSvg });

    if (!this.isGuest) return this.json(route, 401, { title: 'Unauthorized' });
    if (path === '/api/codes' && method === 'GET') {
      const q = url.searchParams.get('q')?.toLowerCase() ?? '';
      return this.json(route, 200, { data: this.codes.filter((code) => code.name.toLowerCase().includes(q)) });
    }
    if (path === '/api/codes' && method === 'POST') {
      const body = request.postDataJSON() as Partial<FakeCode>;
      const code = fakeCode({ ...body, scanCount: 0 } as Partial<FakeCode>);
      this.codes.unshift(code);
      return this.json(route, 200, { data: code, warnings: [] });
    }

    const match = /^\/api\/codes\/([^/]+)(\/active|\/image|\/analytics)?$/.exec(path);
    const code = match ? this.codes.find((candidate) => candidate.id === match[1]) : undefined;
    if (!match || !code) return this.json(route, 404, { title: 'Not Found' });
    if (match[2] === '/analytics') {
      const days = Number(url.searchParams.get('days') ?? '30');
      this.analyticsReads.push(days);
      return this.json(route, 200, { data: this.analytics(code, days) });
    }
    if (match[2] === '/image')
      return route.fulfill({ status: 200, contentType: 'image/svg+xml', body: this.images[code.id] ?? PreviewSvg });
    if (match[2] === '/active') {
      code.isActive = (request.postDataJSON() as { isActive: boolean }).isActive;
      return this.json(route, 200, { data: code });
    }
    if (method === 'GET') return this.json(route, 200, { data: code });
    if (method === 'PUT') {
      Object.assign(code, request.postDataJSON());
      return this.json(route, 200, { data: code, warnings: [] });
    }
    if (method === 'DELETE') {
      this.codes.splice(this.codes.indexOf(code), 1);
      return route.fulfill({ status: 204 });
    }
    return this.json(route, 405, { title: 'Method Not Allowed' });
  }

  /** Builds a code's scan window the way the real host does: every day of the window, oldest first. */
  private analytics(code: FakeCode, days: number) {
    const seeded = this.scans[code.id] ?? { daily: [] };
    const recent = seeded.daily.slice(-days);
    const today = new Date(`${FakeToday}T00:00:00Z`);
    const daily = Array.from({ length: days }, (_, index) => {
      const date = new Date(today);
      date.setUTCDate(today.getUTCDate() - (days - 1 - index));
      return { date: date.toISOString().slice(0, 10), scans: recent[index - (days - recent.length)] ?? 0 };
    });
    return {
      from: daily[0]?.date,
      to: FakeToday,
      totalScans: code.scanCount,
      rangeScans: daily.reduce((sum, day) => sum + day.scans, 0),
      daily,
      devices: seeded.devices ?? [],
      operatingSystems: seeded.operatingSystems ?? [],
      countries: seeded.countries ?? [],
    };
  }

  private json(route: Route, status: number, body: unknown): Promise<void> {
    return route.fulfill({
      status,
      contentType: status >= 400 ? 'application/problem+json' : 'application/json',
      body: JSON.stringify(body),
    });
  }
}
