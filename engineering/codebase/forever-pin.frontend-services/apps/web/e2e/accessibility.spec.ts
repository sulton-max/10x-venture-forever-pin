import AxeBuilder from '@axe-core/playwright';
import { expect, test, type Page } from '@playwright/test';

import { FakeApi, fakeCode, type FakeCode } from './support/FakeApi';

/** Holds the WCAG levels every page must meet. */
const Standards = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'];

/** Lists each violation as `rule: selectors`, so a failure names what to fix; entrance animations finish first. */
async function violations(page: Page): Promise<string[]> {
  await page.evaluate(() =>
    Promise.all(
      document
        .getAnimations()
        .filter((animation) => animation.effect?.getTiming().iterations !== Infinity)
        .map((animation) => animation.finished),
    ),
  );
  const results = await new AxeBuilder({ page }).withTags(Standards).analyze();
  return results.violations.map(
    (violation) => `${violation.id}: ${violation.nodes.map((node) => node.target.join(' ')).join(', ')}`,
  );
}

/** Starts a guest with one scanned code. */
async function withCode(page: Page, baseURL: string): Promise<FakeCode> {
  const code = fakeCode({ name: 'Menu', scanCount: 12 });
  await new FakeApi({
    isGuest: true,
    codes: [code],
    scans: { [code.id]: { daily: [1, 2], devices: [{ key: 'ios', scans: 3 }], countries: [{ key: 'AU', scans: 3 }] } },
  }).install(page, baseURL);
  return code;
}

test.describe('accessibility', () => {
  const Pages: ReadonlyArray<{ name: string; path: (code: FakeCode) => string }> = [
    { name: 'landing', path: () => '/' },
    { name: 'pricing', path: () => '/pricing' },
    { name: 'blog', path: () => '/blog' },
    { name: 'article', path: () => '/blog/dynamic-vs-static-qr-codes' },
    { name: 'codes', path: () => '/app' },
    { name: 'builder', path: () => '/app/new' },
    { name: 'editor', path: (code) => `/app/${code.id}/edit` },
    { name: 'analytics', path: (code) => `/app/${code.id}/analytics` },
    { name: 'billing', path: () => '/app/billing' },
  ];

  for (const entry of Pages) {
    test(`${entry.name} meets WCAG 2.1 AA`, async ({ page, baseURL }) => {
      const code = await withCode(page, baseURL!);

      await page.goto(entry.path(code));
      await page.waitForLoadState('networkidle');

      expect(await violations(page)).toEqual([]);
    });
  }

  test('the download dialog meets WCAG 2.1 AA', async ({ page, baseURL }) => {
    await withCode(page, baseURL!);

    await page.goto('/app');
    await page.getByRole('button', { name: 'Download' }).click();
    await expect(page.getByRole('dialog')).toBeVisible();

    expect(await violations(page)).toEqual([]);
  });
});
