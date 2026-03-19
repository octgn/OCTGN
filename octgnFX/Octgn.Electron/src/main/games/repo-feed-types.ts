/**
 * Types and validation for GitHub repo-based game distribution.
 *
 * Defines:
 * - GameSource model with priority-based source types
 * - Validation and URL construction helpers
 *
 * Core types (GameManifest, FeedIndex, FeedGameEntry, RepoGameSource)
 * are defined in shared/types and re-exported here for convenience.
 */

// Re-export shared types so existing imports from this module still work
export type { GameManifest, FeedIndex, FeedGameEntry, RepoGameSource } from '../../shared/types';

// ---------------------------------------------------------------------------
// Internal source model — unified across all distribution types
// ---------------------------------------------------------------------------

export enum GameSourceType {
  LocalFolder = 0,
  DirectRepo = 1,
  UserFeed = 2,
  BuiltInFeed = 3,
  LegacyNuGet = 4,
}

export interface GameSourceBase {
  type: GameSourceType;
  name: string;
  enabled: boolean;
  isBuiltIn: boolean;
}

export interface LocalFolderSource extends GameSourceBase {
  type: GameSourceType.LocalFolder;
  folderPath: string;
}

export interface DirectRepoSource extends GameSourceBase {
  type: GameSourceType.DirectRepo;
  owner: string;
  repo: string;
  branch: string;
  manifestPath?: string;
}

export interface UserFeedSource extends GameSourceBase {
  type: GameSourceType.UserFeed;
  indexUrl: string;
}

export interface BuiltInFeedSource extends GameSourceBase {
  type: GameSourceType.BuiltInFeed;
  indexUrl: string;
}

export interface LegacyNuGetSource extends GameSourceBase {
  type: GameSourceType.LegacyNuGet;
  feedUrl: string;
  username?: string;
  password?: string;
}

export type GameSource =
  | LocalFolderSource
  | DirectRepoSource
  | UserFeedSource
  | BuiltInFeedSource
  | LegacyNuGetSource;

// ---------------------------------------------------------------------------
// Validation helpers
// ---------------------------------------------------------------------------

const UUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

interface ValidationResult {
  valid: boolean;
  errors: string[];
}

function isObject(val: unknown): val is Record<string, unknown> {
  return val !== null && typeof val === 'object' && !Array.isArray(val);
}

export function validateManifest(data: unknown): ValidationResult {
  const errors: string[] = [];

  if (!isObject(data)) {
    return { valid: false, errors: ['Manifest must be a non-null object'] };
  }

  // Required string fields
  const requiredStrings = ['guid', 'name', 'version', 'versionDate', 'description', 'gamePath'] as const;
  for (const field of requiredStrings) {
    if (data[field] === undefined || data[field] === null) {
      errors.push(`Missing required field: ${field}`);
    } else if (typeof data[field] !== 'string') {
      errors.push(`Field "${field}" must be a string`);
    }
  }

  // authors — required array of strings
  if (data.authors === undefined || data.authors === null) {
    errors.push('Missing required field: authors');
  } else if (!Array.isArray(data.authors) || !data.authors.every((a: unknown) => typeof a === 'string')) {
    errors.push('Field "authors" must be an array of strings');
  } else if (data.authors.length === 0) {
    errors.push('Field "authors" must have at least one entry');
  }

  // guid format
  if (typeof data.guid === 'string' && !UUID_RE.test(data.guid)) {
    errors.push('Field "guid" must be a valid UUID');
  }

  // Optional string fields
  const optionalStrings = ['minimumOctgnVersion', 'changelog'] as const;
  for (const field of optionalStrings) {
    if (data[field] !== undefined && data[field] !== null && typeof data[field] !== 'string') {
      errors.push(`Field "${field}" must be a string`);
    }
  }

  // Optional tags array
  if (data.tags !== undefined && data.tags !== null) {
    if (!Array.isArray(data.tags) || !data.tags.every((t: unknown) => typeof t === 'string')) {
      errors.push('Field "tags" must be an array of strings');
    }
  }

  return { valid: errors.length === 0, errors };
}

export function validateFeedIndex(data: unknown): ValidationResult {
  const errors: string[] = [];

  if (!isObject(data)) {
    return { valid: false, errors: ['Feed index must be a non-null object'] };
  }

  if (data.name === undefined || data.name === null) {
    errors.push('Missing required field: name');
  } else if (typeof data.name !== 'string') {
    errors.push('Field "name" must be a string');
  }

  if (data.games === undefined || data.games === null) {
    errors.push('Missing required field: games');
  } else if (!Array.isArray(data.games)) {
    errors.push('Field "games" must be an array');
  } else {
    for (let i = 0; i < data.games.length; i++) {
      const entry = data.games[i];
      if (!isObject(entry)) {
        errors.push(`games[${i}] must be an object`);
        continue;
      }
      const requiredEntryStrings = ['guid', 'name', 'repo', 'branch'] as const;
      for (const field of requiredEntryStrings) {
        if (entry[field] === undefined || entry[field] === null) {
          errors.push(`games[${i}]: missing required field: ${field}`);
        } else if (typeof entry[field] !== 'string') {
          errors.push(`games[${i}]: field "${field}" must be a string`);
        }
      }
      // guid format
      if (typeof entry.guid === 'string' && !UUID_RE.test(entry.guid)) {
        errors.push(`games[${i}]: field "guid" must be a valid UUID`);
      }
      // optional manifestPath
      if (entry.manifestPath !== undefined && entry.manifestPath !== null && typeof entry.manifestPath !== 'string') {
        errors.push(`games[${i}]: field "manifestPath" must be a string`);
      }
    }
  }

  return { valid: errors.length === 0, errors };
}

// ---------------------------------------------------------------------------
// URL helpers
// ---------------------------------------------------------------------------

export function normalizeRepoUrl(repo: string): { owner: string; repo: string } {
  const trimmed = repo.trim();
  if (!trimmed) {
    throw new Error('Repo URL must not be empty');
  }

  // Full URL form
  if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) {
    let url: URL;
    try {
      url = new URL(trimmed);
    } catch {
      throw new Error(`Invalid repo URL: ${trimmed}`);
    }
    if (url.hostname !== 'github.com') {
      throw new Error(`Only GitHub URLs are supported, got: ${url.hostname}`);
    }
    const parts = url.pathname.replace(/^\//, '').replace(/\/$/, '').replace(/\.git$/, '').split('/');
    if (parts.length < 2 || !parts[0] || !parts[1]) {
      throw new Error(`Invalid GitHub URL — expected owner/repo in path: ${trimmed}`);
    }
    return { owner: parts[0], repo: parts[1] };
  }

  // Shorthand "owner/repo"
  const parts = trimmed.split('/');
  if (parts.length !== 2 || !parts[0] || !parts[1]) {
    throw new Error(`Invalid repo format — expected "owner/repo" or a GitHub URL: ${trimmed}`);
  }
  return { owner: parts[0], repo: parts[1] };
}

export function constructRawManifestUrl(
  owner: string,
  repo: string,
  branch: string,
  manifestPath?: string,
): string {
  const path = (manifestPath ?? 'octgn-manifest.json').replace(/^\//, '');
  return `https://raw.githubusercontent.com/${owner}/${repo}/${branch}/${path}`;
}

export function constructZipballUrl(owner: string, repo: string, branch: string): string {
  return `https://api.github.com/repos/${owner}/${repo}/zipball/${branch}`;
}
