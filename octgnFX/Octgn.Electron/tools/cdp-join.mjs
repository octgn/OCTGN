/**
 * Join a lobby game as spectator (or player).
 * Usage:
 *   node tools/cdp-join.mjs <game-id> [--spectate]
 *   node tools/cdp-join.mjs <game-name-substring> [--spectate]
 *   node tools/cdp-join.mjs                        # lists available games
 */
import { connect } from './cdp-connect.mjs';

const arg = process.argv[2];
const spectate = process.argv.includes('--spectate');

const { page, browser } = await connect();

// Get lobby games
const games = await page.evaluate(() => {
  const lobby = window.__lobbyStore?.getState();
  return lobby?.games?.map(g => ({
    id: g.id,
    name: g.name,
    gameName: g.gameName,
    host: g.host,
    spectators: g.spectators,
  })) ?? [];
});

if (!arg) {
  console.log('Available games:');
  for (const g of games) {
    console.log(`  ${g.id}  "${g.name}" (${g.gameName}) host=${g.host} spectate=${g.spectators}`);
  }
  await browser.close();
  process.exit(0);
}

// Find game by ID, room name, or game type name
const larg = arg.toLowerCase();
const game = games.find(g => g.id === arg)
  || games.find(g => g.name.toLowerCase().includes(larg))
  || games.find(g => g.gameName.toLowerCase().includes(larg));
if (!game) {
  console.error(`No game found matching "${arg}"`);
  await browser.close();
  process.exit(1);
}

console.log(`Joining "${game.name}" (${game.gameName}) as ${spectate ? 'spectator' : 'player'}...`);

const result = await page.evaluate(async ({ id, spectate }) => {
  try {
    await window.__lobbyStore.getState().joinGame(id, undefined, spectate);
    return 'ok';
  } catch (e) {
    return 'error: ' + e.message;
  }
}, { id: game.id, spectate });

console.log('Result:', result);

// Wait for game state
if (result === 'ok') {
  for (let i = 0; i < 15; i++) {
    await new Promise(r => setTimeout(r, 1000));
    const s = await page.evaluate(() => {
      const g = window.__gameStore?.getState();
      return {
        page: window.__appStore?.getState()?.currentPage,
        hasState: !!g?.gameState,
        tableCards: g?.gameState?.table?.cards?.length ?? 0,
      };
    });
    process.stdout.write(`\r  [${i+1}s] page=${s.page} cards=${s.tableCards}`);
    if (s.hasState && s.tableCards > 0) break;
  }
  console.log();
}

await browser.close();
