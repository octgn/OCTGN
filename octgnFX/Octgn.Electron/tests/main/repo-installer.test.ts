// @vitest-environment node
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import AdmZip from 'adm-zip';
import type { InstallProgress } from '../../src/shared/types';

// Mock electron app before importing anything that uses it
vi.mock('electron', () => ({
  app: {
    getPath: (name: string) => {
      if (name === 'userData') return '/mock/userData';
      if (name === 'home') return '/mock/home';
      return '/mock/' + name;
    },
  },
}));

// Mock fs/promises to avoid real file I/O but keep all exports
const { mkdirMock, writeFileMock } = vi.hoisted(() => ({
  mkdirMock: vi.fn().mockResolvedValue(undefined),
  writeFileMock: vi.fn().mockResolvedValue(undefined),
}));

vi.mock('fs/promises', async (importOriginal) => {
  const actual = await importOriginal<typeof import('fs/promises')>();
  return {
    ...actual,
    mkdir: mkdirMock,
    writeFile: writeFileMock,
  };
});

import { installFromRepo } from '../../src/main/games/repo-installer';

const GAME_ID = 'a6c8073d-8079-40f8-9826-9b597b4a6b34';

const DEFINITION_XML = `<?xml version="1.0" encoding="utf-8"?>
<game id="${GAME_ID}" name="Test Game" version="1.0.0">
  <card width="69" height="100" back="back.png" />
  <table name="Table" width="640" height="480" />
</game>`;

function makeZipBuffer(gamePath: string, rootFolder: string, includeDefinition = true): Buffer {
  const zip = new AdmZip();
  if (includeDefinition) {
    zip.addFile(`${rootFolder}/${gamePath}/definition.xml`, Buffer.from(DEFINITION_XML));
  }
  zip.addFile(`${rootFolder}/${gamePath}/sets/cards.xml`, Buffer.from('<sets/>'));
  zip.addFile(`${rootFolder}/src/build.ts`, Buffer.from('// source code - should be ignored'));
  zip.addFile(`${rootFolder}/README.md`, Buffer.from('# Readme - should be ignored'));
  return zip.toBuffer();
}

function mockFetchWithZip(zipBuffer: Buffer) {
  let readCalled = false;
  const mockReader = {
    read: vi.fn().mockImplementation(async () => {
      if (!readCalled) {
        readCalled = true;
        return { done: false, value: new Uint8Array(zipBuffer) };
      }
      return { done: true, value: undefined };
    }),
  };

  return vi.fn().mockResolvedValue({
    ok: true,
    headers: new Map([['content-length', String(zipBuffer.length)]]),
    body: { getReader: () => mockReader },
  });
}

describe('installFromRepo', () => {
  beforeEach(() => {
    mkdirMock.mockClear();
    writeFileMock.mockClear();
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
  });

  it('downloads zip, extracts gamePath files, and returns parsed definition', async () => {
    const zipBuf = makeZipBuffer('dist', 'owner-repo-abc123');
    const fetchSpy = mockFetchWithZip(zipBuf);
    vi.stubGlobal('fetch', fetchSpy);

    const progressCalls: InstallProgress[] = [];
    const result = await installFromRepo('owner', 'repo', 'main', 'dist', GAME_ID, (p) => progressCalls.push({ ...p }));

    expect(result.success).toBe(true);
    expect(result.definition).toBeDefined();
    expect(result.definition!.id).toBe(GAME_ID);
    expect(result.definition!.name).toBe('Test Game');

    // Should have called fetch with the correct GitHub API URL
    expect(fetchSpy).toHaveBeenCalledWith(
      'https://api.github.com/repos/owner/repo/zipball/main',
      expect.objectContaining({
        headers: expect.objectContaining({
          Accept: 'application/vnd.github+json',
        }),
      }),
    );

    // Should have written definition.xml and sets/cards.xml but NOT src/build.ts or README.md
    const writtenPaths = writeFileMock.mock.calls.map((c: unknown[]) => c[0] as string);
    expect(writtenPaths.some((p: string) => p.includes('definition.xml'))).toBe(true);
    expect(writtenPaths.some((p: string) => p.includes('cards.xml'))).toBe(true);
    expect(writtenPaths.some((p: string) => p.includes('build.ts'))).toBe(false);
    expect(writtenPaths.some((p: string) => p.includes('README.md'))).toBe(false);
  });

  it('reports progress through download, extract, parse, and done phases', async () => {
    const zipBuf = makeZipBuffer('dist', 'owner-repo-abc123');
    const fetchSpy = mockFetchWithZip(zipBuf);
    vi.stubGlobal('fetch', fetchSpy);

    const progressCalls: InstallProgress[] = [];
    await installFromRepo('owner', 'repo', 'main', 'dist', GAME_ID, (p) => progressCalls.push({ ...p }));

    const phases = progressCalls.map((p) => p.phase);
    expect(phases).toContain('downloading');
    expect(phases).toContain('extracting');
    expect(phases).toContain('parsing');
    expect(phases).toContain('done');

    // done should be at 100%
    const doneProgress = progressCalls.find((p) => p.phase === 'done');
    expect(doneProgress?.percent).toBe(100);
  });

  it('handles network errors gracefully', async () => {
    const fetchSpy = vi.fn().mockRejectedValue(new Error('Network failure'));
    vi.stubGlobal('fetch', fetchSpy);

    const progressCalls: InstallProgress[] = [];
    const result = await installFromRepo('owner', 'repo', 'main', 'dist', GAME_ID, (p) => progressCalls.push({ ...p }));

    expect(result.success).toBe(false);
    expect(result.error).toContain('Network failure');

    const errorProgress = progressCalls.find((p) => p.phase === 'error');
    expect(errorProgress).toBeDefined();
  });

  it('handles HTTP error status', async () => {
    const fetchSpy = vi.fn().mockResolvedValue({
      ok: false,
      status: 404,
      statusText: 'Not Found',
    });
    vi.stubGlobal('fetch', fetchSpy);

    const progressCalls: InstallProgress[] = [];
    const result = await installFromRepo('owner', 'repo', 'main', 'dist', GAME_ID, (p) => progressCalls.push({ ...p }));

    expect(result.success).toBe(false);
    expect(result.error).toContain('404');
  });

  it('returns error when gamePath not found in zip', async () => {
    const zip = new AdmZip();
    zip.addFile('owner-repo-abc123/other-folder/file.txt', Buffer.from('data'));
    const zipBuf = zip.toBuffer();

    const fetchSpy = mockFetchWithZip(zipBuf);
    vi.stubGlobal('fetch', fetchSpy);

    const progressCalls: InstallProgress[] = [];
    const result = await installFromRepo('owner', 'repo', 'main', 'dist', GAME_ID, (p) => progressCalls.push({ ...p }));

    expect(result.success).toBe(false);
    expect(result.error).toContain('gamePath');
  });

  it('returns error when definition.xml is missing', async () => {
    const zipBuf = makeZipBuffer('dist', 'owner-repo-abc123', false);
    const fetchSpy = mockFetchWithZip(zipBuf);
    vi.stubGlobal('fetch', fetchSpy);

    const progressCalls: InstallProgress[] = [];
    const result = await installFromRepo('owner', 'repo', 'main', 'dist', GAME_ID, (p) => progressCalls.push({ ...p }));

    expect(result.success).toBe(false);
    expect(result.error).toContain('definition.xml');
  });

  it('dynamically detects the root folder name regardless of format', async () => {
    // GitHub uses various formats: owner-repo-shortsha, sometimes different lengths
    const zipBuf = makeZipBuffer('game-data', 'myorg-mygame-f3a9b2c1de');
    const fetchSpy = mockFetchWithZip(zipBuf);
    vi.stubGlobal('fetch', fetchSpy);

    const progressCalls: InstallProgress[] = [];
    const result = await installFromRepo('myorg', 'mygame', 'main', 'game-data', GAME_ID, (p) => progressCalls.push({ ...p }));

    expect(result.success).toBe(true);
    expect(result.definition).toBeDefined();
    expect(result.definition!.id).toBe(GAME_ID);
  });

  it('handles nested gamePath correctly', async () => {
    const zipBuf = makeZipBuffer('GameDatabase/some-id', 'owner-repo-abc123');
    const fetchSpy = mockFetchWithZip(zipBuf);
    vi.stubGlobal('fetch', fetchSpy);

    const progressCalls: InstallProgress[] = [];
    const result = await installFromRepo('owner', 'repo', 'main', 'GameDatabase/some-id', GAME_ID, (p) => progressCalls.push({ ...p }));

    expect(result.success).toBe(true);
    expect(result.definition).toBeDefined();
  });

  it('handles no response body', async () => {
    const fetchSpy = vi.fn().mockResolvedValue({
      ok: true,
      headers: new Map([['content-length', '100']]),
      body: null,
    });
    vi.stubGlobal('fetch', fetchSpy);

    const progressCalls: InstallProgress[] = [];
    const result = await installFromRepo('owner', 'repo', 'main', 'dist', GAME_ID, (p) => progressCalls.push({ ...p }));

    expect(result.success).toBe(false);
    expect(result.error).toContain('body');
  });
});
