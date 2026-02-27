import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, fireEvent, cleanup } from '@testing-library/react';
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
  leaveGame: vi.fn().mockResolvedValue(undefined),
  minimize: vi.fn(),
  maximize: vi.fn(),
  quit: vi.fn(),
  getVersion: vi.fn(),
  onGameStateUpdate: vi.fn(() => vi.fn()),
  gameAction: vi.fn(),
  gameChat: vi.fn().mockResolvedValue(undefined),
  gameSettings: vi.fn().mockResolvedValue(undefined),
  gamePlayerSettings: vi.fn().mockResolvedValue(undefined),
  bootPlayer: vi.fn().mockResolvedValue(undefined),
  startGame: vi.fn().mockResolvedValue(undefined),
  loadDeck: vi.fn(),
  openFileDialog: vi.fn(),
  executeScript: vi.fn(),
  onScriptEvent: vi.fn(() => vi.fn()),
  listInstalledGames: vi.fn(),
  listAvailableGames: vi.fn(),
  installGame: vi.fn(),
  uninstallGame: vi.fn(),
  onInstallProgress: vi.fn(() => vi.fn()),
  listFeeds: vi.fn(),
  addFeed: vi.fn(),
  removeFeed: vi.fn(),
  setFeedEnabled: vi.fn(),
  loadCredentials: vi.fn(),
  saveCredentials: vi.fn(),
  clearCredentials: vi.fn(),
  writeClipboard: vi.fn(),
};

Object.defineProperty(globalThis, 'window', {
  value: { ...globalThis.window, octgn: mockOctgn },
  writable: true,
});

// JSDOM doesn't implement scrollIntoView
Element.prototype.scrollIntoView = vi.fn();

// Import components AFTER mock setup
import PreGameLobby from '@renderer/components/PreGameLobby';
import type { GameState, Player } from '@shared/types';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function makePlayer(overrides: Partial<Player> = {}): Player {
  return {
    id: 1,
    name: 'Player 1',
    color: '#ff0000',
    isHost: false,
    isSpectator: false,
    groups: [],
    counters: [],
    globalVariables: {},
    ...overrides,
  };
}

function makeGameState(overrides: Partial<GameState> = {}): GameState {
  return {
    gameId: 'game-1',
    gameName: 'Test Game',
    players: [
      makePlayer({ id: 1, name: 'Host', color: '#008000', isHost: true }),
      makePlayer({ id: 2, name: 'Guest', color: '#cc0000', isHost: false }),
    ],
    localPlayerId: 1,
    isSpectator: false,
    table: { cards: [] },
    turnNumber: 0,
    activePlayer: 0,
    phase: 0,
    chatMessages: [],
    isStarted: false,
    connectionStatus: 'connected',
    ...overrides,
  };
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('PreGameLobby', () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('renders the game name', () => {
    render(<PreGameLobby gameState={makeGameState()} />);
    expect(screen.getByText('Test Game')).toBeTruthy();
  });

  it('shows player count', () => {
    render(<PreGameLobby gameState={makeGameState()} />);
    expect(screen.getByText('2 players')).toBeTruthy();
  });

  it('renders all player names with their colors', () => {
    render(<PreGameLobby gameState={makeGameState()} />);
    const hostName = screen.getByText('Host');
    const guestName = screen.getByText('Guest');
    expect(hostName).toBeTruthy();
    expect(guestName).toBeTruthy();
    // Check that player names have inline color styles (WPF palette)
    expect(hostName.style.color).toBe('rgb(0, 128, 0)');    // #008000
    expect(guestName.style.color).toBe('rgb(204, 0, 0)');   // #cc0000
  });

  it('shows HOST badge for the host player', () => {
    render(<PreGameLobby gameState={makeGameState()} />);
    expect(screen.getByText('HOST')).toBeTruthy();
  });

  it('shows (You) badge for the local player', () => {
    render(<PreGameLobby gameState={makeGameState()} />);
    expect(screen.getByText('(You)')).toBeTruthy();
  });

  it('shows Start Game button for host', () => {
    render(<PreGameLobby gameState={makeGameState()} />);
    expect(screen.getByText('Start Game')).toBeTruthy();
  });

  it('shows waiting message for non-host', () => {
    render(<PreGameLobby gameState={makeGameState({ localPlayerId: 2 })} />);
    expect(screen.getByText('Waiting for host to start...')).toBeTruthy();
  });

  it('does not show Start Game button for non-host', () => {
    render(<PreGameLobby gameState={makeGameState({ localPlayerId: 2 })} />);
    expect(screen.queryByText('Start Game')).toBeNull();
  });

  it('calls startGame when Start Game is clicked', () => {
    render(<PreGameLobby gameState={makeGameState()} />);
    fireEvent.click(screen.getByText('Start Game'));
    expect(mockOctgn.startGame).toHaveBeenCalled();
  });

  it('calls leaveGame when Leave Game is clicked', () => {
    render(<PreGameLobby gameState={makeGameState()} />);
    fireEvent.click(screen.getByText('Leave Game'));
    expect(mockOctgn.leaveGame).toHaveBeenCalled();
  });

  it('shows Game Settings panel for host', () => {
    render(<PreGameLobby gameState={makeGameState()} />);
    expect(screen.getByText('Game Settings')).toBeTruthy();
  });

  it('does not show Game Settings for non-host', () => {
    render(<PreGameLobby gameState={makeGameState({ localPlayerId: 2 })} />);
    expect(screen.queryByText('Game Settings')).toBeNull();
  });

  it('shows Spectator badge when player is spectator', () => {
    render(<PreGameLobby gameState={makeGameState({ isSpectator: true })} />);
    expect(screen.getByText('Spectator')).toBeTruthy();
  });

  it('shows kick button only on non-self players for host', () => {
    render(<PreGameLobby gameState={makeGameState()} />);
    // Should have one kick button (for Guest, not for Host/self)
    const kickButtons = screen.getAllByTitle('Kick player');
    expect(kickButtons.length).toBe(1);
  });

  it('does not show kick buttons for non-host', () => {
    render(<PreGameLobby gameState={makeGameState({ localPlayerId: 2 })} />);
    expect(screen.queryByTitle('Kick player')).toBeNull();
  });

  it('calls bootPlayer when kick button is clicked', () => {
    render(<PreGameLobby gameState={makeGameState()} />);
    fireEvent.click(screen.getByTitle('Kick player'));
    expect(mockOctgn.bootPlayer).toHaveBeenCalledWith(2, '');
  });

  it('sends chat message on Enter', () => {
    render(<PreGameLobby gameState={makeGameState()} />);
    const input = screen.getByPlaceholderText('Send a message...');
    fireEvent.change(input, { target: { value: 'hello lobby' } });
    fireEvent.keyDown(input, { key: 'Enter' });
    expect(mockOctgn.gameChat).toHaveBeenCalledWith('hello lobby');
  });

  it('renders chat messages with player color', () => {
    const state = makeGameState({
      chatMessages: [
        {
          id: '1',
          playerId: 1,
          playerName: 'Host',
          message: 'Welcome!',
          timestamp: Date.now(),
          isSystem: false,
          color: '#ff0000',
        },
      ],
    });
    render(<PreGameLobby gameState={state} />);
    const nameSpan = screen.getByText('Host:');
    expect(nameSpan.style.color).toBe('rgb(255, 0, 0)');
  });

  it('shows side toggle when two-sided table is enabled', () => {
    const state = makeGameState({ useTwoSidedTable: true });
    render(<PreGameLobby gameState={state} />);
    // Should show "Side A" or "Side B" buttons
    expect(screen.getAllByText(/Side [AB]/).length).toBeGreaterThan(0);
  });

  it('does not show side toggle when two-sided table is disabled', () => {
    const state = makeGameState({ useTwoSidedTable: false });
    render(<PreGameLobby gameState={state} />);
    expect(screen.queryByText(/Side [AB]/)).toBeNull();
  });

  it('shows spectator toggle when allowSpectators is true', () => {
    const state = makeGameState({ allowSpectators: true });
    render(<PreGameLobby gameState={state} />);
    // Host should see "Player" toggle buttons for each player
    expect(screen.getAllByText('Player').length).toBeGreaterThan(0);
  });

  it('shows connection status indicator', () => {
    const { container } = render(
      <PreGameLobby gameState={makeGameState({ connectionStatus: 'connected' })} />,
    );
    const dot = container.querySelector('.bg-octgn-success');
    expect(dot).toBeTruthy();
  });
});
