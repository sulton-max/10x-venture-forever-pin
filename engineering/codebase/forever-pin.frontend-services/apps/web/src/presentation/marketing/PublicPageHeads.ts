import { PostCatalog, type PostMeta } from './blog/BlogPostCatalog';

/** Represents a public page's document head: what crawlers and link previews read before any script runs. */
export interface PublicPageHead {
  /** The page path. */
  readonly path: string;
  /** The document title. */
  readonly title: string;
  /** The meta description. */
  readonly description: string;
}

/** Holds the landing page's head; the app shell carries it. */
const Home: PublicPageHead = {
  path: '/',
  title: 'ForeverPin — programmable codes that never expire',
  description:
    "One QR code, many destinations by context — and you'll never reprint it. Smart routing, every code type, unlimited scans, flat pricing. Your codes, forever.",
};

/** Holds the pricing page's head. */
const Pricing: PublicPageHead = {
  path: '/pricing',
  title: 'Pricing — flat plans, unlimited scans · ForeverPin',
  description:
    'Free, $5, $15, or $39 a month — flat. Unlimited scans on every tier, smart routing and a custom domain from $5, and codes that never expire. No scan caps, no surprise bills.',
};

/** Holds the blog index's head. */
const Blog: PublicPageHead = {
  path: '/blog',
  title: 'Blog — QR codes, smart routing & best practices · ForeverPin',
  description:
    'Guides on why and how to use QR codes: dynamic vs. static, how smart routing works, best practices for codes that actually scan, and why your codes should never expire.',
};

/**
 * Holds every public page's head. The router sets them as the visitor navigates; the build writes them into each
 * page's own document, so a link preview that runs no script still reads the right title. Plain data by design.
 */
export const PublicPageHeads = {
  home: Home,
  pricing: Pricing,
  blog: Blog,

  /** Builds an article's head from its metadata. */
  post(post: Pick<PostMeta, 'slug' | 'title' | 'description'>): PublicPageHead {
    return { path: `/blog/${post.slug}`, title: `${post.title} · ForeverPin`, description: post.description };
  },

  /** Lists the pages that ship their own document: every public page but the landing, which is the shell. */
  prerendered(): ReadonlyArray<PublicPageHead> {
    return [Pricing, Blog, ...PostCatalog.map((post) => PublicPageHeads.post(post))];
  },
} as const;
