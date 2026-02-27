import { create } from 'zustand';
import { Card, Group, Counter } from '../types/game';

interface GameState {
  // Connection state
  connected: boolean;
  connecting: boolean;
  serverAddress: string;
  serverPort: number;

  // Player info
  playerId: string | null;
  playerName: string;
  playerCount: number;
  isHost: boolean;

  // Game state
  gameId: string | null;
  gameName: string;
  cards: Map<number, Card>;
  groups: Map<number, Group>;
  counters: Map<string, Counter>;
  activePlayerId: string | null;
  turnNumber: number;
  phase: number;

  // UI state
  selectedCards: number[];
  hoveredCard: number | null;
  zoomedCard: Card | null;
  showChat: boolean;
  chatMessages: ChatMessage[];

  // Actions
  initialize: () => void;
  connect: (address: string, port: number) => Promise<void>;
  disconnect: () => void;
  hostGame: (port: number) => Promise<void>;

  // Game actions
  selectCards: (cardIds: number[]) => void;
  clearSelection: () => void;
  moveCards: (cardIds: number[], toGroup: number, faceUp: boolean) => void;
  turnCard: (cardId: number, faceUp: boolean) => void;
  rotateCard: (cardId: number, rotation: number) => void;
  shuffleGroup: (groupId: number) => void;

  // Chat
  chatInput: string;
  setChatInput: (input: string) => void;
  sendChat: (message: string) => void;
  receiveChat: (playerId: string, playerName: string, message: string, isSystem?: boolean) => void;

  // Card info
  zoomCard: (card: Card | null) => void;
  hoverCard: (cardId: number | null) => void;
}

interface ChatMessage {
  id: string;
  playerId: string;
  playerName: string;
  message: string;
  timestamp: number;
  isSystem?: boolean;
}

export const useGameStore = create<GameState>((set, get) => ({
  // Initial state
  connected: false,
  connecting: false,
  serverAddress: 'localhost',
  serverPort: 8888,

  playerId: null,
  playerName: 'Player',
  playerCount: 0,
  isHost: false,

  gameId: null,
  gameName: '',
  cards: new Map(),
  groups: new Map(),
  counters: new Map(),
  activePlayerId: null,
  turnNumber: 0,
  phase: 0,

  selectedCards: [],
  hoveredCard: null,
  zoomedCard: null,
  showChat: true,
  chatMessages: [],
  chatInput: '',

  // Initialize
  initialize: () => {
    // Load saved preferences
    const savedName = localStorage.getItem('playerName');
    if (savedName) {
      set({ playerName: savedName });
    }
  },

  // Connect to server
  connect: async (address: string, port: number) => {
    set({ connecting: true, serverAddress: address, serverPort: port });

    try {
      // This would be implemented with WebSocket connection
      // For now, simulate connection
      await new Promise((resolve) => setTimeout(resolve, 1000));

      set({
        connected: true,
        connecting: false,
        playerId: Math.random().toString(36).substring(7),
      });

      get().receiveChat('system', 'System', 'Connected to server', true);
    } catch (error) {
      set({ connecting: false });
      get().receiveChat('system', 'System', `Failed to connect: ${error}`);
    }
  },

  // Disconnect from server
  disconnect: () => {
    set({
      connected: false,
      playerId: null,
      cards: new Map(),
      groups: new Map(),
      selectedCards: [],
    });
    get().receiveChat('system', 'System', 'Disconnected from server');
  },

  // Host a game
  hostGame: async (port: number) => {
    if (!window.electronAPI) {
      throw new Error('Hosting requires Electron app');
    }

    try {
      const result = await window.electronAPI.startServer(port);
      if (result.success) {
        set({ isHost: true, serverPort: result.port });
        await get().connect('localhost', result.port);
      } else {
        throw new Error(result.error);
      }
    } catch (error) {
      get().receiveChat('system', 'System', `Failed to host: ${error}`);
      throw error;
    }
  },

  // Card selection
  selectCards: (cardIds: number[]) => {
    set({ selectedCards: cardIds });
  },

  clearSelection: () => {
    set({ selectedCards: [] });
  },

  // Move cards
  moveCards: (cardIds: number[], toGroup: number, faceUp: boolean) => {
    // This would send to server
    console.log('Moving cards:', cardIds, 'to group:', toGroup, 'faceUp:', faceUp);
    set({ selectedCards: [] });
  },

  // Turn card
  turnCard: (cardId: number, faceUp: boolean) => {
    const cards = new Map(get().cards);
    const card = cards.get(cardId);
    if (card) {
      card.faceUp = faceUp;
      cards.set(cardId, { ...card });
      set({ cards });
    }
  },

  // Rotate card
  rotateCard: (cardId: number, rotation: number) => {
    const cards = new Map(get().cards);
    const card = cards.get(cardId);
    if (card) {
      card.rotation = rotation;
      cards.set(cardId, { ...card });
      set({ cards });
    }
  },

  // Shuffle group
  shuffleGroup: (groupId: number) => {
    console.log('Shuffling group:', groupId);
  },

  // Chat input
  setChatInput: (input: string) => {
    set({ chatInput: input });
  },

  // Chat
  sendChat: (message: string) => {
    const { playerId, playerName } = get();
    if (playerId) {
      get().receiveChat(playerId, playerName, message);
    }
    set({ chatInput: '' });
  },

  receiveChat: (playerId: string, playerName: string, message: string, isSystem?: boolean) => {
    const isSystemMsg = isSystem ?? playerId === 'system';
    const chatMessages = [
      ...get().chatMessages,
      {
        id: Date.now().toString(),
        playerId,
        playerName,
        message,
        timestamp: Date.now(),
        isSystem: isSystemMsg,
      },
    ];
    set({ chatMessages });
  },

  // Card info
  zoomCard: (card: Card | null) => {
    set({ zoomedCard: card });
  },

  hoverCard: (cardId: number | null) => {
    set({ hoveredCard: cardId });
  },
}));

// Type declaration for Electron API
declare global {
  interface Window {
    electronAPI?: {
      startServer: (port: number) => Promise<{ success: boolean; port?: number; error?: string }>;
      stopServer: () => Promise<{ success: boolean }>;
      connectToServer: (host: string, port: number) => Promise<{ success: boolean }>;
      platform: string;
      isMac: boolean;
      isWindows: boolean;
      isLinux: boolean;
    };
  }
}
