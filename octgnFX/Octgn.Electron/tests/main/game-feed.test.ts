/**
 * Unit tests for game-feed.ts — the NuGet v2 Atom feed parser.
 *
 * These cover the bugs we've already hit in production:
 *  - Games missing because IsLatestVersion was never set true in the feed
 *  - Wrong name because d:Id (GUID) was used instead of <title>
 *  - Race condition / version selection when multiple versions exist
 *  - Pagination (next-link following)
 *  - Multi-feed deduplication
 */
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { compareVersions, parseEntries, fetchAvailableGames } from '@main/games/game-feed';
import type { NuGetEntry } from '@main/games/game-feed';
import type { GameFeed } from '@main/games/feed-manager';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function entry(
  id: string,
  title: string,
  version: string,
  opts: {
    isLatest?: boolean;
    isAbsoluteLatest?: boolean;
    downloadCount?: number;
    description?: string;
    authors?: string;
    iconUrl?: string;
    tags?: string;
  } = {},
): NuGetEntry {
  return {
    title,
    author: { name: opts.authors ?? 'Test Author' },
    summary: opts.description ?? '',
    properties: {
      Id: id,
      Version: version,
      NormalizedVersion: version,
      Description: opts.description ?? '',
      Tags: opts.tags ?? '',
      DownloadCount: opts.downloadCount ?? 100,
      IconUrl: opts.iconUrl ?? '',
      IsLatestVersion: opts.isLatest ?? false,
      IsAbsoluteLatestVersion: opts.isAbsoluteLatest ?? false,
    },
  };
}

const FEED_BASE = 'https://www.myget.org/F/testfeed';

const testFeed: GameFeed = {
  name: 'Test Feed',
  url: `${FEED_BASE}/`,
  enabled: true,
  isBuiltIn: false,
};

// Minimal valid Atom feed XML
function makeAtomXml(entries: string[], nextLink?: string): string {
  const linkEl = nextLink ? `<link rel="next" href="${nextLink}"/>` : '';
  return `<?xml version="1.0" encoding="utf-8"?>
<feed xmlns="http://www.w3.org/2005/Atom"
      xmlns:d="http://schemas.microsoft.com/ado/2007/08/dataservices"
      xmlns:m="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata">
  ${linkEl}
  ${entries.join('\n')}
</feed>`;
}

function makeEntryXml(
  id: string,
  title: string,
  version: string,
  isLatest: boolean,
  isAbsoluteLatest: boolean,
  downloadCount = 100,
  iconUrl = '',
  authors = 'Someone',
): string {
  return `
  <entry>
    <title type="text">${title}</title>
    <author><name>${authors}</name></author>
    <summary type="text">A game description</summary>
    <m:properties>
      <d:Id>${id}</d:Id>
      <d:Version>${version}</d:Version>
      <d:NormalizedVersion>${version}</d:NormalizedVersion>
      <d:Description>A game description</d:Description>
      <d:DownloadCount m:type="Edm.Int64">${downloadCount}</d:DownloadCount>
      <d:IconUrl>${iconUrl}</d:IconUrl>
      <d:Tags></d:Tags>
      <d:IsLatestVersion m:type="Edm.Boolean">${isLatest}</d:IsLatestVersion>
      <d:IsAbsoluteLatestVersion m:type="Edm.Boolean">${isAbsoluteLatest}</d:IsAbsoluteLatestVersion>
    </m:properties>
  </entry>`;
}

// ---------------------------------------------------------------------------
// compareVersions
// ---------------------------------------------------------------------------

describe('compareVersions', () => {
  it('identifies newer patch version', () => {
    expect(compareVersions('1.0.0.1', '1.0.0.0')).toBeGreaterThan(0);
    expect(compareVersions('1.0.0.0', '1.0.0.1')).toBeLessThan(0);
  });

  it('handles numeric comparison correctly (not lexicographic)', () => {
    // "1.0.0.9" vs "1.0.0.10" — lexicographic would say 9 > 10, numeric says 10 > 9
    expect(compareVersions('1.0.0.10', '1.0.0.9')).toBeGreaterThan(0);
    expect(compareVersions('1.0.0.9', '1.0.0.10')).toBeLessThan(0);
  });

  it('handles date-style versions like OCTGN uses', () => {
    expect(compareVersions('2019.07.07.00', '2018.01.25.00')).toBeGreaterThan(0);
    expect(compareVersions('2018.01.25.00', '2019.07.07.00')).toBeLessThan(0);
  });

  it('returns 0 for equal versions', () => {
    expect(compareVersions('1.4.4.9', '1.4.4.9')).toBe(0);
    expect(compareVersions('2019.07.07.00', '2019.07.07.00')).toBe(0);
  });

  it('handles different segment counts', () => {
    // "1.0" treated as "1.0.0.0"
    expect(compareVersions('1.0.0.1', '1.0')).toBeGreaterThan(0);
    expect(compareVersions('1.0', '1.0.0.1')).toBeLessThan(0);
  });

  it('handles pre-release-style versions', () => {
    expect(compareVersions('1.4.4.9', '1.4.4.5')).toBeGreaterThan(0);
  });
});

// ---------------------------------------------------------------------------
// parseEntries
// ---------------------------------------------------------------------------

describe('parseEntries', () => {
  it('returns one entry per unique game ID', () => {
    const entries: NuGetEntry[] = [
      entry('game-1', 'Chess',     '1.0.0.10', { isLatest: false }),
      entry('game-1', 'Chess',     '1.0.0.11', { isLatest: true }),
      entry('game-2', 'Dominoes',  '1.0.6.0',  { isLatest: false }),
      entry('game-2', 'Dominoes',  '1.0.6.1',  { isLatest: true }),
    ];
    const result = parseEntries(FEED_BASE, entries);
    expect(result).toHaveLength(2);
    expect(result.map(g => g.name).sort()).toEqual(['Chess', 'Dominoes']);
  });

  it('picks IsLatestVersion=true entry over others', () => {
    const entries: NuGetEntry[] = [
      entry('game-1', 'Chess', '1.0.0.9',  { isLatest: false }),
      entry('game-1', 'Chess', '1.0.0.11', { isLatest: true  }),
      entry('game-1', 'Chess', '1.0.0.10', { isLatest: false }),
    ];
    const [chess] = parseEntries(FEED_BASE, entries);
    expect(chess.version).toBe('1.0.0.11');
  });

  it('falls back to highest version when IsLatestVersion is never set — the The Spoils bug', () => {
    // Real scenario: all versions have IsLatestVersion=false
    const entries: NuGetEntry[] = [
      entry('844d5fe3', 'The Spoils', '1.4.4.1', { isLatest: false }),
      entry('844d5fe3', 'The Spoils', '1.4.4.5', { isLatest: false }),
      entry('844d5fe3', 'The Spoils', '1.4.4.7', { isLatest: false }),
      entry('844d5fe3', 'The Spoils', '1.4.4.9', { isLatest: false }),
    ];
    const result = parseEntries(FEED_BASE, entries);
    expect(result).toHaveLength(1);
    expect(result[0].name).toBe('The Spoils');
    expect(result[0].version).toBe('1.4.4.9');
  });

  it('falls back to IsAbsoluteLatestVersion when IsLatestVersion not set', () => {
    const entries: NuGetEntry[] = [
      entry('game-1', 'MyGame', '1.0.0', { isLatest: false, isAbsoluteLatest: false }),
      entry('game-1', 'MyGame', '1.1.0', { isLatest: false, isAbsoluteLatest: true  }),
      entry('game-1', 'MyGame', '0.9.0', { isLatest: false, isAbsoluteLatest: false }),
    ];
    const [game] = parseEntries(FEED_BASE, entries);
    expect(game.version).toBe('1.1.0');
  });

  it('uses game title (not GUID) as the name — the original name-as-GUID bug', () => {
    const entries: NuGetEntry[] = [
      entry('b74079e6-4dc7-4f0f-9025-b4e444555265', 'Universal Fighting System', '2019.07.07.00', {
        isLatest: true,
      }),
    ];
    const [game] = parseEntries(FEED_BASE, entries);
    expect(game.name).toBe('Universal Fighting System');
    expect(game.id).toBe('b74079e6-4dc7-4f0f-9025-b4e444555265');
  });

  it('skips entries with no Id or no title', () => {
    const entries: NuGetEntry[] = [
      { title: 'No ID game', properties: { Version: '1.0' } },           // no Id
      { properties: { Id: 'some-id', Version: '1.0' } },                 // no title
      entry('game-1', 'Valid Game', '1.0.0', { isLatest: true }),
    ];
    const result = parseEntries(FEED_BASE, entries);
    expect(result).toHaveLength(1);
    expect(result[0].name).toBe('Valid Game');
  });

  it('builds correct download URL from feedBase + id + version', () => {
    const entries: NuGetEntry[] = [
      entry('my-game-id', 'My Game', '2.0.0', { isLatest: true }),
    ];
    const [game] = parseEntries('https://example.com/F/myfeed', entries);
    expect(game.downloadUrl).toBe(
      'https://example.com/F/myfeed/package/my-game-id/2.0.0'
    );
  });

  it('strips trailing slash from feedBase in download URL', () => {
    const entries: NuGetEntry[] = [entry('id', 'Game', '1.0', { isLatest: true })];
    const [game] = parseEntries('https://example.com/F/myfeed/', entries);
    expect(game.downloadUrl).not.toContain('//package');
  });

  it('handles real octgngames feed data — all 5 games present', () => {
    // Exact versions from live fetch 2025-02
    const entries: NuGetEntry[] = [
      entry('b74079e6', 'Universal Fighting System', '2018.01.25.00', { isLatest: false }),
      entry('b74079e6', 'Universal Fighting System', '2018.08.10.00', { isLatest: false }),
      entry('b74079e6', 'Universal Fighting System', '2019.05.14.00', { isLatest: false }),
      entry('b74079e6', 'Universal Fighting System', '2019.07.07.00', { isLatest: true  }),
      entry('844d5fe3', 'The Spoils',                '1.4.4.1',       { isLatest: false }),
      entry('844d5fe3', 'The Spoils',                '1.4.4.9',       { isLatest: false }), // no latest ever set
      entry('8f437fff', 'Standard Playing Cards',    '2.0.0.0',       { isLatest: false }),
      entry('8f437fff', 'Standard Playing Cards',    '2.0.0.3',       { isLatest: true  }),
      entry('2f3dbb9b', 'Chess',                     '1.0.0.0',       { isLatest: false }),
      entry('2f3dbb9b', 'Chess',                     '1.0.0.11',      { isLatest: true  }),
      entry('662e53a4', 'Dominoes',                  '1.0.2',         { isLatest: false }),
      entry('662e53a4', 'Dominoes',                  '1.0.6.1',       { isLatest: true  }),
    ];

    const result = parseEntries(FEED_BASE, entries);
    const names = result.map(g => g.name).sort();
    expect(names).toEqual([
      'Chess',
      'Dominoes',
      'Standard Playing Cards',
      'The Spoils',
      'Universal Fighting System',
    ]);

    const byName = Object.fromEntries(result.map(g => [g.name, g.version]));
    expect(byName['Universal Fighting System']).toBe('2019.07.07.00');
    expect(byName['The Spoils']).toBe('1.4.4.9');
    expect(byName['Standard Playing Cards']).toBe('2.0.0.3');
    expect(byName['Chess']).toBe('1.0.0.11');
    expect(byName['Dominoes']).toBe('1.0.6.1');
  });
});

// ---------------------------------------------------------------------------
// fetchAvailableGames — end-to-end with mocked fetch
// ---------------------------------------------------------------------------

describe('fetchAvailableGames', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  function mockFetch(xml: string) {
    vi.mocked(fetch).mockResolvedValue({
      ok: true,
      status: 200,
      text: async () => xml,
    } as Response);
  }

  it('parses a real-structure Atom feed and returns games', async () => {
    const xml = makeAtomXml([
      makeEntryXml('chess-id', 'Chess', '1.0.0.11', true,  true,  6409),
      makeEntryXml('chess-id', 'Chess', '1.0.0.9',  false, false, 6409),
      makeEntryXml('spoils-id','The Spoils', '1.4.4.9', false, false, 10095),
    ]);
    mockFetch(xml);

    const result = await fetchAvailableGames([testFeed]);
    expect(result).toHaveLength(2);
    const names = result.map(g => g.name).sort();
    expect(names).toEqual(['Chess', 'The Spoils']);
  });

  it('selects correct version from feed — IsLatestVersion=true wins', async () => {
    const xml = makeAtomXml([
      makeEntryXml('chess-id', 'Chess', '1.0.0.9',  false, false, 6409),
      makeEntryXml('chess-id', 'Chess', '1.0.0.11', true,  true,  6409),
    ]);
    mockFetch(xml);

    const [chess] = await fetchAvailableGames([testFeed]);
    expect(chess.version).toBe('1.0.0.11');
  });

  it('uses highest version when IsLatestVersion never set (The Spoils bug)', async () => {
    const xml = makeAtomXml([
      makeEntryXml('spoils-id', 'The Spoils', '1.4.4.1', false, false, 10095),
      makeEntryXml('spoils-id', 'The Spoils', '1.4.4.9', false, false, 10095),
      makeEntryXml('spoils-id', 'The Spoils', '1.4.4.5', false, false, 10095),
    ]);
    mockFetch(xml);

    const [spoils] = await fetchAvailableGames([testFeed]);
    expect(spoils.version).toBe('1.4.4.9');
  });

  it('follows pagination next-link until no more pages', async () => {
    const page1Xml = makeAtomXml(
      [makeEntryXml('game-1', 'Game One', '1.0', true, true, 500)],
      'https://example.com/page2',
    );
    const page2Xml = makeAtomXml([
      makeEntryXml('game-2', 'Game Two', '2.0', true, true, 300),
    ]);

    vi.mocked(fetch)
      .mockResolvedValueOnce({ ok: true, text: async () => page1Xml } as Response)
      .mockResolvedValueOnce({ ok: true, text: async () => page2Xml } as Response);

    const result = await fetchAvailableGames([testFeed]);
    expect(result).toHaveLength(2);
    expect(fetch).toHaveBeenCalledTimes(2);
  });

  it('picks correct version when same game spans multiple pages — not by download count', async () => {
    // Old version on page 1 has MORE downloads; new version on page 2 has IsLatestVersion=true.
    // The correct winner is the one with IsLatestVersion=true, regardless of download count.
    const page1Xml = makeAtomXml(
      [makeEntryXml('chess-id', 'Chess', '1.0.0.9', false, false, 9999)],
      'https://example.com/page2',
    );
    const page2Xml = makeAtomXml([
      makeEntryXml('chess-id', 'Chess', '1.0.0.11', true, true, 100),
    ]);

    vi.mocked(fetch)
      .mockResolvedValueOnce({ ok: true, text: async () => page1Xml } as Response)
      .mockResolvedValueOnce({ ok: true, text: async () => page2Xml } as Response);

    const result = await fetchAvailableGames([testFeed]);
    expect(result).toHaveLength(1);
    // Must pick v1.0.0.11 (IsLatestVersion=true), NOT v1.0.0.9 (higher downloads)
    expect(result[0].version).toBe('1.0.0.11');
  });

  it('deduplicates same game across multiple feeds — keeps highest download count', async () => {
    const feed1Xml = makeAtomXml([makeEntryXml('game-id', 'My Game', '1.0', true, true, 1000)]);
    const feed2Xml = makeAtomXml([makeEntryXml('game-id', 'My Game', '1.0', true, true, 5000)]);

    vi.mocked(fetch)
      .mockResolvedValueOnce({ ok: true, text: async () => feed1Xml } as Response)
      .mockResolvedValueOnce({ ok: true, text: async () => feed2Xml } as Response);

    const feed2: GameFeed = { name: 'Feed 2', url: 'https://example.com/F/feed2/', enabled: true, isBuiltIn: false };
    const result = await fetchAvailableGames([testFeed, feed2]);
    expect(result).toHaveLength(1);
    expect(result[0].downloadCount).toBe(5000);
  });

  it('still returns games from healthy feeds when one feed errors', async () => {
    const goodXml = makeAtomXml([makeEntryXml('game-1', 'Good Game', '1.0', true, true, 100)]);

    vi.mocked(fetch)
      .mockRejectedValueOnce(new Error('Network error'))
      .mockResolvedValueOnce({ ok: true, text: async () => goodXml } as Response);

    const badFeed: GameFeed = { name: 'Bad Feed', url: 'https://broken.example/', enabled: true, isBuiltIn: false };
    const goodFeed: GameFeed = { name: 'Good Feed', url: 'https://good.example/', enabled: true, isBuiltIn: false };
    const result = await fetchAvailableGames([badFeed, goodFeed]);
    expect(result).toHaveLength(1);
    expect(result[0].name).toBe('Good Game');
  });

  it('ignores disabled feeds', async () => {
    const disabledFeed: GameFeed = { ...testFeed, enabled: false };
    const result = await fetchAvailableGames([disabledFeed]);
    expect(result).toHaveLength(0);
    expect(fetch).not.toHaveBeenCalled();
  });

  it('returns results sorted by downloadCount descending', async () => {
    const xml = makeAtomXml([
      makeEntryXml('a', 'Low Downloads',  '1.0', true, true, 100),
      makeEntryXml('b', 'High Downloads', '1.0', true, true, 99999),
      makeEntryXml('c', 'Mid Downloads',  '1.0', true, true, 5000),
    ]);
    mockFetch(xml);

    const result = await fetchAvailableGames([testFeed]);
    expect(result[0].name).toBe('High Downloads');
    expect(result[1].name).toBe('Mid Downloads');
    expect(result[2].name).toBe('Low Downloads');
  });
});

