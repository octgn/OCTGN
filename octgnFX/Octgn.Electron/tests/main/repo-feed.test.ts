import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { fetchManifest, fetchFeedIndex, normalizeRepoUrl } from '../../src/main/games/repo-feed';

describe('normalizeRepoUrl', () => {
  it('parses "owner/repo" format', () => {
    const result = normalizeRepoUrl('octocat/hello-world');
    expect(result).toEqual({ owner: 'octocat', repo: 'hello-world' });
  });

  it('parses "https://github.com/owner/repo" format', () => {
    const result = normalizeRepoUrl('https://github.com/octocat/hello-world');
    expect(result).toEqual({ owner: 'octocat', repo: 'hello-world' });
  });

  it('parses "https://github.com/owner/repo.git" format', () => {
    const result = normalizeRepoUrl('https://github.com/octocat/hello-world.git');
    expect(result).toEqual({ owner: 'octocat', repo: 'hello-world' });
  });

  it('handles trailing slash', () => {
    const result = normalizeRepoUrl('https://github.com/octocat/hello-world/');
    expect(result).toEqual({ owner: 'octocat', repo: 'hello-world' });
  });

  it('throws on invalid input', () => {
    expect(() => normalizeRepoUrl('just-a-name')).toThrow();
  });
});

const validManifest = {
  guid: 'abc-123',
  name: 'Test Game',
  version: '1.0.0',
  description: 'A test game',
  authors: ['Alice', 'Bob'],
  gamePath: 'GameDatabase/abc-123',
  tags: ['card', 'strategy'],
};

describe('fetchManifest', () => {
  let fetchSpy: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    fetchSpy = vi.fn();
    vi.stubGlobal('fetch', fetchSpy);
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
  });

  it('fetches and returns a valid manifest', async () => {
    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(validManifest),
    });

    const result = await fetchManifest('octocat', 'game-repo', 'main');

    expect(fetchSpy).toHaveBeenCalledWith(
      'https://raw.githubusercontent.com/octocat/game-repo/main/octgn-manifest.json',
      expect.objectContaining({ signal: expect.any(AbortSignal) }),
    );
    expect(result.manifest).toEqual(validManifest);
    expect(result.error).toBeUndefined();
  });

  it('uses custom manifestPath when provided', async () => {
    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(validManifest),
    });

    await fetchManifest('octocat', 'game-repo', 'main', 'custom/path.json');

    expect(fetchSpy).toHaveBeenCalledWith(
      'https://raw.githubusercontent.com/octocat/game-repo/main/custom/path.json',
      expect.anything(),
    );
  });

  it('returns error when required fields are missing', async () => {
    const incomplete = { guid: 'abc', name: 'Test' }; // missing version, gamePath
    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(incomplete),
    });

    const result = await fetchManifest('octocat', 'game-repo', 'main');

    expect(result.manifest).toBeUndefined();
    expect(result.error).toBeDefined();
    expect(result.error).toContain('version');
  });

  it('returns error on 404', async () => {
    fetchSpy.mockResolvedValue({
      ok: false,
      status: 404,
      statusText: 'Not Found',
    });

    const result = await fetchManifest('octocat', 'game-repo', 'main');

    expect(result.manifest).toBeUndefined();
    expect(result.error).toBeDefined();
    expect(result.error).toContain('404');
  });

  it('returns error on network failure without throwing', async () => {
    fetchSpy.mockRejectedValue(new Error('Network error'));

    const result = await fetchManifest('octocat', 'game-repo', 'main');

    expect(result.manifest).toBeUndefined();
    expect(result.error).toBeDefined();
    expect(result.error).toContain('Network error');
  });
});

describe('fetchFeedIndex', () => {
  let fetchSpy: ReturnType<typeof vi.fn>;

  const feedIndex = {
    name: 'Test Feed',
    games: [
      { guid: 'g1', name: 'Game One', repo: 'owner1/repo1', branch: 'main' },
      { guid: 'g2', name: 'Game Two', repo: 'owner2/repo2', branch: 'dev', manifestPath: 'game/manifest.json' },
    ],
  };

  const manifest1 = {
    guid: 'g1',
    name: 'Game One',
    version: '1.0.0',
    description: 'First game',
    authors: ['Alice'],
    gamePath: 'GameDatabase/g1',
    tags: ['fun'],
  };

  const manifest2 = {
    guid: 'g2',
    name: 'Game Two',
    version: '2.0.0',
    description: 'Second game',
    authors: ['Bob', 'Carol'],
    gamePath: 'GameDatabase/g2',
  };

  beforeEach(() => {
    fetchSpy = vi.fn();
    vi.stubGlobal('fetch', fetchSpy);
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
  });

  it('fetches feed index and resolves all game manifests', async () => {
    fetchSpy.mockImplementation((url: string) => {
      if (url.includes('feed-index')) {
        return Promise.resolve({ ok: true, json: () => Promise.resolve(feedIndex) });
      }
      if (url.includes('owner1/repo1')) {
        return Promise.resolve({ ok: true, json: () => Promise.resolve(manifest1) });
      }
      if (url.includes('owner2/repo2')) {
        return Promise.resolve({ ok: true, json: () => Promise.resolve(manifest2) });
      }
      return Promise.resolve({ ok: false, status: 404, statusText: 'Not Found' });
    });

    const result = await fetchFeedIndex('https://example.com/feed-index.json');

    expect(result.games).toHaveLength(2);
    expect(result.errors).toHaveLength(0);

    const game1 = result.games.find(g => g.id === 'g1')!;
    expect(game1.name).toBe('Game One');
    expect(game1.version).toBe('1.0.0');
    expect(game1.authors).toBe('Alice');
    expect(game1.tags).toBe('fun');
    expect(game1.downloadUrl).toBe('https://api.github.com/repos/owner1/repo1/zipball/main');
    expect(game1.sourceType).toBe('repo');
    expect(game1.sourceInfo).toBe('owner1/repo1');

    const game2 = result.games.find(g => g.id === 'g2')!;
    expect(game2.name).toBe('Game Two');
    expect(game2.authors).toBe('Bob, Carol');
    expect(game2.tags).toBe('');
    expect(game2.downloadUrl).toBe('https://api.github.com/repos/owner2/repo2/zipball/dev');
  });

  it('returns partial results when one manifest fails', async () => {
    fetchSpy.mockImplementation((url: string) => {
      if (url.includes('feed-index')) {
        return Promise.resolve({ ok: true, json: () => Promise.resolve(feedIndex) });
      }
      if (url.includes('owner1/repo1')) {
        return Promise.resolve({ ok: true, json: () => Promise.resolve(manifest1) });
      }
      // owner2/repo2 fails
      return Promise.resolve({ ok: false, status: 404, statusText: 'Not Found' });
    });

    const result = await fetchFeedIndex('https://example.com/feed-index.json');

    expect(result.games).toHaveLength(1);
    expect(result.games[0].id).toBe('g1');
    expect(result.errors).toHaveLength(1);
    expect(result.errors[0]).toContain('404');
  });
});
