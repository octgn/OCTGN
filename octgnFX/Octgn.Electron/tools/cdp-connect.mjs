/**
 * Shared CDP connection helper.
 * Usage:
 *   import { connect } from './cdp-connect.mjs';
 *   const { page, browser } = await connect();
 *   // ... do stuff
 *   await browser.close();
 */
import { chromium } from 'playwright';

export async function connect(port = 9222) {
  const browser = await chromium.connectOverCDP(`http://127.0.0.1:${port}`);
  const pages = browser.contexts()[0].pages();
  const page = pages.find(p => p.url().includes('5173'));
  if (!page) {
    const urls = pages.map(p => p.url());
    await browser.close();
    throw new Error(`No OCTGN renderer page found. Pages: ${urls.join(', ')}`);
  }
  return { page, browser };
}

export async function getStores(page) {
  return page.evaluate(() => ({
    app: window.__appStore?.getState(),
    auth: window.__authStore?.getState(),
    lobby: window.__lobbyStore?.getState(),
    game: window.__gameStore?.getState(),
  }));
}
