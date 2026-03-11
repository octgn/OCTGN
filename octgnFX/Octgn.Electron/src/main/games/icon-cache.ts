/**
 * Caches game icon images locally under userData/game-icons/.
 * Returns file:// URLs for already-cached icons; fires background downloads
 * for anything not yet cached so next refresh shows the local version.
 */
import path from 'path';
import { existsSync, mkdirSync } from 'fs';
import { writeFile } from 'fs/promises';
import { app } from 'electron';
import type { AvailableGame } from '../../shared/types';

function getCacheDir(): string {
  return path.join(app.getPath('userData'), 'game-icons');
}

function cachePathFor(gameId: string, iconUrl: string): string {
  let ext = '.png';
  try {
    const p = new URL(iconUrl).pathname;
    const e = path.extname(p);
    if (e) ext = e;
  } catch { /* use default */ }
  return path.join(getCacheDir(), `${gameId}${ext}`);
}

/** If the icon for this game is already cached locally, return a file:// URL. Otherwise null. */
export function cachedIconUrl(gameId: string, iconUrl: string): string | null {
  if (!iconUrl) return null;
  const p = cachePathFor(gameId, iconUrl);
  if (existsSync(p)) return `file:///${p.replace(/\\/g, '/')}`;
  return null;
}

/** Download and cache a single icon (best-effort, silently ignores errors). */
async function downloadIcon(gameId: string, iconUrl: string): Promise<void> {
  const dir = getCacheDir();
  if (!existsSync(dir)) mkdirSync(dir, { recursive: true });
  const cachePath = cachePathFor(gameId, iconUrl);
  if (existsSync(cachePath)) return;
  try {
    const resp = await fetch(iconUrl, { signal: AbortSignal.timeout(15_000) });
    if (!resp.ok) return;
    const buf = Buffer.from(await resp.arrayBuffer());
    await writeFile(cachePath, buf);
  } catch { /* ignore network errors */ }
}

/**
 * For games that have an iconUrl, swaps in the cached file:// path if available.
 * Then fires background downloads for any icons not yet cached.
 * Returns the updated game list immediately (no network wait).
 */
export function resolveAndCacheIcons(games: AvailableGame[]): AvailableGame[] {
  const resolved = games.map((game) => {
    if (!game.iconUrl) return game;
    const local = cachedIconUrl(game.id, game.iconUrl);
    return local ? { ...game, iconUrl: local } : game;
  });

  // Background: download anything not yet cached
  for (const game of games) {
    if (game.iconUrl && !game.iconUrl.startsWith('file://')) {
      downloadIcon(game.id, game.iconUrl).catch(() => {});
    }
  }

  return resolved;
}
