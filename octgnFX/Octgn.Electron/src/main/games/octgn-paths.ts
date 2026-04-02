/**
 * Resolves OCTGN data directories following the WPF client's resolution chain:
 *   1. OCTGN_DATA environment variable
 *   2. data.path file next to the OCTGN executable
 *   3. My Documents\Octgn (old default)
 *   4. {OCTGN install}\Data (default)
 *
 * See: Octgn.Library/Providers/DataDirectoryProvider.cs
 */
import { join } from 'path';

/** IO interface for testability (avoids mocking fs/promises). */
export interface OctgnPathsIO {
  exists(path: string): Promise<boolean>;
  readFile(path: string): Promise<string>;
  env(name: string): string | undefined;
  homedir(): string;
  documentsDir(): string;
}

/**
 * Resolve the OCTGN data directory following the WPF client's logic.
 * Returns the data directory path, or null if none found.
 */
export async function resolveOctgnDataDir(io: OctgnPathsIO): Promise<string | null> {
  // 1. OCTGN_DATA environment variable
  const envVal = io.env('OCTGN_DATA');
  if (envVal && await io.exists(envVal)) {
    return envVal;
  }

  // 2. data.path file next to OCTGN executable
  const localAppData = io.env('LOCALAPPDATA') ?? join(io.homedir(), 'AppData', 'Local');
  const octgnInstallPaths = [
    join(localAppData, 'Programs', 'OCTGN'),
    join(localAppData, 'Octgn'),
  ];

  for (const installPath of octgnInstallPaths) {
    const dataPathFile = join(installPath, 'data.path');
    const dataPathExists = await io.exists(dataPathFile);
    if (dataPathExists) {
      try {
        const contents = (await io.readFile(dataPathFile)).trim();
        const contentsExist = await io.exists(contents);
        if (contents && contentsExist) {
          return contents;
        }
      } catch {
        // ignore read errors, continue chain
      }
    }
  }

  // 3. My Documents\Octgn (old default)
  const myDocsOctgn = join(io.documentsDir(), 'Octgn');
  if (await io.exists(myDocsOctgn)) {
    return myDocsOctgn;
  }

  // 4. {OCTGN install}\Data (default)
  for (const installPath of octgnInstallPaths) {
    const dataDir = join(installPath, 'Data');
    if (await io.exists(dataDir)) {
      return dataDir;
    }
  }

  return null;
}

/**
 * Get the user's deck directory: {DataDir}\Decks
 */
export async function resolveUserDecksDir(io: OctgnPathsIO): Promise<string | null> {
  const dataDir = await resolveOctgnDataDir(io);
  if (!dataDir) return null;
  const decksDir = join(dataDir, 'Decks');
  if (await io.exists(decksDir)) return decksDir;
  return null;
}

/**
 * Get the pre-built decks directory for a game: {gameInstallPath}\Decks
 */
export async function resolvePrebuiltDecksDir(
  gameInstallPath: string,
  io: OctgnPathsIO,
): Promise<string | null> {
  const decksDir = join(gameInstallPath, 'Decks');
  if (await io.exists(decksDir)) return decksDir;
  return null;
}
