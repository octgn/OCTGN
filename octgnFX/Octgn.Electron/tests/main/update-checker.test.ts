import { describe, it, expect } from 'vitest';
import { checkForUpdates, type GameUpdate } from '../../src/main/games/update-checker';
import type { AvailableGame, GameDefinition } from '../../src/shared/types';

function makeInstalled(overrides: Partial<GameDefinition> & { id: string; name: string; version: string }): GameDefinition {
  return {
    description: '',
    cardWidth: 63,
    cardHeight: 88,
    cardBack: 'back.png',
    deckSections: [],
    sharedDeckSections: [],
    players: [],
    globalVariables: [],
    phases: [],
    ...overrides,
  };
}

function makeAvailable(overrides: Partial<AvailableGame> & { id: string; name: string; version: string; downloadUrl: string }): AvailableGame {
  return {
    description: '',
    authors: '',
    tags: '',
    ...overrides,
  };
}

describe('checkForUpdates', () => {
  it('returns update when available version is higher than installed', () => {
    const installed = [makeInstalled({ id: 'g1', name: 'Test Game', version: '1.0.0' })];
    const available = [makeAvailable({ id: 'g1', name: 'Test Game', version: '2.0.0', downloadUrl: 'http://dl/g1', sourceType: 'nuget' })];

    const updates = checkForUpdates(installed, available);

    expect(updates).toHaveLength(1);
    expect(updates[0]).toEqual<GameUpdate>({
      gameId: 'g1',
      gameName: 'Test Game',
      installedVersion: '1.0.0',
      availableVersion: '2.0.0',
      sourceType: 'nuget',
      sourceInfo: undefined,
      downloadUrl: 'http://dl/g1',
    });
  });

  it('returns no update when installed version is higher than available', () => {
    const installed = [makeInstalled({ id: 'g1', name: 'Test Game', version: '2.0.0' })];
    const available = [makeAvailable({ id: 'g1', name: 'Test Game', version: '1.0.0', downloadUrl: 'http://dl/g1' })];

    const updates = checkForUpdates(installed, available);

    expect(updates).toHaveLength(0);
  });

  it('returns no update when versions are the same', () => {
    const installed = [makeInstalled({ id: 'g1', name: 'Test Game', version: '1.0.0' })];
    const available = [makeAvailable({ id: 'g1', name: 'Test Game', version: '1.0.0', downloadUrl: 'http://dl/g1' })];

    const updates = checkForUpdates(installed, available);

    expect(updates).toHaveLength(0);
  });

  it('returns no update when game is only installed (not available)', () => {
    const installed = [makeInstalled({ id: 'g1', name: 'Test Game', version: '1.0.0' })];
    const available: AvailableGame[] = [];

    const updates = checkForUpdates(installed, available);

    expect(updates).toHaveLength(0);
  });

  it('returns no update when game is only available (not installed)', () => {
    const installed: GameDefinition[] = [];
    const available = [makeAvailable({ id: 'g1', name: 'Test Game', version: '2.0.0', downloadUrl: 'http://dl/g1' })];

    const updates = checkForUpdates(installed, available);

    expect(updates).toHaveLength(0);
  });

  it('handles multiple games with mixed update status', () => {
    const installed = [
      makeInstalled({ id: 'g1', name: 'Game A', version: '1.0.0' }),
      makeInstalled({ id: 'g2', name: 'Game B', version: '3.0.0' }),
      makeInstalled({ id: 'g3', name: 'Game C', version: '1.0.0' }),
    ];
    const available = [
      makeAvailable({ id: 'g1', name: 'Game A', version: '2.0.0', downloadUrl: 'http://dl/g1', sourceType: 'repo', sourceInfo: 'owner/repo' }),
      makeAvailable({ id: 'g2', name: 'Game B', version: '2.0.0', downloadUrl: 'http://dl/g2' }),
      makeAvailable({ id: 'g3', name: 'Game C', version: '1.5.0', downloadUrl: 'http://dl/g3', sourceType: 'nuget' }),
    ];

    const updates = checkForUpdates(installed, available);

    expect(updates).toHaveLength(2);
    expect(updates.find(u => u.gameId === 'g1')).toBeDefined();
    expect(updates.find(u => u.gameId === 'g2')).toBeUndefined(); // installed is newer
    expect(updates.find(u => u.gameId === 'g3')).toBeDefined();

    const g1Update = updates.find(u => u.gameId === 'g1')!;
    expect(g1Update.sourceType).toBe('repo');
    expect(g1Update.sourceInfo).toBe('owner/repo');
  });

  it('handles date-style versions (e.g. 2019.07.07.00)', () => {
    const installed = [makeInstalled({ id: 'g1', name: 'Test Game', version: '2019.07.07.00' })];
    const available = [makeAvailable({ id: 'g1', name: 'Test Game', version: '2020.01.15.00', downloadUrl: 'http://dl/g1' })];

    const updates = checkForUpdates(installed, available);

    expect(updates).toHaveLength(1);
    expect(updates[0].installedVersion).toBe('2019.07.07.00');
    expect(updates[0].availableVersion).toBe('2020.01.15.00');
  });

  it('handles date-style versions where installed is newer', () => {
    const installed = [makeInstalled({ id: 'g1', name: 'Test Game', version: '2020.01.15.00' })];
    const available = [makeAvailable({ id: 'g1', name: 'Test Game', version: '2019.07.07.00', downloadUrl: 'http://dl/g1' })];

    const updates = checkForUpdates(installed, available);

    expect(updates).toHaveLength(0);
  });
});
