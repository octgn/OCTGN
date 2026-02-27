// OCTGN Shared Types - used by both main and renderer processes

export interface User {
  id: string;
  username: string;
  iconUrl?: string;
  isSubscriber: boolean;
}

export interface Session {
  userId: string;
  sessionId: string;
  deviceId: string;
}

export interface LoginResult {
  success: boolean;
  user?: User;
  session?: Session;
  error?: string;
}

export interface HostedGame {
  id: string;
  name: string;
  hostUser: User;
  gameId: string;
  gameName: string;
  gameVersion: string;
  hasPassword: boolean;
  spectators: boolean;
  hostAddress: string;
  port: number;
  status: GameStatus;
  dateCreated: string;
  playerCount: number;
  maxPlayers: number;
}

export enum GameStatus {
  Unknown = 0,
  GameReady = 1,
  InProgress = 2,
  Finished = 3,
}

export interface GameDefinition {
  id: string;
  name: string;
  version: string;
  description: string;
  iconUrl?: string;
  cardWidth: number;
  cardHeight: number;
  cardBack: string;
  deckSections: string[];
  sharedDeckSections: string[];
  players: PlayerDefinition[];
  globalVariables: Variable[];
  phases: GamePhase[];
}

export interface PlayerDefinition {
  name: string;
  groups: GroupDefinition[];
  counters: CounterDefinition[];
  globalVariables: Variable[];
}

export interface GroupDefinition {
  name: string;
  icon?: string;
  visibility: GroupVisibility;
  ordered: boolean;
  shortcut?: string;
  cardActions: CardAction[];
  groupActions: GroupAction[];
}

export enum GroupVisibility {
  Undefined = 0,
  Nobody = 1,
  Owner = 2,
  Everybody = 3,
}

export interface CounterDefinition {
  name: string;
  icon?: string;
  defaultValue: number;
}

export interface Variable {
  name: string;
  defaultValue: string;
  global: boolean;
}

export interface GamePhase {
  name: string;
  icon?: string;
}

export interface CardAction {
  name: string;
  shortcut?: string;
  execute: string;
  batchExecute?: string;
}

export interface GroupAction {
  name: string;
  shortcut?: string;
  execute: string;
}

// Game state types
export interface Card {
  id: string;
  definitionId: string;
  name: string;
  imageUrl: string;
  faceUp: boolean;
  position: { x: number; y: number };
  rotation: number;
  groupId: string;
  ownerId: string;
  markers: Marker[];
  properties: Record<string, string>;
  peekingPlayers: string[];
  highlighted?: string; // color
  targetedBy?: string;
  size: { width: number; height: number };
}

export interface Marker {
  id: string;
  name: string;
  iconUrl: string;
  count: number;
}

export interface Player {
  id: number;
  name: string;
  color: string;
  isHost: boolean;
  isSpectator: boolean;
  groups: Group[];
  counters: Counter[];
  globalVariables: Record<string, string>;
}

export interface Group {
  id: string;
  name: string;
  cards: Card[];
  visibility: GroupVisibility;
  controller: number;
}

export interface Counter {
  id: number;
  name: string;
  value: number;
}

export interface GameState {
  gameId: string;
  gameName: string;
  players: Player[];
  localPlayerId: number;
  table: {
    cards: Card[];
    board?: {
      name: string;
      imageUrl: string;
      width: number;
      height: number;
    };
  };
  turnNumber: number;
  activePlayer: number;
  phase: number;
  chatMessages: ChatMessage[];
  isStarted: boolean;
}

export interface ChatMessage {
  id: string;
  playerId: number;
  playerName: string;
  message: string;
  timestamp: number;
  isSystem: boolean;
  color?: string;
}

export interface Deck {
  gameId: string;
  sections: DeckSection[];
  notes?: string;
  sleeveUrl?: string;
}

export interface DeckSection {
  name: string;
  cards: DeckCard[];
}

export interface DeckCard {
  id: string;
  name: string;
  quantity: number;
  properties: Record<string, string>;
}

// IPC Channel names
export const IPC_CHANNELS = {
  // Auth
  LOGIN: 'auth:login',
  LOGOUT: 'auth:logout',
  GET_SESSION: 'auth:get-session',

  // Lobby
  GET_GAMES: 'lobby:get-games',
  HOST_GAME: 'lobby:host-game',
  JOIN_GAME: 'lobby:join-game',
  LEAVE_GAME: 'lobby:leave-game',

  // Game
  GAME_STATE_UPDATE: 'game:state-update',
  GAME_ACTION: 'game:action',
  GAME_CHAT: 'game:chat',
  LOAD_DECK: 'game:load-deck',

  // App
  APP_VERSION: 'app:version',
  APP_QUIT: 'app:quit',
  APP_MINIMIZE: 'app:minimize',
  APP_MAXIMIZE: 'app:maximize',
} as const;
