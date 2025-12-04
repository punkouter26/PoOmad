import { test, expect, Page } from '@playwright/test';
import { devLogin, getTodayInfo, getRandomWeight } from './test-utils';

/**
 * Daily Log E2E tests
 * Tests data entry and persistence for daily logs
 */

test.describe('Daily Log Data Persistence', () => {
  
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await devLogin(page);
  });

  test('should save and reload daily log data correctly', async ({ page }) => {
    const { day } = getTodayInfo();
    const testWeight = getRandomWeight();
    
    // Step 1: Click on today's date to open modal
    const todayCell = page.locator('.day-cell').filter({ has: page.locator('.day-number', { hasText: new RegExp(`^${day}$`) }) }).first();
    await todayCell.click();
    
    // Wait for modal to appear
    const modal = page.locator('.modal-content');
    await expect(modal).toBeVisible({ timeout: 5000 });
    
    // Step 2: Enter test data
    // Set OMAD to Yes
    await page.locator('.toggle-btn').filter({ hasText: 'Yes' }).first().click();
    
    // Set Alcohol to No (second toggle group)
    const alcoholNoButton = page.locator('.form-group').filter({ hasText: /alcohol/i }).locator('.toggle-btn').filter({ hasText: 'No' });
    await alcoholNoButton.click();
    
    // Enter weight
    const weightInput = page.locator('input[type="number"]');
    await weightInput.clear();
    await weightInput.fill(testWeight);
    
    // Step 3: Save the data
    await page.getByRole('button', { name: 'Save' }).click();
    
    // Handle weight confirmation if it appears (for changes > 5 lbs)
    const confirmButton = page.getByRole('button', { name: 'Confirm Weight' });
    if (await confirmButton.isVisible({ timeout: 1000 }).catch(() => false)) {
      await confirmButton.click();
    }
    
    // Wait for modal to close
    await expect(modal).not.toBeVisible({ timeout: 5000 });
    
    // Wait for API call to complete and data to persist
    await page.waitForTimeout(500);
    
    // Step 4: Reopen the modal for the same day
    await todayCell.click();
    await expect(modal).toBeVisible({ timeout: 5000 });
    
    // Wait for the modal to fully load data from API
    await page.waitForLoadState('networkidle');
    
    // Debug: log the current value
    const weightValue = await page.locator('input[type="number"]').inputValue();
    console.log(`Weight value after reload: "${weightValue}", expected: "${testWeight}"`);
    
    // Step 5: Verify the data was saved correctly
    // Check OMAD is Yes (first toggle should be active)
    const omadYesButton = page.locator('.form-group').filter({ hasText: /OMAD/i }).locator('.toggle-btn').filter({ hasText: 'Yes' });
    await expect(omadYesButton).toHaveClass(/active/);
    
    // Check Alcohol is No
    const alcoholNoButtonCheck = page.locator('.form-group').filter({ hasText: /alcohol/i }).locator('.toggle-btn').filter({ hasText: 'No' });
    await expect(alcoholNoButtonCheck).toHaveClass(/active/);
    
    // Check weight matches
    const weightInputCheck = page.locator('input[type="number"]');
    await expect(weightInputCheck).toHaveValue(testWeight);
    
    // Close modal
    await page.getByRole('button', { name: 'Cancel' }).click();
  });

  test('should update existing daily log data', async ({ page }) => {
    const { day } = getTodayInfo();
    const initialWeight = '175.0';
    const updatedWeight = '176.5';
    
    // Step 1: Open modal and enter initial data
    const todayCell = page.locator('.day-cell').filter({ has: page.locator('.day-number', { hasText: new RegExp(`^${day}$`) }) }).first();
    await todayCell.click();
    
    const modal = page.locator('.modal-content');
    await expect(modal).toBeVisible({ timeout: 5000 });
    
    // Set initial values
    await page.locator('.form-group').filter({ hasText: /OMAD/i }).locator('.toggle-btn').filter({ hasText: 'Yes' }).click();
    await page.locator('.form-group').filter({ hasText: /alcohol/i }).locator('.toggle-btn').filter({ hasText: 'No' }).click();
    
    const weightInput = page.locator('input[type="number"]');
    await weightInput.clear();
    await weightInput.fill(initialWeight);
    
    await page.getByRole('button', { name: 'Save' }).click();
    await expect(modal).not.toBeVisible({ timeout: 5000 });
    
    // Step 2: Reopen and update the data
    await todayCell.click();
    await expect(modal).toBeVisible({ timeout: 5000 });
    
    // Change OMAD to No
    await page.locator('.form-group').filter({ hasText: /OMAD/i }).locator('.toggle-btn').filter({ hasText: 'No' }).click();
    
    // Change Alcohol to Yes
    await page.locator('.form-group').filter({ hasText: /alcohol/i }).locator('.toggle-btn').filter({ hasText: 'Yes' }).click();
    
    // Update weight
    await weightInput.clear();
    await weightInput.fill(updatedWeight);
    
    await page.getByRole('button', { name: 'Save' }).click();
    await expect(modal).not.toBeVisible({ timeout: 5000 });
    
    // Step 3: Verify updated data persisted
    await todayCell.click();
    await expect(modal).toBeVisible({ timeout: 5000 });
    
    // Check OMAD is now No
    const omadNoButton = page.locator('.form-group').filter({ hasText: /OMAD/i }).locator('.toggle-btn').filter({ hasText: 'No' });
    await expect(omadNoButton).toHaveClass(/active/);
    
    // Check Alcohol is now Yes
    const alcoholYesButton = page.locator('.form-group').filter({ hasText: /alcohol/i }).locator('.toggle-btn').filter({ hasText: 'Yes' });
    await expect(alcoholYesButton).toHaveClass(/active/);
    
    // Check weight is updated
    await expect(weightInput).toHaveValue(updatedWeight);
    
    await page.getByRole('button', { name: 'Cancel' }).click();
  });

  test('should persist data after page refresh', async ({ page }) => {
    const { day } = getTodayInfo();
    const testWeight = '182.3';
    
    // Step 1: Enter and save data
    const todayCell = page.locator('.day-cell').filter({ has: page.locator('.day-number', { hasText: new RegExp(`^${day}$`) }) }).first();
    await todayCell.click();
    
    const modal = page.locator('.modal-content');
    await expect(modal).toBeVisible({ timeout: 5000 });
    
    await page.locator('.form-group').filter({ hasText: /OMAD/i }).locator('.toggle-btn').filter({ hasText: 'Yes' }).click();
    await page.locator('.form-group').filter({ hasText: /alcohol/i }).locator('.toggle-btn').filter({ hasText: 'No' }).click();
    
    const weightInput = page.locator('input[type="number"]');
    await weightInput.clear();
    await weightInput.fill(testWeight);
    
    await page.getByRole('button', { name: 'Save' }).click();
    await expect(modal).not.toBeVisible({ timeout: 5000 });
    
    // Step 2: Refresh the page
    await page.reload();
    await page.waitForLoadState('networkidle');
    
    // Step 3: Reopen modal and verify data persisted
    const todayCellAfterRefresh = page.locator('.day-cell').filter({ has: page.locator('.day-number', { hasText: new RegExp(`^${day}$`) }) }).first();
    await todayCellAfterRefresh.click();
    
    await expect(modal).toBeVisible({ timeout: 5000 });
    
    // Verify all data
    const omadYesButton = page.locator('.form-group').filter({ hasText: /OMAD/i }).locator('.toggle-btn').filter({ hasText: 'Yes' });
    await expect(omadYesButton).toHaveClass(/active/);
    
    const alcoholNoButton = page.locator('.form-group').filter({ hasText: /alcohol/i }).locator('.toggle-btn').filter({ hasText: 'No' });
    await expect(alcoholNoButton).toHaveClass(/active/);
    
    const weightInputCheck = page.locator('input[type="number"]');
    await expect(weightInputCheck).toHaveValue(testWeight);
  });

  test('should delete daily log entry', async ({ page }) => {
    const { day } = getTodayInfo();
    
    // Step 1: Create an entry first
    const todayCell = page.locator('.day-cell').filter({ has: page.locator('.day-number', { hasText: new RegExp(`^${day}$`) }) }).first();
    await todayCell.click();
    
    const modal = page.locator('.modal-content');
    await expect(modal).toBeVisible({ timeout: 5000 });
    
    await page.locator('.form-group').filter({ hasText: /OMAD/i }).locator('.toggle-btn').filter({ hasText: 'Yes' }).click();
    
    const weightInput = page.locator('input[type="number"]');
    await weightInput.clear();
    await weightInput.fill('180.0');
    
    await page.getByRole('button', { name: 'Save' }).click();
    await expect(modal).not.toBeVisible({ timeout: 5000 });
    
    // Step 2: Reopen and delete
    await todayCell.click();
    await expect(modal).toBeVisible({ timeout: 5000 });
    
    // Delete button should be visible for existing entries
    const deleteButton = page.getByRole('button', { name: 'Delete' });
    await expect(deleteButton).toBeVisible();
    await deleteButton.click();
    
    await expect(modal).not.toBeVisible({ timeout: 5000 });
    
    // Step 3: Reopen and verify it's a new entry (no delete button, default values)
    await todayCell.click();
    await expect(modal).toBeVisible({ timeout: 5000 });
    
    // For new entry, Delete button should not be visible
    await expect(page.getByRole('button', { name: 'Delete' })).not.toBeVisible();
  });
});
