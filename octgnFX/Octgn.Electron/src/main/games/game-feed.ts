/**
 * Fetches available game packages from all OCTGN NuGet v2 feeds on MyGet.
 * Feeds (from Octgn.Library/Paths.cs + AppConfig.cs):
 *   - https://www.myget.org/F/octgngames/         (official games)
 *   - https://www.myget.org/F/octgngamedirectory/ (community directory)
 *   - https://www.myget.org/F/thespoils/          (The Spoils)
 */
import { XMLParser } from 'fast-xml-parser';
import type { AvailableGame } from '../../shared/types';

const FEEDS = [
  'https://www.myget.org/F/octgngames',
  'https://www.myget.org/F/octgngamedirectory',
  'https://www.myget.org/F/thespoils',
];

const parser = new XMLParser({
  ignoreAttributes: false,
  attributeNamePrefix: '@_',
  isArray: (tagName) => tagName === 'entry',
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

async function fetchFeed(feedBase: string): Promise<AvailableGame[]> {
  // No $top limit — fetch all packages, ordered by download count
  const url = `${feedBase}/Packages()?$orderby=DownloadCount+desc`;
  const resp = await fetch(url, {
    headers: { Accept: 'application/atom+xml,application/xml' },
    signal: AbortSignal.timeout(20_000),
  });

  if (!resp.ok) {
    throw new Error(`Feed ${feedBase} responded ${resp.status}`);
  }

  const xml = await resp.text();
  const doc = parser.parse(xml) as Record<string, unknown>;
  const feed = (doc['feed'] ?? doc) as Record<string, unknown>;
  const entries = (feed['entry'] ?? []) as NuGetEntry[];

  return entries.flatMap((entry) => {
    const props = entry.properties ?? {};
    const pkgId = text(props.Id) || text(entry.title);
    const version = text(props.NormalizedVersion) || text(props.Version);
    if (!pkgId || !version) return [];

    const downloadUrl = `${feedBase}/package/${encodeURIComponent(pkgId)}/${encodeURIComponent(version)}`;

    return [{
      id: pkgId,
      name: pkgId.replace(/\./g, ' ').trim(),
      version,
      description: text(props.Description),
      authors: text(props.Authors),
      tags: text(props.Tags),
      downloadUrl,
      iconUrl: text(props.IconUrl) || undefined,
      downloadCount: Number(props.DownloadCount) || 0,
    }];
  });
}

export async function fetchAvailableGames(): Promise<AvailableGame[]> {
  // Query all feeds in parallel, tolerate individual failures
  const results = await Promise.allSettled(FEEDS.map(fetchFeed));

  const all: AvailableGame[] = [];
  for (const result of results) {
    if (result.status === 'fulfilled') {
      all.push(...result.value);
    }
    // Log failures but don't surface them — partial results are fine
  }

  // Deduplicate by id, keeping the entry with the highest download count
  const map = new Map<string, AvailableGame>();
  for (const game of all) {
    const existing = map.get(game.id);
    if (!existing || (game.downloadCount ?? 0) > (existing.downloadCount ?? 0)) {
      map.set(game.id, game);
    }
  }

  // Sort by download count descending
  return [...map.values()].sort((a, b) => (b.downloadCount ?? 0) - (a.downloadCount ?? 0));
}
