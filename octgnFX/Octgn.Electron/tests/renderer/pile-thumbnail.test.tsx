import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, screen, fireEvent, cleanup } from '@testing-library/react';
import React from 'react';

// Mock window.octgn
(window as any).octgn = {
  login: vi.fn(), logout: vi.fn(), getSession: vi.fn(), getGames: vi.fn(),
  hostGame: vi.fn(), joinGame: vi.fn(), leaveGame: vi.fn(),
  minimize: vi.fn(), maximize: vi.fn(), quit: vi.fn(), getVersion: vi.fn(),
  onGameStateUpdate: vi.fn(() => vi.fn()), gameAction: vi.fn(),
  gameChat: vi.fn(), loadDeck: vi.fn(), openFileDialog: vi.fn(),
};

import PileThumbnail from '@renderer/components/PileThumbnail';
import { DragDropProvider } from '@renderer/components/DragDropContext';
import type { Card, Group } from '@shared/types';
import { GroupVisibility } from '@shared/types';

function makeCard(overrides: Partial<Card> = {}): Card {
  return {
    id: 'card-1',
    definitionId: 'def-1',
    name: 'Test Card',
    imageUrl: '',
    faceUp: true,
    position: { x: 0, y: 0 },
    rotation: 0,
    groupId: 'group-1',
    ownerId: 'player-1',
    markers: [],
    properties: {},
    peekingPlayers: [],
    size: { width: 100, height: 140 },
    ...overrides,
  };
}

function makeGroup(overrides: Partial<Group> = {}): Group {
  return {
    id: 'group-1',
    name: 'Deck',
    cards: [],
    visibility: GroupVisibility.Owner,
    controller: 1,
    ...overrides,
  };
}

const noop = () => {};

function renderPileThumbnail(props: Partial<React.ComponentProps<typeof PileThumbnail>> = {}) {
  const defaults: React.ComponentProps<typeof PileThumbnail> = {
    group: makeGroup({ cards: [makeCard({ id: 'top-card' }), makeCard({ id: 'bottom-card' })] }),
    isOwn: true,
    onPileClick: noop,
    onCardMoveToGroup: noop,
    ...props,
  };

  return render(
    <DragDropProvider>
      <PileThumbnail {...defaults} />
    </DragDropProvider>,
  );
}

describe('PileThumbnail', () => {
  afterEach(cleanup);

  // ─── Basic rendering ─────────────────────────────────────────────

  it('renders the pile with test id', () => {
    renderPileThumbnail();
    expect(screen.getByTestId('pile-group-1')).toBeTruthy();
  });

  it('displays the group name', () => {
    renderPileThumbnail({ group: makeGroup({ name: 'Discard', cards: [makeCard()] }) });
    expect(screen.getByText('Discard')).toBeTruthy();
  });

  it('shows the card count', () => {
    const group = makeGroup({
      cards: [makeCard({ id: 'c1' }), makeCard({ id: 'c2' }), makeCard({ id: 'c3' })],
    });
    renderPileThumbnail({ group });
    expect(screen.getByText('3')).toBeTruthy();
  });

  it('shows empty placeholder when group has no cards', () => {
    renderPileThumbnail({ group: makeGroup({ cards: [] }) });
    expect(screen.getByText('0')).toBeTruthy();
  });

  it('calls onPileClick when clicked', () => {
    const onPileClick = vi.fn();
    const group = makeGroup({ cards: [makeCard()] });
    renderPileThumbnail({ group, onPileClick });

    fireEvent.click(screen.getByTestId('pile-group-1'));
    expect(onPileClick).toHaveBeenCalledWith(group);
  });

  // ─── Drag from pile (top card) ─────────────────────────────────

  it('makes the top card draggable when isOwn is true', () => {
    renderPileThumbnail({ isOwn: true });

    // The top card should have a draggable inner element
    const pile = screen.getByTestId('pile-group-1');
    const draggables = pile.querySelectorAll('[draggable="true"]');
    expect(draggables.length).toBeGreaterThan(0);
  });

  it('does not make the top card draggable when isOwn is false', () => {
    renderPileThumbnail({ isOwn: false });

    const pile = screen.getByTestId('pile-group-1');
    const draggables = pile.querySelectorAll('[draggable="true"]');
    expect(draggables.length).toBe(0);
  });

  it('fires dragStart on the top card when isOwn', () => {
    renderPileThumbnail({ isOwn: true });

    const pile = screen.getByTestId('pile-group-1');
    const draggable = pile.querySelector('[draggable="true"]');
    expect(draggable).toBeTruthy();

    // Simulate dragStart — dataTransfer should be populated
    const dataTransferData: Record<string, string> = {};
    const dragEvent = new Event('dragstart', { bubbles: true }) as any;
    dragEvent.dataTransfer = {
      effectAllowed: '',
      setData: (key: string, val: string) => { dataTransferData[key] = val; },
      setDragImage: vi.fn(),
    };
    draggable!.dispatchEvent(dragEvent);

    // CardComponent always sets text/plain to card.id
    expect(dataTransferData['text/plain']).toBe('top-card');
    // DragDropContext sets application/octgn-card
    expect(dataTransferData['application/octgn-card']).toBe('top-card');
  });

  // ─── Drop onto pile ────────────────────────────────────────────

  it('accepts drop when isOwn and calls onCardMoveToGroup', () => {
    const onCardMoveToGroup = vi.fn();
    const group = makeGroup({ id: 'target-pile', cards: [makeCard()] });
    renderPileThumbnail({ group, isOwn: true, onCardMoveToGroup });

    const pile = screen.getByTestId('pile-target-pile');

    fireEvent.dragOver(pile, {
      dataTransfer: { dropEffect: '', effectAllowed: 'move' },
    });
    fireEvent.drop(pile, {
      dataTransfer: { getData: (key: string) => key === 'application/octgn-card' ? 'dragged-card-id' : '' },
    });

    expect(onCardMoveToGroup).toHaveBeenCalledWith('dragged-card-id', 'target-pile');
  });

  it('does not accept drop when isOwn is false', () => {
    const onCardMoveToGroup = vi.fn();
    const group = makeGroup({ id: 'target-pile', cards: [makeCard()] });
    renderPileThumbnail({ group, isOwn: false, onCardMoveToGroup });

    const pile = screen.getByTestId('pile-target-pile');

    fireEvent.drop(pile, {
      dataTransfer: { getData: () => 'dragged-card-id' },
    });

    expect(onCardMoveToGroup).not.toHaveBeenCalled();
  });

  // ─── Stacking visuals ─────────────────────────────────────────

  it('shows stacking depth indicators for multiple cards', () => {
    const group = makeGroup({
      cards: [makeCard({ id: 'c1' }), makeCard({ id: 'c2' }), makeCard({ id: 'c3' })],
    });
    const { container } = renderPileThumbnail({ group });

    // With 3+ cards, there should be depth layers (offset divs behind the top card)
    const pile = screen.getByTestId('pile-group-1');
    // The stacking creates extra divs within the .relative container
    const stackContainer = pile.querySelector('.relative.mb-1');
    expect(stackContainer).toBeTruthy();
    // Should have the top card + 2 depth layers = at least 3 child divs
    expect(stackContainer!.children.length).toBeGreaterThanOrEqual(3);
  });
});
