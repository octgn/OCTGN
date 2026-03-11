import { Card, Group, Player, Counter } from '../types/game';

/**
 * Serializable game state for save/load
 */
export interface SerializedGameState {
  version: string;
  gameId: string;
  gameName: string;
  savedAt: string;
  
  players: SerializedPlayer[];
  groups: SerializedGroup[];
  cards: SerializedCard[];
  counters: SerializedCounter[];
  
  turnNumber: number;
  activePlayerId: string | null;
  currentPhase: number;
  
  globalVariables: Record<string, string>;
}

export interface SerializedPlayer {
  id: string;
  name: string;
  userId: string;
  tableSide: boolean;
  spectator: boolean;
  ready: boolean;
  invertedTable: boolean;
  color?: string;
}

export interface SerializedGroup {
  id: number;
  name: string;
  type: string;
  visibility: string;
  visibleTo: string[];
  controllerId: string;
  cardIds: number[];
}

export interface SerializedCard {
  id: number;
  modelId: string;
  groupId: number;
  x: number;
  y: number;
  faceUp: boolean;
  rotation: number;
  ownerId: string;
  name: string;
  properties: Record<string, any>;
  markers: { id: string; name: string; count: number }[];
  anchored: boolean;
  targeted: boolean;
  highlighted?: string;
}

export interface SerializedCounter {
  id: string;
  name: string;
  value: number;
  start: number;
  reset: boolean;
  color?: string;
}

/**
 * Game state serializer
 */
export class GameStateSerializer {
  private static VERSION = '1.0.0';

  /**
   * Serialize game state to JSON
   */
  static serialize(state: {
    gameId: string;
    gameName: string;
    players: Player[];
    groups: Map<number, Group>;
    cards: Map<number, Card>;
    counters: Map<string, Counter>;
    turnNumber: number;
    activePlayerId: string | null;
    currentPhase: number;
    globalVariables: Map<string, string>;
  }): string {
    const serialized: SerializedGameState = {
      version: this.VERSION,
      gameId: state.gameId,
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
      
      cards: Array.from(state.cards.values()).map((c) => ({
        id: c.id,
        modelId: c.modelId,
        groupId: c.groupId,
        x: c.x,
        y: c.y,
        faceUp: c.faceUp,
        rotation: c.rotation,
        ownerId: c.ownerId,
        name: c.name,
        properties: c.properties,
        markers: c.markers,
        anchored: c.anchored,
        targeted: c.targeted,
        highlighted: c.highlighted,
      })),
      
      counters: Array.from(state.counters.values()).map((c) => ({
        id: c.id,
        name: c.name,
        value: c.value,
        start: c.start,
        reset: c.reset,
        color: c.color,
      })),
      
      turnNumber: state.turnNumber,
      activePlayerId: state.activePlayerId,
      currentPhase: state.currentPhase,
      
      globalVariables: Object.fromEntries(state.globalVariables),
    };

    return JSON.stringify(serialized, null, 2);
  }

  /**
   * Deserialize game state from JSON
   */
  static deserialize(json: string): SerializedGameState {
    const parsed = JSON.parse(json) as SerializedGameState;

    // Validate version
    if (!parsed.version) {
      throw new Error('Invalid save file: missing version');
    }

    // Could add migration logic here for older versions

    return parsed;
  }

  /**
   * Export to file (Electron only)
   */
  static async saveToFile(data: string, defaultName?: string): Promise<boolean> {
    if (!window.electronAPI?.saveFileDialog) {
      // Browser fallback - download
      const blob = new Blob([data], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = defaultName || `octgn-save-${Date.now()}.json`;
      a.click();
      URL.revokeObjectURL(url);
      return true;
    }

    const result = await window.electronAPI.saveFileDialog({
      defaultPath: defaultName || `octgn-save-${Date.now()}.json`,
      filters: [
        { name: 'OCTGN Save', extensions: ['o8s', 'json'] },
        { name: 'All Files', extensions: ['*'] },
      ],
    });

    if (result.canceled || !result.filePath) {
      return false;
    }

    const writeResult = await window.electronAPI.writeFile(result.filePath, data);
    return writeResult.success;
  }

  /**
   * Import from file (Electron only)
   */
  static async loadFromFile(): Promise<string | null> {
    if (!window.electronAPI?.openFileDialog) {
      // Browser fallback - file input
      return new Promise((resolve) => {
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = '.o8s,.json';
        input.onchange = async (e) => {
          const file = (e.target as HTMLInputElement).files?.[0];
          if (file) {
            const text = await file.text();
            resolve(text);
          } else {
            resolve(null);
          }
        };
        input.click();
      });
    }

    const result = await window.electronAPI.openFileDialog({
      filters: [
        { name: 'OCTGN Save', extensions: ['o8s', 'json'] },
        { name: 'All Files', extensions: ['*'] },
      ],
      properties: ['openFile'],
    });

    if (result.canceled || !result.filePaths.length) {
      return null;
    }

    const readResult = await window.electronAPI.readFile(result.filePaths[0]);
    return readResult.success ? readResult.data || null : null;
  }
}
