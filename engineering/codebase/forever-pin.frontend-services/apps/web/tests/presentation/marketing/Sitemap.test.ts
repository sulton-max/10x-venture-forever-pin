import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { describe, expect, it } from 'vitest';

import { BlogPosts } from '@/presentation/marketing';

// Vitest runs from the app root, where `public/` holds the files served as-is.
const sitemap = readFileSync(resolve(process.cwd(), 'public/sitemap.xml'), 'utf8');
const robots = readFileSync(resolve(process.cwd(), 'public/robots.txt'), 'utf8');

describe('crawl files', () => {
  it('list every public page and article in the sitemap', () => {
    for (const path of ['/', '/pricing', '/blog'])
      expect(sitemap).toContain(`<loc>https://foreverpin.com${path}</loc>`);
    for (const post of BlogPosts.newestFirst) {
      expect(sitemap).toContain(`<loc>https://foreverpin.com/blog/${post.slug}</loc><lastmod>${post.date}</lastmod>`);
    }
  });

  it('keep crawlers out of the product and point them at the sitemap', () => {
    expect(robots).toMatch(/^Disallow: \/app$/m);
    expect(robots).toContain('Sitemap: https://foreverpin.com/sitemap.xml');
  });
});
