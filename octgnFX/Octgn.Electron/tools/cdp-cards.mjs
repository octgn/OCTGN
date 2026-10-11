/**
 * Dump all cards in the current game (table + player groups).
 * Usage: node tools/cdp-cards.mjs
 */
import { connect } from './cdp-connect.mjs';

const { page, browser } = await connect();

const cards = await page.evaluate(() => {
  const game = window.__gameStore?.getState();
  if (!game?.gameState) return null;
  const gs = game.gameState;

  const result = {
    gameId: gs.gameId,
    gameName: gs.gameName,
    tableCards: gs.table.cards.map(c => ({
      id: c.id,
      definitionId: c.definitionId,
      name: c.name,
      imageUrl: c.imageUrl,
      faceUp: c.faceUp,
      position: c.position,
      size: c.size,
      rotation: c.rotation,
      markers: c.markers,
      properties: c.properties,
    })),
    players: gs.players.map(p => ({
      id: p.id,
      name: p.name,
      groups: p.groups.map(g => ({
        id: g.id,
        name: g.name,
        cards: g.cards.map(c => ({
          id: c.id,
          definitionId: c.definitionId,
          name: c.name,
          imageUrl: c.imageUrl,
          faceUp: c.faceUp,
        })),
      })),
    })),
  };
  return result;
});

if (!cards) {
  console.log('No active game state');
} else {
  console.log(JSON.stringify(cards, null, 2));
}
await browser.close();
