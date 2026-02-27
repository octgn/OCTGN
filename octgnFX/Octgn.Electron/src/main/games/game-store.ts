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
import type { GameDefinition } from '../../shared/types';

function getOctgnGameDatabasePath(): string {
  // %LOCALAPPDATA%\Octgn\GameDatabase on Windows
  const localAppData = process.env.LOCALAPPDATA ?? join(app.getPath('home'), 'AppData', 'Local');
  return join(localAppData, 'Octgn', 'GameDatabase');
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
  const [legacy, ours] = await Promise.all([
    scanDirectory(getOctgnGameDatabasePath()),
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
