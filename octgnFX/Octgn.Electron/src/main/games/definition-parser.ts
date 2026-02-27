import { XMLParser } from 'fast-xml-parser';
import type { GameDefinition, PlayerDefinition, GroupDefinition, CounterDefinition, GamePhase, CardAction, GroupAction, Variable, GroupVisibility } from '../../shared/types';

const parser = new XMLParser({
  ignoreAttributes: false,
  attributeNamePrefix: '@_',
  isArray: (tagName) =>
    ['player', 'group', 'action', 'groupaction', 'counter', 'phase', 'globalvariable', 'variable', 'card', 'cardsize'].includes(tagName.toLowerCase()),
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

    // Card size
    const cardEl = g['card'] as Record<string, unknown> | undefined;
    const cardWidth = cardEl ? num(cardEl, 'width', 63) : 63;
    const cardHeight = cardEl ? num(cardEl, 'height', 88) : 88;
    const cardBack = cardEl ? attr(cardEl, 'back') : '';

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

    // Deck sections — read from first player's groups that are "deck-like"
    // or from a <shared> element if present
    const deckSections = players[0]?.groups.map((gr) => gr.name) ?? [];
    const sharedDeckSections: string[] = [];

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
    };
  } catch {
    return null;
  }
}
