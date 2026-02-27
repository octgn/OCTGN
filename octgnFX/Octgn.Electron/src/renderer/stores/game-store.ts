import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';
import type { GameState, Deck } from '../../shared/types';
import { useToastStore } from './toast-store';

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
  // Typed game actions
  flipCard: (cardId: number, faceUp: boolean) => Promise<void>;
  rotateCard: (cardId: number, rotation: number) => Promise<void>;
  moveCards: (cardIds: number[], groupId: number, indices: number[], faceUp: boolean[]) => Promise<void>;
  moveCardsAt: (cardIds: number[], x: number[], y: number[], indices: number[], faceUp: boolean[]) => Promise<void>;
  nextTurn: () => Promise<void>;
  setCounter: (counterId: number, value: number) => Promise<void>;
  peekCard: (cardId: number) => Promise<void>;
  targetCard: (cardId: number, playerId: number, active: boolean) => Promise<void>;
  highlightCard: (cardId: number, color: string) => Promise<void>;
  addMarker: (cardId: number, markerId: string, markerName: string, count: number) => Promise<void>;
  removeMarker: (cardId: number, markerId: string, markerName: string, count: number) => Promise<void>;
  shuffleGroup: (groupId: number) => Promise<void>;
}

export type GameStore = GameStoreState & GameStoreActions;

const sendTypedAction = async (action: Record<string, unknown>) => {
  try {
    await window.octgn.gameAction(action);
  } catch {
    // errors handled by store
  }
};

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
        const message = err instanceof Error ? err.message : 'Failed to send action';
        set((state) => {
          state.error = message;
        });
        useToastStore.getState().addToast(message, 'error');
      }
    },

    sendChat: async (message) => {
      try {
        await window.octgn.gameChat(message);
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to send chat';
        set((state) => {
          state.error = errorMsg;
        });
        useToastStore.getState().addToast(errorMsg, 'error');
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
        const message = err instanceof Error ? err.message : 'Failed to load deck';
        set((state) => {
          state.isLoading = false;
          state.error = message;
        });
        useToastStore.getState().addToast(message, 'error');
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
        const message = err instanceof Error ? err.message : 'Failed to leave game';
        set((state) => {
          state.error = message;
        });
        useToastStore.getState().addToast(message, 'error');
      }
    },

    subscribe: () => {
      let previousState: GameState | null = null;

      const unsubscribe = window.octgn.onGameStateUpdate((rawState) => {
        const gameState = rawState as GameState;
        const addToast = useToastStore.getState().addToast;

        if (previousState) {
          const prevPlayerIds = new Set(previousState.players.map((p) => p.id));
          const newPlayerIds = new Set(gameState.players.map((p) => p.id));

          for (const player of gameState.players) {
            if (!prevPlayerIds.has(player.id)) {
              addToast(`${player.name} joined the game`, 'info');
            }
          }

          for (const player of previousState.players) {
            if (!newPlayerIds.has(player.id)) {
              addToast(`${player.name} left the game`, 'warning');
            }
          }

          if (!previousState.isStarted && gameState.isStarted) {
            addToast('Game has started!', 'success');
          }
        }

        previousState = structuredClone(gameState);

        set((state) => {
          state.gameState = gameState;
          state.isConnected = true;
          state.error = null;
        });
      });

      return () => {
        previousState = null;
        unsubscribe();
        set((state) => {
          state.isConnected = false;
        });
      };
    },

    // Typed game actions
    flipCard: (cardId, faceUp) => sendTypedAction({ type: 'flipCard', cardId, faceUp }),
    rotateCard: (cardId, rotation) => sendTypedAction({ type: 'rotateCard', cardId, rotation }),
    moveCards: (cardIds, groupId, indices, faceUp) =>
      sendTypedAction({ type: 'moveCards', cardIds, groupId, indices, faceUp }),
    moveCardsAt: (cardIds, x, y, indices, faceUp) =>
      sendTypedAction({ type: 'moveCardsAt', cardIds, x, y, indices, faceUp }),
    nextTurn: () => sendTypedAction({ type: 'nextTurn' }),
    setCounter: (counterId, value) => sendTypedAction({ type: 'setCounter', counterId, value }),
    peekCard: (cardId) => sendTypedAction({ type: 'peekCard', cardId }),
    targetCard: (cardId, playerId, active) =>
      sendTypedAction({ type: 'targetCard', cardId, playerId, active }),
    highlightCard: (cardId, color) => sendTypedAction({ type: 'highlightCard', cardId, color }),
    addMarker: (cardId, markerId, markerName, count) =>
      sendTypedAction({ type: 'addMarker', cardId, markerId, markerName, count }),
    removeMarker: (cardId, markerId, markerName, count) =>
      sendTypedAction({ type: 'removeMarker', cardId, markerId, markerName, count }),
    shuffleGroup: (groupId) => sendTypedAction({ type: 'shuffleGroup', groupId }),
  })),
);

if (typeof window !== 'undefined') {
  (window as any).__gameStore = useGameStore;
}
