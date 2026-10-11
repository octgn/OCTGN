/**
 * Dump current app state: page, user, lobby games, game state summary.
 * Usage: node tools/cdp-state.mjs
 */
import { connect } from './cdp-connect.mjs';

const { page, browser } = await connect();

const state = await page.evaluate(() => {
  const app = window.__appStore?.getState();
  const auth = window.__authStore?.getState();
  const lobby = window.__lobbyStore?.getState();
  const game = window.__gameStore?.getState();
  return {
    currentPage: app?.currentPage,
    user: auth?.user ? { username: auth.user.username, id: auth.user.id } : null,
    lobbyGames: lobby?.games?.map(g => ({
      id: g.id,
      name: g.name,
      gameName: g.gameName,
      host: g.host,
      status: g.status,
      spectators: g.spectators,
      gameId: g.gameId,
    })) ?? [],
    gameState: game?.gameState ? {
      gameId: game.gameState.gameId,
      gameName: game.gameState.gameName,
      tableCards: game.gameState.table?.cards?.length ?? 0,
      players: game.gameState.players?.map(p => ({
        name: p.name,
        groupCount: p.groups?.length ?? 0,
        totalCards: p.groups?.reduce((sum, g) => sum + (g.cards?.length ?? 0), 0) ?? 0,
      })),
      cardSize: game.gameState.cardSize,
      boardUrl: game.gameState.table?.board?.imageUrl,
      backgroundUrl: game.gameState.table?.backgroundUrl,
    } : null,
  };
});

console.log(JSON.stringify(state, null, 2));
await browser.close();
