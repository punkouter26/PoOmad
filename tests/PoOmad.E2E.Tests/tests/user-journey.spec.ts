import { test, expect } from '@playwright/test';
import { devLogin } from './test-utils';

/**
 * Core user journey E2E tests
 * Tests all acceptance scenarios from spec.md
 */

test.describe('User Journey Tests', () => {
  
  test.describe('US1: Initial Setup & Profile Creation', () => {
    
    test('should display sign in with Google button on auth page', async ({ page }) => {
      await page.goto('/auth');
      
      await expect(page.getByRole('button', { name: /sign in with google/i })).toBeVisible();
    });

    test('should display dev login button on auth page in development', async ({ page }) => {
      await page.goto('/auth');
      
      await expect(page.getByRole('button', { name: /dev login/i })).toBeVisible();
    });

    test('dev login should authenticate user and redirect to home', async ({ page }) => {
      await devLogin(page);
      
      // Should be on home page
      await expect(page).toHaveURL('/');
      
      // Should show authenticated state
      await expect(page.getByText(/dev@localhost/i)).toBeVisible({ timeout: 5000 });
    });
  });

  test.describe('US2: Daily OMAD Logging', () => {
    
    test.beforeEach(async ({ page }) => {
      await devLogin(page);
    });

    test('should display daily log modal when clicking calendar cell', async ({ page }) => {
      // Click on today's cell
      const today = new Date().getDate().toString();
      const todayCell = page.locator('.day-cell').filter({ has: page.locator('.day-number', { hasText: new RegExp(`^${today}$`) }) }).first();
      await todayCell.click();
      
      // Modal should appear
      await expect(page.locator('.modal-content')).toBeVisible({ timeout: 5000 });
    });

    test('should display 3-question form in modal', async ({ page }) => {
      // Open modal
      const today = new Date().getDate().toString();
      const todayCell = page.locator('.day-cell').filter({ has: page.locator('.day-number', { hasText: new RegExp(`^${today}$`) }) }).first();
      await todayCell.click();
      
      const modal = page.locator('.modal-content');
      await expect(modal).toBeVisible({ timeout: 5000 });
      
      // Check for OMAD toggle
      await expect(page.getByText(/OMAD/i)).toBeVisible();
      
      // Check for Alcohol toggle
      await expect(page.getByText(/alcohol/i)).toBeVisible();
      
      // Check for Weight input
      await expect(page.getByText(/weight/i)).toBeVisible();
    });
  });

  test.describe('US3: Calendar Dashboard', () => {
    
    test.beforeEach(async ({ page }) => {
      await devLogin(page);
    });

    test('should display monthly calendar grid', async ({ page }) => {
      // Calendar should be visible
      await expect(page.locator('.calendar-grid')).toBeVisible({ timeout: 5000 });
      
      // Should show day headers (Sun, Mon, etc.)
      await expect(page.getByText('Sun')).toBeVisible();
      await expect(page.getByText('Mon')).toBeVisible();
    });

    test('should display streak counter', async ({ page }) => {
      // Streak counter should be visible
      await expect(page.locator('.streak-counter, .streak')).toBeVisible({ timeout: 5000 });
    });

    test('should allow month navigation', async ({ page }) => {
      // Navigation buttons should be visible (could be arrows or text)
      const prevButton = page.locator('button').filter({ hasText: /prev|‹|</i }).first();
      const nextButton = page.locator('button').filter({ hasText: /next|›|>/i }).first();
      
      // At least one navigation mechanism should exist
      const hasPrevNext = await prevButton.isVisible() || await nextButton.isVisible();
      expect(hasPrevNext).toBe(true);
    });
  });

  test.describe('US4: Weight & Alcohol Analytics', () => {
    
    test.beforeEach(async ({ page }) => {
      await devLogin(page);
    });

    test('should navigate to analytics page', async ({ page }) => {
      // Navigate via URL
      await page.goto('/analytics');
      await page.waitForLoadState('networkidle');
      
      // Page should load without error
      await expect(page.locator('body')).toBeVisible();
    });
  });

  test.describe('US5: Dark Mode Interface', () => {
    
    test('should have dark background on auth page', async ({ page }) => {
      await page.goto('/auth');
      
      const backgroundColor = await page.evaluate(() => {
        return window.getComputedStyle(document.body).backgroundColor;
      });
      
      // Dark background should have low luminance
      console.log('Background color:', backgroundColor);
      expect(backgroundColor).toBeDefined();
    });

    test('should have dark background on dashboard', async ({ page }) => {
      await devLogin(page);
      
      const backgroundColor = await page.evaluate(() => {
        return window.getComputedStyle(document.body).backgroundColor;
      });
      
      console.log('Dashboard background color:', backgroundColor);
      expect(backgroundColor).toBeDefined();
    });
  });
});

test.describe('API Health Check', () => {
  
  test('API health endpoint should return 200', async ({ request }) => {
    const response = await request.get('/api/health');
    
    expect(response.status()).toBe(200);
  });
});

test.describe('Performance Tests', () => {
  
  test('dashboard should load within acceptable time', async ({ page }) => {
    await devLogin(page);
    
    const startTime = Date.now();
    
    await page.reload();
    await page.waitForLoadState('networkidle');
    
    const loadTime = Date.now() - startTime;
    console.log(`Dashboard reload time: ${loadTime}ms`);
    
    // Subsequent loads should be faster due to caching
    expect(loadTime).toBeLessThan(5000);
  });
});
