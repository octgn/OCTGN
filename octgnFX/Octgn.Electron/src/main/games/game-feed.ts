/**
 * Fetches available game packages from NuGet v2 feeds.
 * Implements OData paging by following <link rel="next"> in Atom responses.
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

interface NuGetEntry {
  title?: string | { '#text': string };
  properties?: {
    Id?: string;
    Version?: string;
    NormalizedVersion?: string;
    Description?: string;
    Authors?: string;
    Tags?: string;
    DownloadCount?: number | string;
    IconUrl?: string;
  };
}

function text(v: unknown): string {
  if (!v) return '';
  if (typeof v === 'string') return v;
  if (typeof v === 'object' && '#text' in (v as Record<string, unknown>)) {
    return String((v as Record<string, unknown>)['#text'] ?? '');
  }
  return String(v);
}

function parseEntries(feedBase: string, entries: NuGetEntry[]): AvailableGame[] {
  const games: AvailableGame[] = [];
  for (const entry of entries) {
    const props = entry.properties ?? {};
    const pkgId = text(props.Id) || text(entry.title);
    const version = text(props.NormalizedVersion) || text(props.Version);
    if (!pkgId || !version) continue;

    const downloadUrl = `${feedBase.replace(/\/$/, '')}/package/${encodeURIComponent(pkgId)}/${encodeURIComponent(version)}`;

    games.push({
      id: pkgId,
      name: pkgId.replace(/\./g, ' ').trim(),
      version,
      description: text(props.Description),
      authors: text(props.Authors),
      tags: text(props.Tags),
      downloadUrl,
      iconUrl: text(props.IconUrl) || undefined,
      downloadCount: Number(props.DownloadCount) || 0,
    });
  }
  return games;
}

function getNextLink(doc: Record<string, unknown>): string | null {
  const feed = (doc['feed'] ?? doc) as Record<string, unknown>;
  const links = feed['link'];
  if (!links) return null;
  const arr = Array.isArray(links) ? links : [links];
  for (const link of arr) {
    const l = link as Record<string, unknown>;
    if (l['@_rel'] === 'next' && l['@_href']) {
      return String(l['@_href']);
    }
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

  if (!resp.ok) {
    throw new Error(`Feed responded ${resp.status} for ${url}`);
  }

  const xml = await resp.text();
  const doc = parser.parse(xml) as Record<string, unknown>;
  const feed = (doc['feed'] ?? doc) as Record<string, unknown>;
  const entries = ((feed['entry'] ?? []) as NuGetEntry[]);
  const nextUrl = getNextLink(doc);

  return { entries, nextUrl };
}

async function fetchAllFromFeed(feedDef: GameFeed): Promise<AvailableGame[]> {
  const feedBase = feedDef.url.replace(/\/$/, '');
  // Start with packages ordered by download count
  let url: string | null = `${feedBase}/Packages()?$orderby=DownloadCount+desc`;

  const authHeaders: Record<string, string> = {};
  if (feedDef.username && feedDef.password) {
    const creds = Buffer.from(`${feedDef.username}:${feedDef.password}`).toString('base64');
    authHeaders['Authorization'] = `Basic ${creds}`;
  }

  const all: AvailableGame[] = [];

  while (url) {
    const { entries, nextUrl } = await fetchPage(url, authHeaders);
    all.push(...parseEntries(feedBase, entries));
    url = nextUrl;
  }

  return all;
}

/**
 * Fetch all available games from the given enabled feeds.
 * Feeds run in parallel; individual failures are tolerated.
 */
export async function fetchAvailableGames(feeds: GameFeed[]): Promise<AvailableGame[]> {
  const enabled = feeds.filter((f) => f.enabled && f.url);
  const results = await Promise.allSettled(enabled.map(fetchAllFromFeed));

  const all: AvailableGame[] = [];
  for (const result of results) {
    if (result.status === 'fulfilled') {
      all.push(...result.value);
    }
    // Silently tolerate feed failures — partial results are fine
  }

  // Deduplicate by id, keeping highest download count
  const map = new Map<string, AvailableGame>();
  for (const game of all) {
    const existing = map.get(game.id);
    if (!existing || (game.downloadCount ?? 0) > (existing.downloadCount ?? 0)) {
      map.set(game.id, game);
    }
  }

  return [...map.values()].sort((a, b) => (b.downloadCount ?? 0) - (a.downloadCount ?? 0));
}
