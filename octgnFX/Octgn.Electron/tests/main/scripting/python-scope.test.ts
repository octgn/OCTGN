import { describe, it, expect, vi, beforeEach } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';
import { PythonScope, PythonScopeIO } from '@main/scripting/python-scope';
import { SkulptRuntime } from '@main/scripting/skulpt-runtime';
import { ScriptApi, ScriptApiDeps } from '@main/scripting/script-api';
import type { GameState, Player, Card, Group, Counter } from '../../../src/shared/types';

// Read the real versioned API file
const API_FILE = path.join(__dirname, '../../../src/main/scripting/versions/3.1.0.2.py');
const apiSource = fs.readFileSync(API_FILE, 'utf-8');

const testIO: PythonScopeIO = {
  readFile: (p: string) => {
    if (p.includes('3.1.0.2.py')) return apiSource;
    throw new Error(`File not found: ${p}`);
  },
  fileExists: (p: string) => p.includes('3.1.0.2.py'),
};

function makeCard(overrides: Partial<Card> = {}): Card {
  return {
    id: '65537', definitionId: 'def-1', name: 'Test Card',
    imageUrl: '', faceUp: true, position: { x: 0, y: 0 },
    rotation: 0, groupId: 'table', ownerId: '1', markers: [],
    properties: {}, peekingPlayers: [],
    size: { width: 63, height: 88 },
    ...overrides,
  };
}

function makeGameState(): GameState {
  return {
    gameId: 'game-1', gameName: 'Test Game',
    players: [
      {
        id: 1, name: 'Alice', color: '#FF0000', isHost: true, isSpectator: false,
        groups: [{
          id: '16842753', name: 'Hand', cards: [makeCard({ id: '100', groupId: '16842753' })],
          visibility: 2, controller: 1,
        }],
        counters: [{ id: 10, name: 'Life', value: 20 }],
        globalVariables: { role: 'attacker' },
      },
      {
        id: 2, name: 'Bob', color: '#0000FF', isHost: false, isSpectator: false,
        groups: [{
          id: '16908289', name: 'Hand', cards: [],
          visibility: 2, controller: 2,
        }],
        counters: [{ id: 20, name: 'Life', value: 15 }],
        globalVariables: {},
      },
    ],
    localPlayerId: 1,
    isSpectator: false,
    table: { cards: [makeCard()] },
    turnNumber: 5,
    activePlayer: 1,
    phase: 0,
    chatMessages: [],
    isStarted: true,
    globalVariables: { mode: 'standard' },
  };
}

function createScope(): { scope: PythonScope; deps: ScriptApiDeps & { sendProtocolMessage: ReturnType<typeof vi.fn>; addChatMessage: ReturnType<typeof vi.fn> } } {
  const state = makeGameState();
  const deps = {
    getGameState: () => state,
    getLocalPlayerId: () => state.localPlayerId,
    getGameDefinition: () => undefined,
    sendProtocolMessage: vi.fn(),
    addChatMessage: vi.fn(),
  };
  const runtime = new SkulptRuntime();
  const api = new ScriptApi(deps);
  const scope = new PythonScope(runtime, api, '3.1.0.2', testIO);
  return { scope, deps };
}

describe('PythonScope', () => {
  describe('initialize()', () => {
    it('loads versioned API and creates Card/Player/Group/Table classes', async () => {
      const { scope } = createScope();
      const result = await scope.initialize();
      expect(result.success).toBe(true);

      // Verify classes are defined
      expect(scope.hasFunction('notify')).toBe(true);
      expect(scope.hasFunction('whisper')).toBe(true);
      expect(scope.hasFunction('rnd')).toBe(true);
      expect(scope.hasFunction('mute')).toBe(true);
    });

    it('sets up me, players, table globals', async () => {
      const { scope } = createScope();
      await scope.initialize();

      // me should be Player(1) = Alice
      const r1 = await scope.getRuntime().execute('print(me.name)');
      expect(r1.success).toBe(true);
      expect(r1.output).toBe('Alice\n');

      // players should have 2 entries
      const r2 = await scope.getRuntime().execute('print(len(players))');
      expect(r2.success).toBe(true);
      expect(r2.output).toBe('2\n');

      // table should be Table with id 0x01000000
      const r3 = await scope.getRuntime().execute('print(table.name)');
      expect(r3.success).toBe(true);
      expect(r3.output).toBe('Table\n');
    });

    it('version and gameVersion are set', async () => {
      const { scope } = createScope();
      await scope.initialize();

      const r1 = await scope.getRuntime().execute('print(version)');
      expect(r1.success).toBe(true);
      expect(r1.output).toContain('3.4');

      const r2 = await scope.getRuntime().execute('print(gameVersion)');
      expect(r2.success).toBe(true);
      expect(r2.output).toBe('0.0.0\n');
    });

    it('rotation constants are defined', async () => {
      const { scope } = createScope();
      await scope.initialize();

      const result = await scope.getRuntime().execute('print(Rot0, Rot90, Rot180, Rot270)');
      expect(result.output).toBe('0 1 2 3\n');
    });
  });

  describe('_api accessibility from Python', () => {
    it('_api.PlayerName works from Python', async () => {
      const { scope } = createScope();
      await scope.initialize();

      const result = await scope.getRuntime().execute('print(_api.PlayerName(1))');
      expect(result.success).toBe(true);
      expect(result.output).toBe('Alice\n');
    });

    it('_api.CounterGet works from Python', async () => {
      const { scope } = createScope();
      await scope.initialize();

      const result = await scope.getRuntime().execute('print(_api.CounterGet(10))');
      expect(result.success).toBe(true);
      expect(result.output).toBe('20\n');
    });

    it('me.name returns local player name through _api', async () => {
      const { scope } = createScope();
      await scope.initialize();

      const result = await scope.getRuntime().execute('print(me.name)');
      expect(result.success).toBe(true);
      expect(result.output).toBe('Alice\n');
    });
  });

  describe('loadGameScript()', () => {
    it('executes game Python into scope', async () => {
      const { scope } = createScope();
      await scope.initialize();

      const result = await scope.loadGameScript(`
def OnGameStarted():
    notify("Game has started!")
    return True
`);
      expect(result.success).toBe(true);
      expect(scope.hasFunction('OnGameStarted')).toBe(true);
    });

    it('fails if not initialized', async () => {
      const { scope } = createScope();
      const result = await scope.loadGameScript('x = 1');
      expect(result.success).toBe(false);
      expect(result.error).toContain('not initialized');
    });

    it('game script can use API classes', async () => {
      const { scope } = createScope();
      await scope.initialize();

      await scope.loadGameScript(`
def getPlayerName(pid):
    p = Player(pid)
    return p.name
`);

      const result = await scope.callFunction('getPlayerName', 1);
      expect(result.success).toBe(true);
      expect(result.returnValue).toBe('Alice');
    });
  });

  describe('callFunction()', () => {
    it('calls function with arguments', async () => {
      const { scope } = createScope();
      await scope.initialize();

      await scope.loadGameScript('def add(a, b):\n    return a + b');
      const result = await scope.callFunction('add', 3, 4);
      expect(result.success).toBe(true);
      expect(result.returnValue).toBe(7);
    });

    it('fails if not initialized', async () => {
      const { scope } = createScope();
      const result = await scope.callFunction('foo');
      expect(result.success).toBe(false);
    });
  });

  describe('Player class integration', () => {
    it('Player piles accessible', async () => {
      const { scope } = createScope();
      await scope.initialize();

      const result = await scope.getRuntime().execute(`
p = Player(1)
print(len(p.piles))
`);
      expect(result.success).toBe(true);
      expect(result.output).toBe('1\n');
    });

    it('Player counters accessible', async () => {
      const { scope } = createScope();
      await scope.initialize();

      const result = await scope.getRuntime().execute(`
p = Player(1)
print(p.counters['Life'].value)
`);
      expect(result.success).toBe(true);
      expect(result.output).toBe('20\n');
    });
  });

  describe('Card class integration', () => {
    it('Card name accessible', async () => {
      const { scope } = createScope();
      await scope.initialize();

      const result = await scope.getRuntime().execute('print(Card(65537).name)');
      expect(result.success).toBe(true);
      expect(result.output).toBe('Test Card\n');
    });

    it('Card faceUp property works', async () => {
      const { scope } = createScope();
      await scope.initialize();

      const result = await scope.getRuntime().execute('print(Card(65537).isFaceUp)');
      expect(result.success).toBe(true);
      expect(result.output).toBe('True\n');
    });
  });

  describe('notify/whisper integration', () => {
    it('notify calls addChatMessage', async () => {
      const { scope, deps } = createScope();
      await scope.initialize();

      await scope.getRuntime().execute('notify("Hello!")');
      expect(deps.addChatMessage).toHaveBeenCalledWith('Hello!', true);
    });

    it('whisper calls addChatMessage', async () => {
      const { scope, deps } = createScope();
      await scope.initialize();

      await scope.getRuntime().execute('whisper("Secret")');
      expect(deps.addChatMessage).toHaveBeenCalledWith('Secret', false);
    });
  });

  describe('Global variables integration', () => {
    it('getGlobalVariable works', async () => {
      const { scope } = createScope();
      await scope.initialize();

      const result = await scope.getRuntime().execute('print(getGlobalVariable("mode"))');
      expect(result.success).toBe(true);
      expect(result.output).toBe('standard\n');
    });

    it('Player.getGlobalVariable works', async () => {
      const { scope } = createScope();
      await scope.initialize();

      const result = await scope.getRuntime().execute('print(Player(1).getGlobalVariable("role"))');
      expect(result.success).toBe(true);
      expect(result.output).toBe('attacker\n');
    });
  });

  describe('turnNumber integration', () => {
    it('turnNumber returns current turn', async () => {
      const { scope } = createScope();
      await scope.initialize();

      const result = await scope.getRuntime().execute('print(turnNumber())');
      expect(result.success).toBe(true);
      expect(result.output).toBe('5\n');
    });
  });
});
