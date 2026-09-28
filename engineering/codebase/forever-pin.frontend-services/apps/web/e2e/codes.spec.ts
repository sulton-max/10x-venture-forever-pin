import { expect, test } from '@playwright/test';

import { FakeApi, fakeCode } from './support/FakeApi';

test.describe('codes', () => {
  test('creates a code and shows the saved image beside the draft preview', async ({ page, baseURL }) => {
    const api = new FakeApi({ isGuest: true });
    await api.install(page, baseURL!);

    await page.goto('/app/new');
    await page.getByPlaceholder('Spring menu table tent').fill('Table tent');
    await page.getByRole('button', { name: 'Create code' }).click();

    await expect(page.getByText('Code created ✓')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Create another code' })).toBeVisible();
    expect(api.codes.map((code) => code.name)).toEqual(['Table tent']);
  });

  test('lists codes with exact scan counts', async ({ page, baseURL }) => {
    await new FakeApi({ isGuest: true, codes: [fakeCode({ name: 'Menu', scanCount: 1234567 })] }).install(
      page,
      baseURL!,
    );

    await page.goto('/app');

    await expect(page.getByText('Menu', { exact: true })).toBeVisible();
    await expect(page.getByText(/1,234,567/)).toBeVisible();
  });

  test('copies a multi-rule code only as dynamic', async ({ page, baseURL }) => {
    const routed = fakeCode({
      name: 'Routed',
      rules: [
        {
          type: 'conditional',
          order: 1,
          condition: 'device',
          conditionValue: 'Ios',
          content: { type: 'url', url: 'https://apps.apple.com/app/x' },
        },
        { type: 'default', content: { type: 'url', url: 'https://example.com' } },
      ],
    });
    await new FakeApi({ isGuest: true, codes: [routed] }).install(page, baseURL!);

    await page.goto('/app');
    await page.getByRole('button', { name: 'Copy', exact: true }).click();

    await expect(page.getByText('A static copy holds one rule')).toBeVisible();
    await page.getByRole('button', { name: 'Copy as dynamic' }).click();

    await expect(page).toHaveURL(new RegExp(`/app/new\\?copyOf=${routed.id}&mode=dynamic$`));
    await expect(page.getByRole('heading', { name: 'Copy code' })).toBeVisible();
  });

  test('asks before leaving an edited code unsaved', async ({ page, baseURL }) => {
    const code = fakeCode({ name: 'Menu' });
    await new FakeApi({ isGuest: true, codes: [code] }).install(page, baseURL!);

    await page.goto(`/app/${code.id}/edit`);
    await page.getByPlaceholder('Spring menu table tent').fill('Menu v2');
    await page.getByRole('button', { name: 'Back to codes' }).click();

    await expect(page.getByRole('heading', { name: 'Discard unsaved changes?' })).toBeVisible();
    await page.getByRole('button', { name: 'Discard changes' }).click();
    await expect(page).toHaveURL(/\/app$/);
  });
});
