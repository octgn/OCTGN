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

import PileViewer from '@renderer/components/PileViewer';
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

function renderPileViewer(props: React.ComponentProps<typeof PileViewer>) {
  return render(
    <DragDropProvider>
      <PileViewer {...props} />
    </DragDropProvider>,
  );
}

describe('PileViewer', () => {
  afterEach(cleanup);

  it('renders the overlay with player name and group name', () => {
    const group = makeGroup({ name: 'Discard', cards: [makeCard()] });
    renderPileViewer({
      group,
      playerName: 'Alice',
      playerColor: '#ff0000',
      isOwn: false,
      onClose: noop,
    });

    expect(screen.getByText('Alice')).toBeTruthy();
    expect(screen.getByText('Discard')).toBeTruthy();
    expect(screen.getByTestId('pile-viewer-overlay')).toBeTruthy();
  });

  it('shows card count in the header', () => {
    const cards = [
      makeCard({ id: 'c1', name: 'Card A' }),
      makeCard({ id: 'c2', name: 'Card B' }),
      makeCard({ id: 'c3', name: 'Card C' }),
    ];
    const group = makeGroup({ cards });
    renderPileViewer({
      group,
      playerName: 'Bob',
      playerColor: '#00ff00',
      isOwn: false,
      onClose: noop,
    });

    expect(screen.getByText('3')).toBeTruthy();
  });

  it('renders all cards in the grid', () => {
    const cards = [
      makeCard({ id: 'c1', name: 'Alpha Card' }),
      makeCard({ id: 'c2', name: 'Beta Card' }),
    ];
    const group = makeGroup({ cards });
    renderPileViewer({
      group,
      playerName: 'Test',
      playerColor: '#0000ff',
      isOwn: false,
      onClose: noop,
    });

    expect(screen.getAllByText('Alpha Card').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Beta Card').length).toBeGreaterThan(0);
  });

  it('shows empty state when group has no cards', () => {
    const group = makeGroup({ cards: [] });
    renderPileViewer({
      group,
      playerName: 'Test',
      playerColor: '#0000ff',
      isOwn: false,
      onClose: noop,
    });

    expect(screen.getByText('No cards in this pile')).toBeTruthy();
  });

  it('calls onClose when close button is clicked', () => {
    const onClose = vi.fn();
    const group = makeGroup({ cards: [makeCard()] });
    renderPileViewer({
      group,
      playerName: 'Test',
      playerColor: '#0000ff',
      isOwn: false,
      onClose,
    });

    fireEvent.click(screen.getByTestId('pile-viewer-close'));
    expect(onClose).toHaveBeenCalledOnce();
  });

  it('calls onClose when Escape is pressed', () => {
    const onClose = vi.fn();
    const group = makeGroup({ cards: [makeCard()] });
    renderPileViewer({
      group,
      playerName: 'Test',
      playerColor: '#0000ff',
      isOwn: false,
      onClose,
    });

    fireEvent.keyDown(window, { key: 'Escape' });
    expect(onClose).toHaveBeenCalledOnce();
  });

  it('calls onClose when overlay backdrop is clicked', () => {
    const onClose = vi.fn();
    const group = makeGroup({ cards: [makeCard()] });
    renderPileViewer({
      group,
      playerName: 'Test',
      playerColor: '#0000ff',
      isOwn: false,
      onClose,
    });

    fireEvent.click(screen.getByTestId('pile-viewer-overlay'));
    expect(onClose).toHaveBeenCalledOnce();
  });

  // ─── Face-down card identity leak prevention ──────────────────────

  it('does NOT show real card names for face-down cards', () => {
    const cards = [
      makeCard({ id: 'c1', name: 'Secret Dragon', faceUp: false }),
      makeCard({ id: 'c2', name: 'Hidden Spell', faceUp: false }),
    ];
    const group = makeGroup({ cards });
    renderPileViewer({
      group,
      playerName: 'Test',
      playerColor: '#0000ff',
      isOwn: false,
      onClose: noop,
    });

    // The real card names should NOT appear anywhere
    expect(screen.queryByText('Secret Dragon')).toBeNull();
    expect(screen.queryByText('Hidden Spell')).toBeNull();
  });

  it('shows real names for face-up cards but not face-down ones', () => {
    const cards = [
      makeCard({ id: 'c1', name: 'Visible Card', faceUp: true }),
      makeCard({ id: 'c2', name: 'Secret Card', faceUp: false }),
    ];
    const group = makeGroup({ cards });
    renderPileViewer({
      group,
      playerName: 'Test',
      playerColor: '#0000ff',
      isOwn: false,
      onClose: noop,
    });

    // Face-up card name should be visible
    expect(screen.getAllByText('Visible Card').length).toBeGreaterThan(0);
    // Face-down card name should NOT be visible
    expect(screen.queryByText('Secret Card')).toBeNull();
  });

  it('calls onCardClick when a card is clicked and isOwn is true', () => {
    const onCardClick = vi.fn();
    const card = makeCard({ id: 'c1', name: 'Clickable Card' });
    const group = makeGroup({ cards: [card] });
    renderPileViewer({
      group,
      playerName: 'Me',
      playerColor: '#ff0000',
      isOwn: true,
      onClose: noop,
      onCardClick,
    });

    // Click on the card component — find it by its role button
    const cardButtons = screen.getAllByRole('button');
    // The card itself (interactive=true) should be a button; filter out close button
    const cardButton = cardButtons.find((b) => b !== screen.getByTestId('pile-viewer-close'));
    if (cardButton) fireEvent.click(cardButton);
    expect(onCardClick).toHaveBeenCalled();
  });
});
