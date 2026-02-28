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

import PlayerGroupBrowser from '@renderer/components/PlayerGroupBrowser';
import { DragDropProvider } from '@renderer/components/DragDropContext';
import type { Card, Group, Player, Counter } from '@shared/types';
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
    ownerId: '1',
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

function makePlayer(overrides: Partial<Player> = {}): Player {
  return {
    id: 1,
    name: 'Alice',
    color: '#ff0000',
    isHost: false,
    isSpectator: false,
    groups: [],
    counters: [],
    globalVariables: {},
    ...overrides,
  };
}

const noop = () => {};

function renderBrowser(props: Partial<React.ComponentProps<typeof PlayerGroupBrowser>> = {}) {
  const defaults: React.ComponentProps<typeof PlayerGroupBrowser> = {
    players: [
      makePlayer({
        id: 1,
        name: 'Alice',
        color: '#ff0000',
        groups: [
          makeGroup({ id: 'g-hand', name: 'Hand', cards: [makeCard({ id: 'h1' })], visibility: GroupVisibility.Owner }),
          makeGroup({ id: 'g-deck', name: 'Deck', cards: [makeCard({ id: 'd1' }), makeCard({ id: 'd2' })], visibility: GroupVisibility.Owner }),
          makeGroup({ id: 'g-disc', name: 'Discard', cards: [], visibility: GroupVisibility.Everybody }),
        ],
      }),
      makePlayer({
        id: 2,
        name: 'Bob',
        color: '#0000ff',
        groups: [
          makeGroup({ id: 'g2-hand', name: 'Hand', controller: 2, cards: [makeCard({ id: 'bh1' })], visibility: GroupVisibility.Owner }),
          makeGroup({ id: 'g2-deck', name: 'Deck', controller: 2, cards: [makeCard({ id: 'bd1' })], visibility: GroupVisibility.Owner }),
          makeGroup({ id: 'g2-disc', name: 'Discard', controller: 2, cards: [], visibility: GroupVisibility.Everybody }),
        ],
      }),
    ],
    localPlayerId: 1,
    isSpectator: false,
    selectedCardId: null,
    onCardClick: noop,
    onCardContextMenu: noop,
    onCardMoveToGroup: noop,
    ...props,
  };

  return render(
    <DragDropProvider>
      <PlayerGroupBrowser {...defaults} />
    </DragDropProvider>,
  );
}

describe('PlayerGroupBrowser', () => {
  afterEach(cleanup);

  // ─── Basic rendering ─────────────────────────────────────────────
  it('renders the player group browser container', () => {
    renderBrowser();
    expect(screen.getByTestId('player-group-browser')).toBeTruthy();
  });

  it('renders rows for all non-spectator players', () => {
    renderBrowser();
    expect(screen.getByTestId('tab-player-1')).toBeTruthy();
    expect(screen.getByTestId('tab-player-2')).toBeTruthy();
  });

  it('does not render rows for spectator players', () => {
    renderBrowser({
      players: [
        makePlayer({ id: 1, name: 'Alice', isSpectator: false }),
        makePlayer({ id: 3, name: 'Spectator', isSpectator: true }),
      ],
    });
    expect(screen.getByTestId('tab-player-1')).toBeTruthy();
    expect(screen.queryByTestId('tab-player-3')).toBeNull();
  });

  it('shows player names in rows', () => {
    renderBrowser();
    expect(screen.getByText('Alice')).toBeTruthy();
    expect(screen.getByText('Bob')).toBeTruthy();
  });

  it('shows "(you)" indicator on local player row', () => {
    renderBrowser();
    expect(screen.getByText('(you)')).toBeTruthy();
  });

  it('shows Shared row when globalGroups are provided', () => {
    renderBrowser({
      globalGroups: [
        makeGroup({ id: 'g-global', name: 'Shared Deck', cards: [makeCard()], visibility: GroupVisibility.Everybody }),
      ],
    });
    expect(screen.getByTestId('tab-global')).toBeTruthy();
    expect(screen.getByText('Shared')).toBeTruthy();
  });

  it('does not show Shared row when no globalGroups', () => {
    renderBrowser({ globalGroups: undefined });
    expect(screen.queryByTestId('tab-global')).toBeNull();
  });

  // ─── Accordion behavior ──────────────────────────────────────────
  it('shows all player rows simultaneously', () => {
    renderBrowser();
    // Both player rows exist in the DOM at the same time (not tab-switching)
    expect(screen.getByTestId('tab-player-1')).toBeTruthy();
    expect(screen.getByTestId('tab-player-2')).toBeTruthy();
  });

  it('local player row is expanded by default showing own groups', () => {
    renderBrowser();
    // Local player's piles should be visible without clicking (expanded by default)
    // Deck and Discard piles shown (Hand shown as fan, not in group strip)
    expect(screen.getByTestId('pile-g-deck')).toBeTruthy();
    expect(screen.getByTestId('pile-g-disc')).toBeTruthy();
  });

  it('shows hand zone when local player row is expanded', () => {
    renderBrowser();
    expect(screen.getByTestId('hand-zone')).toBeTruthy();
  });

  it('other player rows are collapsed by default', () => {
    renderBrowser();
    // Bob's piles should NOT be visible initially (collapsed)
    expect(screen.queryByTestId('pile-g2-hand')).toBeNull();
    expect(screen.queryByTestId('pile-g2-deck')).toBeNull();
  });

  it('expands other player groups when their row header is clicked', () => {
    renderBrowser();
    fireEvent.click(screen.getByTestId('tab-player-2'));

    // Bob's groups should now be visible (only Everybody-visible ones for non-spectator)
    expect(screen.getByTestId('pile-g2-disc')).toBeTruthy();
  });

  it('collapses a player row when their header is clicked again', () => {
    renderBrowser();
    // Expand Bob
    fireEvent.click(screen.getByTestId('tab-player-2'));
    expect(screen.getByTestId('pile-g2-disc')).toBeTruthy();

    // Collapse Bob
    fireEvent.click(screen.getByTestId('tab-player-2'));
    expect(screen.queryByTestId('pile-g2-disc')).toBeNull();
  });

  it('does not show hand zone for other players', () => {
    renderBrowser();
    fireEvent.click(screen.getByTestId('tab-player-2'));
    // Hand zone should only exist for local player
    const handZones = screen.getAllByTestId('hand-zone');
    expect(handZones.length).toBe(1);
  });

  it('shows group summary pills in row headers', () => {
    renderBrowser();
    // Even when collapsed, Bob's row header should show group name + count summaries
    // Check for group name text in the header area
    const bobHeader = screen.getByTestId('tab-player-2');
    expect(bobHeader.textContent).toContain('Discard');
  });

  // ─── Visibility filtering ────────────────────────────────────────
  it('hides Owner-visibility groups of other players for non-spectators', () => {
    renderBrowser();
    // Expand Bob
    fireEvent.click(screen.getByTestId('tab-player-2'));

    // Bob's Hand and Deck have visibility: Owner — should be hidden from Alice
    expect(screen.queryByTestId('pile-g2-hand')).toBeNull();
    expect(screen.queryByTestId('pile-g2-deck')).toBeNull();

    // Bob's Discard has visibility: Everybody — should be visible
    expect(screen.getByTestId('pile-g2-disc')).toBeTruthy();
  });

  it('shows all groups for own player regardless of visibility', () => {
    renderBrowser();
    // Alice can see her own Owner-visibility groups
    expect(screen.getByTestId('pile-g-deck')).toBeTruthy();
  });

  it('spectators see all groups including Owner and Nobody visibility', () => {
    renderBrowser({
      isSpectator: true,
      players: [
        makePlayer({
          id: 1,
          name: 'Alice',
          groups: [
            makeGroup({ id: 'g-hand', name: 'Hand', cards: [makeCard({ id: 'h1' })], visibility: GroupVisibility.Owner }),
            makeGroup({ id: 'g-secret', name: 'Secret', cards: [], visibility: GroupVisibility.Nobody }),
          ],
        }),
      ],
    });

    // Expand Alice's row (spectator: first active player is expanded by default)
    expect(screen.getByTestId('pile-g-hand')).toBeTruthy();
    expect(screen.getByTestId('pile-g-secret')).toBeTruthy();
  });

  it('hides Nobody-visibility groups for non-spectators even for own player', () => {
    renderBrowser({
      players: [
        makePlayer({
          id: 1,
          name: 'Alice',
          groups: [
            makeGroup({ id: 'g-deck', name: 'Deck', cards: [], visibility: GroupVisibility.Owner }),
            makeGroup({ id: 'g-nobody', name: 'Hidden', cards: [], visibility: GroupVisibility.Nobody }),
          ],
        }),
      ],
    });

    expect(screen.getByTestId('pile-g-deck')).toBeTruthy();
    expect(screen.queryByTestId('pile-g-nobody')).toBeNull();
  });

  // ─── Counters ────────────────────────────────────────────────────
  it('shows counters for selected player', () => {
    renderBrowser({
      players: [
        makePlayer({
          id: 1,
          name: 'Alice',
          counters: [{ id: 1, name: 'Life', value: 20 } as Counter],
          groups: [makeGroup({ id: 'g1', name: 'Deck', visibility: GroupVisibility.Owner })],
        }),
      ],
    });
    expect(screen.getAllByText('Life').length).toBeGreaterThan(0);
    expect(screen.getAllByText('20').length).toBeGreaterThan(0);
  });

  // ─── PileViewer ──────────────────────────────────────────────────
  it('opens pile viewer when a group pile is clicked', () => {
    renderBrowser();
    fireEvent.click(screen.getByTestId('pile-g-deck'));
    expect(screen.getByTestId('pile-viewer-overlay')).toBeTruthy();
  });

  it('closes pile viewer when close button is clicked', () => {
    renderBrowser();
    fireEvent.click(screen.getByTestId('pile-g-deck'));
    expect(screen.getByTestId('pile-viewer-overlay')).toBeTruthy();

    fireEvent.click(screen.getByTestId('pile-viewer-close'));
    expect(screen.queryByTestId('pile-viewer-overlay')).toBeNull();
  });

  // ─── Spectator mode ─────────────────────────────────────────────
  it('renders in spectator mode showing all player rows', () => {
    renderBrowser({ isSpectator: true });
    expect(screen.getByTestId('tab-player-1')).toBeTruthy();
    expect(screen.getByTestId('tab-player-2')).toBeTruthy();
  });

  it('spectator sees first player expanded by default', () => {
    renderBrowser({ isSpectator: true });
    // First active player's groups should be visible
    // Spectators see all groups including Owner visibility
    expect(screen.getByTestId('pile-g-hand')).toBeTruthy();
    expect(screen.getByTestId('pile-g-deck')).toBeTruthy();
  });

  it('shows all groups when viewing another player as spectator', () => {
    renderBrowser({ isSpectator: true });
    // Expand Bob
    fireEvent.click(screen.getByTestId('tab-player-2'));
    // Spectator can see all of Bob's groups including Owner-visibility ones
    expect(screen.getByTestId('pile-g2-hand')).toBeTruthy();
    expect(screen.getByTestId('pile-g2-deck')).toBeTruthy();
    expect(screen.getByTestId('pile-g2-disc')).toBeTruthy();
  });

  // ─── Global groups ───────────────────────────────────────────────
  it('shows global groups when Shared row is expanded', () => {
    renderBrowser({
      globalGroups: [
        makeGroup({ id: 'g-shared', name: 'Community Pile', cards: [], visibility: GroupVisibility.Everybody }),
      ],
    });
    fireEvent.click(screen.getByTestId('tab-global'));
    expect(screen.getByTestId('pile-g-shared')).toBeTruthy();
  });

  it('group card counts are shown', () => {
    renderBrowser();
    // Deck has 2 cards — count appears in pile thumbnail and possibly in header summary
    expect(screen.getAllByText('2').length).toBeGreaterThan(0);
  });
});
