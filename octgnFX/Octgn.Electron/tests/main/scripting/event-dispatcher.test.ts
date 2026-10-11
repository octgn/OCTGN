import { describe, it, expect, vi, beforeEach } from 'vitest';
import { EventDispatcher, GameEvent } from '@main/scripting/event-dispatcher';
import { PythonScope, PythonScopeIO } from '@main/scripting/python-scope';
import { SkulptRuntime } from '@main/scripting/skulpt-runtime';
import { ScriptApi, ScriptApiDeps } from '@main/scripting/script-api';
import * as fs from 'fs';
import * as path from 'path';

const API_FILE = path.join(__dirname, '../../../src/main/scripting/versions/3.1.0.2.py');
const apiSource = fs.readFileSync(API_FILE, 'utf-8');

const testIO: PythonScopeIO = {
  readFile: (p: string) => {
    if (p.includes('3.1.0.2.py')) return apiSource;
    throw new Error(`File not found: ${p}`);
  },
  fileExists: (p: string) => p.includes('3.1.0.2.py'),
};

function makeGameState() {
  return {
    gameId: 'g1', gameName: 'Test',
    players: [{
      id: 1, name: 'Alice', color: '#FF0000', isHost: true, isSpectator: false,
      groups: [{ id: '16842753', name: 'Hand', cards: [], visibility: 2, controller: 1 }],
      counters: [{ id: 10, name: 'Life', value: 20 }],
      globalVariables: {},
    }],
    localPlayerId: 1, isSpectator: false,
    table: { cards: [] }, turnNumber: 1, activePlayer: 1,
    phase: 0, chatMessages: [], isStarted: true,
    globalVariables: {},
  };
}

async function createScope(version = '3.1.0.2') {
  const state = makeGameState();
  const deps: ScriptApiDeps = {
    getGameState: () => state as any,
    getLocalPlayerId: () => 1,
    getGameDefinition: () => undefined,
    sendProtocolMessage: vi.fn(),
    addChatMessage: vi.fn(),
  };
  const runtime = new SkulptRuntime();
  const api = new ScriptApi(deps);
  const scope = new PythonScope(runtime, api, version, testIO);
  await scope.initialize();
  return { scope, deps, runtime };
}

describe('EventDispatcher', () => {
  describe('version 3.1.0.2 event mapping', () => {
    it('maps OnGameStart to OnGameStarted for v3.1.0.2', () => {
      const dispatcher = new EventDispatcher('3.1.0.2');
      expect(dispatcher.mapEventName('OnGameStart')).toBe('OnGameStarted');
    });

    it('maps OnTableLoad to OnTableLoaded for v3.1.0.2', () => {
      const dispatcher = new EventDispatcher('3.1.0.2');
      expect(dispatcher.mapEventName('OnTableLoad')).toBe('OnTableLoaded');
    });

    it('maps OnCardClick to OnCardClicked for v3.1.0.2', () => {
      const dispatcher = new EventDispatcher('3.1.0.2');
      expect(dispatcher.mapEventName('OnCardClick')).toBe('OnCardClicked');
    });

    it('maps OnCardDoubleClick to OnCardDoubleClicked for v3.1.0.2', () => {
      const dispatcher = new EventDispatcher('3.1.0.2');
      expect(dispatcher.mapEventName('OnCardDoubleClick')).toBe('OnCardDoubleClicked');
    });

    it('passes through already-correct names', () => {
      const dispatcher = new EventDispatcher('3.1.0.2');
      expect(dispatcher.mapEventName('OnGameStarted')).toBe('OnGameStarted');
      expect(dispatcher.mapEventName('OnCardClicked')).toBe('OnCardClicked');
    });

    it('maps OnPlayerConnect to OnPlayerConnected', () => {
      const dispatcher = new EventDispatcher('3.1.0.2');
      expect(dispatcher.mapEventName('OnPlayerConnect')).toBe('OnPlayerConnected');
    });

    it('maps OnPlayerLeaveGame to OnPlayerQuit', () => {
      const dispatcher = new EventDispatcher('3.1.0.2');
      expect(dispatcher.mapEventName('OnPlayerLeaveGame')).toBe('OnPlayerQuit');
    });

    it('maps OnLoadDeck to OnDeckLoaded', () => {
      const dispatcher = new EventDispatcher('3.1.0.2');
      expect(dispatcher.mapEventName('OnLoadDeck')).toBe('OnDeckLoaded');
    });

    it('maps OnChangeCounter to OnCounterChanged', () => {
      const dispatcher = new EventDispatcher('3.1.0.2');
      expect(dispatcher.mapEventName('OnChangeCounter')).toBe('OnCounterChanged');
    });

    it('maps OnTurn to OnTurnPassed', () => {
      const dispatcher = new EventDispatcher('3.1.0.2');
      expect(dispatcher.mapEventName('OnTurn')).toBe('OnTurnPassed');
    });

    it('maps OnEndTurn to OnTurnPassed', () => {
      const dispatcher = new EventDispatcher('3.1.0.2');
      expect(dispatcher.mapEventName('OnEndTurn')).toBe('OnTurnPassed');
    });
  });

  describe('version 3.1.0.0/3.1.0.1 — no rename', () => {
    it('keeps OnGameStart for v3.1.0.0', () => {
      const dispatcher = new EventDispatcher('3.1.0.0');
      expect(dispatcher.mapEventName('OnGameStart')).toBe('OnGameStart');
    });

    it('keeps OnTableLoad for v3.1.0.1', () => {
      const dispatcher = new EventDispatcher('3.1.0.1');
      expect(dispatcher.mapEventName('OnTableLoad')).toBe('OnTableLoad');
    });
  });

  describe('dispatch()', () => {
    it('calls the Python function when defined', async () => {
      const { scope } = await createScope();
      const dispatcher = new EventDispatcher('3.1.0.2');

      await scope.loadGameScript(`
results = []
def OnGameStarted():
    results.append("started")
`);

      const result = await dispatcher.dispatch(scope, 'OnGameStart', {});
      expect(result.success).toBe(true);

      // Verify function was called
      const check = await scope.getRuntime().execute('print(len(results))');
      expect(check.output).toBe('1\n');
    });

    it('silently skips if function not defined', async () => {
      const { scope } = await createScope();
      const dispatcher = new EventDispatcher('3.1.0.2');

      const result = await dispatcher.dispatch(scope, 'OnGameStart', {});
      expect(result.success).toBe(true);
      expect(result.skipped).toBe(true);
    });

    it('handles errors without crashing', async () => {
      const { scope } = await createScope();
      const dispatcher = new EventDispatcher('3.1.0.2');

      await scope.loadGameScript(`
def OnGameStarted():
    raise ValueError("oops")
`);

      const result = await dispatcher.dispatch(scope, 'OnGameStart', {});
      expect(result.success).toBe(false);
      expect(result.error).toContain('ValueError');
    });

    it('override events return Python return value', async () => {
      const { scope } = await createScope();
      const dispatcher = new EventDispatcher('3.1.0.2');

      await scope.loadGameScript(`
def OverrideCardsMoved(cards, toGroups, indexs, xs, ys, faceups):
    return True
`);

      const result = await dispatcher.dispatch(scope, 'OverrideCardsMoved', {
        cards: [], toGroups: [], indexs: [], xs: [], ys: [], faceups: [],
      });
      expect(result.success).toBe(true);
      expect(result.returnValue).toBe(true);
    });
  });

  describe('isOverrideEvent()', () => {
    it('identifies override events', () => {
      const dispatcher = new EventDispatcher('3.1.0.2');
      expect(dispatcher.isOverrideEvent('OverrideCardsMoved')).toBe(true);
      expect(dispatcher.isOverrideEvent('OverrideTurnPassed')).toBe(true);
      expect(dispatcher.isOverrideEvent('OverrideGameReset')).toBe(true);
      expect(dispatcher.isOverrideEvent('OnGameStarted')).toBe(false);
    });
  });
});
