import { describe, expect, it } from 'vitest';

import { PublicPageHeads } from '@/presentation/marketing';

import { withPageHead } from '../../../scripts/PublicPageDocuments';

const Shell = `<head>
    <title>Shell</title>
    <meta
      name="description"
      content="Shell description"
    />
    <meta property="og:title" content="Shell" />
    <meta property="og:description" content="Shell description" />
</head>`;

describe('public page documents', () => {
  it('rewrite the title, description and social tags, escaping quotes and keeping prices', () => {
    const html = withPageHead(Shell, { path: '/x', title: 'A "quoted" <title>', description: 'From $5 & up' });

    expect(html).toContain('<title>A &quot;quoted&quot; &lt;title&gt;</title>');
    expect(html).toContain('content="From $5 &amp; up"');
    expect(html).toContain('<meta property="og:title" content="A &quot;quoted&quot; &lt;title&gt;" />');
    expect(html).not.toContain('Shell');
  });

  it('fail the build when the shell lacks a tag it rewrites', () => {
    expect(() => withPageHead('<title>Shell</title>', PublicPageHeads.pricing)).toThrow(/lacks/);
  });

  it('cover every public page but the landing, one document per article', () => {
    const paths = PublicPageHeads.prerendered().map((head) => head.path);

    expect(paths).toContain('/pricing');
    expect(paths).toContain('/blog');
    expect(paths.filter((path) => path.startsWith('/blog/'))).toHaveLength(4);
    expect(paths).not.toContain('/');
  });
});
