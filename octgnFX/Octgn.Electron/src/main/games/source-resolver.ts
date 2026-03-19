import type { AvailableGame } from '../../shared/types';
import { compareVersions } from './game-feed';

export enum GameSourcePriority {
  LocalFolder = 0,
  DirectRepo = 1,
  UserFeed = 2,
  BuiltInFeed = 3,
  LegacyNuGet = 4,
}

export interface ResolvedGame extends AvailableGame {
  sourcePriority: GameSourcePriority;
  sourceName: string;
}

/**
 * Takes games from multiple sources (each tagged with priority + source name)
 * and deduplicates by game GUID using the priority rules:
 * - Higher priority source wins (lower number = higher priority)
 * - Within the same priority tier, prefer higher version
 * - Returns sorted by name alphabetically
 */
export function resolveGames(sources: ResolvedGame[]): AvailableGame[] {
  if (sources.length === 0) return [];

  const byId = new Map<string, ResolvedGame>();

  for (const game of sources) {
    const existing = byId.get(game.id);
    if (!existing) {
      byId.set(game.id, game);
      continue;
    }

    if (game.sourcePriority < existing.sourcePriority) {
      byId.set(game.id, game);
    } else if (
      game.sourcePriority === existing.sourcePriority &&
      compareVersions(game.version, existing.version) > 0
    ) {
      byId.set(game.id, game);
    }
  }

  return Array.from(byId.values())
    .map((g) => {
      const { sourcePriority, sourceName, ...base } = g;
      return { ...base, sourceInfo: `${sourceName} (priority ${sourcePriority})` };
    })
    .sort((a, b) => a.name.localeCompare(b.name));
}

/**
 * Merges games from the new repo-based system with legacy NuGet games.
 * - repoGames: games from repo feeds/direct repos (already resolved within repo sources)
 * - nugetGames: games from existing NuGet feeds
 * Returns: unified list with repo games winning over NuGet for same GUID,
 *          sorted by download count descending.
 */
export function mergeWithLegacy(
  repoGames: AvailableGame[],
  nugetGames: AvailableGame[],
): AvailableGame[] {
  const merged = new Map<string, AvailableGame>();

  // Repo games take priority -- add them first
  for (const game of repoGames) {
    merged.set(game.id, { ...game, sourceType: game.sourceType ?? 'repo' });
  }

  // NuGet games only included if not already present from repo
  for (const game of nugetGames) {
    if (!merged.has(game.id)) {
      merged.set(game.id, { ...game, sourceType: 'nuget' });
    }
  }

  return Array.from(merged.values()).sort(
    (a, b) => (b.downloadCount ?? 0) - (a.downloadCount ?? 0),
  );
}
