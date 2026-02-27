import { describe, it, expect, vi, beforeEach } from 'vitest';
import type { LoginResult } from '@shared/types';

// ---------------------------------------------------------------------------
// Mock window.octgn
// ---------------------------------------------------------------------------
const mockOctgn = {
  login: vi.fn<[string, string], Promise<LoginResult>>(),
  logout: vi.fn<[], Promise<void>>(),
  getSession: vi.fn<[], Promise<LoginResult>>(),
  getGames: vi.fn(),
  hostGame: vi.fn(),
  joinGame: vi.fn(),
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
import { useAuthStore } from '@renderer/stores/auth-store';

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('useAuthStore', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    useAuthStore.setState({
      user: null,
      session: null,
      isLoading: false,
      error: null,
    });
  });

  describe('login — success', () => {
    it('should set user and session on successful login', async () => {
      mockOctgn.login.mockResolvedValue({
        success: true,
        user: { id: 'u1', username: 'alice', isSubscriber: false },
        session: { userId: 'u1', sessionId: 's1', deviceId: 'd1' },
      });

      await useAuthStore.getState().login('alice', 'pw');

      const state = useAuthStore.getState();
      expect(state.user).toEqual({ id: 'u1', username: 'alice', isSubscriber: false });
      expect(state.session).toEqual({ userId: 'u1', sessionId: 's1', deviceId: 'd1' });
      expect(state.isLoading).toBe(false);
      expect(state.error).toBeNull();
    });

    it('should call window.octgn.login with the right arguments', async () => {
      mockOctgn.login.mockResolvedValue({ success: true });

      await useAuthStore.getState().login('bob', 'secret');

      expect(mockOctgn.login).toHaveBeenCalledWith('bob', 'secret');
    });
  });

  describe('login — failure from API', () => {
    it('should set error message when login fails', async () => {
      mockOctgn.login.mockResolvedValue({
        success: false,
        error: 'Unknown username',
      });

      await useAuthStore.getState().login('nobody', 'pw');

      const state = useAuthStore.getState();
      expect(state.user).toBeNull();
      expect(state.session).toBeNull();
      expect(state.isLoading).toBe(false);
      expect(state.error).toBe('Unknown username');
    });

    it('should use fallback error when none provided', async () => {
      mockOctgn.login.mockResolvedValue({ success: false });

      await useAuthStore.getState().login('x', 'y');

      expect(useAuthStore.getState().error).toBe('Login failed');
    });
  });

  describe('login — exception', () => {
    it('should set error from Error instance', async () => {
      mockOctgn.login.mockRejectedValue(new Error('Network down'));

      await useAuthStore.getState().login('alice', 'pw');

      const state = useAuthStore.getState();
      expect(state.isLoading).toBe(false);
      expect(state.error).toBe('Network down');
      expect(state.user).toBeNull();
    });

    it('should set generic error for non-Error exceptions', async () => {
      mockOctgn.login.mockRejectedValue('boom');

      await useAuthStore.getState().login('alice', 'pw');

      expect(useAuthStore.getState().error).toBe('An unexpected error occurred');
    });
  });

  describe('login — loading state', () => {
    it('should set isLoading=true while login is in progress', async () => {
      let resolveLogin!: (v: LoginResult) => void;
      mockOctgn.login.mockReturnValue(
        new Promise<LoginResult>((resolve) => {
          resolveLogin = resolve;
        }),
      );

      const promise = useAuthStore.getState().login('alice', 'pw');
      expect(useAuthStore.getState().isLoading).toBe(true);

      resolveLogin({ success: true });
      await promise;
      expect(useAuthStore.getState().isLoading).toBe(false);
    });

    it('should clear previous error when starting a new login', async () => {
      useAuthStore.setState({ error: 'old error' });

      mockOctgn.login.mockResolvedValue({ success: true });
      await useAuthStore.getState().login('alice', 'pw');

      expect(useAuthStore.getState().error).toBeNull();
    });
  });

  describe('logout', () => {
    it('should clear user and session', async () => {
      useAuthStore.setState({
        user: { id: 'u1', username: 'alice', isSubscriber: false },
        session: { userId: 'u1', sessionId: 's1', deviceId: 'd1' },
      });
      mockOctgn.logout.mockResolvedValue(undefined);

      await useAuthStore.getState().logout();

      const state = useAuthStore.getState();
      expect(state.user).toBeNull();
      expect(state.session).toBeNull();
      expect(state.isLoading).toBe(false);
    });

    it('should call window.octgn.logout', async () => {
      mockOctgn.logout.mockResolvedValue(undefined);

      await useAuthStore.getState().logout();

      expect(mockOctgn.logout).toHaveBeenCalledTimes(1);
    });

    it('should set error if logout throws', async () => {
      mockOctgn.logout.mockRejectedValue(new Error('Logout failed'));

      await useAuthStore.getState().logout();

      expect(useAuthStore.getState().error).toBe('Logout failed');
      expect(useAuthStore.getState().isLoading).toBe(false);
    });
  });
});
