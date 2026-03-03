import { describe, it, expect, vi } from 'vitest';
import { ActionExecutor } from '@main/scripting/action-executor';
import { SkulptRuntime } from '@main/scripting/skulpt-runtime';
import { ScriptApi, ScriptApiDeps } from '@main/scripting/script-api';
import { PythonScope, PythonScopeIO } from '@main/scripting/python-scope';
import type { GameState, CardAction, GroupAction } from '../../../src/shared/types';
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

function makeState(): GameState {
  return {
    gameId: 'g1', gameName: 'Test',
    players: [{
      id: 1, name: 'Alice', color: '#FF0000', isHost: true, isSpectator: false,
      groups: [{
        id: '16842753', name: 'Hand',
        cards: [{
          id: '65537', definitionId: 'def1', name: 'Pawn',
          imageUrl: '', faceUp: true, position: { x: 0, y: 0 },
          rotation: 0, groupId: '16842753', ownerId: '1',
          markers: [], properties: { 'Type': 'Piece' }, peekingPlayers: [],
          size: { width: 63, height: 88 },
        }],
        visibility: 2, controller: 1,
      }],
      counters: [{ id: 10, name: 'Life', value: 20 }],
      globalVariables: {},
    }],
    localPlayerId: 1, isSpectator: false,
    table: { cards: [] }, turnNumber: 1, activePlayer: 1,
    phase: 0, chatMessages: [], isStarted: true,
    globalVariables: {},
  };
}

async function createScope() {
  const state = makeState();
  const addChatMessage = vi.fn();
  const deps: ScriptApiDeps = {
    getGameState: () => state,
    getLocalPlayerId: () => 1,
    getGameDefinition: () => undefined,
    sendProtocolMessage: vi.fn(),
    addChatMessage,
  };
  const runtime = new SkulptRuntime();
  const api = new ScriptApi(deps);
  const scope = new PythonScope(runtime, api, '3.1.0.2', testIO);
  await scope.initialize();
  return { scope, deps, runtime, state, addChatMessage };
}

describe('ActionExecutor', () => {
  describe('executeCardAction()', () => {
    it('calls execute function with Card argument', async () => {
      const { scope, addChatMessage } = await createScope();
      const executor = new ActionExecutor(scope);

      await scope.loadGameScript(`
def doFlip(card):
    notify("flipped " + card.name)
`);

      const action: CardAction = {
        name: 'Flip',
        execute: 'doFlip',
      };

      const result = await executor.executeCardAction(action, 65537);
      expect(result.success).toBe(true);
      expect(addChatMessage).toHaveBeenCalledWith(
        expect.stringContaining('flipped Pawn'),
        true,
      );
    });

    it('returns error for missing function', async () => {
      const { scope } = await createScope();
      const executor = new ActionExecutor(scope);

      const action: CardAction = {
        name: 'Missing',
        execute: 'nonExistentFunc',
      };

      const result = await executor.executeCardAction(action, 65537);
      expect(result.success).toBe(false);
      expect(result.error).toBeDefined();
    });
  });

  describe('executeBatchCardAction()', () => {
    it('calls batchExecute with list of Card args', async () => {
      const { scope, addChatMessage } = await createScope();
      const executor = new ActionExecutor(scope);

      await scope.loadGameScript(`
def doBatchFlip(cards):
    notify("batch " + str(len(cards)))
`);

      const action: CardAction = {
        name: 'Flip All',
        execute: 'doFlipSingle',
        batchExecute: 'doBatchFlip',
      };

      const result = await executor.executeBatchCardAction(action, [65537]);
      expect(result.success).toBe(true);
      expect(addChatMessage).toHaveBeenCalledWith(
        expect.stringContaining('batch 1'),
        true,
      );
    });

    it('falls back to single execute when no batchExecute', async () => {
      const { scope, addChatMessage } = await createScope();
      const executor = new ActionExecutor(scope);

      await scope.loadGameScript(`
def doSingle(card):
    notify("single " + card.name)
`);

      const action: CardAction = {
        name: 'Single',
        execute: 'doSingle',
      };

      const result = await executor.executeBatchCardAction(action, [65537]);
      expect(result.success).toBe(true);
      expect(addChatMessage).toHaveBeenCalledWith(
        expect.stringContaining('single Pawn'),
        true,
      );
    });
  });

  describe('executeGroupAction()', () => {
    it('calls execute function with Group argument', async () => {
      const { scope, addChatMessage } = await createScope();
      const executor = new ActionExecutor(scope);

      await scope.loadGameScript(`
def doShuffle(group):
    notify("shuffled " + group.name)
`);

      const action: GroupAction = {
        name: 'Shuffle',
        execute: 'doShuffle',
      };

      // Group ID for player 1's first group (Hand)
      const groupId = 0x01000000 | (1 << 16) | 1; // 16842753
      const result = await executor.executeGroupAction(action, groupId);
      expect(result.success).toBe(true);
      expect(addChatMessage).toHaveBeenCalledWith(
        expect.stringContaining('shuffled Hand'),
        true,
      );
    });
  });

  describe('mute() reset between actions', () => {
    it('resets muted flag after action that calls mute() without with-statement', async () => {
      const { scope, addChatMessage } = await createScope();
      const executor = new ActionExecutor(scope);

      // Mimics the real flipcoin function: calls mute() without "with", then notify()
      await scope.loadGameScript(`
def flipcoin(group):
    mute()
    n = rnd(1, 2)
    if n == 1:
        notify("heads")
    else:
        notify("tails")
`);

      const action: GroupAction = {
        name: 'Flip a Coin',
        execute: 'flipcoin',
      };

      const result = await executor.executeGroupAction(action, 0x01000000);
      expect(result.success).toBe(true);
      // notify() should have produced a chat message despite mute()
      expect(addChatMessage).toHaveBeenCalledWith(
        expect.stringMatching(/heads|tails/),
        true,
      );
    });

    it('does not leave muted flag on for subsequent actions', async () => {
      const { scope, addChatMessage } = await createScope();
      const executor = new ActionExecutor(scope);

      await scope.loadGameScript(`
def muteAction(group):
    mute()
    notify("first")

def normalAction(group):
    notify("second")
`);

      const muteAction: GroupAction = { name: 'Mute', execute: 'muteAction' };
      const normalAction: GroupAction = { name: 'Normal', execute: 'normalAction' };

      await executor.executeGroupAction(muteAction, 0x01000000);
      addChatMessage.mockClear();

      const result = await executor.executeGroupAction(normalAction, 0x01000000);
      expect(result.success).toBe(true);
      // Second action should NOT be affected by first action's mute()
      expect(addChatMessage).toHaveBeenCalledWith('second', true);
    });
  });

  describe('evaluateShowIf()', () => {
    it('returns true when showIf function returns True', async () => {
      const { scope } = await createScope();
      const executor = new ActionExecutor(scope);

      await scope.loadGameScript(`
def canFlip(card):
    return card.isFaceUp
`);

      const action: CardAction = {
        name: 'Flip',
        execute: 'doFlip',
        showIf: 'canFlip',
      };

      const result = await executor.evaluateShowIf(action, 65537);
      expect(result).toBe(true);
    });

    it('returns false when showIf function returns False', async () => {
      const { scope } = await createScope();
      const executor = new ActionExecutor(scope);

      await scope.loadGameScript(`
def cannotDo(card):
    return False
`);

      const action: CardAction = {
        name: 'Action',
        execute: 'doAction',
        showIf: 'cannotDo',
      };

      const result = await executor.evaluateShowIf(action, 65537);
      expect(result).toBe(false);
    });

    it('returns true when no showIf defined', async () => {
      const { scope } = await createScope();
      const executor = new ActionExecutor(scope);

      const action: CardAction = {
        name: 'Action',
        execute: 'doAction',
      };

      const result = await executor.evaluateShowIf(action, 65537);
      expect(result).toBe(true);
    });
  });

  describe('evaluateGetName()', () => {
    it('returns custom name from getName function', async () => {
      const { scope } = await createScope();
      const executor = new ActionExecutor(scope);

      await scope.loadGameScript(`
def flipName(card):
    if card.isFaceUp:
        return "Flip Down"
    return "Flip Up"
`);

      const action: CardAction = {
        name: 'Flip',
        execute: 'doFlip',
        getName: 'flipName',
      };

      const result = await executor.evaluateGetName(action, 65537);
      expect(result).toBe('Flip Down');
    });

    it('returns default name when no getName defined', async () => {
      const { scope } = await createScope();
      const executor = new ActionExecutor(scope);

      const action: CardAction = {
        name: 'Default Name',
        execute: 'doAction',
      };

      const result = await executor.evaluateGetName(action, 65537);
      expect(result).toBe('Default Name');
    });

    it('returns default name when getName throws', async () => {
      const { scope } = await createScope();
      const executor = new ActionExecutor(scope);

      await scope.loadGameScript(`
def badName(card):
    raise ValueError("oops")
`);

      const action: CardAction = {
        name: 'Fallback',
        execute: 'doAction',
        getName: 'badName',
      };

      const result = await executor.evaluateGetName(action, 65537);
      expect(result).toBe('Fallback');
    });
  });
});
