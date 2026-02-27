import { create } from 'zustand';
import type { User, Session, LoginResult } from '../../shared/types';

interface SavedCredentials {
  username: string;
  password: string;
}

interface AuthState {
  user: User | null;
  session: Session | null;
  isLoading: boolean;
  error: string | null;
  savedUsername: string;
  savedPassword: string;
  rememberMe: boolean;
}

interface AuthActions {
  login: (username: string, password: string, rememberMe: boolean) => Promise<void>;
  logout: () => Promise<void>;
  checkSession: () => Promise<void>;
  loadSavedCredentials: () => Promise<void>;
  setRememberMe: (value: boolean) => void;
  copyError: () => void;
}

export type AuthStore = AuthState & AuthActions;

export const useAuthStore = create<AuthStore>()((set, get, api) => {
  if (import.meta.env.DEV && typeof window !== 'undefined') {
    (window as any).__authStore = api;
  }
  return {
    user: null,
    session: null,
    isLoading: false,
    error: null,
    savedUsername: '',
    savedPassword: '',
    rememberMe: true,

    login: async (username, password, rememberMe) => {
      set({ isLoading: true, error: null });
      try {
        console.log('[auth-store] Calling window.octgn.login...');
        const result: LoginResult = await window.octgn.login(username, password, rememberMe);
        console.log('[auth-store] Login result:', JSON.stringify(result));
        if (result.success) {
          console.log('[auth-store] Setting user:', JSON.stringify(result.user));
          set({
            user: result.user ?? null,
            session: result.session ?? null,
            isLoading: false,
            error: null,
          });
          console.log('[auth-store] State after set — user:', JSON.stringify(get().user));
        } else {
          console.log('[auth-store] Login failed:', result.error);
          set({
            isLoading: false,
            error: result.error ?? 'Login failed',
          });
        }
      } catch (err) {
        console.error('[auth-store] Login exception:', err);
        set({
          isLoading: false,
          error: err instanceof Error ? err.message : 'An unexpected error occurred',
        });
      }
    },

    logout: async () => {
      set({ isLoading: true, error: null });
      try {
        await window.octgn.logout();
        set({ user: null, session: null, isLoading: false });
      } catch (err) {
        set({
          isLoading: false,
          error: err instanceof Error ? err.message : 'Logout failed',
        });
      }
    },

    checkSession: async () => {
      set({ isLoading: true, error: null });
      try {
        const result: LoginResult = await window.octgn.getSession();
        if (result.success) {
          set({
            user: result.user ?? null,
            session: result.session ?? null,
            isLoading: false,
          });
        } else {
          set({ user: null, session: null, isLoading: false });
        }
      } catch {
        set({ user: null, session: null, isLoading: false });
      }
    },

    loadSavedCredentials: async () => {
      try {
        const creds: SavedCredentials | null = await window.octgn.loadCredentials();
        if (creds) {
          set({
            savedUsername: creds.username,
            savedPassword: creds.password,
            rememberMe: Boolean(creds.username),
          });
        }
      } catch {
        // Ignore — credentials just won't be pre-filled
      }
    },

    setRememberMe: (value) => set({ rememberMe: value }),

    copyError: () => {
      const error = get().error;
      if (error && window.octgn?.writeClipboard) {
        window.octgn.writeClipboard(error);
      }
    },
  };
});
