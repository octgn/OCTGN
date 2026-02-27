import { create } from 'zustand';
import { Card, Group, Counter, Player } from '../types/game';
import { GameStateSerializer, SerializedGameState } from '../utils';

interface GameState {
  // Connection state
  connected: boolean;
  connecting: boolean;
  serverAddress: string;
  serverPort: number;
  isHost: boolean;

  // Player info
  playerId: string | null;
  playerName: string;
  playerCount: number;

  // Game identity
  gameId: string | null;
  gameName: string;
  gameDefinition: any | null;

  // Game state
  cards: Map<number, Card>;
  groups: Map<number, Group>;
  counters: Map<string, Counter>;
  players: Player[];
  activePlayerId: string | null;
  turnNumber: number;
  phase: number;
  phases: { name: string; icon?: string }[];

  // UI state
  selectedCards: number[];
  hoveredCard: number | null;
  zoomedCard: Card | null;
  showChat: boolean;
  showHand: boolean;
  chatMessages: ChatMessage[];
  chatInput: string;

  // View state
  panOffset: { x: number; y: number };
  zoom: number;

  // Actions
  initialize: () => void;
  
  // Connection
  setConnected: (connected: boolean) => void;
  setConnecting: (connecting: boolean) => void;
  setPlayerId: (id: string | null) => void;
  setPlayerName: (name: string) => void;
  setIsHost: (isHost: boolean) => void;
  
  // Game state
  setGameId: (id: string | null) => void;
  setGameName: (name: string) => void;
  setGameDefinition: (def: any) => void;
  
  // Cards
  setCards: (cards: Map<number, Card>) => void;
  addCard: (card: Card) => void;
  updateCard: (id: number, updates: Partial<Card>) => void;
  removeCard: (id: number) => void;
  
  // Groups
  setGroups: (groups: Map<number, Group>) => void;
  addGroup: (group: Group) => void;
  
  // Players
  setPlayers: (players: Player[]) => void;
  addPlayer: (player: Player) => void;
  removePlayer: (playerId: string) => void;
  updatePlayer: (id: string, updates: Partial<Player>) => void;
  
  // Turn management
  setActivePlayerId: (id: string | null) => void;
  nextTurn: () => void;
  setPhase: (phase: number) => void;
  
  // Counters
  setCounter: (name: string, value: number) => void;
  
  // Selection
  selectCards: (cardIds: number[]) => void;
  clearSelection: () => void;
  setHoveredCard: (id: number | null) => void;
  setZoomedCard: (card: Card | null) => void;
  
  // UI
  toggleChat: () => void;
  toggleHand: () => void;
  addChatMessage: (message: ChatMessage) => void;
  setChatInput: (input: string) => void;
  
  // View
  setPanOffset: (offset: { x: number; y: number }) => void;
  setZoom: (zoom: number) => void;
  resetView: () => void;
  
  // Serialization
  saveGame: () => Promise<void>;
  loadGame: () => Promise<void>;
  importState: (state: SerializedGameState) => void;
  exportState: () => SerializedGameState;
  
  // Reset
  resetGame: () => void;
}

interface ChatMessage {
  id: string;
  playerId: string;
  playerName: string;
  message: string;
  timestamp: number;
  isSystem?: boolean;
}

const DEFAULT_ZOOM = 1;
const DEFAULT_PAN = { x: 0, y: 0 };

export const useGameStore = create<GameState>((set, get) => ({
  // Initial state
  connected: false,
  connecting: false,
  serverAddress: 'localhost',
  serverPort: 8888,
  isHost: false,

  playerId: null,
  playerName: 'Player',
  playerCount: 0,

  gameId: null,
  gameName: '',
  gameDefinition: null,

  cards: new Map(),
  groups: new Map(),
  counters: new Map(),
  players: [],
  activePlayerId: null,
  turnNumber: 0,
  phase: 0,
  phases: [],

  selectedCards: [],
  hoveredCard: null,
  zoomedCard: null,
  showChat: true,
  showHand: true,
  chatMessages: [],
  chatInput: '',

  panOffset: DEFAULT_PAN,
  zoom: DEFAULT_ZOOM,

  // Initialize
  initialize: () => {
    const savedName = localStorage.getItem('playerName');
    if (savedName) {
      set({ playerName: savedName });
    }
  },

  // Connection
  setConnected: (connected) => set({ connected }),
  setConnecting: (connecting) => set({ connecting }),
  setPlayerId: (id) => set({ playerId: id }),
  setPlayerName: (name) => {
    localStorage.setItem('playerName', name);
    set({ playerName: name });
  },
  setIsHost: (isHost) => set({ isHost }),

  // Game state
  setGameId: (id) => set({ gameId: id }),
  setGameName: (name) => set({ gameName: name }),
  setGameDefinition: (def) => set({ gameDefinition: def }),

  // Cards
  setCards: (cards) => set({ cards }),
  addCard: (card) => set((state) => {
    const cards = new Map(state.cards);
    cards.set(card.id, card);
    return { cards };
  }),
  updateCard: (id, updates) => set((state) => {
    const cards = new Map(state.cards);
    const card = cards.get(id);
    if (card) {
      cards.set(id, { ...card, ...updates });
      return { cards };
    }
    return state;
  }),
  removeCard: (id) => set((state) => {
    const cards = new Map(state.cards);
    cards.delete(id);
    return { cards };
  }),

  // Groups
  setGroups: (groups) => set({ groups }),
  addGroup: (group) => set((state) => {
    const groups = new Map(state.groups);
    groups.set(group.id, group);
    return { groups };
  }),

  // Players
  setPlayers: (players) => set({ players, playerCount: players.length }),
  addPlayer: (player) => set((state) => ({
    players: [...state.players, player],
    playerCount: state.players.length + 1,
  })),
  removePlayer: (playerId) => set((state) => ({
    players: state.players.filter((p) => p.id !== playerId),
    playerCount: state.players.length - 1,
  })),
  updatePlayer: (id, updates) => set((state) => ({
    players: state.players.map((p) =>
      p.id === id ? { ...p, ...updates } : p
    ),
  })),

  // Turn management
  setActivePlayerId: (id) => set({ activePlayerId: id }),
  nextTurn: () => set((state) => ({ turnNumber: state.turnNumber + 1 })),
  setPhase: (phase) => set({ phase }),

  // Counters
  setCounter: (name, value) => set((state) => {
    const counters = new Map(state.counters);
    counters.set(name, counters.get(name) || { id: name, name, value, start: 0, reset: false });
    const counter = counters.get(name)!;
    counters.set(name, { ...counter, value });
    return { counters };
  }),

  // Selection
  selectCards: (cardIds) => set({ selectedCards: cardIds }),
  clearSelection: () => set({ selectedCards: [] }),
  setHoveredCard: (id) => set({ hoveredCard: id }),
  setZoomedCard: (card) => set({ zoomedCard: card }),

  // UI
  toggleChat: () => set((state) => ({ showChat: !state.showChat })),
  toggleHand: () => set((state) => ({ showHand: !state.showHand })),
  addChatMessage: (message) => set((state) => ({
    chatMessages: [...state.chatMessages, message],
  })),
  setChatInput: (input) => set({ chatInput: input }),

  // View
  setPanOffset: (offset) => set({ panOffset: offset }),
  setZoom: (zoom) => set({ zoom: Math.max(0.25, Math.min(3, zoom)) }),
  resetView: () => set({ panOffset: DEFAULT_PAN, zoom: DEFAULT_ZOOM }),

  // Serialization
  saveGame: async () => {
    const state = get();
    const serialized = GameStateSerializer.serialize({
      gameId: state.gameId || '',
      gameName: state.gameName,
      players: state.players,
      groups: state.groups,
      cards: state.cards,
      counters: state.counters,
      turnNumber: state.turnNumber,
      activePlayerId: state.activePlayerId,
      currentPhase: state.phase,
      globalVariables: new Map(),
    });
    await GameStateSerializer.saveToFile(serialized, `octgn-save-${Date.now()}.json`);
  },

  loadGame: async () => {
    const json = await GameStateSerializer.loadFromFile();
    if (json) {
      const state = GameStateSerializer.deserialize(json);
      get().importState(state);
    }
  },

  importState: (state) => {
    // Reconstruct maps from arrays
    const cards = new Map(state.cards.map((c) => [c.id, c as Card]));
    const groups = new Map<number, Group>();
    
    state.groups.forEach((g) => {
      groups.set(g.id, {
        ...g,
        cards: g.cardIds.map((id) => cards.get(id)!).filter(Boolean),
      } as Group);
    });

    const counters = new Map(state.counters.map((c) => [c.id, c as Counter]));

    set({
      gameId: state.gameId,
      gameName: state.gameName,
      cards,
      groups,
      counters,
      players: state.players as Player[],
      turnNumber: state.turnNumber,
      activePlayerId: state.activePlayerId,
      phase: state.currentPhase,
    });
  },

  exportState: () => {
    const state = get();
    return {
      version: '1.0.0',
      gameId: state.gameId || '',
      gameName: state.gameName,
      savedAt: new Date().toISOString(),
      players: state.players.map((p) => ({
        id: p.id,
        name: p.name,
        userId: p.userId,
        tableSide: p.tableSide,
        spectator: p.spectator,
        ready: p.ready,
        invertedTable: p.invertedTable,
        color: p.color,
      })),
      groups: Array.from(state.groups.values()).map((g) => ({
        id: g.id,
        name: g.name,
        type: g.type,
        visibility: g.visibility,
        visibleTo: g.visibleTo,
        controllerId: g.controllerId,
        cardIds: g.cards.map((c) => c.id),
      })),
      cards: Array.from(state.cards.values()),
      counters: Array.from(state.counters.values()),
      turnNumber: state.turnNumber,
      activePlayerId: state.activePlayerId,
      currentPhase: state.phase,
      globalVariables: {},
    };
  },

  // Reset
  resetGame: () => set({
    cards: new Map(),
    groups: new Map(),
    counters: new Map(),
    players: [],
    activePlayerId: null,
    turnNumber: 0,
    phase: 0,
    selectedCards: [],
    hoveredCard: null,
    zoomedCard: null,
    chatMessages: [],
    panOffset: DEFAULT_PAN,
    zoom: DEFAULT_ZOOM,
  }),
}));
