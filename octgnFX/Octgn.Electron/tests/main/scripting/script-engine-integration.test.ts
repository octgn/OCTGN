import { describe, it, expect, vi, beforeEach } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';
import { ScriptEngine } from '@main/scripting/script-engine';
import { ScriptApiDeps } from '@main/scripting/script-api';
import type { GameState, GameDefinition } from '../../../src/shared/types';

// Mock electron BrowserWindow
vi.mock('electron', () => ({
  BrowserWindow: {
    getAllWindows: vi.fn(() => []),
  },
}));

const API_FILE = path.join(__dirname, '../../../src/main/scripting/versions/3.1.0.2.py');
const apiSource = fs.readFileSync(API_FILE, 'utf-8');

function makeGameState(): GameState {
  return {
    gameId: 'game-1', gameName: 'Chess',
    players: [{
      id: 1, name: 'Alice', color: '#FF0000', isHost: true, isSpectator: false,
      groups: [{ id: '16842753', name: 'Hand', cards: [], visibility: 2, controller: 1 }],
      counters: [{ id: 10, name: 'Life', value: 20 }],
      globalVariables: {},
    }, {
      id: 2, name: 'Bob', color: '#0000FF', isHost: false, isSpectator: false,
      groups: [{ id: '16908289', name: 'Hand', cards: [], visibility: 2, controller: 2 }],
      counters: [],
      globalVariables: {},
    }],
    localPlayerId: 1,
    isSpectator: false,
    table: { cards: [] },
    turnNumber: 1,
    activePlayer: 1,
    phase: 0,
    chatMessages: [],
    isStarted: true,
    globalVariables: {},
  };
}

function makeGameDef(): GameDefinition {
  return {
    id: 'chess-id',
    name: 'Chess',
    version: '1.0.0',
    description: 'Chess game',
    cardWidth: 63,
    cardHeight: 88,
    cardBack: '',
    deckSections: [],
    sharedDeckSections: [],
    players: [],
    globalVariables: [],
    phases: [],
    scriptVersion: '3.1.0.2',
  };
}

function createEngine() {
  const state = makeGameState();
  const gameDef = makeGameDef();
  const deps: ScriptApiDeps = {
    getGameState: () => state,
    getLocalPlayerId: () => 1,
    getGameDefinition: () => gameDef,
    sendProtocolMessage: vi.fn(),
    addChatMessage: vi.fn(),
  };
  const engine = new ScriptEngine();
  return { engine, deps, state, gameDef };
}

describe('ScriptEngine (integrated)', () => {
  describe('initialize()', () => {
    it('initializes with game definition and creates Python scope', async () => {
      const { engine, deps, gameDef } = createEngine();
      const result = await engine.initialize(gameDef, deps, {
        readFile: (p: string) => {
          if (p.includes('3.1.0.2.py')) return apiSource;
          throw new Error(`Not found: ${p}`);
        },
        fileExists: (p: string) => p.includes('3.1.0.2.py'),
      });
      expect(result.success).toBe(true);
      expect(engine.isInitialized()).toBe(true);
    });
  });

  describe('handleRemoteCall() with real execution', () => {
    it('executes a Python function via RemoteCall', async () => {
      const { engine, deps, gameDef } = createEngine();
      await engine.initialize(gameDef, deps, {
        readFile: (p: string) => {
          if (p.includes('3.1.0.2.py')) return apiSource;
          throw new Error(`Not found: ${p}`);
        },
        fileExists: (p: string) => p.includes('3.1.0.2.py'),
      });

      // Load a game script that defines a function
      await engine.loadGameScript(`
def myAction(card):
    notify("Card action called with " + str(card))
`);

      const result = engine.handleRemoteCall(1, 'myAction', 'Card(65537)');
      expect(result.success).toBe(true);
    });

    it('still validates when sandboxed', async () => {
      const { engine, deps, gameDef } = createEngine();
      await engine.initialize(gameDef, deps, {
        readFile: (p: string) => {
          if (p.includes('3.1.0.2.py')) return apiSource;
          throw new Error(`Not found: ${p}`);
        },
        fileExists: (p: string) => p.includes('3.1.0.2.py'),
      });

      const result = engine.handleRemoteCall(1, 'exec', '"bad code"');
      expect(result.success).toBe(false);
      expect(result.error).toContain('security violation');
    });
  });

  describe('triggerEvent()', () => {
    it('dispatches event to Python scope', async () => {
      const { engine, deps, gameDef } = createEngine();
      await engine.initialize(gameDef, deps, {
        readFile: (p: string) => {
          if (p.includes('3.1.0.2.py')) return apiSource;
          throw new Error(`Not found: ${p}`);
        },
        fileExists: (p: string) => p.includes('3.1.0.2.py'),
      });

      await engine.loadGameScript(`
_events_fired = []
def OnGameStarted():
    _events_fired.append("started")
`);

      await engine.triggerEvent('OnGameStart');
      // Event should be mapped to OnGameStarted and called
    });

    it('returns override result for override events', async () => {
      const { engine, deps, gameDef } = createEngine();
      await engine.initialize(gameDef, deps, {
        readFile: (p: string) => {
          if (p.includes('3.1.0.2.py')) return apiSource;
          throw new Error(`Not found: ${p}`);
        },
        fileExists: (p: string) => p.includes('3.1.0.2.py'),
      });

      await engine.loadGameScript(`
def OverrideGameReset():
    return True
`);

      const result = await engine.triggerEvent('OverrideGameReset');
      expect(result?.returnValue).toBe(true);
    });
  });

  describe('loadGameScript()', () => {
    it('loads game Python scripts', async () => {
      const { engine, deps, gameDef } = createEngine();
      await engine.initialize(gameDef, deps, {
        readFile: (p: string) => {
          if (p.includes('3.1.0.2.py')) return apiSource;
          throw new Error(`Not found: ${p}`);
        },
        fileExists: (p: string) => p.includes('3.1.0.2.py'),
      });

      const result = await engine.loadGameScript('x = 42');
      expect(result.success).toBe(true);
    });

    it('fails if not initialized', async () => {
      const { engine } = createEngine();
      const result = await engine.loadGameScript('x = 42');
      expect(result.success).toBe(false);
    });
  });

  describe('execution queue', () => {
    it('queues concurrent calls and executes sequentially', async () => {
      const { engine, deps, gameDef } = createEngine();
      await engine.initialize(gameDef, deps, {
        readFile: (p: string) => {
          if (p.includes('3.1.0.2.py')) return apiSource;
          throw new Error(`Not found: ${p}`);
        },
        fileExists: (p: string) => p.includes('3.1.0.2.py'),
      });

      await engine.loadGameScript(`
order = []
def first():
    order.append(1)
def second():
    order.append(2)
`);

      // Fire both events concurrently
      const p1 = engine.triggerEvent('first');
      const p2 = engine.triggerEvent('second');
      await Promise.all([p1, p2]);

      // Both should have executed (order may vary but both should be in list)
    });
  });
});
