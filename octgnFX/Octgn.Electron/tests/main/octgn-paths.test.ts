import { describe, it, expect } from 'vitest';
import { join } from 'path';
import {
  resolveOctgnDataDir,
  resolveUserDecksDir,
  resolvePrebuiltDecksDir,
  type OctgnPathsIO,
} from '@main/games/octgn-paths';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/** Normalize path separators for cross-platform comparison. */
function norm(p: string): string {
  return p.replace(/\\/g, '/');
}

/** Create a mock IO with a set of existing paths and file contents. */
function mockIO(opts: {
  existingPaths?: string[];
  files?: Record<string, string>;
  env?: Record<string, string>;
  homedir?: string;
  documentsDir?: string;
} = {}): OctgnPathsIO {
  const existing = new Set((opts.existingPaths ?? []).map(norm));
  const files = Object.fromEntries(
    Object.entries(opts.files ?? {}).map(([k, v]) => [norm(k), v]),
  );
  return {
    exists: async (path: string) => existing.has(norm(path)),
    readFile: async (path: string) => {
      const n = norm(path);
      if (n in files) return files[n];
      throw new Error(`File not found: ${path}`);
    },
    env: (name: string) => opts.env?.[name],
    homedir: () => opts.homedir ?? 'C:/Users/test',
    documentsDir: () => opts.documentsDir ?? 'C:/Users/test/Documents',
  };
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('resolveOctgnDataDir', () => {
  it('priority 1: uses OCTGN_DATA environment variable when set and exists', async () => {
    const io = mockIO({
      env: { OCTGN_DATA: 'D:/OctgnData' },
      existingPaths: ['D:/OctgnData'],
    });

    const result = await resolveOctgnDataDir(io);
    expect(norm(result!)).toBe('D:/OctgnData');
  });

  it('priority 1: skips OCTGN_DATA when directory does not exist', async () => {
    const io = mockIO({
      env: {
        OCTGN_DATA: 'D:/NonExistent',
        LOCALAPPDATA: 'C:/Users/test/AppData/Local',
      },
      existingPaths: ['C:/Users/test/Documents/Octgn'],
      documentsDir: 'C:/Users/test/Documents',
    });

    const result = await resolveOctgnDataDir(io);
    // Should skip env var and fall through to My Documents
    expect(norm(result!)).toBe('C:/Users/test/Documents/Octgn');
  });

  it('priority 2: reads data.path file from Programs/OCTGN install path', async () => {
    const io = mockIO({
      env: { LOCALAPPDATA: 'C:/Users/test/AppData/Local' },
      existingPaths: [
        'C:/Users/test/AppData/Local/Programs/OCTGN/data.path',
        'C:/Users/test/AppData/Local/Programs/OCTGN/Data',
      ],
      files: {
        'C:/Users/test/AppData/Local/Programs/OCTGN/data.path':
          'C:/Users/test/AppData/Local/Programs/OCTGN/Data',
      },
    });

    const result = await resolveOctgnDataDir(io);
    expect(norm(result!)).toBe('C:/Users/test/AppData/Local/Programs/OCTGN/Data');
  });

  it('priority 2: reads data.path file from legacy Octgn install path', async () => {
    const io = mockIO({
      env: { LOCALAPPDATA: 'C:/Users/test/AppData/Local' },
      existingPaths: [
        'C:/Users/test/AppData/Local/Octgn/data.path',
        'D:/CustomPath',
      ],
      files: {
        'C:/Users/test/AppData/Local/Octgn/data.path': 'D:/CustomPath\n',
      },
    });

    const result = await resolveOctgnDataDir(io);
    expect(norm(result!)).toBe('D:/CustomPath');
  });

  it('priority 2: skips data.path when the path it points to does not exist', async () => {
    const io = mockIO({
      env: { LOCALAPPDATA: 'C:/Users/test/AppData/Local' },
      existingPaths: [
        'C:/Users/test/AppData/Local/Programs/OCTGN/data.path',
        'C:/Users/test/Documents/Octgn',
      ],
      files: {
        'C:/Users/test/AppData/Local/Programs/OCTGN/data.path': 'D:/NonExistent',
      },
      documentsDir: 'C:/Users/test/Documents',
    });

    const result = await resolveOctgnDataDir(io);
    expect(norm(result!)).toBe('C:/Users/test/Documents/Octgn');
  });

  it('priority 3: falls back to My Documents/Octgn', async () => {
    const io = mockIO({
      env: { LOCALAPPDATA: 'C:/Users/test/AppData/Local' },
      existingPaths: ['C:/Users/test/Documents/Octgn'],
      documentsDir: 'C:/Users/test/Documents',
    });

    const result = await resolveOctgnDataDir(io);
    expect(norm(result!)).toBe('C:/Users/test/Documents/Octgn');
  });

  it('priority 4: falls back to {install}/Data directory', async () => {
    const io = mockIO({
      env: { LOCALAPPDATA: 'C:/Users/test/AppData/Local' },
      existingPaths: ['C:/Users/test/AppData/Local/Programs/OCTGN/Data'],
      documentsDir: 'C:/Users/test/Documents',
    });

    const result = await resolveOctgnDataDir(io);
    expect(norm(result!)).toBe('C:/Users/test/AppData/Local/Programs/OCTGN/Data');
  });

  it('priority 4: uses legacy Octgn/Data path when Programs path missing', async () => {
    const io = mockIO({
      env: { LOCALAPPDATA: 'C:/Users/test/AppData/Local' },
      existingPaths: ['C:/Users/test/AppData/Local/Octgn/Data'],
      documentsDir: 'C:/Users/test/Documents',
    });

    const result = await resolveOctgnDataDir(io);
    expect(norm(result!)).toBe('C:/Users/test/AppData/Local/Octgn/Data');
  });

  it('returns null when no OCTGN data directory found', async () => {
    const io = mockIO({
      env: { LOCALAPPDATA: 'C:/Users/test/AppData/Local' },
      documentsDir: 'C:/Users/test/Documents',
    });

    const result = await resolveOctgnDataDir(io);
    expect(result).toBeNull();
  });

  it('derives LOCALAPPDATA from homedir when env var not set', async () => {
    const io = mockIO({
      existingPaths: ['C:/Users/test/AppData/Local/Programs/OCTGN/Data'],
      homedir: 'C:/Users/test',
      documentsDir: 'C:/Users/test/Documents',
    });

    const result = await resolveOctgnDataDir(io);
    expect(norm(result!)).toBe('C:/Users/test/AppData/Local/Programs/OCTGN/Data');
  });
});

describe('resolveUserDecksDir', () => {
  it('returns {dataDir}/Decks when it exists', async () => {
    const io = mockIO({
      env: { LOCALAPPDATA: 'C:/Users/test/AppData/Local' },
      existingPaths: [
        'C:/Users/test/AppData/Local/Programs/OCTGN/Data',
        'C:/Users/test/AppData/Local/Programs/OCTGN/data.path',
        'C:/Users/test/AppData/Local/Programs/OCTGN/Data/Decks',
      ],
      files: {
        'C:/Users/test/AppData/Local/Programs/OCTGN/data.path':
          'C:/Users/test/AppData/Local/Programs/OCTGN/Data',
      },
    });

    const result = await resolveUserDecksDir(io);
    expect(norm(result!)).toBe('C:/Users/test/AppData/Local/Programs/OCTGN/Data/Decks');
  });

  it('returns null when data dir has no Decks subfolder', async () => {
    const io = mockIO({
      env: { LOCALAPPDATA: 'C:/Users/test/AppData/Local' },
      existingPaths: [
        'C:/Users/test/AppData/Local/Programs/OCTGN/Data',
        'C:/Users/test/AppData/Local/Programs/OCTGN/data.path',
      ],
      files: {
        'C:/Users/test/AppData/Local/Programs/OCTGN/data.path':
          'C:/Users/test/AppData/Local/Programs/OCTGN/Data',
      },
    });

    const result = await resolveUserDecksDir(io);
    expect(result).toBeNull();
  });

  it('returns null when no OCTGN data directory found', async () => {
    const io = mockIO();
    const result = await resolveUserDecksDir(io);
    expect(result).toBeNull();
  });
});

describe('resolvePrebuiltDecksDir', () => {
  it('returns {gameInstallPath}/Decks when it exists', async () => {
    const io = mockIO({
      existingPaths: ['C:/Games/Chess/Decks'],
    });

    const result = await resolvePrebuiltDecksDir('C:/Games/Chess', io);
    expect(norm(result!)).toBe('C:/Games/Chess/Decks');
  });

  it('returns null when game has no Decks folder', async () => {
    const io = mockIO();
    const result = await resolvePrebuiltDecksDir('C:/Games/Chess', io);
    expect(result).toBeNull();
  });
});
