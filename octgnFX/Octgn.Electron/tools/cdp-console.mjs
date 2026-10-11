/**
 * Evaluate arbitrary JS in the Electron renderer.
 * Usage: node tools/cdp-console.mjs "expression"
 * Example: node tools/cdp-console.mjs "window.__gameStore.getState().gameState.table.cards.length"
 */
import { connect } from './cdp-connect.mjs';

const expr = process.argv[2];
if (!expr) {
  console.error('Usage: node tools/cdp-console.mjs "expression"');
  process.exit(1);
}

const { page, browser } = await connect();
try {
  const result = await page.evaluate(expr);
  console.log(typeof result === 'object' ? JSON.stringify(result, null, 2) : result);
} catch (e) {
  console.error('Error:', e.message);
}
await browser.close();
