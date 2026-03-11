/**
 * Check which card images are loading/failing in the game view.
 * Inspects all <img> elements in the game board and reports their status.
 * Usage: node tools/cdp-check-images.mjs
 */
import { connect } from './cdp-connect.mjs';

const { page, browser } = await connect();

const imageStatus = await page.evaluate(() => {
  const imgs = document.querySelectorAll('img');
  return Array.from(imgs).map(img => ({
    src: img.src,
    naturalWidth: img.naturalWidth,
    naturalHeight: img.naturalHeight,
    complete: img.complete,
    displayed: img.offsetWidth > 0 && img.offsetHeight > 0,
    alt: img.alt,
    className: img.className?.substring(0, 80),
  }));
});

console.log(`Found ${imageStatus.length} <img> elements:\n`);
for (const img of imageStatus) {
  const status = img.naturalWidth > 0 ? 'OK' : 'FAILED';
  console.log(`[${status}] ${img.src}`);
  console.log(`       natural=${img.naturalWidth}x${img.naturalHeight} complete=${img.complete} displayed=${img.displayed}`);
  if (img.alt) console.log(`       alt="${img.alt}"`);
  console.log();
}

// Also check for network errors related to octgn-asset
const networkInfo = await page.evaluate(() => {
  // Check if there are any console errors we can detect
  const game = window.__gameStore?.getState();
  if (!game?.gameState) return { error: 'No game state' };

  const allCards = [
    ...game.gameState.table.cards,
    ...game.gameState.players.flatMap(p => p.groups.flatMap(g => g.cards || [])),
  ];

  return {
    totalCards: allCards.length,
    withImageUrl: allCards.filter(c => c.imageUrl).length,
    withoutImageUrl: allCards.filter(c => !c.imageUrl).length,
    uniqueImageUrls: [...new Set(allCards.filter(c => c.imageUrl).map(c => c.imageUrl))],
    cardsWithoutImages: allCards.filter(c => !c.imageUrl).map(c => ({
      id: c.id,
      name: c.name,
      definitionId: c.definitionId,
    })),
  };
});

console.log('\n--- Card Image Summary ---');
console.log(JSON.stringify(networkInfo, null, 2));

await browser.close();
