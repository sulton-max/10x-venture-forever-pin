import type { Router } from 'vue-router';

/**
 * Keeps one `<link rel="canonical">` naming the current public page without its query, so shared links with
 * tracking parameters index as one page; product routes, which crawlers never index, carry none.
 */
export function installCanonicalLink(router: Router, isPublic: (path: string) => boolean): () => void {
  if (typeof document === 'undefined') return () => undefined;
  return router.afterEach((to, _from, failure) => {
    if (failure) return;
    let link = document.head.querySelector<HTMLLinkElement>('link[rel="canonical"]');
    if (!isPublic(to.path)) {
      link?.remove();
      return;
    }
    if (!link) {
      link = document.createElement('link');
      link.rel = 'canonical';
      document.head.appendChild(link);
    }
    link.href = new URL(to.path, window.location.origin).href;
  });
}
