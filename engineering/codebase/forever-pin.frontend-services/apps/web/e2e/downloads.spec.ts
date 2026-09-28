import { readFile } from 'node:fs/promises';

import { expect, test } from '@playwright/test';

import { FakeApi, fakeCode } from './support/FakeApi';

/** Builds an SVG the way the renderer does: measured in modules, sized in pixels. */
function renderedSvg(modules: number): string {
  return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${modules} ${modules}" width="740" height="740"><rect width="${modules}" height="${modules}" fill="#fff"/></svg>`;
}

test.describe('downloads', () => {
  test('saves a print SVG at the chosen physical width', async ({ page, baseURL }) => {
    const code = fakeCode({ name: 'Menu' });
    await new FakeApi({ isGuest: true, codes: [code], images: { [code.id]: renderedSvg(37) } }).install(page, baseURL!);

    await page.goto('/app');
    await page.getByRole('button', { name: 'Download' }).click();
    await expect(page.getByText('Scans comfortably from about 30 cm away.')).toBeVisible();
    await page.getByRole('button', { name: '5 cm' }).click();
    const [download] = await Promise.all([
      page.waitForEvent('download'),
      page.getByRole('button', { name: 'Download SVG · 5 cm' }).click(),
    ]);

    expect(download.suggestedFilename()).toBe('Menu.svg');
    const svg = await readFile(await download.path(), 'utf8');
    expect(svg).toContain('<svg width="50mm" height="50mm"');
    expect(svg).not.toContain('width="740"');
  });

  test('flags a print width too small for a dense code', async ({ page, baseURL }) => {
    const code = fakeCode({ name: 'Contact card', mode: 'static' });
    await new FakeApi({ isGuest: true, codes: [code], images: { [code.id]: renderedSvg(89) } }).install(page, baseURL!);

    await page.goto('/app');
    await page.getByRole('button', { name: 'Download' }).click();
    await page.getByRole('button', { name: '2 cm' }).click();

    await expect(page.getByText('its modules need at least 36 mm of width')).toBeVisible();
    await page.getByRole('button', { name: '5 cm' }).click();
    await expect(page.getByText('its modules need at least 36 mm of width')).toHaveCount(0);
  });
});
