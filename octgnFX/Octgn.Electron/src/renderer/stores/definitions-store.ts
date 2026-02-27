import { create } from 'zustand';
import type { GameDefinition, AvailableGame, InstallProgress, GameFeed } from '../../shared/types';

interface DefinitionsState {
  installedGames: GameDefinition[];
  availableGames: AvailableGame[];
  feeds: GameFeed[];
  installProgress: Record<string, InstallProgress>;
  isLoadingInstalled: boolean;
  isLoadingAvailable: boolean;
  isLoadingFeeds: boolean;

  loadInstalled: () => Promise<void>;
  fetchAvailable: () => Promise<void>;
  install: (gameId: string, downloadUrl: string) => Promise<void>;
  uninstall: (gameId: string) => Promise<void>;

  loadFeeds: () => Promise<void>;
  addFeed: (name: string, url: string, username?: string, password?: string) => Promise<{ success: boolean; error?: string }>;
  removeFeed: (name: string) => Promise<void>;
  setFeedEnabled: (name: string, enabled: boolean) => Promise<void>;
}

export const useDefinitionsStore = create<DefinitionsState>((set, get) => {
  // Subscribe to install progress events from main
  if (window.octgn?.onInstallProgress) {
    window.octgn.onInstallProgress((raw) => {
      const progress = raw as InstallProgress;
      set((s) => ({
        installProgress: { ...s.installProgress, [progress.gameId]: progress },
      }));
      if (progress.phase === 'done') {
        get().loadInstalled();
      }
    });
  }

  return {
    installedGames: [],
    availableGames: [],
    feeds: [],
    installProgress: {},
    isLoadingInstalled: false,
    isLoadingAvailable: false,
    isLoadingFeeds: false,

    loadInstalled: async () => {
      set({ isLoadingInstalled: true });
      try {
        const games = (await window.octgn.listInstalledGames()) as GameDefinition[];
        set({ installedGames: games });
      } finally {
        set({ isLoadingInstalled: false });
      }
    },

    fetchAvailable: async () => {
      set({ isLoadingAvailable: true, availableGames: [] });
      try {
        const games = (await window.octgn.listAvailableGames()) as AvailableGame[];
        set({ availableGames: games });
      } finally {
        set({ isLoadingAvailable: false });
      }
    },

    install: async (gameId, downloadUrl) => {
      set((s) => ({
        installProgress: {
          ...s.installProgress,
          [gameId]: { gameId, phase: 'downloading', percent: 0 },
        },
      }));
      await window.octgn.installGame(gameId, downloadUrl);
    },

    uninstall: async (gameId) => {
      await window.octgn.uninstallGame(gameId);
      set((s) => ({
        installedGames: s.installedGames.filter((g) => g.id !== gameId),
      }));
    },

    loadFeeds: async () => {
      set({ isLoadingFeeds: true });
      try {
        const feeds = (await window.octgn.listFeeds()) as GameFeed[];
        set({ feeds });
      } finally {
        set({ isLoadingFeeds: false });
      }
    },

    addFeed: async (name, url, username, password) => {
      const result = (await window.octgn.addFeed(name, url, username, password)) as { success: boolean; error?: string };
      if (result.success) await get().loadFeeds();
      return result;
    },

    removeFeed: async (name) => {
      await window.octgn.removeFeed(name);
      await get().loadFeeds();
    },

    setFeedEnabled: async (name, enabled) => {
      await window.octgn.setFeedEnabled(name, enabled);
      set((s) => ({
        feeds: s.feeds.map((f) => f.name === name ? { ...f, enabled } : f),
      }));
      // Re-fetch available games with the new feed state
      get().fetchAvailable();
    },
  };
});
