import { describe, it, expect, vi } from 'vitest';
import { ScriptApi, ScriptApiDeps } from '@main/scripting/script-api';
import type { GameState } from '../../../src/shared/types';

function makeState(): GameState {
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

function createApi(dialogSync?: (type: string, params: Record<string, unknown>) => unknown) {
  const state = makeState();
  const deps: ScriptApiDeps = {
    getGameState: () => state,
    getLocalPlayerId: () => 1,
    getGameDefinition: () => undefined,
    sendProtocolMessage: vi.fn(),
    addChatMessage: vi.fn(),
    requestDialogSync: dialogSync,
  };
  return new ScriptApi(deps);
}

describe('Dialog methods (synchronous)', () => {
  describe('Confirm()', () => {
    it('returns true when user confirms', () => {
      const api = createApi((type, params) => {
        expect(type).toBe('confirm');
        expect(params).toHaveProperty('message', 'Delete this card?');
        return true;
      });
      expect(api.Confirm('Delete this card?')).toBe(true);
    });

    it('returns false when user denies', () => {
      const api = createApi(() => false);
      expect(api.Confirm('Delete?')).toBe(false);
    });

    it('returns true (default) when no dialog handler', () => {
      const api = createApi();
      expect(api.Confirm('Delete?')).toBe(true);
    });

    it('returns true (default) when handler throws', () => {
      const api = createApi(() => { throw new Error('UI error'); });
      expect(api.Confirm('Delete?')).toBe(true);
    });
  });

  describe('AskInteger()', () => {
    it('returns user-provided integer', () => {
      const api = createApi((type, params) => {
        expect(type).toBe('askInteger');
        expect(params).toHaveProperty('question', 'How many?');
        expect(params).toHaveProperty('defaultAnswer', 5);
        return 42;
      });
      expect(api.AskInteger('How many?', 5)).toBe(42);
    });

    it('returns default when no handler', () => {
      const api = createApi();
      expect(api.AskInteger('How many?', 7)).toBe(7);
    });

    it('returns default when handler throws', () => {
      const api = createApi(() => { throw new Error('fail'); });
      expect(api.AskInteger('How many?', 3)).toBe(3);
    });
  });

  describe('AskString()', () => {
    it('returns user-provided string', () => {
      const api = createApi((type, params) => {
        expect(type).toBe('askString');
        expect(params).toHaveProperty('question', 'Enter name:');
        expect(params).toHaveProperty('defaultAnswer', 'Bob');
        return 'Alice';
      });
      expect(api.AskString('Enter name:', 'Bob')).toBe('Alice');
    });

    it('returns default when no handler', () => {
      const api = createApi();
      expect(api.AskString('Name?', 'default')).toBe('default');
    });

    it('returns default when handler throws', () => {
      const api = createApi(() => { throw new Error('fail'); });
      expect(api.AskString('Name?', 'fallback')).toBe('fallback');
    });
  });

  describe('AskChoice()', () => {
    it('returns user selection index', () => {
      const api = createApi((type, params) => {
        expect(type).toBe('askChoice');
        expect(params).toHaveProperty('question', 'Pick one:');
        expect(params).toHaveProperty('choices');
        return 1;
      });
      expect(api.AskChoice('Pick one:', ['A', 'B', 'C'], [], [])).toBe(1);
    });

    it('returns -1 when no handler', () => {
      const api = createApi();
      expect(api.AskChoice('Pick:', ['A'], [], [])).toBe(-1);
    });

    it('returns -1 when handler throws', () => {
      const api = createApi(() => { throw new Error('fail'); });
      expect(api.AskChoice('Pick:', ['A', 'B'], [], [])).toBe(-1);
    });

    it('passes colors and buttons to handler', () => {
      const handler = vi.fn(() => 0);
      const api = createApi(handler);
      api.AskChoice('Pick:', ['Red', 'Blue'], ['#FF0000', '#0000FF'], ['OK', 'Cancel']);
      expect(handler).toHaveBeenCalledWith('askChoice', {
        question: 'Pick:',
        choices: ['Red', 'Blue'],
        colors: ['#FF0000', '#0000FF'],
        buttons: ['OK', 'Cancel'],
      });
    });
  });

  describe('end-to-end with PythonScope', () => {
    // These tests verify dialog methods work through the full Python stack
    // They use the same test infrastructure as python-scope.test.ts

    it('confirm() returns dialog result in Python', async () => {
      // Import dynamically to avoid circular deps in type-only test file
      const { SkulptRuntime } = await import('@main/scripting/skulpt-runtime');
      const { PythonScope } = await import('@main/scripting/python-scope');
      const fs = await import('fs');
      const path = await import('path');

      const API_FILE = path.join(__dirname, '../../../src/main/scripting/versions/3.1.0.2.py');
      const apiSource = fs.readFileSync(API_FILE, 'utf-8');

      const state = makeState();
      const deps: ScriptApiDeps = {
        getGameState: () => state,
        getLocalPlayerId: () => 1,
        getGameDefinition: () => undefined,
        sendProtocolMessage: vi.fn(),
        addChatMessage: vi.fn(),
        requestDialogSync: (type) => {
          if (type === 'confirm') return false;
          return null;
        },
      };
      const runtime = new SkulptRuntime();
      const api = new ScriptApi(deps);
      const scope = new PythonScope(runtime, api, '3.1.0.2', {
        readFile: (p: string) => {
          if (p.includes('3.1.0.2.py')) return apiSource;
          throw new Error(`Not found: ${p}`);
        },
        fileExists: (p: string) => p.includes('3.1.0.2.py'),
      });
      await scope.initialize();

      await scope.loadGameScript(`
result = confirm("Are you sure?")
`);
      const check = await runtime.execute('print(result)');
      expect(check.output).toBe('False\n');
    });

    it('askInteger() returns dialog result in Python', async () => {
      const { SkulptRuntime } = await import('@main/scripting/skulpt-runtime');
      const { PythonScope } = await import('@main/scripting/python-scope');
      const fs = await import('fs');
      const path = await import('path');

      const API_FILE = path.join(__dirname, '../../../src/main/scripting/versions/3.1.0.2.py');
      const apiSource = fs.readFileSync(API_FILE, 'utf-8');

      const state = makeState();
      const deps: ScriptApiDeps = {
        getGameState: () => state,
        getLocalPlayerId: () => 1,
        getGameDefinition: () => undefined,
        sendProtocolMessage: vi.fn(),
        addChatMessage: vi.fn(),
        requestDialogSync: (type) => {
          if (type === 'askInteger') return 99;
          return null;
        },
      };
      const runtime = new SkulptRuntime();
      const api = new ScriptApi(deps);
      const scope = new PythonScope(runtime, api, '3.1.0.2', {
        readFile: (p: string) => {
          if (p.includes('3.1.0.2.py')) return apiSource;
          throw new Error(`Not found: ${p}`);
        },
        fileExists: (p: string) => p.includes('3.1.0.2.py'),
      });
      await scope.initialize();

      await scope.loadGameScript(`
val = askInteger("How many?", 0)
`);
      const check = await runtime.execute('print(val)');
      expect(check.output).toBe('99\n');
    });
  });
});
