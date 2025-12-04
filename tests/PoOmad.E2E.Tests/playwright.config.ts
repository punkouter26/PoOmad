import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for PoOmad E2E tests
 * @see https://playwright.dev/docs/test-configuration
 */
export default defineConfig({
  testDir: './tests',
  /* Run tests in files in parallel */
  fullyParallel: false,  // Disable parallel for Blazor WASM apps
  /* Fail the build on CI if you accidentally left test.only in the source code. */
  forbidOnly: !!process.env.CI,
  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,
  /* Single worker to avoid conflicts with shared API server */
  workers: 1,
  /* Reporter to use. See https://playwright.dev/docs/test-reporters */
  reporter: [
    ['html'],
    ['list']
  ],
  /* Increase timeout for Blazor WASM initialization */
  timeout: 60000,
  /* Shared settings for all the projects below. */
  use: {
    /* Base URL to use in actions like `await page.goto('/')`. */
    baseURL: 'http://localhost:5000',

    /* Collect trace when retrying the failed test. */
    trace: 'on-first-retry',

    /* Ignore HTTPS errors for local development */
    ignoreHTTPSErrors: true,

    /* Screenshot on failure */
    screenshot: 'only-on-failure',
  },

  /* Configure projects for major browsers */
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
    /* Test against mobile viewports. */
    {
      name: 'Mobile Chrome',
      use: { ...devices['Pixel 5'] },
    },
  ],

  /* Run your local dev server before starting the tests */
  webServer: {
    command: 'dotnet run --project ../../src/PoOmad.Api/PoOmad.Api',
    url: 'http://localhost:5000/api/health',
    reuseExistingServer: true,  // Always reuse to avoid port conflicts
    timeout: 120 * 1000, // 2 minutes for server startup
  },
});
