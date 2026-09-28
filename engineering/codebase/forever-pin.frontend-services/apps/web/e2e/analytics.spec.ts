import { expect, test } from '@playwright/test';

import { FakeApi, fakeCode } from './support/FakeApi';

test.describe('analytics', () => {
  test("shows a dynamic code's scans by day, device and country", async ({ page, baseURL }) => {
    const code = fakeCode({ name: 'Menu', scanCount: 42 });
    const api = new FakeApi({
      isGuest: true,
      codes: [code],
      scans: {
        [code.id]: {
          daily: [3, 5],
          devices: [
            { key: 'ios', scans: 5 },
            { key: 'android', scans: 3 },
          ],
          operatingSystems: [
            { key: 'iOS', scans: 5 },
            { key: 'Android', scans: 3 },
          ],
          countries: [
            { key: 'AU', scans: 6 },
            { key: null, scans: 2 },
          ],
        },
      },
    });
    await api.install(page, baseURL!);

    await page.goto('/app');
    await page.getByRole('button', { name: 'Analytics' }).click();

    await expect(page).toHaveURL(new RegExp(`/app/${code.id}/analytics$`));
    await expect(page.getByRole('heading', { name: 'Menu · Analytics' })).toBeVisible();
    await expect(page.getByText('Scans · last 30 days')).toBeVisible();
    await expect(page.getByText('42', { exact: true })).toBeVisible();
    await expect(page.getByText('Australia').first()).toBeVisible();
    await expect(page.getByText('iPhone & iPad')).toBeVisible();
    await expect(page.getByRole('link', { name: 'IP Geolocation by DB-IP' })).toBeVisible();
    await expect(page.getByRole('img', { name: /8 scans over 30 days, peaking at 5/ })).toBeAttached();

    await page.getByRole('button', { name: '7 days' }).click();

    await expect(page.getByText('Scans · last 7 days')).toBeVisible();
    expect(api.analyticsReads).toEqual([30, 7]);
  });

  test('explains that a static code is not counted', async ({ page, baseURL }) => {
    const code = fakeCode({ name: 'Wifi card', mode: 'static', scanCount: 0 });
    const api = new FakeApi({ isGuest: true, codes: [code] });
    await api.install(page, baseURL!);

    await page.goto('/app');
    await expect(page.getByText('Wifi card', { exact: true })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Analytics' })).toHaveCount(0);

    await page.goto(`/app/${code.id}/analytics`);

    await expect(page.getByText("Static codes aren't counted")).toBeVisible();
    expect(api.analyticsReads).toEqual([]);
  });
});
