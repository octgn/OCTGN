import { describe, it, expect, vi, beforeEach } from 'vitest';
import type { GameState, Card, Group, Player } from '@shared/types';
import { GroupVisibility } from '@shared/types';

// Mock window.octgn before importing store
const mockOctgn = {
  login: vi.fn(), logout: vi.fn(), getSession: vi.fn(), getGames: vi.fn(),
  hostGame: vi.fn(), joinGame: vi.fn(), leaveGame: vi.fn(),
  minimize: vi.fn(), maximize: vi.fn(), quit: vi.fn(), getVersion: vi.fn(),
  onGameStateUpdate: vi.fn(() => vi.fn()), gameAction: vi.fn(),
  gameChat: vi.fn(), loadDeck: vi.fn(), getDeckPaths: vi.fn(),
  gameSettings: vi.fn(), gamePlayerSettings: vi.fn(), bootPlayer: vi.fn(),
  startGame: vi.fn(), openFileDialog: vi.fn(),
};

Object.defineProperty(globalThis, 'window', {
  value: { ...globalThis.window, octgn: mockOctgn },
  writable: true,
});

import { useGameStore } from '@renderer/stores/game-store';

function makeCard(id: string, groupId: string): Card {
  return {
    id, definitionId: `def-${id}`, name: `Card ${id}`,
    imageUrl: '', faceUp: true, position: { x: 0, y: 0 },
    rotation: 0, groupId, ownerId: '1', markers: [],
    properties: {}, peekingPlayers: [],
    size: { width: 100, height: 140 },
  };
}

function makeGameState(handCards: Card[]): GameState {
  return {
    players: [{
      id: 1, name: 'Alice', color: '#ff0000',
      isHost: false, isSpectator: false,
      groups: [{
        id: 'hand-1', name: 'Hand', cards: handCards,
        visibility: GroupVisibility.Owner, controller: 1,
      }],
      counters: [], globalVariables: {},
    }],
    table: { cards: [] },
    isStarted: true,
    localPlayerId: 1,
    turnNumber: 1,
    activePlayerId: 1,
    chat: [],
    phase: '',
    settings: {
      twoSidedTable: false,
      allowSpectators: true,
      muteSpectators: false,
      allowCardList: false,
    },
  };
}

describe('game-store reorderHandCard', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should move a card from index 0 to index 2', () => {
    const cards = [makeCard('a', 'hand-1'), makeCard('b', 'hand-1'), makeCard('c', 'hand-1')];
    useGameStore.setState({ gameState: makeGameState(cards) });

    useGameStore.getState().reorderHandCard('hand-1', 'a', 2);

    const group = useGameStore.getState().gameState!.players[0].groups[0];
    expect(group.cards.map(c => c.id)).toEqual(['b', 'c', 'a']);
  });

  it('should move a card from index 2 to index 0', () => {
    const cards = [makeCard('a', 'hand-1'), makeCard('b', 'hand-1'), makeCard('c', 'hand-1')];
    useGameStore.setState({ gameState: makeGameState(cards) });

    useGameStore.getState().reorderHandCard('hand-1', 'c', 0);

    const group = useGameStore.getState().gameState!.players[0].groups[0];
    expect(group.cards.map(c => c.id)).toEqual(['c', 'a', 'b']);
  });

  it('should be a no-op when moving card to same position', () => {
    const cards = [makeCard('a', 'hand-1'), makeCard('b', 'hand-1'), makeCard('c', 'hand-1')];
    useGameStore.setState({ gameState: makeGameState(cards) });

    useGameStore.getState().reorderHandCard('hand-1', 'b', 1);

    const group = useGameStore.getState().gameState!.players[0].groups[0];
    expect(group.cards.map(c => c.id)).toEqual(['a', 'b', 'c']);
  });

  it('should not send any server action (local-only reorder)', () => {
    const cards = [makeCard('a', 'hand-1'), makeCard('b', 'hand-1')];
    useGameStore.setState({ gameState: makeGameState(cards) });

    useGameStore.getState().reorderHandCard('hand-1', 'a', 1);

    expect(mockOctgn.gameAction).not.toHaveBeenCalled();
  });

  it('should handle moving to the last position', () => {
    const cards = [makeCard('a', 'hand-1'), makeCard('b', 'hand-1'), makeCard('c', 'hand-1'), makeCard('d', 'hand-1')];
    useGameStore.setState({ gameState: makeGameState(cards) });

    useGameStore.getState().reorderHandCard('hand-1', 'b', 3);

    const group = useGameStore.getState().gameState!.players[0].groups[0];
    expect(group.cards.map(c => c.id)).toEqual(['a', 'c', 'd', 'b']);
  });

  it('should do nothing if card not found in group', () => {
    const cards = [makeCard('a', 'hand-1'), makeCard('b', 'hand-1')];
    useGameStore.setState({ gameState: makeGameState(cards) });

    useGameStore.getState().reorderHandCard('hand-1', 'nonexistent', 0);

    const group = useGameStore.getState().gameState!.players[0].groups[0];
    expect(group.cards.map(c => c.id)).toEqual(['a', 'b']);
  });

  it('should do nothing if group not found', () => {
    const cards = [makeCard('a', 'hand-1')];
    useGameStore.setState({ gameState: makeGameState(cards) });

    useGameStore.getState().reorderHandCard('nonexistent-group', 'a', 0);

    const group = useGameStore.getState().gameState!.players[0].groups[0];
    expect(group.cards.map(c => c.id)).toEqual(['a']);
  });

  it('should clamp newIndex to valid range', () => {
    const cards = [makeCard('a', 'hand-1'), makeCard('b', 'hand-1'), makeCard('c', 'hand-1')];
    useGameStore.setState({ gameState: makeGameState(cards) });

    useGameStore.getState().reorderHandCard('hand-1', 'a', 99);

    const group = useGameStore.getState().gameState!.players[0].groups[0];
    expect(group.cards.map(c => c.id)).toEqual(['b', 'c', 'a']);
  });
});
