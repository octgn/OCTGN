/**
 * Watch a live game for state changes, logging diffs and taking screenshots.
 * Must already be on the game page (use cdp-join.mjs first).
 * Usage: node tools/cdp-watch.mjs [duration-seconds] [--no-screenshots]
 * Default duration: 120 seconds
 */
import { connect } from './cdp-connect.mjs';

const duration = parseInt(process.argv[2] ?? '120', 10);
const takeScreenshots = !process.argv.includes('--no-screenshots');

console.log(`Watching for ${duration}s (screenshots: ${takeScreenshots ? 'on' : 'off'})...`);

const { page, browser } = await connect();

const currentPage = await page.evaluate(() => window.__appStore?.getState()?.currentPage);
if (currentPage !== 'game') {
  console.error(`Not on game page (current: ${currentPage}). Join a game first with cdp-join.mjs.`);
  await browser.close();
  process.exit(1);
}

let lastHash = '';
let screenshotN = 0;
const start = Date.now();

while (Date.now() - start < duration * 1000) {
  const gs = await page.evaluate(() => {
    const s = window.__gameStore?.getState()?.gameState;
    if (!s) return null;
    return {
      gameName: s.gameName,
      turnNumber: s.turnNumber,
      activePlayer: s.activePlayer,
      isStarted: s.isStarted,
      connectionStatus: s.connectionStatus,
      players: s.players?.map(p => ({
        name: p.name,
        groups: p.groups?.map(g => ({ name: g.name, cards: g.cards?.length ?? 0 })),
        counters: p.counters?.map(c => ({ name: c.name, value: c.value })),
      })),
      tableCards: s.table?.cards?.length ?? 0,
      chatCount: s.chatMessages?.length ?? 0,
      lastChat: s.chatMessages?.slice(-1)?.[0],
    };
  });

  if (!gs) {
    console.log(`[${Math.round((Date.now() - start) / 1000)}s] No game state`);
    await page.waitForTimeout(2000);
    continue;
  }

  const hash = JSON.stringify({
    turn: gs.turnNumber,
    active: gs.activePlayer,
    table: gs.tableCards,
    chat: gs.chatCount,
    groups: gs.players?.map(p => p.groups?.map(g => g.cards)),
    counters: gs.players?.map(p => p.counters?.map(c => c.value)),
  });

  if (hash !== lastHash) {
    const elapsed = Math.round((Date.now() - start) / 1000);
    console.log(`\n[${elapsed}s] === STATE CHANGE ===`);
    console.log(`  "${gs.gameName}" turn=${gs.turnNumber} active=${gs.activePlayer} table=${gs.tableCards} chat=${gs.chatCount}`);

    for (const p of gs.players ?? []) {
      const groups = p.groups?.filter(g => g.cards > 0).map(g => `${g.name}(${g.cards})`).join(', ') || 'empty';
      const counters = p.counters?.filter(c => c.value !== 0).map(c => `${c.name}=${c.value}`).join(', ') || '';
      console.log(`  ${p.name}: [${groups}] ${counters}`);
    }

    if (gs.lastChat) {
      console.log(`  Chat: ${gs.lastChat.playerName}: ${gs.lastChat.message}`);
    }

    lastHash = hash;

    if (takeScreenshots) {
      screenshotN++;
      const file = `watch-${elapsed}s.png`;
      await page.screenshot({ path: file });
      console.log(`  Screenshot: ${file}`);
    }
  }

  await page.waitForTimeout(1500);
}

console.log(`\nDone. ${screenshotN} screenshots taken.`);
await browser.close();
