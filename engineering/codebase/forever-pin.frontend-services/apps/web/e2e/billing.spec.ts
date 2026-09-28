import { expect, test } from '@playwright/test';

import { FakeApi } from './support/FakeApi';

test.describe('billing', () => {
  test('treats a Checkout return as pending until the server confirms a plan', async ({ page, baseURL }) => {
    await new FakeApi({ isGuest: true }).install(page, baseURL!);

    await page.goto('/app/billing?status=success');

    await expect(page.getByText('Confirming your subscription…')).toBeVisible();
    await expect(page.getByText("You're all set.")).toHaveCount(0);
    await expect(page.getByRole('heading', { name: 'Choose a plan' })).toBeVisible();
  });
});
