import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';
import type { GameState, Deck } from '../../shared/types';

interface GameStoreState {
  gameState: GameState | null;
  isConnected: boolean;
  isLoading: boolean;
  error: string | null;
}

interface GameStoreActions {
  sendAction: (action: Record<string, unknown>) => Promise<void>;
  sendChat: (message: string) => Promise<void>;
  loadDeck: (deck: Deck) => Promise<void>;
  leaveGame: () => Promise<void>;
  subscribe: () => () => void;
}

export type GameStore = GameStoreState & GameStoreActions;

export const useGameStore = create<GameStore>()(
  immer((set) => ({
    gameState: null,
    isConnected: false,
    isLoading: false,
    error: null,

    sendAction: async (action) => {
      try {
        await window.octgn.gameAction(action);
      } catch (err) {
        set((state) => {
          state.error = err instanceof Error ? err.message : 'Failed to send action';
        });
      }
    },

    sendChat: async (message) => {
      try {
        await window.octgn.gameChat(message);
      } catch (err) {
        set((state) => {
          state.error = err instanceof Error ? err.message : 'Failed to send chat';
        });
      }
    },

    loadDeck: async (deck) => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });
      try {
        await window.octgn.loadDeck(deck);
        set((state) => {
          state.isLoading = false;
        });
      } catch (err) {
        set((state) => {
          state.isLoading = false;
          state.error = err instanceof Error ? err.message : 'Failed to load deck';
        });
      }
    },

    leaveGame: async () => {
      try {
        await window.octgn.leaveGame();
        set((state) => {
          state.gameState = null;
          state.isConnected = false;
        });
      } catch (err) {
        set((state) => {
          state.error = err instanceof Error ? err.message : 'Failed to leave game';
        });
      }
    },

    subscribe: () => {
      const unsubscribe = window.octgn.onGameStateUpdate((rawState) => {
        const gameState = rawState as GameState;
        set((state) => {
          state.gameState = gameState;
          state.isConnected = true;
        });
      });

      return () => {
        unsubscribe();
        set((state) => {
          state.isConnected = false;
        });
      };
    },
  })),
);
