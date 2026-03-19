/**
 * Persistent feed management — stores user feed list in userData/feeds.json.
 * Mirrors the original OCTGN FeedProvider.cs (Octgn.Library).
 *
 * Supports three feed types:
 *  - 'nuget'       — classic NuGet v2 package feeds (MyGet, etc.)
 *  - 'repo-index'  — JSON index file pointing to multiple game repos
 *  - 'direct-repo' — a single owner/repo pointing to one game
 */
import { app } from 'electron';
import { readFile, writeFile, mkdir } from 'fs/promises';
import { join } from 'path';

export interface GameFeed {
  name: string;
  url: string;
  enabled: boolean;
  isBuiltIn: boolean;
  username?: string;
  password?: string;
  feedType: 'nuget' | 'repo-index' | 'direct-repo';
  branch?: string; // only for direct-repo
}

const BUILT_IN_FEEDS: GameFeed[] = [
  { name: 'OCTGN Official',   url: 'https://www.myget.org/F/octgngames/',        enabled: true,  isBuiltIn: true, feedType: 'nuget' },
  { name: 'Community Games',  url: 'https://www.myget.org/F/octgngamedirectory', enabled: true,  isBuiltIn: true, feedType: 'nuget' },
  { name: 'The Spoils',       url: 'https://www.myget.org/F/thespoils/',         enabled: true,  isBuiltIn: true, feedType: 'nuget' },
  {
    name: 'OCTGN Community (GitHub)',
    url: 'https://raw.githubusercontent.com/octgn/octgn-community-feed/main/index.json',
    enabled: true,
    isBuiltIn: true,
    feedType: 'repo-index',
  },
];

function getFeedsPath(): string {
  return join(app.getPath('userData'), 'feeds.json');
}

async function loadRaw(): Promise<GameFeed[]> {
  try {
    const raw = await readFile(getFeedsPath(), 'utf-8');
    const parsed = JSON.parse(raw) as GameFeed[];
    // Backward compat: default missing feedType to 'nuget'
    return parsed.map((f) => ({ ...f, feedType: f.feedType ?? 'nuget' as const }));
  } catch {
    return [];
  }
}

async function saveRaw(feeds: GameFeed[]): Promise<void> {
  await mkdir(join(app.getPath('userData')), { recursive: true });
  await writeFile(getFeedsPath(), JSON.stringify(feeds, null, 2), 'utf-8');
}

/**
 * Returns the merged feed list: built-ins (with saved enabled state) + user-added feeds.
 */
export async function listFeeds(): Promise<GameFeed[]> {
  const saved = await loadRaw();

  // Start with built-ins, applying any saved enabled overrides
  const merged = BUILT_IN_FEEDS.map((builtin) => {
    const override = saved.find((s) => s.isBuiltIn && s.name === builtin.name);
    return override ? { ...builtin, enabled: override.enabled } : { ...builtin };
  });

  // Append user-added (non-built-in) feeds
  for (const feed of saved) {
    if (!feed.isBuiltIn) merged.push(feed);
  }

  return merged;
}

export async function addFeed(
  name: string,
  url: string,
  username?: string,
  password?: string,
): Promise<{ success: boolean; error?: string }> {
  const saved = await loadRaw();
  const all = await listFeeds();

  if (all.some((f) => f.url.toLowerCase() === url.toLowerCase())) {
    return { success: false, error: 'A feed with that URL already exists' };
  }

  saved.push({ name, url, enabled: true, isBuiltIn: false, username, password, feedType: 'nuget' });
  await saveRaw(saved);
  return { success: true };
}

export async function addDirectRepo(
  name: string,
  repoUrl: string,
  branch?: string,
): Promise<{ success: boolean; error?: string }> {
  const saved = await loadRaw();
  const all = await listFeeds();

  if (all.some((f) => f.url.toLowerCase() === repoUrl.toLowerCase())) {
    return { success: false, error: 'A feed with that URL already exists' };
  }

  const entry: GameFeed = {
    name,
    url: repoUrl,
    enabled: true,
    isBuiltIn: false,
    feedType: 'direct-repo',
  };
  if (branch) entry.branch = branch;
  saved.push(entry);
  await saveRaw(saved);
  return { success: true };
}

export async function addRepoFeed(
  name: string,
  indexUrl: string,
): Promise<{ success: boolean; error?: string }> {
  const saved = await loadRaw();
  const all = await listFeeds();

  if (all.some((f) => f.url.toLowerCase() === indexUrl.toLowerCase())) {
    return { success: false, error: 'A feed with that URL already exists' };
  }

  saved.push({ name, url: indexUrl, enabled: true, isBuiltIn: false, feedType: 'repo-index' });
  await saveRaw(saved);
  return { success: true };
}

export async function removeFeed(name: string): Promise<{ success: boolean; error?: string }> {
  const saved = await loadRaw();
  const builtin = BUILT_IN_FEEDS.find((f) => f.name === name);
  if (builtin) {
    return { success: false, error: 'Cannot remove a built-in feed' };
  }
  const filtered = saved.filter((f) => f.name !== name);
  await saveRaw(filtered);
  return { success: true };
}

export async function setFeedEnabled(
  name: string,
  enabled: boolean,
): Promise<void> {
  const saved = await loadRaw();
  const existing = saved.find((f) => f.name === name);
  if (existing) {
    existing.enabled = enabled;
  } else {
    // Save an override for built-in feeds
    saved.push({ name, url: '', enabled, isBuiltIn: true, feedType: 'nuget' });
  }
  await saveRaw(saved);
}
