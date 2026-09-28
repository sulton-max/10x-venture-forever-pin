import { defineConfig, devices } from '@playwright/test';

// Browser checks run the production build against an in-memory fake API (e2e/support/FakeApi.ts), so every
// product flow is deterministic and needs no backend; the backend's own suites cover the HTTP contract.
export default defineConfig({
  testDir: 'e2e',
  fullyParallel: true,
  forbidOnly: Boolean(process.env.CI),
  retries: process.env.CI ? 1 : 0,
  reporter: process.env.CI ? 'github' : 'list',
  use: {
    baseURL: 'http://localhost:7031',
    trace: 'retain-on-failure',
  },
  projects: [
    { name: 'desktop', use: { ...devices['Desktop Chrome'] } },
    { name: 'phone', use: { ...devices['Pixel 7'] } },
  ],
  webServer: {
    command: 'pnpm exec vite preview --port 7031 --strictPort --host localhost',
    env: { VITE_HTTPS: 'false' },
    url: 'http://localhost:7031',
    reuseExistingServer: !process.env.CI,
  },
});
