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
  addDirectRepo: (name: string, repoUrl: string, branch?: string) => Promise<{ success: boolean; error?: string }>;
  addRepoFeed: (name: string, indexUrl: string) => Promise<{ success: boolean; error?: string }>;
  removeFeed: (name: string) => Promise<void>;
  setFeedEnabled: (name: string, enabled: boolean) => Promise<void>;
  installFromRepo: (owner: string, repo: string, branch: string, gamePath: string, gameId: string) => Promise<void>;
}

// Monotonically incrementing sequence number — only the latest fetch wins.
let fetchAvailableSeq = 0;

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
      // Claim this sequence slot. If another call comes in while we're awaiting,
      // it will increment the counter and our result will be discarded.
      const seq = ++fetchAvailableSeq;
      set({ isLoadingAvailable: true });
      try {
        const games = (await window.octgn.listAvailableGames()) as AvailableGame[];
        // Only commit if no newer fetch has started since we launched
        if (seq === fetchAvailableSeq) {
          set({ availableGames: games, isLoadingAvailable: false });
        }
      } catch {
        if (seq === fetchAvailableSeq) {
          set({ isLoadingAvailable: false });
        }
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

    addDirectRepo: async (name, repoUrl, branch) => {
      const result = (await window.octgn.addDirectRepo(name, repoUrl, branch)) as { success: boolean; error?: string };
      if (result.success) await get().loadFeeds();
      return result;
    },

    addRepoFeed: async (name, indexUrl) => {
      const result = (await window.octgn.addRepoFeed(name, indexUrl)) as { success: boolean; error?: string };
      if (result.success) await get().loadFeeds();
      return result;
    },

    removeFeed: async (name) => {
      await window.octgn.removeFeed(name);
      await get().loadFeeds();
    },

    setFeedEnabled: async (name, enabled) => {
      // Optimistic update — flip the toggle immediately so the UI responds instantly
      set((s) => ({
        feeds: s.feeds.map((f) => (f.name === name ? { ...f, enabled } : f)),
      }));
      // Persist to disk (fast local write, non-blocking for UX)
      await window.octgn.setFeedEnabled(name, enabled);
      // Re-fetch available games — race-safe because fetchAvailable uses a sequence counter
      get().fetchAvailable();
    },

    installFromRepo: async (owner, repo, branch, gamePath, gameId) => {
      set((s) => ({
        installProgress: {
          ...s.installProgress,
          [gameId]: { gameId, phase: 'downloading', percent: 0 },
        },
      }));
      await window.octgn.installFromRepo(owner, repo, branch, gamePath, gameId);
    },
  };
});
