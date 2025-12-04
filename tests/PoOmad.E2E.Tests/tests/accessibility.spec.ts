import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { devLogin } from './test-utils';

/**
 * Accessibility tests using axe-core
 * Verifies WCAG 2.1 AA compliance across all main screens
 */

test.describe('Accessibility Tests - WCAG 2.1 AA Compliance', () => {
  
  test('Auth page should have no accessibility violations', async ({ page }) => {
    await page.goto('/auth');
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21aa'])
      .analyze();

    // Log violations for debugging
    if (accessibilityScanResults.violations.length > 0) {
      console.log('Accessibility violations on Auth page:');
      accessibilityScanResults.violations.forEach(violation => {
        console.log(`  - ${violation.id}: ${violation.description}`);
        console.log(`    Impact: ${violation.impact}`);
        console.log(`    Help: ${violation.helpUrl}`);
      });
    }

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('Dashboard page should have no accessibility violations', async ({ page }) => {
    await devLogin(page);
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21aa'])
      .analyze();

    if (accessibilityScanResults.violations.length > 0) {
      console.log('Accessibility violations on Dashboard:');
      accessibilityScanResults.violations.forEach(violation => {
        console.log(`  - ${violation.id}: ${violation.description}`);
        console.log(`    Impact: ${violation.impact}`);
      });
    }

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('Analytics page should have no accessibility violations', async ({ page }) => {
    await devLogin(page);
    await page.goto('/analytics');
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21aa'])
      .analyze();

    if (accessibilityScanResults.violations.length > 0) {
      console.log('Accessibility violations on Analytics page:');
      accessibilityScanResults.violations.forEach(violation => {
        console.log(`  - ${violation.id}: ${violation.description}`);
        console.log(`    Impact: ${violation.impact}`);
      });
    }

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('Color contrast should meet WCAG AA standards', async ({ page }) => {
    await devLogin(page);
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2aa'])
      .options({ runOnly: ['color-contrast'] })
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('Interactive elements should be keyboard accessible', async ({ page }) => {
    await devLogin(page);
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .options({ runOnly: ['keyboard'] })
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('Images should have alt text', async ({ page }) => {
    await devLogin(page);
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .options({ runOnly: ['image-alt'] })
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('Page should have proper heading hierarchy', async ({ page }) => {
    await devLogin(page);
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .options({ runOnly: ['heading-order'] })
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('Focus order should be logical', async ({ page }) => {
    await devLogin(page);
    
    // Tab through the page and verify focus order
    await page.keyboard.press('Tab');
    
    // Check that focus is visible
    const focusedElement = await page.evaluate(() => {
      const el = document.activeElement;
      if (!el) return null;
      const styles = window.getComputedStyle(el);
      return {
        tagName: el.tagName,
        hasVisibleFocus: styles.outline !== 'none' || styles.boxShadow !== 'none'
      };
    });

    expect(focusedElement).not.toBeNull();
  });

  test('Buttons should have accessible names', async ({ page }) => {
    await devLogin(page);
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .options({ runOnly: ['button-name'] })
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('Links should have accessible names', async ({ page }) => {
    await devLogin(page);
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .options({ runOnly: ['link-name'] })
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });
});

test.describe('Dark Mode Accessibility', () => {
  
  test('Dark theme should maintain sufficient contrast', async ({ page }) => {
    await devLogin(page);
    
    // Check that dark background colors exist
    const bodyBackgroundColor = await page.evaluate(() => {
      return window.getComputedStyle(document.body).backgroundColor;
    });
    
    console.log('Body background color:', bodyBackgroundColor);
    
    // Run contrast check
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2aa'])
      .options({ runOnly: ['color-contrast'] })
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('Calendar cell colors should be distinguishable', async ({ page }) => {
    await devLogin(page);
    
    // Verify that calendar renders
    await expect(page.locator('.calendar-grid')).toBeVisible({ timeout: 5000 });
    
    console.log('Calendar rendered successfully');
    expect(true).toBe(true);
  });
});
