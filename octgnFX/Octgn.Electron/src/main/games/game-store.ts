/**
 * Main-process game store: discovers installed game definitions from:
 * 1. %LOCALAPPDATA%\Octgn\GameDatabase\ (existing OCTGN install)
 * 2. <userData>/games/                  (games installed by this app)
 */
import { app } from 'electron';
import { readdir, readFile, access } from 'fs/promises';
import { join } from 'path';
import { constants } from 'fs';
import { parseDefinitionXml } from './definition-parser';
import { resolveUserDecksDir, resolvePrebuiltDecksDir, type OctgnPathsIO } from './octgn-paths';
import type { GameDefinition } from '../../shared/types';

/** Possible GameDatabase paths in priority order (newest install path first). */
function getOctgnGameDatabasePaths(): string[] {
  const localAppData = process.env.LOCALAPPDATA ?? join(app.getPath('home'), 'AppData', 'Local');
  return [
    join(localAppData, 'Programs', 'OCTGN', 'Data', 'GameDatabase'),
    join(localAppData, 'Octgn', 'GameDatabase'),
  ];
}

async function getOctgnGameDatabasePath(): Promise<string | null> {
  for (const p of getOctgnGameDatabasePaths()) {
    if (await exists(p)) return p;
  }
  return null;
}

function getOurGamesPath(): string {
  return join(app.getPath('userData'), 'games');
}

async function exists(p: string): Promise<boolean> {
  try {
    await access(p, constants.F_OK);
    return true;
  } catch {
    return false;
  }
}

async function loadGameFromDir(dir: string): Promise<GameDefinition | null> {
  const xmlPath = join(dir, 'definition.xml');
  if (!(await exists(xmlPath))) return null;
  try {
    const xml = await readFile(xmlPath);
    return parseDefinitionXml(xml);
  } catch {
    return null;
  }
}

async function scanDirectory(baseDir: string): Promise<GameDefinition[]> {
  if (!(await exists(baseDir))) return [];
  let entries: string[];
  try {
    entries = await readdir(baseDir);
  } catch {
    return [];
  }
  const results: GameDefinition[] = [];
  for (const entry of entries) {
    const def = await loadGameFromDir(join(baseDir, entry));
    if (def) results.push(def);
  }
  return results;
}

export async function listInstalledGames(): Promise<GameDefinition[]> {
  const legacyPath = await getOctgnGameDatabasePath();
  const [legacy, ours] = await Promise.all([
    legacyPath ? scanDirectory(legacyPath) : Promise.resolve([]),
    scanDirectory(getOurGamesPath()),
  ]);

  // Deduplicate by id, our install wins over legacy
  const map = new Map<string, GameDefinition>();
  for (const g of legacy) map.set(g.id, g);
  for (const g of ours) map.set(g.id, g);
  return [...map.values()];
}

export async function uninstallGame(gameId: string): Promise<{ success: boolean; error?: string }> {
  const gameDir = join(getOurGamesPath(), gameId);
  if (!(await exists(gameDir))) {
    return { success: false, error: 'Game not found in managed install directory' };
  }
  const { rm } = await import('fs/promises');
  try {
    await rm(gameDir, { recursive: true, force: true });
    return { success: true };
  } catch (err) {
    return { success: false, error: String(err) };
  }
}

export function getInstallDir(gameId: string): string {
  return join(getOurGamesPath(), gameId);
}

/**
 * Find the directory for a specific game by ID.
 * Checks our managed install dir first, then legacy OCTGN paths.
 */
export async function findGameDir(gameId: string): Promise<string | null> {
  // Check our install dir first
  const ourDir = join(getOurGamesPath(), gameId);
  if (await exists(ourDir)) return ourDir;

  // Check legacy OCTGN paths
  const legacyBase = await getOctgnGameDatabasePath();
  if (legacyBase) {
    const legacyDir = join(legacyBase, gameId);
    if (await exists(legacyDir)) return legacyDir;
  }
  return null;
}

/** Default IO implementation for octgn-paths (uses real fs + env). */
function defaultIO(): OctgnPathsIO {
  return {
    exists,
    readFile: async (path: string) => (await readFile(path, 'utf-8')),
    env: (name: string) => process.env[name],
    homedir: () => app.getPath('home'),
    documentsDir: () => app.getPath('documents'),
  };
}

/**
 * Find the user's deck directory following the WPF client's resolution chain:
 * OCTGN_DATA env → data.path file → My Documents\Octgn → {install}\Data
 * Then returns {dataDir}\Decks.
 */
export async function getUserDecksDir(): Promise<string | null> {
  return resolveUserDecksDir(defaultIO());
}

/**
 * Find the pre-built decks directory for a specific game: {gameInstallPath}\Decks
 */
export async function getPrebuiltDecksDir(gameId: string): Promise<string | null> {
  const gameDir = await findGameDir(gameId);
  if (!gameDir) return null;
  return resolvePrebuiltDecksDir(gameDir, defaultIO());
}
