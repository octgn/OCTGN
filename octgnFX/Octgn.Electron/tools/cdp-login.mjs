/**
 * Trigger login on the Electron app (clicks Sign In if on login page).
 * Usage: node tools/cdp-login.mjs
 */
import { connect } from './cdp-connect.mjs';

const { page, browser } = await connect();

const currentPage = await page.evaluate(() => window.__appStore?.getState()?.currentPage);
console.log('Current page:', currentPage);

if (currentPage === 'login') {
  // Wait for any animation to finish, then click the form submit
  await new Promise(r => setTimeout(r, 1000));

  // Use evaluate to submit the form directly via the auth store
  const result = await page.evaluate(async () => {
    // Get form inputs
    const inputs = document.querySelectorAll('input');
    const username = inputs[0]?.value;
    const password = inputs[1]?.value;

    if (!username) return 'no-username';

    // Submit via the auth store
    const auth = window.__authStore?.getState();
    if (auth?.login) {
      try {
        const result = await auth.login(username, password);
        return JSON.stringify(result);
      } catch (e) {
        return 'error: ' + e.message;
      }
    }
    return 'no-auth-store';
  });

  console.log('Login result:', result);
  await new Promise(r => setTimeout(r, 2000));
}

const after = await page.evaluate(() => ({
  page: window.__appStore?.getState()?.currentPage,
  user: window.__authStore?.getState()?.user?.username,
}));
console.log('After:', JSON.stringify(after));

await browser.close();
