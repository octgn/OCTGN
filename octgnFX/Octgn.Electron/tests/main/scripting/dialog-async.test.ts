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

function createApi(requestDialog?: (type: string, params: Record<string, unknown>) => Promise<unknown>) {
  const state = makeState();
  const deps: ScriptApiDeps = {
    getGameState: () => state,
    getLocalPlayerId: () => 1,
    getGameDefinition: () => undefined,
    sendProtocolMessage: vi.fn(),
    addChatMessage: vi.fn(),
    requestDialog,
  };
  return new ScriptApi(deps);
}

describe('Async dialog methods (Promise-based)', () => {
  describe('Confirm()', () => {
    it('returns a Promise that resolves to user response when requestDialog is provided', () => {
      const api = createApi(async (type, params) => {
        expect(type).toBe('confirm');
        expect(params).toHaveProperty('message', 'Delete this card?');
        return true;
      });
      const result = api.Confirm('Delete this card?');
      expect(result).toBeInstanceOf(Promise);
    });

    it('resolves to false when user denies', async () => {
      const api = createApi(async () => false);
      const result = await api.Confirm('Delete?');
      expect(result).toBe(false);
    });

    it('resolves to true when user confirms', async () => {
      const api = createApi(async () => true);
      const result = await api.Confirm('Sure?');
      expect(result).toBe(true);
    });

    it('returns true synchronously when no requestDialog provided', () => {
      const api = createApi();
      const result = api.Confirm('Delete?');
      // Without async handler, falls back to sync default
      expect(result).toBe(true);
    });
  });

  describe('AskInteger()', () => {
    it('returns a Promise that resolves to user-provided integer', async () => {
      const api = createApi(async (type, params) => {
        expect(type).toBe('askInteger');
        expect(params).toHaveProperty('question', 'How many?');
        expect(params).toHaveProperty('defaultAnswer', 5);
        return 42;
      });
      const result = api.AskInteger('How many?', 5);
      expect(result).toBeInstanceOf(Promise);
      expect(await result).toBe(42);
    });

    it('returns defaultAnswer when no requestDialog provided', () => {
      const api = createApi();
      expect(api.AskInteger('How many?', 7)).toBe(7);
    });

    it('returns null when user cancels (resolves to null)', async () => {
      const api = createApi(async () => null);
      const result = await api.AskInteger('How many?', 5);
      expect(result).toBeNull();
    });
  });

  describe('AskString()', () => {
    it('returns a Promise that resolves to user-provided string', async () => {
      const api = createApi(async (type, params) => {
        expect(type).toBe('askString');
        expect(params).toHaveProperty('question', 'Enter name:');
        expect(params).toHaveProperty('defaultAnswer', 'Bob');
        return 'Alice';
      });
      const result = await api.AskString('Enter name:', 'Bob');
      expect(result).toBe('Alice');
    });

    it('returns defaultAnswer when no requestDialog provided', () => {
      const api = createApi();
      expect(api.AskString('Name?', 'default')).toBe('default');
    });

    it('returns null when user cancels', async () => {
      const api = createApi(async () => null);
      const result = await api.AskString('Name?', 'default');
      expect(result).toBeNull();
    });
  });

  describe('AskChoice()', () => {
    it('returns a Promise that resolves to user selection index', async () => {
      const api = createApi(async (type, params) => {
        expect(type).toBe('askChoice');
        expect(params).toHaveProperty('question', 'Pick one:');
        expect(params).toHaveProperty('choices', ['A', 'B', 'C']);
        return 1;
      });
      const result = await api.AskChoice('Pick one:', ['A', 'B', 'C'], [], []);
      expect(result).toBe(1);
    });

    it('returns -1 when no requestDialog provided', () => {
      const api = createApi();
      expect(api.AskChoice('Pick:', ['A'], [], [])).toBe(-1);
    });

    it('passes colors and buttons to handler', async () => {
      const handler = vi.fn(async () => 0);
      const api = createApi(handler);
      await api.AskChoice('Pick:', ['Red', 'Blue'], ['#FF0000', '#0000FF'], ['OK', 'Cancel']);
      expect(handler).toHaveBeenCalledWith('askChoice', {
        question: 'Pick:',
        choices: ['Red', 'Blue'],
        colors: ['#FF0000', '#0000FF'],
        buttons: ['OK', 'Cancel'],
      });
    });
  });

  describe('AskMarker()', () => {
    it('returns a Promise that resolves to marker data', async () => {
      const markerData = { id: 'marker1', name: 'Damage', count: 1 };
      const api = createApi(async (type) => {
        expect(type).toBe('askMarker');
        return markerData;
      });
      const result = await api.AskMarker();
      expect(result).toEqual(markerData);
    });

    it('returns null when no requestDialog provided', () => {
      const api = createApi();
      expect(api.AskMarker()).toBeNull();
    });

    it('returns null when user cancels', async () => {
      const api = createApi(async () => null);
      const result = await api.AskMarker();
      expect(result).toBeNull();
    });
  });

  describe('AskCard()', () => {
    it('returns a Promise that resolves to card list', async () => {
      const cards = [{ id: 'card1', name: 'Ace of Spades' }];
      const api = createApi(async (type, params) => {
        expect(type).toBe('askCard');
        expect(params).toHaveProperty('properties');
        expect(params).toHaveProperty('operator', 'and');
        expect(params).toHaveProperty('title', 'Pick a card');
        return cards;
      });
      const result = await api.AskCard({ Name: ['Ace'] }, 'and', 'Pick a card');
      expect(result).toEqual(cards);
    });

    it('returns null when no requestDialog provided', () => {
      const api = createApi();
      expect(api.AskCard({}, null, '')).toBeNull();
    });
  });

  describe('error handling', () => {
    it('returns default value when async handler rejects', async () => {
      const api = createApi(async () => { throw new Error('UI error'); });
      // Confirm defaults to true on error
      const result = await api.Confirm('Delete?');
      expect(result).toBe(true);
    });

    it('AskInteger returns defaultAnswer on rejection', async () => {
      const api = createApi(async () => { throw new Error('fail'); });
      const result = await api.AskInteger('How many?', 3);
      expect(result).toBe(3);
    });

    it('AskString returns defaultAnswer on rejection', async () => {
      const api = createApi(async () => { throw new Error('fail'); });
      const result = await api.AskString('Name?', 'fallback');
      expect(result).toBe('fallback');
    });
  });

  describe('backward compatibility with requestDialogSync', () => {
    it('still works with sync callback when no async handler', () => {
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
      const api = new ScriptApi(deps);
      // Should use sync handler and return synchronously
      expect(api.Confirm('Sure?')).toBe(false);
    });

    it('prefers async requestDialog over sync requestDialogSync', async () => {
      const state = makeState();
      const syncHandler = vi.fn(() => false);
      const asyncHandler = vi.fn(async () => true);
      const deps: ScriptApiDeps = {
        getGameState: () => state,
        getLocalPlayerId: () => 1,
        getGameDefinition: () => undefined,
        sendProtocolMessage: vi.fn(),
        addChatMessage: vi.fn(),
        requestDialogSync: syncHandler,
        requestDialog: asyncHandler,
      };
      const api = new ScriptApi(deps);
      const result = await api.Confirm('Sure?');
      expect(result).toBe(true);
      expect(asyncHandler).toHaveBeenCalled();
      expect(syncHandler).not.toHaveBeenCalled();
    });
  });
});

describe('Skulpt async suspension integration', () => {
  it('dialog method returning Promise triggers Skulpt suspension', async () => {
    // This test verifies the SkulptRuntime proxy correctly converts
    // Promise returns to Skulpt suspensions
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
      requestDialog: async (type) => {
        if (type === 'askInteger') return 42;
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

    // askInteger should suspend, get 42, resume
    await scope.loadGameScript(`
val = askInteger("How many?", 0)
`);
    const check = await runtime.execute('print(val)');
    expect(check.output).toBe('42\n');
  });

  it('confirm() suspension returns correct boolean', async () => {
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
      requestDialog: async (type) => {
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

  it('askString() suspension returns correct string', async () => {
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
      requestDialog: async (type) => {
        if (type === 'askString') return 'Hello World';
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
val = askString("Enter name:", "default")
`);
    const check = await runtime.execute('print(val)');
    expect(check.output).toBe('Hello World\n');
  });
});
