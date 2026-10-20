import tailwindcss from '@tailwindcss/vite';
import vue from '@vitejs/plugin-vue';
import { fileURLToPath } from 'node:url';
import { colorModeScript } from '@wow-two-beta/fe/foundation/primitives';
import { defineConfig, type Plugin } from 'vite';
import mkcert from 'vite-plugin-mkcert';

import { publicPageDocuments } from './scripts/PublicPageDocuments';
import { ColorModeStorageKey } from './src/bootstrap/ColorMode';

/** Holds the build's version: CI passes `APP_VERSION`; a local build is `dev`. */
const version = process.env.APP_VERSION || 'dev';

/**
 * Serves and emits `/theme.js`: the SDK's pre-paint colour script for the app's storage key, so a dark app never
 * flashes light. A same-origin file rather than an inline script, so a strict Content-Security-Policy keeps it working.
 */
function themeScript(): Plugin {
  const source = colorModeScript({ storageKey: ColorModeStorageKey, defaultMode: 'system' });
  return {
    name: 'foreverpin-theme-script',
    configureServer(server) {
      server.middlewares.use('/theme.js', (_request, response) => {
        response.setHeader('Content-Type', 'text/javascript');
        response.end(source);
      });
    },
    configurePreviewServer(server) {
      server.middlewares.use('/theme.js', (_request, response) => {
        response.setHeader('Content-Type', 'text/javascript');
        response.end(source);
      });
    },
    generateBundle() {
      this.emitFile({ type: 'asset', fileName: 'theme.js', source });
    },
  };
}

// The app calls same-origin /api; in development Vite proxies it, and /health, to the management API.
export default defineConfig(({ command, mode }) => {
  const https = mode !== 'http' && process.env.VITE_HTTPS !== 'false';
  const api = process.env.VITE_API_TARGET ?? 'https://localhost:7020';

  return {
    base: '/',
    define: { __APP_VERSION__: JSON.stringify(version) },
    plugins: [
      vue(),
      tailwindcss(),
      themeScript(),
      publicPageDocuments(),
      ...(command === 'serve' && https ? [mkcert()] : []),
    ],
    resolve: { alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) } },
    build: { outDir: 'dist', emptyOutDir: true, target: 'es2022' },
    server: {
      host: 'localhost',
      port: Number(process.env.VITE_PORT ?? 7024),
      strictPort: true,
      https: https ? {} : undefined,
      proxy: {
        '/api': { target: api, changeOrigin: false, secure: false },
        '/health': { target: api, changeOrigin: false, secure: false },
      },
    },
  };
});
