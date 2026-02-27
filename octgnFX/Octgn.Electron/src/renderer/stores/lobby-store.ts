import { create } from 'zustand';
import type { HostedGame } from '../../shared/types';
import { useAppStore } from './app-store';

interface LobbyFilter {
  searchText: string;
  hideUninstalled: boolean;
}

interface LobbyState {
  games: HostedGame[];
  isLoading: boolean;
  error: string | null;
  filter: LobbyFilter;
}

interface LobbyActions {
  refreshGames: () => Promise<void>;
  hostGame: (options: Record<string, unknown>) => Promise<void>;
  joinGame: (gameId: string, password?: string, spectator?: boolean) => Promise<void>;
  setFilter: (filter: Partial<LobbyFilter>) => void;
  startAutoRefresh: () => () => void;
}

export type LobbyStore = LobbyState & LobbyActions;

const AUTO_REFRESH_INTERVAL = 15_000;

/** Sort priority: GameReady first, then InProgress, then others. Within each group, newest first. */
function sortGames(games: HostedGame[]): HostedGame[] {
  const statusPriority: Record<number, number> = {
    1: 0, // GameReady
    2: 1, // InProgress
    0: 2, // Unknown
    3: 3, // Finished
  };

  return [...games].sort((a, b) => {
    const priorityDiff = (statusPriority[a.status] ?? 9) - (statusPriority[b.status] ?? 9);
    if (priorityDiff !== 0) return priorityDiff;
    return new Date(b.dateCreated).getTime() - new Date(a.dateCreated).getTime();
  });
}

function filterGames(games: HostedGame[], filter: LobbyFilter): HostedGame[] {
  let filtered = games;
  if (filter.searchText) {
    const search = filter.searchText.toLowerCase();
    filtered = filtered.filter(
      (g) =>
        g.name.toLowerCase().includes(search) ||
        g.gameName.toLowerCase().includes(search) ||
        g.hostUser.username.toLowerCase().includes(search),
    );
  }
  return filtered;
}

export const useLobbyStore = create<LobbyStore>()((set, get) => ({
  games: [],
  isLoading: false,
  error: null,
  filter: {
    searchText: '',
    hideUninstalled: false,
  },

  refreshGames: async () => {
    set({ isLoading: true, error: null });
    try {
      const games: HostedGame[] = await window.octgn.getGames();
      set({ games: sortGames(games), isLoading: false });
    } catch (err) {
      set({
        isLoading: false,
        error: err instanceof Error ? err.message : 'Failed to fetch games',
      });
    }
  },

  hostGame: async (options) => {
    set({ isLoading: true, error: null });
    try {
      await window.octgn.hostGame(options);
      // Refresh the list after hosting
      await get().refreshGames();
    } catch (err) {
      set({
        isLoading: false,
        error: err instanceof Error ? err.message : 'Failed to host game',
      });
    }
  },

  joinGame: async (gameId, password?, spectator?) => {
    set({ isLoading: true, error: null });
    try {
      const result = await window.octgn.joinGame(gameId, password, spectator);
      set({ isLoading: false });
      if (result && result.success) {
        useAppStore.getState().navigate('game');
      }
    } catch (err) {
      set({
        isLoading: false,
        error: err instanceof Error ? err.message : 'Failed to join game',
      });
    }
  },

  setFilter: (partial) => {
    set((state) => ({
      filter: { ...state.filter, ...partial },
    }));
  },

  startAutoRefresh: () => {
    // Perform an initial refresh immediately
    get().refreshGames();

    const intervalId = setInterval(() => {
      get().refreshGames();
    }, AUTO_REFRESH_INTERVAL);

    // Return cleanup function
    return () => clearInterval(intervalId);
  },
}));

if (typeof window !== 'undefined') {
  (window as any).__lobbyStore = useLobbyStore;
}

/** Selector that returns games after applying the current filter. */
export function useFilteredGames(): HostedGame[] {
  const games = useLobbyStore((s) => s.games);
  const filter = useLobbyStore((s) => s.filter);
  return filterGames(games, filter);
}
