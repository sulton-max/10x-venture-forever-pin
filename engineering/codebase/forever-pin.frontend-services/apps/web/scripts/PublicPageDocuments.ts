import { existsSync } from 'node:fs';
import { mkdir, readFile, writeFile } from 'node:fs/promises';
import { dirname, extname, join, resolve, sep } from 'node:path';

import type { Plugin } from 'vite';

import { PublicPageHeads, type PublicPageHead } from '../src/presentation/marketing/PublicPageHeads';

/** Escapes text for an HTML attribute or element body. */
function escapeHtml(value: string): string {
  return value.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

/** Holds the head fragments a page rewrites, each matched across the shell's line breaks. */
const HeadFragments = [
  { pattern: /(<title>)[^<]*(<\/title>)/, value: (head: PublicPageHead) => head.title },
  { pattern: /(<meta\s+name="description"\s+content=")[^"]*(")/, value: (head: PublicPageHead) => head.description },
  { pattern: /(<meta\s+property="og:title"\s+content=")[^"]*(")/, value: (head: PublicPageHead) => head.title },
  {
    pattern: /(<meta\s+property="og:description"\s+content=")[^"]*(")/,
    value: (head: PublicPageHead) => head.description,
  },
] as const;

/** Rewrites the shell's title, description and their Open Graph twins for one page; a missing tag fails the build. */
export function withPageHead(html: string, head: PublicPageHead): string {
  return HeadFragments.reduce((document, fragment) => {
    if (!fragment.pattern.test(document)) throw new Error(`The app shell lacks ${fragment.pattern.source}.`);
    // A replacer function, so a `$5` price in the text is never read as a capture reference.
    return document.replace(fragment.pattern, (_match, open: string, close: string) => {
      return `${open}${escapeHtml(fragment.value(head))}${close}`;
    });
  }, html);
}

/**
 * Gives each public page its own document: the shell carries the landing head, and the build writes
 * `pricing/index.html`, `blog/index.html` and one per article with that page's head. The host serves a route's own
 * document before the shell, so link previews that run no script read the right title and description; `vite
 * preview` follows the same rule.
 */
export function publicPageDocuments(): Plugin {
  let outDir = '';
  let isBuild = false;
  return {
    name: 'foreverpin:public-page-documents',
    configResolved(config) {
      outDir = resolve(config.root, config.build.outDir);
      isBuild = config.command === 'build';
    },
    transformIndexHtml: (html) => withPageHead(html, PublicPageHeads.home),
    configurePreviewServer(server) {
      // Mirrors the host: an extensionless path with its own document gets it instead of the shell.
      server.middlewares.use((request, _response, next) => {
        let path: string;
        try {
          path = decodeURIComponent((request.url ?? '/').split('?')[0] ?? '/');
        } catch {
          return next();
        }
        const document = resolve(outDir, `.${path}`, 'index.html');
        if (path !== '/' && !extname(path) && document.startsWith(outDir + sep) && existsSync(document))
          request.url = `${path.replace(/\/$/, '')}/index.html`;
        next();
      });
    },
    async closeBundle() {
      if (!isBuild) return;
      const shell = await readFile(join(outDir, 'index.html'), 'utf8');
      for (const head of PublicPageHeads.prerendered()) {
        const file = join(outDir, head.path, 'index.html');
        await mkdir(dirname(file), { recursive: true });
        await writeFile(file, withPageHead(shell, head));
      }
    },
  };
}
