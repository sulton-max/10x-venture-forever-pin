import { expect, test } from '@playwright/test';

import { FakeApi } from './support/FakeApi';

test.describe('session gate', () => {
  test('keeps a deep link through the guest gate', async ({ page, baseURL }) => {
    await new FakeApi().install(page, baseURL!);

    await page.goto('/app/new');
    await page.getByRole('button', { name: 'Continue as guest' }).click();

    await expect(page.getByRole('heading', { name: 'Create a code' })).toBeVisible();
    await expect(page).toHaveURL(/\/app\/new$/);
  });

  test('offers a retry instead of a new guest when the session check fails', async ({ page, baseURL }) => {
    const api = new FakeApi({ isGuest: true, failMe: 503 });
    await api.install(page, baseURL!);

    await page.goto('/app');
    await expect(page.getByRole('heading', { name: "We couldn't reach ForeverPin" })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Continue as guest' })).toHaveCount(0);

    api.failMe = undefined;
    await page.getByRole('button', { name: 'Try again' }).click();

    await expect(page.getByRole('heading', { name: 'Your codes' })).toBeVisible();
  });

  test('keeps crawlers off the product', async ({ page, baseURL }) => {
    await new FakeApi({ isGuest: true }).install(page, baseURL!);

    await page.goto('/app');

    await expect(page.locator('meta[name="robots"]')).toHaveAttribute('content', 'noindex');
    await expect(page.locator('link[rel="canonical"]')).toHaveCount(0);
  });
});
