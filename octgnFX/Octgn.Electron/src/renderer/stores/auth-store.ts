import { create } from 'zustand';
import type { User, Session, LoginResult } from '../../shared/types';

interface AuthState {
  user: User | null;
  session: Session | null;
  isLoading: boolean;
  error: string | null;
}

interface AuthActions {
  login: (username: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  checkSession: () => Promise<void>;
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

    login: async (username, password) => {
      set({ isLoading: true, error: null });
      try {
        const result: LoginResult = await window.octgn.login(username, password);
        if (result.success) {
          set({
            user: result.user ?? null,
            session: result.session ?? null,
            isLoading: false,
            error: null,
          });
        } else {
          set({
            isLoading: false,
            error: result.error ?? 'Login failed',
          });
        }
      } catch (err) {
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
  };
});
