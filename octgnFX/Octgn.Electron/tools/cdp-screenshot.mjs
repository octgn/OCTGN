/**
 * Take a screenshot of the Electron app.
 * Usage: node tools/cdp-screenshot.mjs [filename]
 * Default filename: electron-screenshot.png
 */
import { connect } from './cdp-connect.mjs';

const filename = process.argv[2] || 'electron-screenshot.png';
const { page, browser } = await connect();
await page.screenshot({ path: filename });
console.log(`Screenshot saved to ${filename}`);
await browser.close();
