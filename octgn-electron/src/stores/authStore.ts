import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { octgnApi, Session } from '../services/OctgnApiService';

interface User {
  id: string;
  username: string;
  iconUrl?: string;
  isSubscribed: boolean;
}

interface AuthState {
  user: User | null;
  session: Session | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  
  login: (username: string, password: string) => Promise<boolean>;
  logout: () => Promise<void>;
  checkSession: () => Promise<boolean>;
  clearError: () => void;
  setUser: (user: User | null) => void;
  setSession: (session: Session | null) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      session: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,

      login: async (username: string, password: string): Promise<boolean> => {
        set({ isLoading: true, error: null });

        try {
          let deviceId = localStorage.getItem('octgn_device_id');
          if (!deviceId) {
            deviceId = crypto.randomUUID();
            localStorage.setItem('octgn_device_id', deviceId);
          }

          const result = await octgnApi.createSession(username, password, deviceId);

          if (result.type === 'Ok' && result.sessionKey && result.userId) {
            const session: Session = {
              userId: result.userId,
              deviceId,
              sessionKey: result.sessionKey,
              username: result.username || username,
            };

            const user: User = {
              id: result.userId,
              username: result.username || username,
              isSubscribed: false,
            };

            octgnApi.setSession(session);

            set({
              user,
              session,
              isAuthenticated: true,
              isLoading: false,
              error: null,
            });

            return true;
          } else {
            const errorMessage = getLoginErrorMessage(result.type);
            set({
              user: null,
              session: null,
              isAuthenticated: false,
              isLoading: false,
              error: errorMessage,
            });
            return false;
          }
        } catch (error: any) {
          set({
            user: null,
            session: null,
            isAuthenticated: false,
            isLoading: false,
            error: error.message || 'Login failed',
          });
          return false;
        }
      },

      logout: async () => {
        const { session } = get();
        if (session) {
          try {
            await octgnApi.clearSession(session.userId, session.deviceId, session.sessionKey);
          } catch (e) {
            console.error('Error clearing session:', e);
          }
        }
        octgnApi.setSession(null);
        set({
          user: null,
          session: null,
          isAuthenticated: false,
          error: null,
        });
      },

      checkSession: async (): Promise<boolean> => {
        const { session } = get();
        if (!session) return false;

        try {
          const isValid = await octgnApi.validateSession(
            session.userId,
            session.deviceId,
            session.sessionKey
          );

          if (!isValid) {
            set({
              user: null,
              session: null,
              isAuthenticated: false,
            });
          }

          return isValid;
        } catch (error) {
          set({
            user: null,
            session: null,
            isAuthenticated: false,
          });
          return false;
        }
      },

      clearError: () => set({ error: null }),
      setUser: (user) => set({ user }),
      setSession: (session) => {
        octgnApi.setSession(session);
        set({ session, isAuthenticated: !!session });
      },
    }),
    {
      name: 'octgn-auth',
      partialize: (state) => ({
        user: state.user,
        session: state.session,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);

function getLoginErrorMessage(type: string): string {
  switch (type) {
    case 'UnknownUsername':
      return 'Username not found';
    case 'PasswordWrong':
      return 'Incorrect password';
    case 'EmailUnverified':
      return 'Please verify your email address first';
    case 'NotSubscribed':
      return 'A subscription is required';
    case 'NoEmailAssociated':
      return 'No email associated with this account';
    case 'UnknownError':
    default:
      return 'Login failed. Please try again.';
  }
}

export default useAuthStore;
