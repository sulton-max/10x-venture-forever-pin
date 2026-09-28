import type { Component } from 'vue';

import { PostCatalog, type PostMeta } from './BlogPostCatalog';

/** Represents one article: its listing metadata and its lazily loaded body. */
export interface PostEntry extends PostMeta {
  /** Loads the article body, rendered inside `.prose`. */
  readonly load: () => Promise<{ default: Component }>;
}

/** Holds each article's body loader by slug; bodies load on demand. */
const Bodies: Readonly<Record<string, PostEntry['load']>> = {
  'why-qr-codes-should-never-expire': () => import('./posts/WhyQrCodesShouldNeverExpirePost.vue'),
  'smart-routing-one-code-many-destinations': () => import('./posts/SmartRoutingOneCodeManyDestinationsPost.vue'),
  'qr-best-practices-that-scan': () => import('./posts/QrBestPracticesThatScanPost.vue'),
  'dynamic-vs-static-qr-codes': () => import('./posts/DynamicVsStaticQrCodesPost.vue'),
};

/** Lists every article; add its metadata to the catalogue and its body here, and it appears in the index. */
const PostEntries: ReadonlyArray<PostEntry> = PostCatalog.map((meta) => {
  const load = Bodies[meta.slug];
  if (!load) throw new Error(`The article "${meta.slug}" has no body.`);
  return { ...meta, load };
});

/** Reads the article catalogue: newest first, lookups by slug and follow-up reading. */
export const BlogPosts = {
  /** Lists every article, newest first. */
  newestFirst: [...PostEntries].sort((left, right) => right.date.localeCompare(left.date)) as ReadonlyArray<PostEntry>,

  /** Finds the article with `slug`, or `undefined`. */
  find(slug: string): PostEntry | undefined {
    return PostEntries.find((entry) => entry.slug === slug);
  },

  /** Lists up to `count` other articles, newest first. */
  related(slug: string, count: number): ReadonlyArray<PostEntry> {
    return BlogPosts.newestFirst.filter((entry) => entry.slug !== slug).slice(0, count);
  },
} as const;
