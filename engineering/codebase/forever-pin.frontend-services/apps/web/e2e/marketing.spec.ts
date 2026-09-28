import { expect, test } from '@playwright/test';

test.describe('marketing', () => {
  test('renders the landing page without calling the API', async ({ page }) => {
    const calls: string[] = [];
    page.on('request', (request) => {
      if (new URL(request.url()).pathname.startsWith('/api/')) calls.push(request.url());
    });

    await page.goto('/');

    await expect(page.getByRole('heading', { level: 1 })).toContainText('A QR code smart enough');
    await expect(page).toHaveTitle('ForeverPin — programmable codes that never expire');
    expect(calls).toEqual([]);
  });

  test('gives an article its own title, description and canonical address', async ({ page }) => {
    await page.goto('/blog/why-qr-codes-should-never-expire?utm_source=share');

    await expect(page).toHaveTitle('Why your QR codes should never expire · ForeverPin');
    await expect(page.locator('meta[name="description"]')).toHaveAttribute('content', /Print 10,000 flyers/);
    await expect(page.locator('link[rel="canonical"]')).toHaveAttribute(
      'href',
      /\/blog\/why-qr-codes-should-never-expire$/,
    );
  });

  test('shows the not-found page for an unknown address', async ({ page }) => {
    await page.goto('/no/such/page');

    await expect(page.getByRole('heading', { name: 'This page wandered off' })).toBeVisible();
    await expect(page).toHaveTitle('Page not found · ForeverPin');
  });

  test('reaches the blog from the phone menu', async ({ page, isMobile }) => {
    test.skip(!isMobile, 'The site menu replaces the header links on phones only.');
    await page.goto('/pricing');

    await page.getByRole('button', { name: 'Open the site menu' }).click();
    await page.getByRole('menuitem', { name: 'Blog' }).click();

    await expect(page).toHaveURL(/\/blog$/);
  });
});

test.describe('page documents', () => {
  test('serve each public page its own head before any script runs', async ({ request }) => {
    const pricing = await (await request.get('/pricing')).text();
    const article = await (await request.get('/blog/dynamic-vs-static-qr-codes')).text();

    expect(pricing).toContain('<title>Pricing — flat plans, unlimited scans · ForeverPin</title>');
    expect(pricing).toContain('Free, $5, $15, or $39 a month');
    expect(article).toContain('<title>Dynamic vs static QR codes: which should you use? · ForeverPin</title>');
  });
});
