import { describe, it, expect, vi, beforeEach } from 'vitest';
import type { LoginResult, GameState } from '@shared/types';

// ---------------------------------------------------------------------------
// Mock window.octgn
// ---------------------------------------------------------------------------
const mockOctgn = {
  login: vi.fn(),
  logout: vi.fn(),
  getSession: vi.fn(),
  getAppState: vi.fn<[], Promise<{ session: LoginResult; gameState: GameState | null }>>(),
  loadCredentials: vi.fn(),
  saveCredentials: vi.fn(),
  clearCredentials: vi.fn(),
  writeClipboard: vi.fn(),
  getGames: vi.fn(),
  hostGame: vi.fn(),
  joinGame: vi.fn(),
  leaveGame: vi.fn(),
  minimize: vi.fn(),
  maximize: vi.fn(),
  quit: vi.fn(),
  getVersion: vi.fn(),
  onGameStateUpdate: vi.fn(() => () => {}),
  gameAction: vi.fn(),
  gameChat: vi.fn(),
  gameSettings: vi.fn(),
  gamePlayerSettings: vi.fn(),
  bootPlayer: vi.fn(),
  startGame: vi.fn(),
  loadDeck: vi.fn(),
  getDeckPaths: vi.fn(),
  openFileDialog: vi.fn(),
  executeScript: vi.fn(),
  onScriptEvent: vi.fn(() => () => {}),
  listInstalledGames: vi.fn(),
  listAvailableGames: vi.fn(),
  installGame: vi.fn(),
  uninstallGame: vi.fn(),
  onInstallProgress: vi.fn(() => () => {}),
  listFeeds: vi.fn(),
  addFeed: vi.fn(),
  removeFeed: vi.fn(),
  setFeedEnabled: vi.fn(),
};

// Assign octgn to the existing jsdom window (don't overwrite window itself)
(window as any).octgn = mockOctgn;

// Import AFTER mock setup
import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import App from '@renderer/App';
import { useAppStore } from '@renderer/stores/app-store';
import { useAuthStore } from '@renderer/stores/auth-store';
import { useGameStore } from '@renderer/stores/game-store';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function makeGameState(): GameState {
  return {
    gameId: 'game-1',
    gameName: 'Test Game',
    players: [],
    localPlayerId: 1,
    isSpectator: false,
    table: { cards: [] },
    turnNumber: 0,
    activePlayer: 0,
    phase: 0,
    chatMessages: [],
    isStarted: true,
  };
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('App initialization', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    useAppStore.setState({ currentPage: 'login' });
    useAuthStore.setState({ user: null, session: null, isLoading: false, error: null });
    useGameStore.setState({ gameState: null, isConnected: false, isLoading: false, error: null });
  });

  it('should show loading state initially', () => {
    // Make getAppState never resolve to keep loading state visible
    mockOctgn.getAppState.mockReturnValue(new Promise(() => {}));

    render(<App />);
    expect(screen.getByText('Loading...')).toBeTruthy();
  });

  it('should navigate to lobby when session exists but no game', async () => {
    mockOctgn.getAppState.mockResolvedValue({
      session: {
        success: true,
        user: { id: 'u1', username: 'alice', isSubscriber: false },
        session: { userId: 'u1', sessionId: 's1', deviceId: 'd1' },
      },
      gameState: null,
    });

    render(<App />);

    await waitFor(() => {
      expect(useAppStore.getState().currentPage).toBe('lobby');
    });

    expect(useAuthStore.getState().user).toEqual({
      id: 'u1',
      username: 'alice',
      isSubscriber: false,
    });
  });

  it('should navigate to game and subscribe when session and gameState exist', async () => {
    const gameState = makeGameState();
    mockOctgn.getAppState.mockResolvedValue({
      session: {
        success: true,
        user: { id: 'u1', username: 'alice', isSubscriber: false },
        session: { userId: 'u1', sessionId: 's1', deviceId: 'd1' },
      },
      gameState,
    });

    render(<App />);

    await waitFor(() => {
      expect(useAppStore.getState().currentPage).toBe('game');
    });

    // Auth should be restored
    expect(useAuthStore.getState().user?.username).toBe('alice');
    // Should have subscribed to game state updates
    expect(mockOctgn.onGameStateUpdate).toHaveBeenCalled();
  });

  it('should stay on login when no session exists', async () => {
    mockOctgn.getAppState.mockResolvedValue({
      session: { success: false },
      gameState: null,
    });

    render(<App />);

    await waitFor(() => {
      expect(screen.queryByText('Loading...')).toBeNull();
    });

    expect(useAppStore.getState().currentPage).toBe('login');
    expect(useAuthStore.getState().user).toBeNull();
  });

  it('should stay on login when getAppState fails', async () => {
    mockOctgn.getAppState.mockRejectedValue(new Error('IPC not available'));

    render(<App />);

    await waitFor(() => {
      expect(screen.queryByText('Loading...')).toBeNull();
    });

    expect(useAppStore.getState().currentPage).toBe('login');
  });
});
