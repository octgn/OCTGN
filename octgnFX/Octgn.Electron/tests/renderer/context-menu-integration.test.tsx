import { describe, it, expect, vi, afterEach, beforeEach } from 'vitest';
import { render, screen, fireEvent, cleanup, renderHook } from '@testing-library/react';
import React from 'react';
import ContextMenu from '../../src/renderer/components/ContextMenu';
import { useContextMenuItems } from '../../src/renderer/hooks/useContextMenuItems';
import type { Card, GameState, Group, Player } from '../../src/shared/types';

// Mock window.octgn
const mockExecuteAction = vi.fn();
beforeEach(() => {
  (globalThis as any).window = {
    ...(globalThis as any).window,
    octgn: {
      executeAction: mockExecuteAction,
    },
  };
});

afterEach(() => {
  cleanup();
  vi.clearAllMocks();
});

function makeGameState(overrides: Partial<GameState> = {}): GameState {
  const localPlayerId = 1;
  const group: Group = {
    id: '10',
    name: 'Hand',
    cards: [],
    visibility: 2,
    controller: localPlayerId,
  };
  const player: Player = {
    id: localPlayerId,
    name: 'Player 1',
    color: '#ff0000',
    groups: [group],
    counters: [],
    globalVariables: [],
    isSubscriber: false,
    invertedTable: false,
  };
  return {
    gameName: 'Test Game',
    localPlayerId,
    players: [player],
    table: { cards: [], width: 800, height: 600 },
    chatMessages: [],
    turnNumber: 1,
    activePlayer: 1,
    isStarted: true,
    isSpectator: false,
    connectionStatus: 'connected' as const,
    useTwoSidedTable: false,
    actionDefs: {
      Hand: {
        cardActions: [
          {
            type: 'action',
            actionType: 'card',
            action: { name: 'Capture piece', shortcut: 'Del', execute: 'kill' },
          },
        ],
        groupActions: [
          {
            type: 'action',
            actionType: 'group',
            action: { name: 'Draw card', execute: 'draw' },
          },
        ],
      },
    },
    ...overrides,
  } as GameState;
}

function makeCard(overrides: Partial<Card> = {}): Card {
  return {
    id: '42',
    name: 'Test Card',
    faceUp: true,
    x: 0,
    y: 0,
    rotation: 0,
    ownerId: '1',
    controllerId: '1',
    imageUrl: '',
    size: { width: 63, height: 88 },
    markers: [],
    ...overrides,
  } as Card;
}

describe('Context Menu Integration', () => {
  it('built-in Flip action calls flipCard callback when clicked', () => {
    const flipCard = vi.fn();
    const rotateCard = vi.fn();
    const peekCard = vi.fn();
    const moveCards = vi.fn();
    const gameState = makeGameState();
    const card = makeCard({ faceUp: true });

    // Put card in the Hand group
    gameState.players[0].groups[0].cards = [card];

    const { result } = renderHook(() => useContextMenuItems());

    const items = result.current.buildCardMenuItems(card, gameState, {
      flipCard,
      rotateCard,
      peekCard,
      moveCards,
    });

    // Render context menu with the built items
    const onClose = vi.fn();
    render(<ContextMenu x={100} y={100} items={items} onClose={onClose} />);

    // Click "Flip Face Down"
    const flipButton = screen.getByText('Flip Face Down');
    fireEvent.click(flipButton);

    expect(flipCard).toHaveBeenCalledTimes(1);
    expect(flipCard).toHaveBeenCalledWith(42, false);
  });

  it('built-in Peek action calls peekCard callback when clicked', () => {
    const flipCard = vi.fn();
    const rotateCard = vi.fn();
    const peekCard = vi.fn();
    const moveCards = vi.fn();
    const gameState = makeGameState();
    const card = makeCard({ faceUp: false }); // face down so Peek is available

    gameState.players[0].groups[0].cards = [card];

    const { result } = renderHook(() => useContextMenuItems());

    const items = result.current.buildCardMenuItems(card, gameState, {
      flipCard,
      rotateCard,
      peekCard,
      moveCards,
    });

    const onClose = vi.fn();
    render(<ContextMenu x={100} y={100} items={items} onClose={onClose} />);

    const peekButton = screen.getByText('Peek');
    fireEvent.click(peekButton);

    expect(peekCard).toHaveBeenCalledTimes(1);
    expect(peekCard).toHaveBeenCalledWith(42);
  });

  it('game-defined card action calls executeAction when clicked', () => {
    const flipCard = vi.fn();
    const rotateCard = vi.fn();
    const peekCard = vi.fn();
    const moveCards = vi.fn();
    const gameState = makeGameState();
    const card = makeCard();

    gameState.players[0].groups[0].cards = [card];

    const { result } = renderHook(() => useContextMenuItems());

    const items = result.current.buildCardMenuItems(card, gameState, {
      flipCard,
      rotateCard,
      peekCard,
      moveCards,
    });

    const onClose = vi.fn();
    render(<ContextMenu x={100} y={100} items={items} onClose={onClose} />);

    // Click game-defined action "Capture piece"
    const captureButton = screen.getByText('Capture piece');
    fireEvent.click(captureButton);

    expect(mockExecuteAction).toHaveBeenCalledTimes(1);
    expect(mockExecuteAction).toHaveBeenCalledWith({
      type: 'card',
      action: { name: 'Capture piece', shortcut: 'Del', execute: 'kill' },
      cardId: 42,
      groupId: 10,
    });
  });

  it('group context menu shuffle action calls shuffleGroup when clicked', () => {
    const shuffleGroup = vi.fn();
    const gameState = makeGameState();
    const group: Group = {
      id: '20',
      name: 'Deck',
      cards: [],
      visibility: 1,
      controller: 1,
    };

    // Add actionDefs for Deck group
    gameState.actionDefs = {
      ...gameState.actionDefs,
      Deck: { cardActions: [], groupActions: [] },
    };

    const { result } = renderHook(() => useContextMenuItems());

    const items = result.current.buildGroupMenuItems(group, gameState, { shuffleGroup });

    const onClose = vi.fn();
    render(<ContextMenu x={100} y={100} items={items} onClose={onClose} />);

    const shuffleButton = screen.getByText('Shuffle');
    fireEvent.click(shuffleButton);

    expect(shuffleGroup).toHaveBeenCalledTimes(1);
    expect(shuffleGroup).toHaveBeenCalledWith(20);
  });

  it('onClose is called after action item is clicked', () => {
    const flipCard = vi.fn();
    const gameState = makeGameState();
    const card = makeCard();

    gameState.players[0].groups[0].cards = [card];

    const { result } = renderHook(() => useContextMenuItems());

    const items = result.current.buildCardMenuItems(card, gameState, {
      flipCard,
      rotateCard: vi.fn(),
      peekCard: vi.fn(),
      moveCards: vi.fn(),
    });

    const onClose = vi.fn();
    render(<ContextMenu x={100} y={100} items={items} onClose={onClose} />);

    fireEvent.click(screen.getByText('Flip Face Down'));

    expect(onClose).toHaveBeenCalledTimes(1);
  });
});
