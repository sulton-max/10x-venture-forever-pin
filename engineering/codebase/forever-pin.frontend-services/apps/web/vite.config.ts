import tailwindcss from '@tailwindcss/vite';
import vue from '@vitejs/plugin-vue';
import { fileURLToPath } from 'node:url';
import { defineConfig } from 'vite';
import mkcert from 'vite-plugin-mkcert';

import { publicPageDocuments } from './scripts/PublicPageDocuments';

// The app calls same-origin /api; in development Vite proxies it, and /health, to the management API.
export default defineConfig(({ command, mode }) => {
  const https = mode !== 'http' && process.env.VITE_HTTPS !== 'false';
  const api = process.env.VITE_API_TARGET ?? 'https://localhost:7020';

  return {
    base: '/',
    plugins: [vue(), tailwindcss(), publicPageDocuments(), ...(command === 'serve' && https ? [mkcert()] : [])],
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
