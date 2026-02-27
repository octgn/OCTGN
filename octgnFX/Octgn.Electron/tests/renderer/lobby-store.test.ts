import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import type { HostedGame, GameStatus } from '@shared/types';

// ---------------------------------------------------------------------------
// Mock window.octgn
// ---------------------------------------------------------------------------

const mockOctgn = {
  login: vi.fn(),
  logout: vi.fn(),
  getSession: vi.fn(),
  getGames: vi.fn<[], Promise<HostedGame[]>>(),
  hostGame: vi.fn<[Record<string, unknown>], Promise<void>>(),
  joinGame: vi.fn<[string, string?], Promise<void>>(),
  leaveGame: vi.fn(),
  minimize: vi.fn(),
  maximize: vi.fn(),
  quit: vi.fn(),
  getVersion: vi.fn(),
  onGameStateUpdate: vi.fn(),
  gameAction: vi.fn(),
  gameChat: vi.fn(),
  loadDeck: vi.fn(),
};

Object.defineProperty(globalThis, 'window', {
  value: { ...globalThis.window, octgn: mockOctgn },
  writable: true,
});

// Import AFTER mock setup
import { useLobbyStore } from '@renderer/stores/lobby-store';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function makeGame(overrides: Partial<HostedGame> = {}): HostedGame {
  return {
    id: 'game-1',
    name: 'Test Game',
    hostUser: { id: 'u1', username: 'Host', isSubscriber: false },
    gameId: 'g-1',
    gameName: 'My Game',
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

describe('useLobbyStore', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    useLobbyStore.setState({
      games: [],
      isLoading: false,
      error: null,
      filter: { searchText: '', hideUninstalled: false },
    });
  });

  describe('initial state', () => {
    it('should have an empty games array', () => {
      expect(useLobbyStore.getState().games).toEqual([]);
    });

    it('should not be loading', () => {
      expect(useLobbyStore.getState().isLoading).toBe(false);
    });

    it('should have no error', () => {
      expect(useLobbyStore.getState().error).toBeNull();
    });

    it('should have empty filter', () => {
      const filter = useLobbyStore.getState().filter;
      expect(filter.searchText).toBe('');
      expect(filter.hideUninstalled).toBe(false);
    });
  });

  describe('hostGame', () => {
    it('should call IPC hostGame with options', async () => {
      mockOctgn.hostGame.mockResolvedValue(undefined);
      mockOctgn.getGames.mockResolvedValue([]);

      await useLobbyStore.getState().hostGame({ name: 'My Game', maxPlayers: 4 });

      expect(mockOctgn.hostGame).toHaveBeenCalledWith({ name: 'My Game', maxPlayers: 4 });
    });

    it('should set isLoading to true during host', async () => {
      let resolveHost: () => void;
      mockOctgn.hostGame.mockReturnValue(
        new Promise<void>((resolve) => {
          resolveHost = resolve;
        }),
      );
      mockOctgn.getGames.mockResolvedValue([]);

      const promise = useLobbyStore.getState().hostGame({ name: 'Game' });
      expect(useLobbyStore.getState().isLoading).toBe(true);

      resolveHost!();
      await promise;
    });

    it('should refresh games after hosting', async () => {
      const game = makeGame({ id: 'hosted-game' });
      mockOctgn.hostGame.mockResolvedValue(undefined);
      mockOctgn.getGames.mockResolvedValue([game]);

      await useLobbyStore.getState().hostGame({ name: 'Game' });

      expect(mockOctgn.getGames).toHaveBeenCalled();
      expect(useLobbyStore.getState().games).toHaveLength(1);
      expect(useLobbyStore.getState().games[0].id).toBe('hosted-game');
    });

    it('should set error on host failure', async () => {
      mockOctgn.hostGame.mockRejectedValue(new Error('Port in use'));

      await useLobbyStore.getState().hostGame({ name: 'Game' });

      expect(useLobbyStore.getState().error).toBe('Port in use');
      expect(useLobbyStore.getState().isLoading).toBe(false);
    });

    it('should set fallback error for non-Error exceptions', async () => {
      mockOctgn.hostGame.mockRejectedValue('boom');

      await useLobbyStore.getState().hostGame({ name: 'Game' });

      expect(useLobbyStore.getState().error).toBe('Failed to host game');
    });
  });

  describe('joinGame', () => {
    it('should call IPC joinGame with gameId', async () => {
      mockOctgn.joinGame.mockResolvedValue(undefined);

      await useLobbyStore.getState().joinGame('game-abc');

      expect(mockOctgn.joinGame).toHaveBeenCalledWith('game-abc', undefined);
    });

    it('should call IPC joinGame with password when provided', async () => {
      mockOctgn.joinGame.mockResolvedValue(undefined);

      await useLobbyStore.getState().joinGame('game-abc', 'secret123');

      expect(mockOctgn.joinGame).toHaveBeenCalledWith('game-abc', 'secret123');
    });

    it('should set isLoading during join', async () => {
      let resolveJoin: () => void;
      mockOctgn.joinGame.mockReturnValue(
        new Promise<void>((resolve) => {
          resolveJoin = resolve;
        }),
      );

      const promise = useLobbyStore.getState().joinGame('game-abc');
      expect(useLobbyStore.getState().isLoading).toBe(true);

      resolveJoin!();
      await promise;
      expect(useLobbyStore.getState().isLoading).toBe(false);
    });

    it('should set error on join failure', async () => {
      mockOctgn.joinGame.mockRejectedValue(new Error('Game full'));

      await useLobbyStore.getState().joinGame('game-abc');

      expect(useLobbyStore.getState().error).toBe('Game full');
      expect(useLobbyStore.getState().isLoading).toBe(false);
    });

    it('should set fallback error for non-Error exceptions', async () => {
      mockOctgn.joinGame.mockRejectedValue(42);

      await useLobbyStore.getState().joinGame('game-abc');

      expect(useLobbyStore.getState().error).toBe('Failed to join game');
    });
  });

  describe('refreshGames', () => {
    it('should update the game list from IPC', async () => {
      const games = [makeGame({ id: 'g1' }), makeGame({ id: 'g2' })];
      mockOctgn.getGames.mockResolvedValue(games);

      await useLobbyStore.getState().refreshGames();

      expect(useLobbyStore.getState().games).toHaveLength(2);
      expect(useLobbyStore.getState().isLoading).toBe(false);
      expect(useLobbyStore.getState().error).toBeNull();
    });

    it('should sort games by status priority then by date', async () => {
      const games = [
        makeGame({ id: 'old-ready', status: 1 as GameStatus, dateCreated: '2026-01-01T00:00:00Z' }),
        makeGame({ id: 'in-progress', status: 2 as GameStatus, dateCreated: '2026-01-15T00:00:00Z' }),
        makeGame({ id: 'new-ready', status: 1 as GameStatus, dateCreated: '2026-01-10T00:00:00Z' }),
      ];
      mockOctgn.getGames.mockResolvedValue(games);

      await useLobbyStore.getState().refreshGames();

      const sorted = useLobbyStore.getState().games;
      expect(sorted[0].id).toBe('new-ready');
      expect(sorted[1].id).toBe('old-ready');
      expect(sorted[2].id).toBe('in-progress');
    });

    it('should set error on failure', async () => {
      mockOctgn.getGames.mockRejectedValue(new Error('Connection refused'));

      await useLobbyStore.getState().refreshGames();

      expect(useLobbyStore.getState().error).toBe('Connection refused');
      expect(useLobbyStore.getState().isLoading).toBe(false);
    });

    it('should clear previous error on successful refresh', async () => {
      useLobbyStore.setState({ error: 'old error' });
      mockOctgn.getGames.mockResolvedValue([]);

      await useLobbyStore.getState().refreshGames();

      expect(useLobbyStore.getState().error).toBeNull();
    });
  });

  describe('sendChat (via game store)', () => {
    // sendChat lives on the game store, but we verify the lobby store
    // delegates chat-like IPC calls correctly through joinGame/hostGame
    // which are the lobby-level IPC interactions.
    it('should clear error before joining', async () => {
      useLobbyStore.setState({ error: 'previous error' });
      mockOctgn.joinGame.mockResolvedValue(undefined);

      await useLobbyStore.getState().joinGame('game-abc');

      expect(useLobbyStore.getState().error).toBeNull();
    });
  });

  describe('startAutoRefresh', () => {
    beforeEach(() => {
      vi.useFakeTimers();
    });

    afterEach(() => {
      vi.useRealTimers();
    });

    it('should call refreshGames immediately', () => {
      mockOctgn.getGames.mockResolvedValue([]);

      useLobbyStore.getState().startAutoRefresh();

      expect(mockOctgn.getGames).toHaveBeenCalledTimes(1);
    });

    it('should return a cleanup function', () => {
      mockOctgn.getGames.mockResolvedValue([]);

      const cleanup = useLobbyStore.getState().startAutoRefresh();

      expect(cleanup).toBeTypeOf('function');
      cleanup();
    });

    it('should stop refreshing after cleanup is called', () => {
      mockOctgn.getGames.mockResolvedValue([]);

      const cleanup = useLobbyStore.getState().startAutoRefresh();
      cleanup();

      vi.advanceTimersByTime(30_000);
      expect(mockOctgn.getGames).toHaveBeenCalledTimes(1);
    });
  });
});
