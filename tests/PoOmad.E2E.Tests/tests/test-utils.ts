import { Page, expect } from '@playwright/test';

/**
 * Shared test utilities for E2E tests
 */

/**
 * Login using the development auth bypass endpoint.
 * This creates a test user session without requiring Google OAuth.
 * Only works in Development environment.
 */
export async function devLogin(page: Page): Promise<void> {
  // Navigate to dev login endpoint - this sets the auth cookie and redirects to home
  const response = await page.goto('/api/auth/dev-login', { waitUntil: 'domcontentloaded' });
  
  // The dev-login endpoint should redirect to / 
  // Wait for Blazor WASM to fully load
  await page.waitForLoadState('networkidle');
  
  // Give Blazor time to initialize and check auth state
  await page.waitForTimeout(2000);
  
  // Now wait for the dashboard to appear (calendar-container is inside dashboard)
  const maxRetries = 5;
  for (let i = 0; i < maxRetries; i++) {
    const url = page.url();
    console.log(`devLogin attempt ${i + 1}: URL is ${url}`);
    
    // If still on auth page, the cookie might not have been set properly
    // Click the Dev Login button directly
    if (url.includes('/auth')) {
      const devLoginButton = page.getByRole('button', { name: /Dev Login/i });
      if (await devLoginButton.isVisible()) {
        console.log('Clicking Dev Login button');
        await devLoginButton.click();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(2000);
        continue;
      }
    }
    
    // Check if dashboard is visible
    const dashboard = page.locator('.dashboard');
    const calendar = page.locator('.calendar-container');
    
    if (await dashboard.isVisible() || await calendar.isVisible()) {
      console.log('Dashboard/calendar is visible, login successful');
      return;
    }
    
    // Wait a bit and try again
    await page.waitForTimeout(1000);
  }
  
  // Final check - assert that we're on the dashboard
  await expect(page.locator('.dashboard, .calendar-container').first()).toBeVisible({ timeout: 10000 });
}

/**
 * Get today's date information for test assertions
 */
export function getTodayInfo(): { day: string; fullDate: string; isoDate: string } {
  const today = new Date();
  return {
    day: today.getDate().toString(),
    fullDate: today.toLocaleDateString('en-US', { 
      month: 'short', 
      day: '2-digit', 
      year: 'numeric' 
    }),
    isoDate: today.toISOString().split('T')[0]
  };
}

/**
 * Generate a weight value that's within the 5 lb threshold of typical values
 * to avoid triggering the confirmation dialog
 */
export function getRandomWeight(): string {
  // Use a weight close to 175 lbs (typical test starting weight) to avoid 5 lb threshold
  return (173 + Math.random() * 4).toFixed(1);
}

/**
 * Wait for Blazor WASM to fully initialize
 */
export async function waitForBlazorReady(page: Page): Promise<void> {
  await page.waitForLoadState('networkidle');
  // Wait a bit more for Blazor to hydrate
  await page.waitForTimeout(500);
}
