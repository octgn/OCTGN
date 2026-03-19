/**
 * Tests for repo-feed-types.ts — schema validation, URL construction,
 * and type definitions for GitHub-based game distribution.
 */
import { describe, it, expect } from 'vitest';
import {
  validateManifest,
  validateFeedIndex,
  normalizeRepoUrl,
  constructRawManifestUrl,
  constructZipballUrl,
  GameSourceType,
} from '@main/games/repo-feed-types';

// ---------------------------------------------------------------------------
// validateManifest
// ---------------------------------------------------------------------------
describe('validateManifest', () => {
  const validManifest = {
    guid: 'a6c8d2e8-7e8d-44e4-ad6b-f9b3d2ea0c25',
    name: 'Android: Netrunner',
    version: '3.2.1',
    versionDate: '2026-01-15T12:00:00Z',
    description: 'A card game of asymmetric cyber-warfare',
    authors: ['NISEI', 'FFG'],
    gamePath: 'game/',
  };

  it('accepts a valid manifest with all required fields', () => {
    const result = validateManifest(validManifest);
    expect(result.valid).toBe(true);
    expect(result.errors).toHaveLength(0);
  });

  it('accepts a manifest with all optional fields', () => {
    const result = validateManifest({
      ...validManifest,
      minimumOctgnVersion: '3.4.400.0',
      tags: ['lcg', 'cyberpunk'],
      changelog: 'Fixed card images',
    });
    expect(result.valid).toBe(true);
    expect(result.errors).toHaveLength(0);
  });

  it('rejects null/undefined input', () => {
    expect(validateManifest(null).valid).toBe(false);
    expect(validateManifest(undefined).valid).toBe(false);
  });

  it('rejects non-object input', () => {
    expect(validateManifest('string').valid).toBe(false);
    expect(validateManifest(42).valid).toBe(false);
    expect(validateManifest([]).valid).toBe(false);
  });

  it('reports missing required fields', () => {
    const result = validateManifest({});
    expect(result.valid).toBe(false);
    expect(result.errors).toContain('Missing required field: guid');
    expect(result.errors).toContain('Missing required field: name');
    expect(result.errors).toContain('Missing required field: version');
    expect(result.errors).toContain('Missing required field: versionDate');
    expect(result.errors).toContain('Missing required field: description');
    expect(result.errors).toContain('Missing required field: authors');
    expect(result.errors).toContain('Missing required field: gamePath');
  });

  it('rejects wrong types for string fields', () => {
    const result = validateManifest({
      ...validManifest,
      guid: 123,
      name: true,
    });
    expect(result.valid).toBe(false);
    expect(result.errors).toContain('Field "guid" must be a string');
    expect(result.errors).toContain('Field "name" must be a string');
  });

  it('rejects non-array authors', () => {
    const result = validateManifest({
      ...validManifest,
      authors: 'single author',
    });
    expect(result.valid).toBe(false);
    expect(result.errors).toContain('Field "authors" must be an array of strings');
  });

  it('rejects authors with non-string elements', () => {
    const result = validateManifest({
      ...validManifest,
      authors: ['valid', 42],
    });
    expect(result.valid).toBe(false);
    expect(result.errors).toContain('Field "authors" must be an array of strings');
  });

  it('rejects empty authors array', () => {
    const result = validateManifest({
      ...validManifest,
      authors: [],
    });
    expect(result.valid).toBe(false);
    expect(result.errors).toContain('Field "authors" must have at least one entry');
  });

  it('rejects invalid guid format', () => {
    const result = validateManifest({
      ...validManifest,
      guid: 'not-a-guid',
    });
    expect(result.valid).toBe(false);
    expect(result.errors).toContain('Field "guid" must be a valid UUID');
  });

  it('accepts uppercase guid', () => {
    const result = validateManifest({
      ...validManifest,
      guid: 'A6C8D2E8-7E8D-44E4-AD6B-F9B3D2EA0C25',
    });
    expect(result.valid).toBe(true);
  });

  it('rejects non-array tags when provided', () => {
    const result = validateManifest({
      ...validManifest,
      tags: 'not-array',
    });
    expect(result.valid).toBe(false);
    expect(result.errors).toContain('Field "tags" must be an array of strings');
  });

  it('rejects non-string optional fields when provided', () => {
    const result = validateManifest({
      ...validManifest,
      minimumOctgnVersion: 123,
      changelog: false,
    });
    expect(result.valid).toBe(false);
    expect(result.errors).toContain('Field "minimumOctgnVersion" must be a string');
    expect(result.errors).toContain('Field "changelog" must be a string');
  });
});

// ---------------------------------------------------------------------------
// validateFeedIndex
// ---------------------------------------------------------------------------
describe('validateFeedIndex', () => {
  const validIndex = {
    name: 'OCTGN Community Games',
    games: [
      {
        guid: 'a6c8d2e8-7e8d-44e4-ad6b-f9b3d2ea0c25',
        name: 'Android: Netrunner',
        repo: 'octgn-community/anr-game-definition',
        branch: 'main',
      },
    ],
  };

  it('accepts a valid feed index', () => {
    const result = validateFeedIndex(validIndex);
    expect(result.valid).toBe(true);
    expect(result.errors).toHaveLength(0);
  });

  it('accepts a game entry with optional manifestPath', () => {
    const result = validateFeedIndex({
      name: 'Test Feed',
      games: [
        {
          guid: 'a6c8d2e8-7e8d-44e4-ad6b-f9b3d2ea0c25',
          name: 'Test Game',
          repo: 'owner/repo',
          branch: 'main',
          manifestPath: 'game/octgn-manifest.json',
        },
      ],
    });
    expect(result.valid).toBe(true);
  });

  it('accepts an empty games array', () => {
    const result = validateFeedIndex({ name: 'Empty Feed', games: [] });
    expect(result.valid).toBe(true);
  });

  it('rejects null/undefined input', () => {
    expect(validateFeedIndex(null).valid).toBe(false);
    expect(validateFeedIndex(undefined).valid).toBe(false);
  });

  it('rejects non-object input', () => {
    expect(validateFeedIndex('string').valid).toBe(false);
  });

  it('reports missing required fields', () => {
    const result = validateFeedIndex({});
    expect(result.valid).toBe(false);
    expect(result.errors).toContain('Missing required field: name');
    expect(result.errors).toContain('Missing required field: games');
  });

  it('rejects non-array games', () => {
    const result = validateFeedIndex({ name: 'Test', games: 'not-array' });
    expect(result.valid).toBe(false);
    expect(result.errors).toContain('Field "games" must be an array');
  });

  it('validates each game entry in the array', () => {
    const result = validateFeedIndex({
      name: 'Test Feed',
      games: [
        { guid: 'bad', name: 'Test', repo: 'owner/repo', branch: 'main' },
        { guid: 'a6c8d2e8-7e8d-44e4-ad6b-f9b3d2ea0c25', repo: 'owner/repo', branch: 'main' },
      ],
    });
    expect(result.valid).toBe(false);
    expect(result.errors.some((e: string) => e.includes('games[0]') && e.includes('guid'))).toBe(true);
    expect(result.errors.some((e: string) => e.includes('games[1]') && e.includes('name'))).toBe(true);
  });

  it('accepts full GitHub URLs in repo field', () => {
    const result = validateFeedIndex({
      name: 'Test Feed',
      games: [
        {
          guid: 'a6c8d2e8-7e8d-44e4-ad6b-f9b3d2ea0c25',
          name: 'Test Game',
          repo: 'https://github.com/owner/repo',
          branch: 'main',
        },
      ],
    });
    expect(result.valid).toBe(true);
  });
});

// ---------------------------------------------------------------------------
// normalizeRepoUrl
// ---------------------------------------------------------------------------
describe('normalizeRepoUrl', () => {
  it('parses "owner/repo" shorthand', () => {
    const result = normalizeRepoUrl('octgn-community/anr-game');
    expect(result).toEqual({ owner: 'octgn-community', repo: 'anr-game' });
  });

  it('parses full GitHub HTTPS URL', () => {
    const result = normalizeRepoUrl('https://github.com/octgn-community/anr-game');
    expect(result).toEqual({ owner: 'octgn-community', repo: 'anr-game' });
  });

  it('parses GitHub URL with trailing slash', () => {
    const result = normalizeRepoUrl('https://github.com/octgn-community/anr-game/');
    expect(result).toEqual({ owner: 'octgn-community', repo: 'anr-game' });
  });

  it('parses GitHub URL with .git suffix', () => {
    const result = normalizeRepoUrl('https://github.com/octgn-community/anr-game.git');
    expect(result).toEqual({ owner: 'octgn-community', repo: 'anr-game' });
  });

  it('throws on invalid format (no slash)', () => {
    expect(() => normalizeRepoUrl('justarepo')).toThrow();
  });

  it('throws on empty string', () => {
    expect(() => normalizeRepoUrl('')).toThrow();
  });

  it('throws on URL with wrong host', () => {
    expect(() => normalizeRepoUrl('https://gitlab.com/owner/repo')).toThrow();
  });

  it('strips whitespace', () => {
    const result = normalizeRepoUrl('  owner/repo  ');
    expect(result).toEqual({ owner: 'owner', repo: 'repo' });
  });
});

// ---------------------------------------------------------------------------
// constructRawManifestUrl
// ---------------------------------------------------------------------------
describe('constructRawManifestUrl', () => {
  it('builds URL with default manifest path', () => {
    const url = constructRawManifestUrl('octgn-community', 'anr-game', 'main');
    expect(url).toBe(
      'https://raw.githubusercontent.com/octgn-community/anr-game/main/octgn-manifest.json',
    );
  });

  it('builds URL with custom manifest path', () => {
    const url = constructRawManifestUrl('owner', 'repo', 'develop', 'game/manifest.json');
    expect(url).toBe(
      'https://raw.githubusercontent.com/owner/repo/develop/game/manifest.json',
    );
  });

  it('strips leading slash from manifestPath', () => {
    const url = constructRawManifestUrl('owner', 'repo', 'main', '/custom/manifest.json');
    expect(url).toBe(
      'https://raw.githubusercontent.com/owner/repo/main/custom/manifest.json',
    );
  });
});

// ---------------------------------------------------------------------------
// constructZipballUrl
// ---------------------------------------------------------------------------
describe('constructZipballUrl', () => {
  it('builds the GitHub API zipball URL', () => {
    const url = constructZipballUrl('octgn-community', 'anr-game', 'main');
    expect(url).toBe(
      'https://api.github.com/repos/octgn-community/anr-game/zipball/main',
    );
  });

  it('handles branch names with slashes', () => {
    const url = constructZipballUrl('owner', 'repo', 'feature/new-cards');
    expect(url).toBe(
      'https://api.github.com/repos/owner/repo/zipball/feature/new-cards',
    );
  });
});

// ---------------------------------------------------------------------------
// GameSourceType enum
// ---------------------------------------------------------------------------
describe('GameSourceType', () => {
  it('has correct priority values (lower = higher priority)', () => {
    expect(GameSourceType.LocalFolder).toBe(0);
    expect(GameSourceType.DirectRepo).toBe(1);
    expect(GameSourceType.UserFeed).toBe(2);
    expect(GameSourceType.BuiltInFeed).toBe(3);
    expect(GameSourceType.LegacyNuGet).toBe(4);
  });

  it('LocalFolder has highest priority', () => {
    expect(GameSourceType.LocalFolder).toBeLessThan(GameSourceType.DirectRepo);
    expect(GameSourceType.LocalFolder).toBeLessThan(GameSourceType.LegacyNuGet);
  });
});
