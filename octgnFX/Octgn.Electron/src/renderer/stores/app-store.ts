import { create } from 'zustand';

export type AppPage = 'login' | 'lobby' | 'game' | 'deck-builder' | 'settings';

interface AppState {
  currentPage: AppPage;
  version: string;
  isMaximized: boolean;
}

interface AppActions {
  navigate: (page: AppPage) => void;
  minimize: () => Promise<void>;
  maximize: () => Promise<void>;
  quit: () => Promise<void>;
  loadVersion: () => Promise<void>;
  setMaximized: (maximized: boolean) => void;
}

export type AppStore = AppState & AppActions;

export const useAppStore = create<AppStore>()((set) => ({
  currentPage: 'login',
  version: '',
  isMaximized: false,

  navigate: (page) => {
    set({ currentPage: page });
  },

  minimize: async () => {
    await window.octgn.minimize();
  },

  maximize: async () => {
    await window.octgn.maximize();
    set((state) => ({ isMaximized: !state.isMaximized }));
  },

  quit: async () => {
    await window.octgn.quit();
  },

  loadVersion: async () => {
    try {
      const version: string = await window.octgn.getVersion();
      set({ version });
    } catch {
      // Version fetch is non-critical
    }
  },

  setMaximized: (maximized) => {
    set({ isMaximized: maximized });
  },
}));
