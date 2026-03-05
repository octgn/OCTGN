import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, screen, cleanup, fireEvent } from '@testing-library/react';
import React from 'react';

// Mock window.octgn
(window as any).octgn = {
  login: vi.fn(), logout: vi.fn(), getSession: vi.fn(), getGames: vi.fn(),
  hostGame: vi.fn(), joinGame: vi.fn(), leaveGame: vi.fn(),
  minimize: vi.fn(), maximize: vi.fn(), quit: vi.fn(), getVersion: vi.fn(),
  onGameStateUpdate: vi.fn(() => vi.fn()), gameAction: vi.fn(),
  gameChat: vi.fn(), loadDeck: vi.fn(), openFileDialog: vi.fn(),
};

import HandZone from '@renderer/components/HandZone';
import { DragDropProvider } from '@renderer/components/DragDropContext';
import type { Card } from '@shared/types';

function makeCard(id: string, name: string): Card {
  return {
    id, definitionId: `def-${id}`, name,
    imageUrl: '', faceUp: true, position: { x: 0, y: 0 },
    rotation: 0, groupId: 'hand-1', ownerId: '1', markers: [],
    properties: {}, peekingPlayers: [],
    size: { width: 100, height: 140 },
  };
}

afterEach(cleanup);

const noop = () => {};

function renderHand(cards: Card[], overrides: Partial<React.ComponentProps<typeof HandZone>> = {}) {
  const props = {
    cards,
    handGroupId: 'hand-1',
    selectedCardIds: new Set<string>(),
    interactive: true,
    onCardClick: noop,
    onCardContextMenu: noop,
    onCardMoveToGroup: vi.fn(),
    onReorderCard: vi.fn(),
    ...overrides,
  };
  const result = render(
    <DragDropProvider>
      <HandZone {...props} />
    </DragDropProvider>,
  );
  return { ...result, onCardMoveToGroup: props.onCardMoveToGroup, onReorderCard: props.onReorderCard };
}

describe('HandZone reorder', () => {
  it('renders an insertion indicator when dragging within the hand', () => {
    const cards = [makeCard('a', 'Alpha'), makeCard('b', 'Beta'), makeCard('c', 'Charlie')];
    renderHand(cards);

    // Find the draggable card element inside the card wrapper
    const cardWrapper = screen.getByTestId('hand-card-a');
    const draggable = cardWrapper.querySelector('[draggable="true"]');
    expect(draggable).toBeTruthy();

    // Start drag — this fires CardComponent's onDragStart which calls DragDropContext.startDrag
    fireEvent.dragStart(draggable!, {
      dataTransfer: {
        effectAllowed: 'move',
        setData: vi.fn(),
        setDragImage: vi.fn(),
        getData: (key: string) => key === 'application/octgn-card' ? 'a' : 'hand-1',
      },
    });

    // Drag over the hand zone — triggers handleHandDragOver which sets insertIndex
    const handZone = screen.getByTestId('hand-zone');
    fireEvent.dragOver(handZone, {
      clientX: 300,
      dataTransfer: {
        effectAllowed: 'move',
        dropEffect: 'move',
        setData: vi.fn(),
        getData: (key: string) => key === 'application/octgn-card' ? 'a' : 'hand-1',
      },
    });

    // Should show an insertion indicator
    expect(screen.getByTestId('hand-insert-indicator')).toBeTruthy();
  });

  it('calls onReorderCard when dropping a card back on the same hand', () => {
    const cards = [makeCard('a', 'Alpha'), makeCard('b', 'Beta'), makeCard('c', 'Charlie')];
    const { onReorderCard, onCardMoveToGroup } = renderHand(cards);

    // Start drag on the actual draggable element
    const cardWrapper = screen.getByTestId('hand-card-a');
    const draggable = cardWrapper.querySelector('[draggable="true"]')!;

    fireEvent.dragStart(draggable, {
      dataTransfer: {
        effectAllowed: 'move',
        setData: vi.fn(),
        setDragImage: vi.fn(),
        getData: (key: string) => key === 'application/octgn-card' ? 'a' : 'hand-1',
      },
    });

    // Drop on the hand zone — the dataTransfer getData provides source zone
    const handZone = screen.getByTestId('hand-zone');
    fireEvent.drop(handZone, {
      clientX: 500,
      dataTransfer: {
        effectAllowed: 'move',
        dropEffect: 'move',
        setData: vi.fn(),
        getData: (key: string) => {
          if (key === 'application/octgn-card') return 'a';
          if (key === 'application/octgn-zone') return 'hand-1';
          return '';
        },
      },
    });

    // Should call reorder, NOT moveToGroup
    expect(onReorderCard).toHaveBeenCalledWith('a', expect.any(Number));
    expect(onCardMoveToGroup).not.toHaveBeenCalled();
  });

  it('does NOT call onReorderCard when dropping from a different zone', () => {
    const cards = [makeCard('a', 'Alpha'), makeCard('b', 'Beta')];
    const { onReorderCard, onCardMoveToGroup } = renderHand(cards);

    // Drop with a different source zone
    const handZone = screen.getByTestId('hand-zone');
    fireEvent.drop(handZone, {
      dataTransfer: {
        effectAllowed: 'move',
        dropEffect: 'move',
        setData: vi.fn(),
        getData: (key: string) => {
          if (key === 'application/octgn-card') return 'x';
          if (key === 'application/octgn-zone') return 'other-zone';
          return '';
        },
      },
    });

    expect(onReorderCard).not.toHaveBeenCalled();
    expect(onCardMoveToGroup).toHaveBeenCalledWith('x', 'hand-1');
  });

  it('does not show insertion indicator when not interactive', () => {
    const cards = [makeCard('a', 'Alpha'), makeCard('b', 'Beta')];
    renderHand(cards, { interactive: false });

    const handZone = screen.getByTestId('hand-zone');
    fireEvent.dragOver(handZone, {
      clientX: 300,
      dataTransfer: { effectAllowed: 'move', dropEffect: 'move', setData: vi.fn(), getData: vi.fn(() => '') },
    });

    expect(screen.queryByTestId('hand-insert-indicator')).toBeNull();
  });

  it('each card wrapper has a data-testid for targeting', () => {
    const cards = [makeCard('a', 'Alpha'), makeCard('b', 'Beta')];
    renderHand(cards);

    expect(screen.getByTestId('hand-card-a')).toBeTruthy();
    expect(screen.getByTestId('hand-card-b')).toBeTruthy();
  });

  it('keeps insertion indicator visible during dragOver even after child element boundaries', () => {
    const cards = [makeCard('a', 'Alpha'), makeCard('b', 'Beta'), makeCard('c', 'Charlie')];
    renderHand(cards);

    // Start drag
    const cardWrapper = screen.getByTestId('hand-card-a');
    const draggable = cardWrapper.querySelector('[draggable="true"]')!;
    fireEvent.dragStart(draggable, {
      dataTransfer: {
        effectAllowed: 'move',
        setData: vi.fn(),
        setDragImage: vi.fn(),
        getData: vi.fn(() => ''),
      },
    });

    // Drag over hand zone to show indicator
    const handZone = screen.getByTestId('hand-zone');
    fireEvent.dragOver(handZone, {
      clientX: 300,
      dataTransfer: { effectAllowed: 'move', dropEffect: 'move', setData: vi.fn(), getData: vi.fn(() => '') },
    });

    expect(screen.getByTestId('hand-insert-indicator')).toBeTruthy();

    // Fire another dragOver at a different position — indicator should still be there
    fireEvent.dragOver(handZone, {
      clientX: 400,
      dataTransfer: { effectAllowed: 'move', dropEffect: 'move', setData: vi.fn(), getData: vi.fn(() => '') },
    });

    expect(screen.queryByTestId('hand-insert-indicator')).toBeTruthy();
  });

  it('clears insertion indicator on dragEnd', () => {
    const cards = [makeCard('a', 'Alpha'), makeCard('b', 'Beta'), makeCard('c', 'Charlie')];
    renderHand(cards);

    // Start drag
    const cardWrapper = screen.getByTestId('hand-card-a');
    const draggable = cardWrapper.querySelector('[draggable="true"]')!;
    fireEvent.dragStart(draggable, {
      dataTransfer: {
        effectAllowed: 'move',
        setData: vi.fn(),
        setDragImage: vi.fn(),
        getData: vi.fn(() => ''),
      },
    });

    // Drag over hand zone to show indicator
    const handZone = screen.getByTestId('hand-zone');
    fireEvent.dragOver(handZone, {
      clientX: 300,
      dataTransfer: { effectAllowed: 'move', dropEffect: 'move', setData: vi.fn(), getData: vi.fn(() => '') },
    });

    expect(screen.getByTestId('hand-insert-indicator')).toBeTruthy();

    // End the drag (cancel)
    fireEvent.dragEnd(draggable, {
      dataTransfer: { effectAllowed: 'move', setData: vi.fn(), getData: vi.fn(() => '') },
    });

    // Indicator should be gone
    expect(screen.queryByTestId('hand-insert-indicator')).toBeNull();
  });
});
