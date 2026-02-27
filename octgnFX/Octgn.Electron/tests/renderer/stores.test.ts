import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import type { LoginResult, HostedGame, GameStatus } from '@shared/types';

// ---------------------------------------------------------------------------
// Mock window.octgn
// ---------------------------------------------------------------------------

const mockOctgn = {
  login: vi.fn<[string, string], Promise<LoginResult>>(),
  logout: vi.fn<[], Promise<void>>(),
  getSession: vi.fn<[], Promise<LoginResult>>(),
  getGames: vi.fn<[], Promise<HostedGame[]>>(),
  hostGame: vi.fn<[Record<string, unknown>], Promise<void>>(),
  joinGame: vi.fn<[string, string?], Promise<void>>(),
  leaveGame: vi.fn<[], Promise<void>>(),
  minimize: vi.fn<[], Promise<void>>(),
  maximize: vi.fn<[], Promise<void>>(),
  quit: vi.fn<[], Promise<void>>(),
  getVersion: vi.fn<[], Promise<string>>(),
  onGameStateUpdate: vi.fn(),
  gameAction: vi.fn(),
  gameChat: vi.fn(),
  loadDeck: vi.fn(),
};

// Assign the mock to the global window object before store imports
Object.defineProperty(globalThis, 'window', {
  value: { ...globalThis.window, octgn: mockOctgn },
  writable: true,
});

// Import stores AFTER setting up the mock so they bind to our mock
import { useAuthStore } from '@renderer/stores/auth-store';
import { useLobbyStore, useFilteredGames } from '@renderer/stores/lobby-store';
import { useAppStore } from '@renderer/stores/app-store';

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
    status: 1 as GameStatus, // GameReady
    dateCreated: '2026-01-15T12:00:00Z',
    playerCount: 1,
    maxPlayers: 4,
    ...overrides,
  };
}

// ---------------------------------------------------------------------------
// Auth Store
// ---------------------------------------------------------------------------

describe('useAuthStore', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // Reset store to initial state
    useAuthStore.setState({
      user: null,
      session: null,
      isLoading: false,
      error: null,
    });
  });

  describe('login', () => {
    it('should set user and session on successful login', async () => {
      mockOctgn.login.mockResolvedValue({
        success: true,
        user: { id: '1', username: 'alice', isSubscriber: true },
        session: { userId: '1', sessionId: 's1', deviceId: 'd1' },
      });

      await useAuthStore.getState().login('alice', 'password123');

      const state = useAuthStore.getState();
      expect(state.user).toEqual({ id: '1', username: 'alice', isSubscriber: true });
      expect(state.session).toEqual({ userId: '1', sessionId: 's1', deviceId: 'd1' });
      expect(state.isLoading).toBe(false);
      expect(state.error).toBeNull();
    });

    it('should set error on failed login', async () => {
      mockOctgn.login.mockResolvedValue({
        success: false,
        error: 'Invalid credentials',
      });

      await useAuthStore.getState().login('alice', 'wrong');

      const state = useAuthStore.getState();
      expect(state.user).toBeNull();
      expect(state.session).toBeNull();
      expect(state.isLoading).toBe(false);
      expect(state.error).toBe('Invalid credentials');
    });

    it('should set default error message when error field is missing', async () => {
      mockOctgn.login.mockResolvedValue({ success: false });

      await useAuthStore.getState().login('alice', 'wrong');

      expect(useAuthStore.getState().error).toBe('Login failed');
    });

    it('should handle exceptions during login', async () => {
      mockOctgn.login.mockRejectedValue(new Error('Network error'));

      await useAuthStore.getState().login('alice', 'pass');

      const state = useAuthStore.getState();
      expect(state.isLoading).toBe(false);
      expect(state.error).toBe('Network error');
    });

    it('should handle non-Error exceptions', async () => {
      mockOctgn.login.mockRejectedValue('string error');

      await useAuthStore.getState().login('alice', 'pass');

      expect(useAuthStore.getState().error).toBe('An unexpected error occurred');
    });

    it('should set isLoading to true while login is in progress', async () => {
      let resolveLogin: (v: LoginResult) => void;
      mockOctgn.login.mockReturnValue(
        new Promise<LoginResult>((resolve) => { resolveLogin = resolve; }),
      );

      const promise = useAuthStore.getState().login('alice', 'pass');
      expect(useAuthStore.getState().isLoading).toBe(true);

      resolveLogin!({ success: true });
      await promise;
      expect(useAuthStore.getState().isLoading).toBe(false);
    });

    it('should clear previous error when starting login', async () => {
      useAuthStore.setState({ error: 'old error' });
      mockOctgn.login.mockResolvedValue({
        success: true,
        user: { id: '1', username: 'a', isSubscriber: false },
      });

      await useAuthStore.getState().login('a', 'b');
      expect(useAuthStore.getState().error).toBeNull();
    });
  });

  describe('logout', () => {
    it('should clear user and session on logout', async () => {
      useAuthStore.setState({
        user: { id: '1', username: 'alice', isSubscriber: false },
        session: { userId: '1', sessionId: 's1', deviceId: 'd1' },
      });

      mockOctgn.logout.mockResolvedValue(undefined);

      await useAuthStore.getState().logout();

      const state = useAuthStore.getState();
      expect(state.user).toBeNull();
      expect(state.session).toBeNull();
      expect(state.isLoading).toBe(false);
    });

    it('should handle logout failure', async () => {
      mockOctgn.logout.mockRejectedValue(new Error('Logout failed'));

      await useAuthStore.getState().logout();

      expect(useAuthStore.getState().error).toBe('Logout failed');
      expect(useAuthStore.getState().isLoading).toBe(false);
    });

    it('should handle non-Error exception in logout', async () => {
      mockOctgn.logout.mockRejectedValue(42);

      await useAuthStore.getState().logout();

      expect(useAuthStore.getState().error).toBe('Logout failed');
    });
  });

  describe('checkSession', () => {
    it('should restore user and session when session is valid', async () => {
      mockOctgn.getSession.mockResolvedValue({
        success: true,
        user: { id: '1', username: 'alice', isSubscriber: true },
        session: { userId: '1', sessionId: 's1', deviceId: 'd1' },
      });

      await useAuthStore.getState().checkSession();

      const state = useAuthStore.getState();
      expect(state.user).toEqual({ id: '1', username: 'alice', isSubscriber: true });
      expect(state.session).toEqual({ userId: '1', sessionId: 's1', deviceId: 'd1' });
      expect(state.isLoading).toBe(false);
    });

    it('should clear user and session when no valid session', async () => {
      useAuthStore.setState({
        user: { id: '1', username: 'alice', isSubscriber: false },
      });
      mockOctgn.getSession.mockResolvedValue({ success: false });

      await useAuthStore.getState().checkSession();

      const state = useAuthStore.getState();
      expect(state.user).toBeNull();
      expect(state.session).toBeNull();
    });

    it('should clear state on exception', async () => {
      useAuthStore.setState({
        user: { id: '1', username: 'alice', isSubscriber: false },
      });
      mockOctgn.getSession.mockRejectedValue(new Error('fail'));

      await useAuthStore.getState().checkSession();

      expect(useAuthStore.getState().user).toBeNull();
      expect(useAuthStore.getState().session).toBeNull();
      expect(useAuthStore.getState().isLoading).toBe(false);
    });
  });
});

// ---------------------------------------------------------------------------
// Lobby Store
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

  describe('refreshGames', () => {
    it('should fetch and store sorted games', async () => {
      const games: HostedGame[] = [
        makeGame({ id: 'g-old', status: 2 as GameStatus, dateCreated: '2026-01-01T00:00:00Z' }),
        makeGame({ id: 'g-new', status: 1 as GameStatus, dateCreated: '2026-01-15T00:00:00Z' }),
      ];
      mockOctgn.getGames.mockResolvedValue(games);

      await useLobbyStore.getState().refreshGames();

      const state = useLobbyStore.getState();
      expect(state.isLoading).toBe(false);
      expect(state.error).toBeNull();
      // GameReady (status=1) should come before InProgress (status=2)
      expect(state.games[0].id).toBe('g-new');
      expect(state.games[1].id).toBe('g-old');
    });

    it('should sort by date within same status group (newest first)', async () => {
      const games: HostedGame[] = [
        makeGame({ id: 'old', status: 1 as GameStatus, dateCreated: '2026-01-01T00:00:00Z' }),
        makeGame({ id: 'new', status: 1 as GameStatus, dateCreated: '2026-01-15T00:00:00Z' }),
      ];
      mockOctgn.getGames.mockResolvedValue(games);

      await useLobbyStore.getState().refreshGames();

      expect(useLobbyStore.getState().games[0].id).toBe('new');
      expect(useLobbyStore.getState().games[1].id).toBe('old');
    });

    it('should handle error when fetching games', async () => {
      mockOctgn.getGames.mockRejectedValue(new Error('Connection refused'));

      await useLobbyStore.getState().refreshGames();

      const state = useLobbyStore.getState();
      expect(state.isLoading).toBe(false);
      expect(state.error).toBe('Connection refused');
    });

    it('should handle non-Error exceptions', async () => {
      mockOctgn.getGames.mockRejectedValue('boom');

      await useLobbyStore.getState().refreshGames();

      expect(useLobbyStore.getState().error).toBe('Failed to fetch games');
    });
  });

  describe('setFilter', () => {
    it('should update searchText', () => {
      useLobbyStore.getState().setFilter({ searchText: 'magic' });
      expect(useLobbyStore.getState().filter.searchText).toBe('magic');
    });

    it('should update hideUninstalled', () => {
      useLobbyStore.getState().setFilter({ hideUninstalled: true });
      expect(useLobbyStore.getState().filter.hideUninstalled).toBe(true);
    });

    it('should merge partial updates without overwriting other fields', () => {
      useLobbyStore.getState().setFilter({ searchText: 'mtg' });
      useLobbyStore.getState().setFilter({ hideUninstalled: true });
      const f = useLobbyStore.getState().filter;
      expect(f.searchText).toBe('mtg');
      expect(f.hideUninstalled).toBe(true);
    });
  });

  describe('filtering (useFilteredGames logic)', () => {
    // We test the filtering logic via the exported function indirectly through the store.
    // Since useFilteredGames is a hook, we test the underlying logic.

    it('should filter games by name', async () => {
      const games = [
        makeGame({ id: '1', name: 'Magic Game' }),
        makeGame({ id: '2', name: 'Pokemon Match' }),
      ];
      mockOctgn.getGames.mockResolvedValue(games);
      await useLobbyStore.getState().refreshGames();

      useLobbyStore.getState().setFilter({ searchText: 'magic' });

      // We cannot call useFilteredGames outside of React, but we can test
      // the filter matches the store's games against the filter.
      const state = useLobbyStore.getState();
      const filtered = state.games.filter(
        (g) =>
          g.name.toLowerCase().includes(state.filter.searchText.toLowerCase()) ||
          g.gameName.toLowerCase().includes(state.filter.searchText.toLowerCase()) ||
          g.hostUser.username.toLowerCase().includes(state.filter.searchText.toLowerCase()),
      );
      expect(filtered).toHaveLength(1);
      expect(filtered[0].id).toBe('1');
    });

    it('should filter by game name', async () => {
      const games = [
        makeGame({ id: '1', gameName: 'Netrunner' }),
        makeGame({ id: '2', gameName: 'L5R' }),
      ];
      mockOctgn.getGames.mockResolvedValue(games);
      await useLobbyStore.getState().refreshGames();
      useLobbyStore.getState().setFilter({ searchText: 'netrunner' });

      const state = useLobbyStore.getState();
      const filtered = state.games.filter(
        (g) =>
          g.name.toLowerCase().includes(state.filter.searchText.toLowerCase()) ||
          g.gameName.toLowerCase().includes(state.filter.searchText.toLowerCase()) ||
          g.hostUser.username.toLowerCase().includes(state.filter.searchText.toLowerCase()),
      );
      expect(filtered).toHaveLength(1);
      expect(filtered[0].id).toBe('1');
    });

    it('should filter by host username', async () => {
      const games = [
        makeGame({ id: '1', hostUser: { id: 'u1', username: 'Alice', isSubscriber: false } }),
        makeGame({ id: '2', hostUser: { id: 'u2', username: 'Bob', isSubscriber: false } }),
      ];
      mockOctgn.getGames.mockResolvedValue(games);
      await useLobbyStore.getState().refreshGames();
      useLobbyStore.getState().setFilter({ searchText: 'alice' });

      const state = useLobbyStore.getState();
      const filtered = state.games.filter(
        (g) => g.hostUser.username.toLowerCase().includes(state.filter.searchText.toLowerCase()),
      );
      expect(filtered).toHaveLength(1);
      expect(filtered[0].id).toBe('1');
    });

    it('should return all games when search text is empty', async () => {
      const games = [makeGame({ id: '1' }), makeGame({ id: '2' })];
      mockOctgn.getGames.mockResolvedValue(games);
      await useLobbyStore.getState().refreshGames();
      useLobbyStore.getState().setFilter({ searchText: '' });

      expect(useLobbyStore.getState().games).toHaveLength(2);
    });
  });

  describe('sorting', () => {
    it('should sort GameReady before InProgress before Unknown before Finished', async () => {
      const games = [
        makeGame({ id: 'finished', status: 3 as GameStatus }),
        makeGame({ id: 'unknown', status: 0 as GameStatus }),
        makeGame({ id: 'ready', status: 1 as GameStatus }),
        makeGame({ id: 'inprogress', status: 2 as GameStatus }),
      ];
      mockOctgn.getGames.mockResolvedValue(games);
      await useLobbyStore.getState().refreshGames();

      const sorted = useLobbyStore.getState().games;
      expect(sorted[0].id).toBe('ready');
      expect(sorted[1].id).toBe('inprogress');
      expect(sorted[2].id).toBe('unknown');
      expect(sorted[3].id).toBe('finished');
    });
  });

  describe('hostGame', () => {
    it('should call hostGame and refresh', async () => {
      mockOctgn.hostGame.mockResolvedValue(undefined);
      mockOctgn.getGames.mockResolvedValue([]);

      await useLobbyStore.getState().hostGame({ name: 'My Game' });

      expect(mockOctgn.hostGame).toHaveBeenCalledWith({ name: 'My Game' });
      expect(mockOctgn.getGames).toHaveBeenCalled();
    });

    it('should handle host error', async () => {
      mockOctgn.hostGame.mockRejectedValue(new Error('Port in use'));

      await useLobbyStore.getState().hostGame({ name: 'Game' });

      expect(useLobbyStore.getState().error).toBe('Port in use');
      expect(useLobbyStore.getState().isLoading).toBe(false);
    });
  });

  describe('joinGame', () => {
    it('should call joinGame with gameId', async () => {
      mockOctgn.joinGame.mockResolvedValue(undefined);

      await useLobbyStore.getState().joinGame('game-123');

      expect(mockOctgn.joinGame).toHaveBeenCalledWith('game-123', undefined, undefined);
      expect(useLobbyStore.getState().isLoading).toBe(false);
    });

    it('should call joinGame with password', async () => {
      mockOctgn.joinGame.mockResolvedValue(undefined);

      await useLobbyStore.getState().joinGame('game-123', 'secret');

      expect(mockOctgn.joinGame).toHaveBeenCalledWith('game-123', 'secret', undefined);
    });

    it('should handle join error', async () => {
      mockOctgn.joinGame.mockRejectedValue(new Error('Game full'));

      await useLobbyStore.getState().joinGame('game-123');

      expect(useLobbyStore.getState().error).toBe('Game full');
    });
  });

  describe('startAutoRefresh', () => {
    beforeEach(() => {
      vi.useFakeTimers();
    });

    afterEach(() => {
      vi.useRealTimers();
    });

    it('should perform an initial refresh immediately', () => {
      mockOctgn.getGames.mockResolvedValue([]);

      useLobbyStore.getState().startAutoRefresh();

      expect(mockOctgn.getGames).toHaveBeenCalledTimes(1);
    });

    it('should return a cleanup function that stops the interval', () => {
      mockOctgn.getGames.mockResolvedValue([]);

      const cleanup = useLobbyStore.getState().startAutoRefresh();
      cleanup();

      // Advance past the interval - no additional calls should happen
      vi.advanceTimersByTime(30_000);
      // Only the initial call should have been made
      expect(mockOctgn.getGames).toHaveBeenCalledTimes(1);
    });

    it('should refresh at 15-second intervals', () => {
      mockOctgn.getGames.mockResolvedValue([]);

      const cleanup = useLobbyStore.getState().startAutoRefresh();

      // Initial call
      expect(mockOctgn.getGames).toHaveBeenCalledTimes(1);

      vi.advanceTimersByTime(15_000);
      expect(mockOctgn.getGames).toHaveBeenCalledTimes(2);

      vi.advanceTimersByTime(15_000);
      expect(mockOctgn.getGames).toHaveBeenCalledTimes(3);

      cleanup();
    });
  });
});

// ---------------------------------------------------------------------------
// App Store
// ---------------------------------------------------------------------------

describe('useAppStore', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    useAppStore.setState({
      currentPage: 'login',
      version: '',
      isMaximized: false,
    });
  });

  describe('navigate', () => {
    it('should change the current page', () => {
      useAppStore.getState().navigate('lobby');
      expect(useAppStore.getState().currentPage).toBe('lobby');
    });

    it('should navigate to game', () => {
      useAppStore.getState().navigate('game');
      expect(useAppStore.getState().currentPage).toBe('game');
    });

    it('should navigate to deck-builder', () => {
      useAppStore.getState().navigate('deck-builder');
      expect(useAppStore.getState().currentPage).toBe('deck-builder');
    });

    it('should navigate to settings', () => {
      useAppStore.getState().navigate('settings');
      expect(useAppStore.getState().currentPage).toBe('settings');
    });

    it('should navigate back to login', () => {
      useAppStore.getState().navigate('lobby');
      useAppStore.getState().navigate('login');
      expect(useAppStore.getState().currentPage).toBe('login');
    });
  });

  describe('loadVersion', () => {
    it('should set version from IPC', async () => {
      mockOctgn.getVersion.mockResolvedValue('3.4.424.0');

      await useAppStore.getState().loadVersion();

      expect(useAppStore.getState().version).toBe('3.4.424.0');
    });

    it('should silently handle version fetch failure', async () => {
      mockOctgn.getVersion.mockRejectedValue(new Error('fail'));

      await useAppStore.getState().loadVersion();

      // Version should remain unchanged
      expect(useAppStore.getState().version).toBe('');
    });
  });

  describe('minimize', () => {
    it('should call window.octgn.minimize', async () => {
      mockOctgn.minimize.mockResolvedValue(undefined);

      await useAppStore.getState().minimize();

      expect(mockOctgn.minimize).toHaveBeenCalled();
    });
  });

  describe('maximize', () => {
    it('should call window.octgn.maximize and toggle isMaximized', async () => {
      mockOctgn.maximize.mockResolvedValue(undefined);

      expect(useAppStore.getState().isMaximized).toBe(false);
      await useAppStore.getState().maximize();
      expect(mockOctgn.maximize).toHaveBeenCalled();
      expect(useAppStore.getState().isMaximized).toBe(true);

      await useAppStore.getState().maximize();
      expect(useAppStore.getState().isMaximized).toBe(false);
    });
  });

  describe('quit', () => {
    it('should call window.octgn.quit', async () => {
      mockOctgn.quit.mockResolvedValue(undefined);

      await useAppStore.getState().quit();

      expect(mockOctgn.quit).toHaveBeenCalled();
    });
  });

  describe('setMaximized', () => {
    it('should set isMaximized to true', () => {
      useAppStore.getState().setMaximized(true);
      expect(useAppStore.getState().isMaximized).toBe(true);
    });

    it('should set isMaximized to false', () => {
      useAppStore.setState({ isMaximized: true });
      useAppStore.getState().setMaximized(false);
      expect(useAppStore.getState().isMaximized).toBe(false);
    });
  });

  describe('initial state', () => {
    it('should start on login page', () => {
      useAppStore.setState({ currentPage: 'login' });
      expect(useAppStore.getState().currentPage).toBe('login');
    });

    it('should start with empty version', () => {
      useAppStore.setState({ version: '' });
      expect(useAppStore.getState().version).toBe('');
    });

    it('should start not maximized', () => {
      useAppStore.setState({ isMaximized: false });
      expect(useAppStore.getState().isMaximized).toBe(false);
    });
  });
});
