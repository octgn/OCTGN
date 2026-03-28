/**
 * Game Package Loader
 * 
 * Handles loading and parsing OCTGN game definition files (.o8g)
 * 
 * Game packages are ZIP files containing:
 * - set.xml: Game definition
 * - Cards/: Card images
 * - Scripts/: Python scripts
 * - Sounds/: Sound files
 * - Proxies/: Proxy templates
 */

import { GameDefinition, CardSize, PropertyDef, GamePhase, DeckSection, GameBoard } from '../types/game';

export interface GamePackage {
  id: string;
  name: string;
  version: string;
  definition: GameDefinition;
  installPath: string;
  cardDatabase: CardDatabase;
}

export interface CardDatabase {
  cards: Map<string, CardInfo>;
  sets: Map<string, SetInfo>;
}

export interface CardInfo {
  id: string;
  name: string;
  setId: string;
  properties: Record<string, string>;
  imageUrl?: string;
  alternates?: Record<string, CardInfo>;
}

export interface SetInfo {
  id: string;
  name: string;
  gameId: string;
  cards: CardInfo[];
}

/**
 * Parse a game definition XML
 */
export function parseGameDefinition(xml: string): GameDefinition {
  const parser = new DOMParser();
  const doc = parser.parseFromString(xml, 'text/xml');

  const gameEl = doc.querySelector('game');
  if (!gameEl) {
    throw new Error('Invalid game definition: missing game element');
  }

  const definition: GameDefinition = {
    id: gameEl.getAttribute('id') || '',
    name: gameEl.getAttribute('name') || '',
    description: gameEl.querySelector('description')?.textContent || '',
    version: gameEl.getAttribute('version') || '1.0.0',
    author: gameEl.getAttribute('author') || '',
    iconUrl: gameEl.getAttribute('iconurl') || undefined,
    gameUrl: gameEl.getAttribute('gameurl') || undefined,
    cardSizes: {},
    defaultCardSize: '',
    cardProperties: [],
    globalVariables: [],
    phases: [],
    deckSections: {},
    scripts: [],
    sounds: {},
    boards: {},
  };

  // Parse card sizes
  gameEl.querySelectorAll('cardsizes > cardsize').forEach((sizeEl) => {
    const size: CardSize = {
      name: sizeEl.getAttribute('name') || '',
      width: parseInt(sizeEl.getAttribute('width') || '250', 10),
      height: parseInt(sizeEl.getAttribute('height') || '350', 10),
      cornerRadius: parseInt(sizeEl.getAttribute('cornerradius') || '10', 10),
    };
    definition.cardSizes[size.name] = size;
    if (!definition.defaultCardSize) {
      definition.defaultCardSize = size.name;
    }
  });

  // Parse card properties
  gameEl.querySelectorAll('card > property').forEach((propEl) => {
    const prop: PropertyDef = {
      name: propEl.getAttribute('name') || '',
      type: (propEl.getAttribute('type') as any) || 'String',
      hidden: propEl.getAttribute('hidden') === 'True',
      ignoreText: propEl.getAttribute('ignoretext') === 'True',
    };
    definition.cardProperties.push(prop);
  });

  // Parse global variables
  gameEl.querySelectorAll('globalvariables > variable').forEach((varEl) => {
    definition.globalVariables.push({
      name: varEl.getAttribute('name') || '',
      value: varEl.getAttribute('value') || '',
      reset: varEl.getAttribute('reset') === 'True',
    });
  });

  // Parse phases
  gameEl.querySelectorAll('phases > phase').forEach((phaseEl) => {
    definition.phases.push({
      name: phaseEl.getAttribute('name') || '',
      icon: phaseEl.getAttribute('icon') || undefined,
    });
  });

  // Parse deck sections
  gameEl.querySelectorAll('decksections > decksection').forEach((sectionEl) => {
    const section: DeckSection = {
      name: sectionEl.getAttribute('name') || '',
      group: sectionEl.getAttribute('group') || '',
      shared: sectionEl.getAttribute('shared') === 'True',
    };
    definition.deckSections[section.name] = section;
  });

  // Parse scripts
  gameEl.querySelectorAll('scripts > script').forEach((scriptEl) => {
    definition.scripts.push(scriptEl.textContent || '');
  });

  // Parse sounds
  gameEl.querySelectorAll('sounds > sound').forEach((soundEl) => {
    const name = soundEl.getAttribute('name') || '';
    const src = soundEl.getAttribute('src') || '';
    definition.sounds[name] = src;
  });

  // Parse game boards
  gameEl.querySelectorAll('gameboards > gameboard').forEach((boardEl) => {
    const name = boardEl.getAttribute('name') || '';
    definition.boards[name] = {
      name,
      imageUrl: boardEl.getAttribute('src') || '',
      width: parseInt(boardEl.getAttribute('width') || '1000', 10),
      height: parseInt(boardEl.getAttribute('height') || '700', 10),
      x: parseInt(boardEl.getAttribute('x') || '0', 10),
      y: parseInt(boardEl.getAttribute('y') || '0', 10),
    };
  });

  return definition;
}

/**
 * Parse a card set XML
 */
export function parseCardSet(xml: string): SetInfo {
  const parser = new DOMParser();
  const doc = parser.parseFromString(xml, 'text/xml');

  const setEl = doc.querySelector('set');
  if (!setEl) {
    throw new Error('Invalid card set: missing set element');
  }

  const set: SetInfo = {
    id: setEl.getAttribute('id') || '',
    name: setEl.getAttribute('name') || '',
    gameId: setEl.getAttribute('gameId') || '',
    cards: [],
  };

  setEl.querySelectorAll('cards > card').forEach((cardEl) => {
    const card: CardInfo = {
      id: cardEl.getAttribute('id') || '',
      name: cardEl.getAttribute('name') || '',
      setId: set.id,
      properties: {},
    };

    // Parse card properties
    cardEl.querySelectorAll('property').forEach((propEl) => {
      const propName = propEl.getAttribute('name') || '';
      const propValue = propEl.textContent || '';
      card.properties[propName] = propValue;
    });

    // Parse alternate cards
    cardEl.querySelectorAll('alternate').forEach((altEl) => {
      if (!card.alternates) {
        card.alternates = {};
      }
      const alt: CardInfo = {
        id: altEl.getAttribute('id') || '',
        name: altEl.getAttribute('name') || card.name,
        setId: set.id,
        properties: { ...card.properties },
      };
      altEl.querySelectorAll('property').forEach((propEl) => {
        const propName = propEl.getAttribute('name') || '';
        const propValue = propEl.textContent || '';
        alt.properties[propName] = propValue;
      });
      card.alternates[alt.name] = alt;
    });

    set.cards.push(card);
  });

  return set;
}

/**
 * Game package manager singleton
 */
class GamePackageManager {
  private games: Map<string, GamePackage> = new Map();
  private gamesPath: string = '';

  async loadGames(gamesPath: string): Promise<void> {
    this.gamesPath = gamesPath;
    // In Electron, this would scan the games directory
    // For now, we'll just initialize empty
  }

  getGame(gameId: string): GamePackage | undefined {
    return this.games.get(gameId);
  }

  getAllGames(): GamePackage[] {
    return Array.from(this.games.values());
  }

  async installGame(packagePath: string): Promise<GamePackage> {
    // TODO: Implement game installation from .o8g file
    throw new Error('Game installation not implemented');
  }

  async uninstallGame(gameId: string): Promise<void> {
    // TODO: Implement game uninstallation
    throw new Error('Game uninstallation not implemented');
  }

  getCard(gameId: string, cardId: string): CardInfo | undefined {
    const game = this.games.get(gameId);
    if (!game) return undefined;
    return game.cardDatabase.cards.get(cardId);
  }
}

export const gamePackageManager = new GamePackageManager();
