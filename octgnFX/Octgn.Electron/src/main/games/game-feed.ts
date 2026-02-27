/**
 * Fetches available game packages from NuGet v2 feeds.
 * Implements OData paging by following <link rel="next"> in Atom responses.
 *
 * Real feed XML structure (from MyGet):
 *   <entry>
 *     <title type="text">Universal Fighting System</title>   ← game name
 *     <author><name>AuthorOne, AuthorTwo</name></author>    ← authors (NOT d:Authors)
 *     <summary type="text">...</summary>                    ← fallback description
 *     <m:properties>
 *       <d:Id>b74079e6-…</d:Id>                            ← game GUID (our game id)
 *       <d:Version>2018.08.10.00</d:Version>
 *       <d:Description>…</d:Description>
 *       <d:DownloadCount m:type="Edm.Int64">14054</d:DownloadCount>
 *       <d:IconUrl>http://…</d:IconUrl>
 *       <d:Tags>tag1 tag2</d:Tags>
 *       <d:IsLatestVersion m:type="Edm.Boolean">true</d:IsLatestVersion>
 *     </m:properties>
 *   </entry>
 */
import { XMLParser } from 'fast-xml-parser';
import type { AvailableGame } from '../../shared/types';
import type { GameFeed } from './feed-manager';

const parser = new XMLParser({
  ignoreAttributes: false,
  attributeNamePrefix: '@_',
  isArray: (tagName) => ['entry', 'link'].includes(tagName),
  removeNSPrefix: true,
});

export interface NuGetEntry {
  title?: string | { '#text': string; '@_type'?: string };
  summary?: string | { '#text': string; '@_type'?: string };
  author?: { name?: string } | string;
  properties?: {
    Id?: string;
    Version?: string;
    NormalizedVersion?: string;
    Description?: string;
    Tags?: string;
    DownloadCount?: number | string | { '#text': number | string };
    IconUrl?: string;
    IsLatestVersion?: string | boolean;
    IsAbsoluteLatestVersion?: string | boolean;
  };
}

function text(v: unknown): string {
  if (!v) return '';
  if (typeof v === 'string') return v;
  if (typeof v === 'object') {
    const o = v as Record<string, unknown>;
    if ('#text' in o) return String(o['#text'] ?? '');
    if ('name' in o) return String(o['name'] ?? ''); // <author><name>
  }
  return String(v);
}

function isTruthy(v: unknown): boolean {
  if (v === true || v === 'true') return true;
  return false;
}

/**
 * Compares two NuGet-style version strings (e.g. "2019.07.07.00", "1.0.0.11").
 * Returns >0 if a is newer, <0 if b is newer, 0 if equal.
 * Exported for unit testing.
 */
export function compareVersions(a: string, b: string): number {
  const pa = a.split('.').map((n) => parseInt(n, 10) || 0);
  const pb = b.split('.').map((n) => parseInt(n, 10) || 0);
  const len = Math.max(pa.length, pb.length);
  for (let i = 0; i < len; i++) {
    const diff = (pa[i] ?? 0) - (pb[i] ?? 0);
    if (diff !== 0) return diff;
  }
  return 0;
}

/** Exported for unit testing. */
export function parseEntries(feedBase: string, entries: NuGetEntry[]): AvailableGame[] {
  // Collect all versions per game, then pick the best one.
  // Some feeds never set IsLatestVersion=true (e.g. The Spoils in octgngames),
  // so we can't simply filter on that flag — we'd drop the whole game.
  type Candidate = { game: AvailableGame; isLatest: boolean; isAbsoluteLatest: boolean };
  const candidates = new Map<string, Candidate>();

  for (const entry of entries) {
    const props = entry.properties ?? {};
    const gameId = text(props.Id);
    const gameName = text(entry.title); // Real name — NOT the GUID
    if (!gameId || !gameName) continue;

    const isLatest = isTruthy(props.IsLatestVersion);
    const isAbsoluteLatest = isTruthy(props.IsAbsoluteLatestVersion);
    const version = text(props.NormalizedVersion) || text(props.Version);
    const description = text(props.Description) || text(entry.summary);
    const authors = text(entry.author);
    const downloadUrl = `${feedBase.replace(/\/$/, '')}/package/${encodeURIComponent(gameId)}/${encodeURIComponent(version)}`;

    const game: AvailableGame = {
      id: gameId,
      name: gameName,
      version,
      description,
      authors,
      tags: text(props.Tags),
      downloadUrl,
      iconUrl: text(props.IconUrl) || undefined,
      downloadCount: Number(text(props.DownloadCount)) || 0,
    };

    const existing = candidates.get(gameId);
    if (!existing) {
      candidates.set(gameId, { game, isLatest, isAbsoluteLatest });
    } else {
      // Priority: IsLatestVersion=true > IsAbsoluteLatestVersion=true > highest version string
      const win =
        (!existing.isLatest && isLatest) ||
        (!existing.isLatest && !existing.isAbsoluteLatest && isAbsoluteLatest) ||
        (!existing.isLatest && !existing.isAbsoluteLatest && !isAbsoluteLatest &&
          compareVersions(version, existing.game.version) > 0);
      if (win) candidates.set(gameId, { game, isLatest, isAbsoluteLatest });
    }
  }

  return [...candidates.values()].map(({ game }) => game);
}

function getNextLink(doc: Record<string, unknown>): string | null {
  const feed = (doc['feed'] ?? doc) as Record<string, unknown>;
  const links = feed['link'];
  if (!links) return null;
  const arr = Array.isArray(links) ? links : [links];
  for (const link of arr) {
    const l = link as Record<string, unknown>;
    if (l['@_rel'] === 'next' && l['@_href']) return String(l['@_href']);
  }
  return null;
}

async function fetchPage(
  url: string,
  headers: Record<string, string> = {},
): Promise<{ entries: NuGetEntry[]; nextUrl: string | null }> {
  const resp = await fetch(url, {
    headers: { Accept: 'application/atom+xml,application/xml', ...headers },
    signal: AbortSignal.timeout(20_000),
  });

  if (!resp.ok) throw new Error(`Feed responded ${resp.status} for ${url}`);

  const xml = await resp.text();
  const doc = parser.parse(xml) as Record<string, unknown>;
  const feed = (doc['feed'] ?? doc) as Record<string, unknown>;
  const entries = (feed['entry'] ?? []) as NuGetEntry[];
  const nextUrl = getNextLink(doc);

  return { entries, nextUrl };
}

async function fetchAllFromFeed(feedDef: GameFeed): Promise<AvailableGame[]> {
  const feedBase = feedDef.url.replace(/\/$/, '');
  let url: string | null = `${feedBase}/Packages()?$orderby=DownloadCount+desc`;

  const authHeaders: Record<string, string> = {};
  if (feedDef.username && feedDef.password) {
    const creds = Buffer.from(`${feedDef.username}:${feedDef.password}`).toString('base64');
    authHeaders['Authorization'] = `Basic ${creds}`;
  }

  // Collect ALL raw entries across every page first, then parse once.
  // Calling parseEntries per-page and concatenating the results is wrong:
  // a game's versions can span pages, so per-page winner-picking uses incomplete
  // information and the cross-page dedup (by downloadCount) can pick the wrong version.
  const allEntries: NuGetEntry[] = [];
  while (url) {
    const { entries, nextUrl } = await fetchPage(url, authHeaders);
    allEntries.push(...entries);
    url = nextUrl;
  }

  return parseEntries(feedBase, allEntries);
}

export async function fetchAvailableGames(feeds: GameFeed[]): Promise<AvailableGame[]> {
  const enabled = feeds.filter((f) => f.enabled && f.url);
  const results = await Promise.allSettled(enabled.map(fetchAllFromFeed));

  const all: AvailableGame[] = [];
  for (const result of results) {
    if (result.status === 'fulfilled') all.push(...result.value);
  }

  // Deduplicate by id — if same game appears in multiple feeds, keep highest download count
  const map = new Map<string, AvailableGame>();
  for (const game of all) {
    const existing = map.get(game.id);
    if (!existing || (game.downloadCount ?? 0) > (existing.downloadCount ?? 0)) {
      map.set(game.id, game);
    }
  }

  return [...map.values()].sort((a, b) => (b.downloadCount ?? 0) - (a.downloadCount ?? 0));
}
