export interface Card {
  id: number;
  modelId: string;
  groupId: number;
  x: number;
  y: number;
  width: number;
  height: number;
  faceUp: boolean;
  rotation: number; // 0, 90, 180, 270
  ownerId: string;
  
  // Card properties
  name: string;
  imageUrl?: string;
  backImageUrl?: string;
  properties: Record<string, any>;
  
  // State
  markers: Marker[];
  anchored: boolean;
  targeted: boolean;
  targetedBy?: string;
  highlighted?: string;
  filterColor?: string;
  
  // Alternate card faces
  alternate?: string;
  alternates?: Record<string, CardAlternate>;
  
  // Additional state
  faceUp?: boolean; // Alias for consistency
  selected?: boolean;
}

export interface CardAlternate {
  name: string;
  properties: Record<string, any>;
  imageUrl?: string;
}

export interface Marker {
  id: string;
  name: string;
  count: number;
  icon?: string;
}

export interface Group {
  id: number;
  name: string;
  type: GroupType;
  visibility: Visibility;
  visibleTo: string[];
  cards: Card[];
  controllerId: string;
  
  // Position (for table groups)
  x?: number;
  y?: number;
  width?: number;
  height?: number;
}

export type GroupType = 'hand' | 'pile' | 'table' | 'deck' | 'discard';

export type Visibility = 'nobody' | 'owner' | 'all' | 'defined';

export interface Counter {
  id: string;
  name: string;
  value: number;
  start: number;
  reset: boolean;
  color?: string;
}

export interface Player {
  id: string;
  name: string;
  userId: string;
  publicKey: bigint;
  tableSide: boolean; // true = bottom, false = top
  spectator: boolean;
  ready: boolean;
  color?: string;
  
  // Player-specific groups
  hand: Group;
  deck: Group;
  discard: Group;
  counters: Counter[];
  
  // State
  disconnected: boolean;
  invertedTable: boolean;
}

export interface GameDefinition {
  id: string;
  name: string;
  description: string;
  version: string;
  author: string;
  iconUrl?: string;
  gameUrl?: string;
  
  // Card sizes
  cardSizes: Record<string, CardSize>;
  defaultCardSize: string;
  
  // Card properties
  cardProperties: PropertyDef[];
  
  // Global variables
  globalVariables: GlobalVariable[];
  
  // Phases
  phases: GamePhase[];
  
  // Deck sections
  deckSections: Record<string, DeckSection>;
  
  // Scripts
  scripts: string[];
  
  // Sounds
  sounds: Record<string, string>;
  
  // Game boards
  boards: Record<string, GameBoard>;
}

export interface CardSize {
  name: string;
  width: number;
  height: number;
  cornerRadius: number;
}

export interface PropertyDef {
  name: string;
  type: PropertyType;
  hidden?: boolean;
  ignoreText?: boolean;
}

export type PropertyType = 'String' | 'Integer' | 'Char' | 'Boolean';

export interface GlobalVariable {
  name: string;
  value: string;
  reset: boolean;
}

export interface GamePhase {
  name: string;
  icon?: string;
}

export interface DeckSection {
  name: string;
  group: string;
  shared: boolean;
}

export interface GameBoard {
  name: string;
  imageUrl: string;
  width: number;
  height: number;
  x: number;
  y: number;
}

export interface Deck {
  gameId: string;
  name: string;
  sections: Record<string, DeckCard[]>;
  notes?: string;
  sleeve?: string;
}

export interface DeckCard {
  id: string;
  cardId: string;
  name: string;
  quantity: number;
  properties: Record<string, any>;
}
