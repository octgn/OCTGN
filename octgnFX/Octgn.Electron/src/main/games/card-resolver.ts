import { readFile as fsReadFile, readdir as fsReaddir } from 'fs/promises';
import { join } from 'path';
import { parseSetXml, type CardDefinition } from './set-parser';
import { parseDefinitionXml } from './definition-parser';
import { findGameDir as fsFindGameDir } from './game-store';
import type { GameDefinition } from '../../shared/types';
import { log } from '../logger';

export interface CardResolverIO {
  readFile: (path: string, encoding: string) => Promise<string>;
  readdir: (path: string) => Promise<string[]>;
  findGameDir: (gameId: string) => Promise<string | null>;
}

const defaultIO: CardResolverIO = {
  readFile: (path, encoding) => fsReadFile(path, encoding as BufferEncoding) as Promise<string>,
  readdir: (path) => fsReaddir(path) as Promise<string[]>,
  findGameDir: fsFindGameDir,
};

/**
 * Lazy-loading resolver that maps card definition GUIDs to their
 * CardDefinition (name, properties) by scanning set.xml files
 * from locally installed game definitions.
 */
export class CardResolver {
  private cache = new Map<string, CardDefinition>();
  private loadedGames = new Set<string>();
  private gameDefinitions = new Map<string, GameDefinition>();
  private io: CardResolverIO;

  constructor(io?: Partial<CardResolverIO>) {
    this.io = { ...defaultIO, ...io };
  }

  /**
   * Load all card definitions for a game by scanning its set.xml files.
   * No-ops if the game was already loaded.
   */
  async loadGame(gameId: string): Promise<void> {
    if (this.loadedGames.has(gameId)) return;
    this.loadedGames.add(gameId);

    const gameDir = await this.io.findGameDir(gameId);
    if (!gameDir) {
      log('RESOLVER', `Game ${gameId} not found locally — card names will be generic`);
      return;
    }

    // Load definition.xml for group names, game metadata
    try {
      const defXml = await this.io.readFile(join(gameDir, 'definition.xml'), 'utf-8');
      const def = parseDefinitionXml(defXml);
      if (def) {
        this.gameDefinitions.set(gameId, def);
        log('RESOLVER', `Loaded game definition: ${def.name} (${def.players.length} player defs, ${def.players[0]?.groups.length ?? 0} groups)`);
      }
    } catch {
      log('RESOLVER', `No definition.xml found for game ${gameId}`);
    }

    // Scan Sets/*/set.xml
    const setsDir = join(gameDir, 'Sets');
    let setDirs: string[];
    try {
      setDirs = await this.io.readdir(setsDir);
    } catch {
      log('RESOLVER', `No Sets directory for game ${gameId}`);
      return;
    }

    let cardCount = 0;
    for (const setDir of setDirs) {
      const setXmlPath = join(setsDir, setDir, 'set.xml');
      try {
        const xml = await this.io.readFile(setXmlPath, 'utf-8');
        const cards = parseSetXml(xml);
        for (const card of cards) {
          this.cache.set(card.id, card);
          cardCount++;
        }
      } catch {
        // set.xml doesn't exist or isn't parseable — skip
      }
    }

    log('RESOLVER', `Loaded ${cardCount} card definitions from ${setDirs.length} sets for game ${gameId}`);
  }

  /**
   * Look up a card definition by its GUID.
   */
  resolve(cardGuid: string): CardDefinition | undefined {
    return this.cache.get(cardGuid);
  }

  /**
   * Get the parsed GameDefinition for a loaded game.
   */
  getGameDefinition(gameId: string): GameDefinition | undefined {
    return this.gameDefinitions.get(gameId);
  }

  /**
   * Resolve a group name from the game definition.
   * Group IDs in the protocol encode player index in high bytes and group index in low byte.
   * The group index (0-based) maps to the groups array in the player definition.
   */
  resolveGroupName(gameId: string, groupId: number): string | undefined {
    const def = this.gameDefinitions.get(gameId);
    if (!def || def.players.length === 0) return undefined;

    // Group index is in the low byte (1-based in protocol, maps to 0-based array index)
    const groupIndex = (groupId & 0xFF) - 1;
    const playerDef = def.players[0];
    if (groupIndex >= 0 && groupIndex < playerDef.groups.length) {
      return playerDef.groups[groupIndex].name;
    }
    return undefined;
  }
}
