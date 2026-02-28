import { XMLParser } from 'fast-xml-parser';
import type { GameDefinition, PlayerDefinition, GroupDefinition, CounterDefinition, GamePhase, CardAction, GroupAction, Variable, GroupVisibility, TableDefinition, BoardDefinition, CardSizeDefinition, DeckSectionDef } from '../../shared/types';

const parser = new XMLParser({
  ignoreAttributes: false,
  attributeNamePrefix: '@_',
  isArray: (tagName) =>
    ['player', 'group', 'action', 'groupaction', 'counter', 'phase', 'globalvariable', 'variable', 'size', 'gameboard', 'section'].includes(tagName.toLowerCase()),
});

function attr(obj: Record<string, unknown>, name: string, fallback = ''): string {
  return (obj[`@_${name}`] as string | undefined) ?? fallback;
}

function num(obj: Record<string, unknown>, name: string, fallback = 0): number {
  const v = parseInt(attr(obj, name), 10);
  return isNaN(v) ? fallback : v;
}

function toVisibility(v: string): GroupVisibility {
  switch (v?.toLowerCase()) {
    case 'nobody': return 1;
    case 'owner': return 2;
    case 'everybody': return 3;
    default: return 0;
  }
}

function parseActions(raw: unknown): CardAction[] {
  if (!raw) return [];
  const arr = Array.isArray(raw) ? raw : [raw];
  return arr.map((a) => {
    const o = a as Record<string, unknown>;
    return {
      name: attr(o, 'name'),
      shortcut: attr(o, 'shortcut') || undefined,
      execute: attr(o, 'execute'),
      batchExecute: attr(o, 'batchexecute') || undefined,
    };
  });
}

function parseGroupActions(raw: unknown): GroupAction[] {
  if (!raw) return [];
  const arr = Array.isArray(raw) ? raw : [raw];
  return arr.map((a) => {
    const o = a as Record<string, unknown>;
    return {
      name: attr(o, 'name'),
      shortcut: attr(o, 'shortcut') || undefined,
      execute: attr(o, 'execute'),
    };
  });
}

function parseGroup(raw: Record<string, unknown>): GroupDefinition {
  return {
    name: attr(raw, 'name'),
    icon: attr(raw, 'icon') || undefined,
    visibility: toVisibility(attr(raw, 'visibility')),
    ordered: attr(raw, 'ordered') === 'true',
    shortcut: attr(raw, 'shortcut') || undefined,
    cardActions: parseActions(raw['action']),
    groupActions: parseGroupActions(raw['groupaction']),
  };
}

function parseCounter(raw: Record<string, unknown>): CounterDefinition {
  return {
    name: attr(raw, 'name'),
    icon: attr(raw, 'icon') || undefined,
    defaultValue: num(raw, 'default'),
  };
}

function parseVariables(raw: unknown): Variable[] {
  if (!raw) return [];
  const arr = Array.isArray(raw) ? raw : [raw];
  return arr.map((v) => {
    const o = v as Record<string, unknown>;
    return {
      name: attr(o, 'name'),
      defaultValue: attr(o, 'default'),
      global: attr(o, 'global') === 'true',
    };
  });
}

function numFloat(obj: Record<string, unknown>, name: string, fallback = 0): number {
  const v = parseFloat(attr(obj, name));
  return isNaN(v) ? fallback : v;
}

const VALID_BG_STYLES = new Set(['stretch', 'tile', 'uniform', 'uniformToFill']);

function parseTable(raw: Record<string, unknown>): TableDefinition {
  const bgStyle = attr(raw, 'backgroundStyle');
  const background = attr(raw, 'background') || undefined;

  const table: TableDefinition = {
    name: attr(raw, 'name'),
    width: num(raw, 'width'),
    height: num(raw, 'height'),
    background,
    backgroundStyle: VALID_BG_STYLES.has(bgStyle) ? bgStyle as TableDefinition['backgroundStyle'] : undefined,
  };

  // Legacy board from table attributes
  const boardPath = attr(raw, 'board') || undefined;
  const boardPosition = attr(raw, 'boardPosition') || undefined;
  if (boardPath && boardPosition) {
    const parts = boardPosition.split(',');
    if (parts.length === 4) {
      table.board = {
        name: 'Default',
        source: boardPath,
        x: parseFloat(parts[0]) || 0,
        y: parseFloat(parts[1]) || 0,
        width: parseFloat(parts[2]) || 0,
        height: parseFloat(parts[3]) || 0,
      };
    }
  }

  return table;
}

function parseCardSize(raw: Record<string, unknown>): CardSizeDefinition {
  const width = num(raw, 'width', 63);
  const height = num(raw, 'height', 88);
  const cornerRadius = num(raw, 'cornerRadius', 0);

  let backWidth = num(raw, 'backWidth', -1);
  let backHeight = num(raw, 'backHeight', -1);
  let backCornerRadius = num(raw, 'backCornerRadius', -1);

  // C# convention: -1 means "use front values"
  if (backWidth < 0) backWidth = width;
  if (backHeight < 0) backHeight = height;
  if (backCornerRadius < 0) backCornerRadius = cornerRadius;

  return {
    name: attr(raw, 'name'),
    width,
    height,
    cornerRadius,
    back: attr(raw, 'back'),
    front: attr(raw, 'front'),
    backWidth,
    backHeight,
    backCornerRadius,
  };
}

function parseBoards(gameEl: Record<string, unknown>): BoardDefinition[] | undefined {
  const gameboardsEl = gameEl['gameboards'] as Record<string, unknown> | undefined;
  if (!gameboardsEl) return undefined;

  const boards: BoardDefinition[] = [];

  // Default board from gameboards attributes
  const defaultSrc = attr(gameboardsEl, 'src');
  if (defaultSrc) {
    boards.push({
      name: attr(gameboardsEl, 'name'),
      source: defaultSrc,
      x: num(gameboardsEl, 'x'),
      y: num(gameboardsEl, 'y'),
      width: num(gameboardsEl, 'width'),
      height: num(gameboardsEl, 'height'),
    });
  }

  // Additional gameboard children
  const rawChildren = gameboardsEl['gameboard'];
  if (rawChildren) {
    const arr = Array.isArray(rawChildren) ? rawChildren : [rawChildren];
    for (const b of arr) {
      const o = b as Record<string, unknown>;
      boards.push({
        name: attr(o, 'name'),
        source: attr(o, 'src'),
        x: num(o, 'x'),
        y: num(o, 'y'),
        width: num(o, 'width'),
        height: num(o, 'height'),
      });
    }
  }

  return boards.length > 0 ? boards : undefined;
}

function parseDeckSections(raw: unknown, shared: boolean): DeckSectionDef[] {
  if (!raw) return [];
  const sections: DeckSectionDef[] = [];
  const sectionArr = (raw as Record<string, unknown>)['section'];
  if (!sectionArr) return [];
  const arr = Array.isArray(sectionArr) ? sectionArr : [sectionArr];
  for (const s of arr) {
    const o = s as Record<string, unknown>;
    const name = attr(o, 'name');
    const group = attr(o, 'group') || name; // default group = section name
    if (name) sections.push({ name, group, shared });
  }
  return sections;
}

function parsePlayer(raw: Record<string, unknown>): PlayerDefinition {
  const groups: GroupDefinition[] = [];
  const counters: CounterDefinition[] = [];

  const rawGroups = raw['group'];
  if (rawGroups) {
    const arr = Array.isArray(rawGroups) ? rawGroups : [rawGroups];
    for (const g of arr) groups.push(parseGroup(g as Record<string, unknown>));
  }

  const rawCounters = raw['counter'];
  if (rawCounters) {
    const arr = Array.isArray(rawCounters) ? rawCounters : [rawCounters];
    for (const c of arr) counters.push(parseCounter(c as Record<string, unknown>));
  }

  return {
    name: attr(raw, 'name'),
    groups,
    counters,
    globalVariables: parseVariables(raw['globalvariable'] ?? raw['variable']),
  };
}

/**
 * Parse a definition.xml buffer (or string) into a GameDefinition.
 * Returns null if the XML is invalid or missing required fields.
 */
export function parseDefinitionXml(xml: string | Buffer): GameDefinition | null {
  try {
    const xmlStr = typeof xml === 'string' ? xml : xml.toString('utf-8');
    const doc = parser.parse(xmlStr) as Record<string, unknown>;
    const g = doc['game'] as Record<string, unknown> | undefined;
    if (!g) return null;

    const id = attr(g, 'id');
    const name = attr(g, 'name');
    if (!id || !name) return null;

    // Card size — note: 'card' is NOT in isArray, so it's a single object
    const cardEl = g['card'] as Record<string, unknown> | undefined;
    const cardWidth = cardEl ? num(cardEl, 'width', 63) : 63;
    const cardHeight = cardEl ? num(cardEl, 'height', 88) : 88;
    const cardBack = cardEl ? attr(cardEl, 'back') : '';

    // Default card size (full definition)
    const defaultCardSize = cardEl ? parseCardSize(cardEl) : undefined;

    // Alternative card sizes from <card><size> children
    const cardSizes: Record<string, CardSizeDefinition> = {};
    if (cardEl) {
      const rawSizes = cardEl['size'];
      if (rawSizes) {
        const arr = Array.isArray(rawSizes) ? rawSizes : [rawSizes];
        for (const s of arr) {
          const size = parseCardSize(s as Record<string, unknown>);
          if (size.name) {
            cardSizes[size.name] = size;
          }
        }
      }
    }

    // Table
    const tableEl = g['table'] as Record<string, unknown> | undefined;
    const table = tableEl ? parseTable(tableEl) : undefined;

    // Game boards
    const boards = parseBoards(g);

    // Players
    const players: PlayerDefinition[] = [];
    const rawPlayers = g['player'];
    if (rawPlayers) {
      const arr = Array.isArray(rawPlayers) ? rawPlayers : [rawPlayers];
      for (const p of arr) players.push(parsePlayer(p as Record<string, unknown>));
    }

    // Phases
    const phases: GamePhase[] = [];
    const rawPhases = g['phase'];
    if (rawPhases) {
      const arr = Array.isArray(rawPhases) ? rawPhases : [rawPhases];
      for (const p of arr) {
        const o = p as Record<string, unknown>;
        phases.push({ name: attr(o, 'name'), icon: attr(o, 'icon') || undefined });
      }
    }

    // Deck sections — parse from <deck> and <sharedDeck> elements if present,
    // otherwise fallback to generating from player groups
    const deckEl = g['deck'] as Record<string, unknown> | undefined;
    const sharedDeckEl = g['sharedDeck'] as Record<string, unknown> | undefined;
    const deckSections: DeckSectionDef[] = deckEl
      ? parseDeckSections(deckEl, false)
      : (players[0]?.groups.map((gr) => ({ name: gr.name, group: gr.name, shared: false })) ?? []);
    const sharedDeckSections: DeckSectionDef[] = sharedDeckEl
      ? parseDeckSections(sharedDeckEl, true)
      : [];

    return {
      id,
      name,
      version: attr(g, 'version', '0.0.0.0'),
      description: attr(g, 'description'),
      iconUrl: attr(g, 'iconurl') || undefined,
      cardWidth,
      cardHeight,
      cardBack,
      deckSections,
      sharedDeckSections,
      players,
      globalVariables: parseVariables(g['globalvariable'] ?? g['variable']),
      phases,
      table,
      boards,
      cardSizes,
      defaultCardSize,
    };
  } catch {
    return null;
  }
}
