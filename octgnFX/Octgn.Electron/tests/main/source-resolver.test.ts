import { describe, it, expect } from 'vitest';
import {
  resolveGames,
  mergeWithLegacy,
  GameSourcePriority,
  type ResolvedGame,
} from '../../src/main/games/source-resolver';
import type { AvailableGame } from '../../src/shared/types';

function makeGame(overrides: Partial<AvailableGame> = {}): AvailableGame {
  return {
    id: 'aaaa-bbbb-cccc',
    name: 'Test Game',
    version: '1.0.0',
    description: 'A test game',
    authors: 'Test Author',
    tags: 'test',
    downloadUrl: 'https://example.com/test.nupkg',
    downloadCount: 100,
    ...overrides,
  };
}

function makeResolved(
  game: Partial<AvailableGame>,
  priority: GameSourcePriority,
  sourceName: string,
): ResolvedGame {
  return {
    ...makeGame(game),
    sourcePriority: priority,
    sourceName,
  };
}

describe('resolveGames', () => {
  it('returns empty array for empty input', () => {
    expect(resolveGames([])).toEqual([]);
  });

  it('returns single source as-is', () => {
    const games = [
      makeResolved({ id: 'game-1', name: 'Alpha' }, GameSourcePriority.BuiltInFeed, 'default'),
    ];
    const result = resolveGames(games);
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe('game-1');
  });

  it('LocalFolder wins over BuiltInFeed for same game', () => {
    const games = [
      makeResolved({ id: 'game-1', name: 'My Game', version: '1.0.0' }, GameSourcePriority.BuiltInFeed, 'octgn-feed'),
      makeResolved({ id: 'game-1', name: 'My Game', version: '1.0.0' }, GameSourcePriority.LocalFolder, 'local-dev'),
    ];
    const result = resolveGames(games);
    expect(result).toHaveLength(1);
    expect(result[0].sourceInfo).toContain('local-dev');
  });

  it('DirectRepo wins over UserFeed for same game', () => {
    const games = [
      makeResolved({ id: 'game-1', name: 'My Game', version: '2.0.0' }, GameSourcePriority.UserFeed, 'community-feed'),
      makeResolved({ id: 'game-1', name: 'My Game', version: '1.0.0' }, GameSourcePriority.DirectRepo, 'my-repo'),
    ];
    const result = resolveGames(games);
    expect(result).toHaveLength(1);
    expect(result[0].sourceInfo).toContain('my-repo');
    // DirectRepo wins even though UserFeed has higher version
    expect(result[0].version).toBe('1.0.0');
  });

  it('within same priority tier, higher version wins', () => {
    const games = [
      makeResolved({ id: 'game-1', name: 'My Game', version: '1.0.0' }, GameSourcePriority.BuiltInFeed, 'feed-a'),
      makeResolved({ id: 'game-1', name: 'My Game', version: '2.5.0' }, GameSourcePriority.BuiltInFeed, 'feed-b'),
    ];
    const result = resolveGames(games);
    expect(result).toHaveLength(1);
    expect(result[0].version).toBe('2.5.0');
    expect(result[0].sourceInfo).toContain('feed-b');
  });

  it('different games from different sources are all included', () => {
    const games = [
      makeResolved({ id: 'game-1', name: 'Alpha' }, GameSourcePriority.LocalFolder, 'local'),
      makeResolved({ id: 'game-2', name: 'Beta' }, GameSourcePriority.BuiltInFeed, 'feed'),
      makeResolved({ id: 'game-3', name: 'Charlie' }, GameSourcePriority.LegacyNuGet, 'myget'),
    ];
    const result = resolveGames(games);
    expect(result).toHaveLength(3);
  });

  it('returns results sorted by name alphabetically', () => {
    const games = [
      makeResolved({ id: 'game-3', name: 'Zebra' }, GameSourcePriority.BuiltInFeed, 'feed'),
      makeResolved({ id: 'game-1', name: 'Alpha' }, GameSourcePriority.BuiltInFeed, 'feed'),
      makeResolved({ id: 'game-2', name: 'Middle' }, GameSourcePriority.BuiltInFeed, 'feed'),
    ];
    const result = resolveGames(games);
    expect(result.map((g) => g.name)).toEqual(['Alpha', 'Middle', 'Zebra']);
  });
});

describe('mergeWithLegacy', () => {
  it('game in both repo and NuGet: repo version wins', () => {
    const repoGames: AvailableGame[] = [
      makeGame({ id: 'game-1', name: 'Repo Game', version: '1.0.0' }),
    ];
    const nugetGames: AvailableGame[] = [
      makeGame({ id: 'game-1', name: 'NuGet Game', version: '2.0.0', downloadCount: 500 }),
    ];
    const result = mergeWithLegacy(repoGames, nugetGames);
    expect(result).toHaveLength(1);
    expect(result[0].name).toBe('Repo Game');
    expect(result[0].sourceType).toBe('repo');
  });

  it('game only in NuGet is included', () => {
    const repoGames: AvailableGame[] = [];
    const nugetGames: AvailableGame[] = [
      makeGame({ id: 'nuget-only', name: 'NuGet Only', downloadCount: 200 }),
    ];
    const result = mergeWithLegacy(repoGames, nugetGames);
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe('nuget-only');
    expect(result[0].sourceType).toBe('nuget');
  });

  it('game only in repo is included', () => {
    const repoGames: AvailableGame[] = [
      makeGame({ id: 'repo-only', name: 'Repo Only' }),
    ];
    const nugetGames: AvailableGame[] = [];
    const result = mergeWithLegacy(repoGames, nugetGames);
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe('repo-only');
    expect(result[0].sourceType).toBe('repo');
  });

  it('mixed scenario with multiple games', () => {
    const repoGames: AvailableGame[] = [
      makeGame({ id: 'shared', name: 'Shared Game', downloadCount: 50 }),
      makeGame({ id: 'repo-only', name: 'Repo Exclusive', downloadCount: 10 }),
    ];
    const nugetGames: AvailableGame[] = [
      makeGame({ id: 'shared', name: 'Shared Game NuGet', downloadCount: 999 }),
      makeGame({ id: 'nuget-only', name: 'NuGet Exclusive', downloadCount: 300 }),
    ];
    const result = mergeWithLegacy(repoGames, nugetGames);
    expect(result).toHaveLength(3);
    // shared game should be repo version
    const shared = result.find((g) => g.id === 'shared');
    expect(shared?.name).toBe('Shared Game');
    expect(shared?.sourceType).toBe('repo');
    // both exclusives present
    expect(result.find((g) => g.id === 'repo-only')).toBeDefined();
    expect(result.find((g) => g.id === 'nuget-only')).toBeDefined();
  });

  it('NuGet games get sourceType nuget tag', () => {
    const nugetGames: AvailableGame[] = [
      makeGame({ id: 'g1', name: 'Game 1' }),
      makeGame({ id: 'g2', name: 'Game 2', sourceType: undefined }),
    ];
    const result = mergeWithLegacy([], nugetGames);
    expect(result.every((g) => g.sourceType === 'nuget')).toBe(true);
  });

  it('returns sorted by download count descending', () => {
    const repoGames: AvailableGame[] = [
      makeGame({ id: 'r1', name: 'Low Count', downloadCount: 10 }),
    ];
    const nugetGames: AvailableGame[] = [
      makeGame({ id: 'n1', name: 'High Count', downloadCount: 1000 }),
      makeGame({ id: 'n2', name: 'Mid Count', downloadCount: 500 }),
    ];
    const result = mergeWithLegacy(repoGames, nugetGames);
    const counts = result.map((g) => g.downloadCount ?? 0);
    expect(counts).toEqual([...counts].sort((a, b) => b - a));
  });
});
