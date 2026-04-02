import { EventEmitter } from 'events';

export interface Card {
  id: number;
  modelId: string;
  groupId: number;
  x: number;
  y: number;
  faceUp: boolean;
  rotation: number;
  ownerId: string;
  markers: Map<string, number>;
  properties: Map<string, any>;
  anchored: boolean;
  targetted: boolean;
  targetedBy?: string;
  highlighted?: string;
  filterColor?: string;
}

export interface Group {
  id: number;
  name: string;
  type: 'hand' | 'pile' | 'table' | 'deck';
  visibility: 'nobody' | 'owner' | 'all' | 'defined';
  visibleTo: string[];
  cards: Card[];
  controllerId: string;
}

export interface Counter {
  id: number;
  name: string;
  value: number;
  start: number;
  reset: boolean;
}

export interface TableState {
  groups: Map<number, Group>;
  cards: Map<number, Card>;
  counters: Map<string, Counter>;
  activePlayer?: string;
  turnNumber: number;
  phase: number;
  globalVariables: Map<string, string>;
  tableBackground: string;
  board?: string;
}

export class GameState extends EventEmitter {
  private state: TableState;
  private nextCardId = 1;

  constructor() {
    super();
    this.state = this.createInitialState();
  }

  private createInitialState(): TableState {
    const groups = new Map<number, Group>();

    // Create default table group
    groups.set(0, {
      id: 0,
      name: 'Table',
      type: 'table',
      visibility: 'all',
      visibleTo: [],
      cards: [],
      controllerId: '',
    });

    return {
      groups,
      cards: new Map(),
      counters: new Map(),
      activePlayer: undefined,
      turnNumber: 0,
      phase: 0,
      globalVariables: new Map(),
      tableBackground: '',
    };
  }

  // Card operations
  createCard(modelId: string, groupId: number, size?: string): Card {
    const id = this.nextCardId++;
    const card: Card = {
      id,
      modelId,
      groupId,
      x: 0,
      y: 0,
      faceUp: false,
      rotation: 0,
      ownerId: '',
      markers: new Map(),
      properties: new Map(),
      anchored: false,
      targetted: false,
    };

    this.state.cards.set(id, card);
    const group = this.state.groups.get(groupId);
    if (group) {
      group.cards.push(card);
    }

    this.emit('card-created', card);
    return card;
  }

  moveCard(cardIds: number[], toGroupId: number, toIndex: number[], faceUp: boolean[]): void {
    const toGroup = this.state.groups.get(toGroupId);
    if (!toGroup) return;

    for (let i = 0; i < cardIds.length; i++) {
      const cardId = cardIds[i];
      const card = this.state.cards.get(cardId);
      if (!card) continue;

      // Remove from old group
      const oldGroup = this.state.groups.get(card.groupId);
      if (oldGroup) {
        oldGroup.cards = oldGroup.cards.filter((c) => c.id !== cardId);
      }

      // Update card state
      card.groupId = toGroupId;
      card.faceUp = faceUp[i] ?? card.faceUp;

      // Add to new group
      const idx = Math.min(toIndex[i] ?? toGroup.cards.length, toGroup.cards.length);
      toGroup.cards.splice(idx, 0, card);
    }

    this.emit('cards-moved', { cardIds, toGroupId, toIndex, faceUp });
  }

  turnCard(cardId: number, faceUp: boolean): void {
    const card = this.state.cards.get(cardId);
    if (card) {
      card.faceUp = faceUp;
      this.emit('card-turned', { cardId, faceUp });
    }
  }

  rotateCard(cardId: number, rotation: number): void {
    const card = this.state.cards.get(cardId);
    if (card) {
      card.rotation = rotation;
      this.emit('card-rotated', { cardId, rotation });
    }
  }

  moveCardTo(cardId: number, x: number, y: number, idx?: number): void {
    const card = this.state.cards.get(cardId);
    if (card) {
      card.x = x;
      card.y = y;
      this.emit('card-moved-to', { cardId, x, y, idx });
    }
  }

  deleteCard(cardId: number): void {
    const card = this.state.cards.get(cardId);
    if (card) {
      const group = this.state.groups.get(card.groupId);
      if (group) {
        group.cards = group.cards.filter((c) => c.id !== cardId);
      }
      this.state.cards.delete(cardId);
      this.emit('card-deleted', { cardId });
    }
  }

  // Marker operations
  addMarker(cardId: number, markerId: string, count: number): void {
    const card = this.state.cards.get(cardId);
    if (card) {
      const current = card.markers.get(markerId) || 0;
      card.markers.set(markerId, current + count);
      this.emit('marker-added', { cardId, markerId, count });
    }
  }

  removeMarker(cardId: number, markerId: string, count: number): void {
    const card = this.state.cards.get(cardId);
    if (card) {
      const current = card.markers.get(markerId) || 0;
      const newCount = Math.max(0, current - count);
      if (newCount === 0) {
        card.markers.delete(markerId);
      } else {
        card.markers.set(markerId, newCount);
      }
      this.emit('marker-removed', { cardId, markerId, count });
    }
  }

  // Group operations
  createGroup(group: Omit<Group, 'id'>): number {
    const id = this.state.groups.size;
    this.state.groups.set(id, { ...group, id });
    this.emit('group-created', { id, group });
    return id;
  }

  getGroupCards(groupId: number): Card[] {
    const group = this.state.groups.get(groupId);
    return group ? group.cards : [];
  }

  shuffleGroup(groupId: number): number[] {
    const group = this.state.groups.get(groupId);
    if (!group) return [];

    // Fisher-Yates shuffle
    for (let i = group.cards.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [group.cards[i], group.cards[j]] = [group.cards[j], group.cards[i]];
    }

    const positions = group.cards.map((c, i) => i);
    this.emit('group-shuffled', { groupId, positions });
    return positions;
  }

  // Counter operations
  setCounter(name: string, value: number): void {
    this.state.counters.set(name, {
      id: this.state.counters.size,
      name,
      value,
      start: value,
      reset: false,
    });
    this.emit('counter-changed', { name, value });
  }

  incrementCounter(name: string, delta: number): void {
    const counter = this.state.counters.get(name);
    if (counter) {
      counter.value += delta;
      this.emit('counter-changed', { name, value: counter.value });
    }
  }

  // Turn management
  setActivePlayer(playerId: string): void {
    this.state.activePlayer = playerId;
    this.emit('active-player-changed', { playerId });
  }

  clearActivePlayer(): void {
    this.state.activePlayer = undefined;
    this.emit('active-player-cleared', {});
  }

  nextTurn(playerId: string, force: boolean): void {
    this.state.turnNumber++;
    this.state.activePlayer = playerId;
    this.emit('turn-changed', { turnNumber: this.state.turnNumber, activePlayer: playerId });
  }

  setPhase(phase: number): void {
    this.state.phase = phase;
    this.emit('phase-changed', { phase });
  }

  // Global variables
  setGlobalVariable(name: string, value: string): void {
    const oldValue = this.state.globalVariables.get(name);
    this.state.globalVariables.set(name, value);
    this.emit('global-variable-changed', { name, oldValue, value });
  }

  // Serialization for game state sync
  serialize(): string {
    return JSON.stringify({
      groups: Array.from(this.state.groups.entries()).map(([id, group]) => ({
        ...group,
        cards: group.cards.map((c) => ({
          ...c,
          markers: Array.from(c.markers.entries()),
          properties: Array.from(c.properties.entries()),
        })),
      })),
      counters: Array.from(this.state.counters.entries()),
      activePlayer: this.state.activePlayer,
      turnNumber: this.state.turnNumber,
      phase: this.state.phase,
      globalVariables: Array.from(this.state.globalVariables.entries()),
    });
  }

  deserialize(data: string): void {
    const parsed = JSON.parse(data);
    // Reconstruct state from serialized data
    // This is used for game state sync between players
    this.emit('state-loaded', {});
  }

  // Getters
  get cards(): Map<number, Card> {
    return this.state.cards;
  }

  get groups(): Map<number, Group> {
    return this.state.groups;
  }

  get counters(): Map<string, Counter> {
    return this.state.counters;
  }

  get activePlayer(): string | undefined {
    return this.state.activePlayer;
  }

  get turnNumber(): number {
    return this.state.turnNumber;
  }
}
