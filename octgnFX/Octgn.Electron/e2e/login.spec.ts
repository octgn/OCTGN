import { test, expect } from '@playwright/test';

/**
 * Inject a mock window.octgn API before the app loads.
 * The Vite dev server renders the React app but has no Electron preload,
 * so we must provide window.octgn ourselves.
 */
function injectOctgnMock(page: import('@playwright/test').Page, overrides: Record<string, unknown> = {}) {
  return page.addInitScript((opts) => {
    const noop = () => Promise.resolve();
    const mockApi: Record<string, unknown> = {
      // Auth
      login: async (username: string, password: string, _rememberMe?: boolean) => {
        // default: simulate successful login after a short delay
        await new Promise((r) => setTimeout(r, 300));
        return {
          success: true,
          user: { id: 'u1', username, isSubscriber: false },
          session: { userId: 'u1', sessionId: 's1', deviceId: 'd1' },
        };
      },
      logout: noop,
      getSession: async () => ({ success: false }),

      // Credentials
      loadCredentials: async () => null,
      saveCredentials: noop,
      clearCredentials: noop,

      // Clipboard
      writeClipboard: (text: string) => {
        (window as any).__lastClipboard = text;
      },

      // Stubs for other APIs the app may call
      getGames: async () => [],
      hostGame: noop,
      joinGame: noop,
      leaveGame: noop,
      minimize: noop,
      maximize: noop,
      quit: noop,
      getVersion: async () => '0.1.0-test',
      onGameStateUpdate: () => () => {},
      gameAction: noop,
      gameChat: noop,
      loadDeck: noop,
      openFileDialog: noop,
      executeScript: noop,
      onScriptEvent: () => () => {},
      listInstalledGames: async () => [],
      listAvailableGames: async () => [],
      installGame: noop,
      uninstallGame: noop,
      onInstallProgress: () => () => {},
      listFeeds: async () => [],
      addFeed: noop,
      removeFeed: noop,
      setFeedEnabled: noop,

      // Allow per-test overrides
      ...opts,
    };
    (window as any).octgn = mockApi;
  }, overrides);
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

test.describe('Login Page', () => {
  test('should render the login form with all elements', async ({ page }) => {
    await injectOctgnMock(page);
    await page.goto('/');

    // Title
    await expect(page.locator('h1:has-text("OCTGN")')).toBeVisible();

    // Username and password inputs
    await expect(page.getByTestId('login-username')).toBeVisible();
    await expect(page.getByTestId('login-password')).toBeVisible();

    // Remember me checkbox
    await expect(page.getByTestId('login-remember')).toBeVisible();
    await expect(page.getByTestId('login-remember')).toBeChecked();

    // Sign In button
    await expect(page.getByTestId('login-submit')).toBeVisible();
    await expect(page.getByTestId('login-submit')).toBeDisabled();

    // Links
    await expect(page.getByText('Create Account')).toBeVisible();
    await expect(page.getByText('Forgot Password')).toBeVisible();
  });

  test('should enable submit when both fields are filled', async ({ page }) => {
    await injectOctgnMock(page);
    await page.goto('/');

    const submit = page.getByTestId('login-submit');
    await expect(submit).toBeDisabled();

    await page.getByTestId('login-username').fill('alice');
    await expect(submit).toBeDisabled(); // still disabled — no password

    await page.getByTestId('login-password').fill('password123');
    await expect(submit).toBeEnabled();
  });

  test('should show error with copy button on login failure', async ({ page }) => {
    await injectOctgnMock(page, {
      login: `async () => ({ success: false, error: 'Incorrect password' })`,
    });
    // Re-inject with proper function (addInitScript serializes, so use evaluate approach)
    await page.addInitScript(() => {
      (window as any).octgn.login = async () => ({
        success: false,
        error: 'Incorrect password',
      });
    });
    await page.goto('/');

    await page.getByTestId('login-username').fill('alice');
    await page.getByTestId('login-password').fill('wrongpw');
    await page.getByTestId('login-submit').click();

    // Error should appear
    const errorEl = page.getByTestId('login-error');
    await expect(errorEl).toBeVisible();
    await expect(errorEl).toContainText('Incorrect password');

    // Copy button should be present
    const copyBtn = page.getByTestId('login-copy-error');
    await expect(copyBtn).toBeVisible();

    // Click copy and verify via our mock
    await copyBtn.click();
    const clipboardText = await page.evaluate(() => (window as any).__lastClipboard);
    expect(clipboardText).toBe('Incorrect password');
  });

  test('should show login animation overlay during authentication', async ({ page }) => {
    // Use a slow login to observe the overlay
    await page.addInitScript(() => {
      (window as any).octgn = {
        login: () => new Promise((resolve) => setTimeout(() => resolve({
          success: true,
          user: { id: 'u1', username: 'alice', isSubscriber: false },
          session: { userId: 'u1', sessionId: 's1', deviceId: 'd1' },
        }), 2000)),
        logout: async () => {},
        getSession: async () => ({ success: false }),
        loadCredentials: async () => null,
        saveCredentials: async () => {},
        clearCredentials: async () => {},
        writeClipboard: () => {},
        getGames: async () => [],
        hostGame: async () => {},
        joinGame: async () => {},
        leaveGame: async () => {},
        minimize: () => {},
        maximize: () => {},
        quit: () => {},
        getVersion: async () => '0.1.0',
        onGameStateUpdate: () => () => {},
        gameAction: async () => {},
        gameChat: async () => {},
        loadDeck: async () => {},
        openFileDialog: async () => null,
        executeScript: async () => {},
        onScriptEvent: () => () => {},
        listInstalledGames: async () => [],
        listAvailableGames: async () => [],
        installGame: async () => {},
        uninstallGame: async () => {},
        onInstallProgress: () => () => {},
        listFeeds: async () => [],
        addFeed: async () => {},
        removeFeed: async () => {},
        setFeedEnabled: async () => {},
      };
    });
    await page.goto('/');

    await page.getByTestId('login-username').fill('alice');
    await page.getByTestId('login-password').fill('password');
    await page.getByTestId('login-submit').click();

    // The overlay spinner should appear
    await expect(page.locator('.login-spinner')).toBeVisible({ timeout: 3000 });
    await expect(page.getByText('Authenticating')).toBeVisible();
  });

  test('should pre-fill saved credentials', async ({ page }) => {
    await page.addInitScript(() => {
      (window as any).octgn = {
        login: async () => ({ success: true, user: { id: 'u1', username: 'saved-user', isSubscriber: false }, session: { userId: 'u1', sessionId: 's1', deviceId: 'd1' } }),
        logout: async () => {},
        getSession: async () => ({ success: false }),
        loadCredentials: async () => ({ username: 'saved-user', password: 'saved-pass' }),
        saveCredentials: async () => {},
        clearCredentials: async () => {},
        writeClipboard: () => {},
        getGames: async () => [],
        hostGame: async () => {},
        joinGame: async () => {},
        leaveGame: async () => {},
        minimize: () => {},
        maximize: () => {},
        quit: () => {},
        getVersion: async () => '0.1.0',
        onGameStateUpdate: () => () => {},
        gameAction: async () => {},
        gameChat: async () => {},
        loadDeck: async () => {},
        openFileDialog: async () => null,
        executeScript: async () => {},
        onScriptEvent: () => () => {},
        listInstalledGames: async () => [],
        listAvailableGames: async () => [],
        installGame: async () => {},
        uninstallGame: async () => {},
        onInstallProgress: () => () => {},
        listFeeds: async () => [],
        addFeed: async () => {},
        removeFeed: async () => {},
        setFeedEnabled: async () => {},
      };
    });
    await page.goto('/');

    // Wait for credentials to load
    await expect(page.getByTestId('login-username')).toHaveValue('saved-user', { timeout: 3000 });
    await expect(page.getByTestId('login-password')).toHaveValue('saved-pass');

    // Submit should be enabled
    await expect(page.getByTestId('login-submit')).toBeEnabled();
  });

  test('should toggle remember-me checkbox', async ({ page }) => {
    await injectOctgnMock(page);
    await page.goto('/');

    const checkbox = page.getByTestId('login-remember');
    await expect(checkbox).toBeChecked();

    await checkbox.uncheck();
    await expect(checkbox).not.toBeChecked();

    await checkbox.check();
    await expect(checkbox).toBeChecked();
  });

  test('should complete login flow and navigate away from login page', async ({ page }) => {
    await injectOctgnMock(page);
    await page.goto('/');

    await page.getByTestId('login-username').fill('alice');
    await page.getByTestId('login-password').fill('password');
    await page.getByTestId('login-submit').click();

    // The login animation overlay should appear
    await expect(page.locator('[data-testid="login-overlay"]')).toBeVisible({ timeout: 3000 });

    // After transition completes, verify we navigated away from login.
    // The app store tracks the current page.
    await page.waitForFunction(
      () => (window as any).__appStore?.getState?.()?.currentPage !== 'login',
      { timeout: 10000 },
    );
  });
});
