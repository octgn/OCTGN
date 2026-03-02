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
  gameIconUrl?: string;
}

export enum GameStatus {
  Unknown = 0,
  GameReady = 1,
  InProgress = 2,
  Finished = 3,
}

export interface TableDefinition {
  name: string;
  width: number;
  height: number;
  background?: string;
  backgroundStyle?: 'stretch' | 'tile' | 'uniform' | 'uniformToFill';
  board?: BoardDefinition;
}

export interface BoardDefinition {
  name: string;
  source: string;
  x: number;
  y: number;
  width: number;
  height: number;
}

export interface CardSizeDefinition {
  name: string;
  width: number;
  height: number;
  cornerRadius: number;
  back: string;
  front: string;
  backWidth: number;
  backHeight: number;
  backCornerRadius: number;
}

export interface DeckSectionDef {
  name: string;
  group: string;
  shared: boolean;
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
  deckSections: DeckSectionDef[];
  sharedDeckSections: DeckSectionDef[];
  players: PlayerDefinition[];
  globalVariables: Variable[];
  phases: GamePhase[];
  table?: TableDefinition;
  boards?: BoardDefinition[];
  cardSizes?: Record<string, CardSizeDefinition>;
  defaultCardSize?: CardSizeDefinition;
  globalPlayer?: { groups: GroupDefinition[] };
  scriptVersion?: string;
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
  showIf?: string;
  getName?: string;
  isDefault?: boolean;
}

export interface GroupAction {
  name: string;
  shortcut?: string;
  execute: string;
  showIf?: string;
  getName?: string;
  isDefault?: boolean;
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
  invertedTable?: boolean;
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
  gameDefinition?: { name: string; id: string };
  players: Player[];
  localPlayerId: number;
  isSpectator: boolean;
  table: {
    cards: Card[];
    board?: {
      name: string;
      imageUrl: string;
      width: number;
      height: number;
      x?: number;
      y?: number;
    };
    width?: number;
    height?: number;
    backgroundStyle?: 'stretch' | 'tile' | 'uniform' | 'uniformToFill';
    /** Resolved URL for the table background image (e.g. wood texture) */
    backgroundUrl?: string;
  };
  turnNumber: number;
  activePlayer: number;
  phase: number;
  chatMessages: ChatMessage[];
  isStarted: boolean;
  useTwoSidedTable?: boolean;
  allowSpectators?: boolean;
  muteSpectators?: boolean;
  allowCardList?: boolean;
  connectionStatus?: 'connected' | 'disconnected' | 'reconnecting';
  globalVariables?: Record<string, string>;
  /** Default card size from game definition (in mm) */
  cardSize?: { width: number; height: number };
  /** Named card sizes from game definition (in mm) */
  cardSizes?: Record<string, { width: number; height: number }>;
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
  shared?: boolean;
}

export interface DeckCard {
  id: string;
  name: string;
  quantity: number;
  properties: Record<string, string>;
}

export interface GameFeed {
  name: string;
  url: string;
  enabled: boolean;
  isBuiltIn: boolean;
  username?: string;
  password?: string;
}

export interface AvailableGame {
  id: string;
  name: string;
  version: string;
  description: string;
  authors: string;
  tags: string;
  downloadUrl: string;
  iconUrl?: string;
  downloadCount?: number;
}

export interface InstallProgress {
  gameId: string;
  phase: 'downloading' | 'extracting' | 'parsing' | 'done' | 'error';
  percent: number;
  error?: string;
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
  DECK_PATHS: 'game:deck-paths',
  GAME_SETTINGS: 'game:settings',
  GAME_PLAYER_SETTINGS: 'game:player-settings',
  GAME_BOOT_PLAYER: 'game:boot-player',
  GAME_START: 'game:start',

  // App
  GET_APP_STATE: 'app:get-state',
  APP_VERSION: 'app:version',
  APP_QUIT: 'app:quit',
  APP_MINIMIZE: 'app:minimize',
  APP_MAXIMIZE: 'app:maximize',
  OPEN_FILE_DIALOG: 'app:open-file-dialog',

  // Scripting
  SCRIPT_EVENT: 'script:event',
  SCRIPT_EXECUTE: 'script:execute',
  SCRIPT_DIALOG_REQUEST: 'script:dialog-request',
  SCRIPT_DIALOG_RESPONSE: 'script:dialog-response',

  // Game definitions
  GAMES_LIST_INSTALLED: 'games:list-installed',
  GAMES_LIST_AVAILABLE: 'games:list-available',
  GAMES_INSTALL: 'games:install',
  GAMES_UNINSTALL: 'games:uninstall',
  GAMES_INSTALL_PROGRESS: 'games:install-progress',

  // Feeds
  FEEDS_LIST: 'feeds:list',
  FEEDS_ADD: 'feeds:add',
  FEEDS_REMOVE: 'feeds:remove',
  FEEDS_SET_ENABLED: 'feeds:set-enabled',

  // Credentials
  CREDS_LOAD: 'creds:load',
  CREDS_SAVE: 'creds:save',
  CREDS_CLEAR: 'creds:clear',
  CLIPBOARD_WRITE: 'clipboard:write',

  // Game assets
  GAME_RESOLVE_ASSET: 'game:resolve-asset',
} as const;
