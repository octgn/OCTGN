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

  it('renders tabs for all non-spectator players', () => {
    renderBrowser();
    expect(screen.getByTestId('tab-player-1')).toBeTruthy();
    expect(screen.getByTestId('tab-player-2')).toBeTruthy();
  });

  it('does not render tabs for spectator players', () => {
    renderBrowser({
      players: [
        makePlayer({ id: 1, name: 'Alice', isSpectator: false }),
        makePlayer({ id: 3, name: 'Spectator', isSpectator: true }),
      ],
    });
    expect(screen.getByTestId('tab-player-1')).toBeTruthy();
    expect(screen.queryByTestId('tab-player-3')).toBeNull();
  });

  it('shows player names in tabs', () => {
    renderBrowser();
    expect(screen.getByText('Alice')).toBeTruthy();
    expect(screen.getByText('Bob')).toBeTruthy();
  });

  it('shows "(you)" indicator on local player tab', () => {
    renderBrowser();
    expect(screen.getByText('(you)')).toBeTruthy();
  });

  it('shows Shared tab when globalGroups are provided', () => {
    renderBrowser({
      globalGroups: [
        makeGroup({ id: 'g-global', name: 'Shared Deck', cards: [makeCard()], visibility: GroupVisibility.Everybody }),
      ],
    });
    expect(screen.getByTestId('tab-global')).toBeTruthy();
    expect(screen.getByText('Shared')).toBeTruthy();
  });

  it('does not show Shared tab when no globalGroups', () => {
    renderBrowser({ globalGroups: undefined });
    expect(screen.queryByTestId('tab-global')).toBeNull();
  });

  // ─── Tab switching ────────────────────────────────────────────────
  it('shows own groups by default (local player selected)', () => {
    renderBrowser();
    // Should show Deck and Discard piles (Hand is shown as fan, not in group strip)
    expect(screen.getByTestId('pile-g-deck')).toBeTruthy();
    expect(screen.getByTestId('pile-g-disc')).toBeTruthy();
  });

  it('shows hand zone when viewing own groups', () => {
    renderBrowser();
    expect(screen.getByTestId('hand-zone')).toBeTruthy();
  });

  it('hides hand zone when viewing other player groups', () => {
    renderBrowser();
    fireEvent.click(screen.getByTestId('tab-player-2'));
    expect(screen.queryByTestId('hand-zone')).toBeNull();
  });

  it('switches to other player groups when their tab is clicked', () => {
    renderBrowser();
    fireEvent.click(screen.getByTestId('tab-player-2'));

    // Bob's Discard (Everybody visibility) should be visible
    expect(screen.getByTestId('pile-g2-disc')).toBeTruthy();
  });

  it('switches to global groups when Shared tab is clicked', () => {
    renderBrowser({
      globalGroups: [
        makeGroup({ id: 'g-shared', name: 'Community Pile', cards: [], visibility: GroupVisibility.Everybody }),
      ],
    });
    fireEvent.click(screen.getByTestId('tab-global'));
    expect(screen.getByTestId('pile-g-shared')).toBeTruthy();
  });

  it('shows group card counts', () => {
    renderBrowser();
    // Deck has 2 cards
    expect(screen.getByText('2')).toBeTruthy();
  });

  // ─── Pile viewer ──────────────────────────────────────────────────
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
    expect(screen.getByText('Life')).toBeTruthy();
    expect(screen.getByText('20')).toBeTruthy();
  });

  // ─── Visibility filtering ────────────────────────────────────────
  it('hides Owner-visibility groups of other players for non-spectators', () => {
    renderBrowser();
    // Switch to Bob's tab
    fireEvent.click(screen.getByTestId('tab-player-2'));

    // Bob's Hand and Deck have visibility: Owner — should be hidden from Alice
    expect(screen.queryByTestId('pile-g2-hand')).toBeNull();
    expect(screen.queryByTestId('pile-g2-deck')).toBeNull();

    // Bob's Discard has visibility: Everybody — should be visible
    expect(screen.getByTestId('pile-g2-disc')).toBeTruthy();
  });

  it('shows all groups for own player regardless of Owner visibility', () => {
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

    // Spectator viewing Alice — sees everything
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

  // ─── Spectator mode ─────────────────────────────────────────────
  it('renders in spectator mode showing all player tabs', () => {
    renderBrowser({ isSpectator: true });
    expect(screen.getByTestId('tab-player-1')).toBeTruthy();
    expect(screen.getByTestId('tab-player-2')).toBeTruthy();
  });

  it('shows all groups when viewing another player as spectator', () => {
    renderBrowser({ isSpectator: true });
    fireEvent.click(screen.getByTestId('tab-player-2'));
    // Spectator sees all groups including Owner-visibility ones
    expect(screen.getByTestId('pile-g2-hand')).toBeTruthy();
    expect(screen.getByTestId('pile-g2-deck')).toBeTruthy();
    expect(screen.getByTestId('pile-g2-disc')).toBeTruthy();
  });
});
