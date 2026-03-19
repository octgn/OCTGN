import type { AvailableGame } from '../../shared/types';
import {
  type GameManifest,
  type FeedIndex,
  normalizeRepoUrl,
  constructRawManifestUrl,
  constructZipballUrl,
} from './repo-feed-types';

// Re-export for convenience
export { normalizeRepoUrl } from './repo-feed-types';
export type { GameManifest, FeedIndex, FeedGameEntry } from './repo-feed-types';

export async function fetchManifest(
  owner: string,
  repo: string,
  branch: string,
  manifestPath?: string,
): Promise<{ manifest?: GameManifest; error?: string }> {
  const url = constructRawManifestUrl(owner, repo, branch, manifestPath);

  try {
    const response = await fetch(url, { signal: AbortSignal.timeout(15_000) });

    if (!response.ok) {
      return { error: `Failed to fetch manifest from ${owner}/${repo}: ${response.status} ${response.statusText}` };
    }

    const data = await response.json();

    // Validate required fields
    const missing: string[] = [];
    if (!data.guid) missing.push('guid');
    if (!data.name) missing.push('name');
    if (!data.version) missing.push('version');
    if (!data.gamePath) missing.push('gamePath');
    if (!data.description) missing.push('description');
    if (!Array.isArray(data.authors) || data.authors.length === 0) missing.push('authors');

    if (missing.length > 0) {
      return { error: `Invalid manifest from ${owner}/${repo}: missing required fields: ${missing.join(', ')}` };
    }

    return { manifest: data as GameManifest };
  } catch (err: unknown) {
    const message = err instanceof Error ? err.message : String(err);
    return { error: `Error fetching manifest from ${owner}/${repo}: ${message}` };
  }
}

export async function fetchFeedIndex(
  feedUrl: string,
): Promise<{ games: AvailableGame[]; errors: string[] }> {
  const games: AvailableGame[] = [];
  const errors: string[] = [];

  let index: FeedIndex;
  try {
    const response = await fetch(feedUrl, { signal: AbortSignal.timeout(15_000) });
    if (!response.ok) {
      return { games, errors: [`Failed to fetch feed index: ${response.status} ${response.statusText}`] };
    }
    index = await response.json();
  } catch (err: unknown) {
    const message = err instanceof Error ? err.message : String(err);
    return { games, errors: [`Error fetching feed index: ${message}`] };
  }

  // Validate feed index structure
  if (!index.games || !Array.isArray(index.games)) {
    return { games, errors: ['Invalid feed index: missing "games" array'] };
  }

  // Fetch all manifests in parallel
  const results = await Promise.all(
    index.games.map(async (entry) => {
      const { owner, repo } = normalizeRepoUrl(entry.repo);
      const result = await fetchManifest(owner, repo, entry.branch, entry.manifestPath);

      if (result.error || !result.manifest) {
        return { error: result.error || `Unknown error for ${entry.repo}` };
      }

      const manifest = result.manifest;
      const game: AvailableGame = {
        id: manifest.guid,
        name: manifest.name,
        version: manifest.version,
        description: manifest.description,
        authors: manifest.authors.join(', '),
        tags: (manifest.tags || []).join(' '),
        downloadUrl: constructZipballUrl(owner, repo, entry.branch),
        sourceType: 'repo',
        sourceInfo: `${owner}/${repo}`,
      };

      return { game };
    }),
  );

  for (const result of results) {
    if ('game' in result && result.game) {
      games.push(result.game);
    }
    if ('error' in result && result.error) {
      errors.push(result.error);
    }
  }

  return { games, errors };
}
