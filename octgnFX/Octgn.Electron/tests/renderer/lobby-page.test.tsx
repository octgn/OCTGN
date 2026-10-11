import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, cleanup } from '@testing-library/react';
import React from 'react';

// ---------------------------------------------------------------------------
// Mock window.octgn
// ---------------------------------------------------------------------------

const mockOctgn = {
  login: vi.fn(),
  logout: vi.fn(),
  getSession: vi.fn(),
  getGames: vi.fn(),
  hostGame: vi.fn(),
  joinGame: vi.fn(),
  leaveGame: vi.fn(),
  minimize: vi.fn(),
  maximize: vi.fn(),
  quit: vi.fn(),
  getVersion: vi.fn(),
  onGameStateUpdate: vi.fn(() => vi.fn()),
  gameAction: vi.fn(),
  gameChat: vi.fn(),
  loadDeck: vi.fn(),
  openFileDialog: vi.fn(),
  listInstalledGames: vi.fn().mockResolvedValue([]),
  listAvailableGames: vi.fn().mockResolvedValue([]),
  listFeeds: vi.fn().mockResolvedValue([]),
};

Object.defineProperty(globalThis, 'window', {
  value: { ...globalThis.window, octgn: mockOctgn },
  writable: true,
});

// Import AFTER mock setup
import LobbyPage from '@renderer/pages/LobbyPage';
import { useLobbyStore } from '@renderer/stores/lobby-store';
import type { HostedGame, GameStatus } from '@shared/types';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function makeGame(overrides: Partial<HostedGame> = {}): HostedGame {
  return {
    id: 'game-1',
    name: 'Test Game',
    hostUser: { id: 'u1', username: 'TestHost', isSubscriber: false },
    gameId: 'g-1',
    gameName: 'Chess',
    gameVersion: '1.0',
    hasPassword: false,
    spectators: true,
    hostAddress: '127.0.0.1',
    port: 5000,
    status: 1 as GameStatus,
    dateCreated: '2026-01-15T12:00:00Z',
    playerCount: 1,
    maxPlayers: 4,
    ...overrides,
  };
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('LobbyPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockOctgn.getGames.mockResolvedValue([]);
  });

  afterEach(() => {
    cleanup();
    useLobbyStore.setState({
      games: [],
      isLoading: false,
      error: null,
      filter: { searchText: '', hideUninstalled: false },
    });
  });

  it('should display the host username for each game', async () => {
    const games = [
      makeGame({ id: 'g1', name: 'Chess Match', hostUser: { id: 'u1', username: 'ChessMaster42', isSubscriber: false } }),
      makeGame({ id: 'g2', name: 'MTG Draft', hostUser: { id: 'u2', username: 'CardShark99', isSubscriber: false } }),
    ];
    useLobbyStore.setState({ games });

    render(<LobbyPage />);

    expect(screen.getByText('ChessMaster42')).toBeDefined();
    expect(screen.getByText('CardShark99')).toBeDefined();
  });

  it('should render the host user icon when iconUrl is provided', async () => {
    const games = [
      makeGame({
        id: 'g1',
        hostUser: {
          id: 'u1',
          username: 'Alice',
          iconUrl: 'https://octgn.net/icons/alice.png',
          isSubscriber: false,
        },
      }),
    ];
    useLobbyStore.setState({ games });

    render(<LobbyPage />);

    const img = screen.getByAltText('Alice');
    expect(img).toBeDefined();
    expect(img.getAttribute('src')).toBe('https://octgn.net/icons/alice.png');
  });

  it('should render a fallback avatar when host has no iconUrl', async () => {
    const games = [
      makeGame({
        id: 'g1',
        hostUser: { id: 'u1', username: 'Bob', isSubscriber: false },
      }),
    ];
    useLobbyStore.setState({ games });

    render(<LobbyPage />);

    // Should show the first letter of the username as avatar fallback
    expect(screen.getByText('B')).toBeDefined();
  });
});
