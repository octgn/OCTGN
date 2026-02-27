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

interface NuGetEntry {
  title?: string | { '#text': string; '@_type'?: string };
  summary?: string | { '#text': string; '@_type'?: string };
  author?: { name?: string } | string;
  properties?: {
    Id?: string;
    Version?: string;
    NormalizedVersion?: string;
    Description?: string;
    Tags?: string;
    DownloadCount?: number | string;
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

function parseEntries(feedBase: string, entries: NuGetEntry[]): AvailableGame[] {
  const games: AvailableGame[] = [];

  for (const entry of entries) {
    const props = entry.properties ?? {};

    // Only include latest versions to avoid duplicates per game
    // (some feeds may not have this field — include entry if field absent)
    if ('IsLatestVersion' in props && !isTruthy(props.IsLatestVersion)) {
      continue;
    }

    const gameId = text(props.Id);
    const gameName = text(entry.title);  // Real name — NOT the GUID
    if (!gameId || !gameName) continue;

    const version = text(props.NormalizedVersion) || text(props.Version);
    const description = text(props.Description) || text(entry.summary);
    const authors = text(entry.author);
    const downloadUrl = `${feedBase.replace(/\/$/, '')}/package/${encodeURIComponent(gameId)}/${encodeURIComponent(version)}`;

    games.push({
      id: gameId,
      name: gameName,
      version,
      description,
      authors,
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

  const all: AvailableGame[] = [];
  while (url) {
    const { entries, nextUrl } = await fetchPage(url, authHeaders);
    all.push(...parseEntries(feedBase, entries));
    url = nextUrl;
  }

  return all;
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
