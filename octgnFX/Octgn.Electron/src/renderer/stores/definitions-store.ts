import { create } from 'zustand';
import type { GameDefinition, AvailableGame, InstallProgress } from '../../shared/types';

interface DefinitionsState {
  installedGames: GameDefinition[];
  availableGames: AvailableGame[];
  installProgress: Record<string, InstallProgress>;
  isLoadingInstalled: boolean;
  isLoadingAvailable: boolean;

  loadInstalled: () => Promise<void>;
  fetchAvailable: () => Promise<void>;
  install: (gameId: string, downloadUrl: string) => Promise<void>;
  uninstall: (gameId: string) => Promise<void>;
}

export const useDefinitionsStore = create<DefinitionsState>((set, get) => {
  // Subscribe to install progress events from main
  if (window.octgn?.onInstallProgress) {
    window.octgn.onInstallProgress((raw) => {
      const progress = raw as InstallProgress;
      set((s) => ({
        installProgress: { ...s.installProgress, [progress.gameId]: progress },
      }));
      // When done, refresh installed list
      if (progress.phase === 'done') {
        get().loadInstalled();
      }
    });
  }

  return {
    installedGames: [],
    availableGames: [],
    installProgress: {},
    isLoadingInstalled: false,
    isLoadingAvailable: false,

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
      set({ isLoadingAvailable: true });
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
      // Progress updates come via onInstallProgress listener above
    },

    uninstall: async (gameId) => {
      await window.octgn.uninstallGame(gameId);
      set((s) => ({
        installedGames: s.installedGames.filter((g) => g.id !== gameId),
      }));
    },
  };
});
