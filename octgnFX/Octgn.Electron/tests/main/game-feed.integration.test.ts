// @vitest-environment node
/**
 * Integration tests for game-feed.ts — hit the real live feeds.
 *
 * These tests require network access and verify that the parser produces
 * the expected set of games from each production feed.
 *
 * Run with: TEST_INTEGRATION=1 npx vitest run tests/main/game-feed.integration.test.ts
 *
 * Skipped in normal `vitest run` unless TEST_INTEGRATION=1 is set.
 */
import { describe, it, expect } from 'vitest';
import { compareVersions, fetchAvailableGames } from '@main/games/game-feed';
import type { GameFeed } from '@main/games/feed-manager';

const RUN_INTEGRATION = process.env.TEST_INTEGRATION === '1';

const octgnOfficial: GameFeed = {
  name: 'OCTGN Official',
  url: 'https://www.myget.org/F/octgngames/',
  enabled: true,
  isBuiltIn: true,
};

const community: GameFeed = {
  name: 'Community Games',
  url: 'https://www.myget.org/F/octgngamedirectory',
  enabled: true,
  isBuiltIn: true,
};

const theSpoils: GameFeed = {
  name: 'The Spoils',
  url: 'https://www.myget.org/F/thespoils/',
  enabled: true,
  isBuiltIn: true,
};

// 30 s per test — real network can be slow
const TIMEOUT = 30_000;

describe.skipIf(!RUN_INTEGRATION)('INTEGRATION — live feed fetches', () => {
  it('OCTGN Official feed contains Chess, Standard Playing Cards, Dominoes, Universal Fighting System, The Spoils', async () => {
    const games = await fetchAvailableGames([octgnOfficial]);
    const names = games.map((g) => g.name);
    console.log('OCTGN Official games:', names);

    expect(names).toContain('Chess');
    expect(names).toContain('Standard Playing Cards');
    expect(names).toContain('Dominoes');
    expect(names).toContain('Universal Fighting System');
    expect(names).toContain('The Spoils');
  }, TIMEOUT);

  it('OCTGN Official — Chess is version 1.0.0.11 or newer', async () => {
    const games = await fetchAvailableGames([octgnOfficial]);
    const chess = games.find((g) => g.name === 'Chess');
    expect(chess).toBeDefined();
    expect(compareVersions(chess!.version, '1.0.0.11')).toBeGreaterThanOrEqual(0);
    console.log('Chess version:', chess?.version);
  }, TIMEOUT);

  it('OCTGN Official — The Spoils is present despite IsLatestVersion never being set', async () => {
    const games = await fetchAvailableGames([octgnOfficial]);
    const spoils = games.find((g) => g.name === 'The Spoils');
    expect(spoils).toBeDefined();
    console.log('The Spoils version:', spoils?.version);
  }, TIMEOUT);

  it('OCTGN Official — all games have non-empty name, id, version, and downloadUrl', async () => {
    const games = await fetchAvailableGames([octgnOfficial]);
    for (const game of games) {
      expect(game.id, `${game.name} id`).toBeTruthy();
      expect(game.name, `${game.id} name`).toBeTruthy();
      expect(game.version, `${game.name} version`).toBeTruthy();
      expect(game.downloadUrl, `${game.name} downloadUrl`).toContain('/package/');
    }
  }, TIMEOUT);

  it('Community Games feed returns games (expect many)', async () => {
    const games = await fetchAvailableGames([community]);
    expect(games.length).toBeGreaterThan(10);
    console.log(`Community Games count: ${games.length}`);
    console.log('First 5:', games.slice(0, 5).map((g) => `${g.name} v${g.version}`));
  }, TIMEOUT);

  it('The Spoils feed returns games', async () => {
    const games = await fetchAvailableGames([theSpoils]);
    expect(games.length).toBeGreaterThan(0);
    console.log('The Spoils feed games:', games.map((g) => `${g.name} v${g.version}`));
  }, TIMEOUT);

  it('all 3 feeds combined — deduplicates, sorted by downloadCount descending', async () => {
    const games = await fetchAvailableGames([octgnOfficial, community, theSpoils]);
    expect(games.length).toBeGreaterThan(10);

    // Sorted descending
    for (let i = 0; i < games.length - 1; i++) {
      expect(games[i].downloadCount ?? 0).toBeGreaterThanOrEqual(games[i + 1].downloadCount ?? 0);
    }

    // Each game ID appears only once (deduplication)
    const ids = games.map((g) => g.id);
    expect(new Set(ids).size).toBe(ids.length);

    console.log(`Total unique games across all feeds: ${games.length}`);
    console.log('Top 10:', games.slice(0, 10).map((g) => `${g.name} (${g.downloadCount ?? 0})`));
  }, TIMEOUT);
});
