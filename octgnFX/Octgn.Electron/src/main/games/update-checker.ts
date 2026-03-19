import type { AvailableGame, GameDefinition } from '../../shared/types';
import { compareVersions } from './game-feed';

export interface GameUpdate {
  gameId: string;
  gameName: string;
  installedVersion: string;
  availableVersion: string;
  sourceType: 'repo' | 'nuget' | 'local';
  sourceInfo?: string;
  downloadUrl: string;
}

/**
 * Compares installed games against available games and returns updates.
 * A game has an update if:
 * - Same GUID exists in both installed and available
 * - Available version is higher than installed version
 */
export function checkForUpdates(
  installed: GameDefinition[],
  available: AvailableGame[],
): GameUpdate[] {
  const availableMap = new Map<string, AvailableGame>();
  for (const game of available) {
    availableMap.set(game.id, game);
  }

  const updates: GameUpdate[] = [];

  for (const inst of installed) {
    const avail = availableMap.get(inst.id);
    if (!avail) continue;

    if (compareVersions(avail.version, inst.version) > 0) {
      updates.push({
        gameId: inst.id,
        gameName: avail.name,
        installedVersion: inst.version,
        availableVersion: avail.version,
        sourceType: (avail.sourceType as 'repo' | 'nuget' | 'local') ?? 'nuget',
        sourceInfo: avail.sourceInfo,
        downloadUrl: avail.downloadUrl,
      });
    }
  }

  return updates;
}
