import { describe, it, expect, vi } from 'vitest';
import { ScriptApi, ScriptApiDeps } from '@main/scripting/script-api';
import type { GameState } from '../../../src/shared/types';

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
          markers: [{ id: 'marker1', name: 'Damage', iconUrl: '', count: 3 }],
          properties: { 'Type': 'Piece' }, peekingPlayers: [],
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

function createApi(overrides?: Partial<ScriptApiDeps>) {
  const state = makeState();
  const deps: ScriptApiDeps = {
    getGameState: () => state,
    getLocalPlayerId: () => 1,
    getGameDefinition: () => undefined,
    sendProtocolMessage: vi.fn(),
    addChatMessage: vi.fn(),
    ...overrides,
  };
  return { api: new ScriptApi(deps), deps, state };
}

describe('ScriptApi Full Coverage (Phase 8)', () => {
  describe('AskMarker()', () => {
    it('returns marker tuple from dialog', () => {
      const { api } = createApi({
        requestDialogSync: (type) => {
          if (type === 'askMarker') return ['marker1', 'Damage'];
          return null;
        },
      });
      const result = api.AskMarker();
      expect(result).toEqual(['marker1', 'Damage']);
    });

    it('returns null when no handler', () => {
      const { api } = createApi();
      expect(api.AskMarker()).toBeNull();
    });
  });

  describe('AskCard()', () => {
    it('returns card id from dialog', () => {
      const { api } = createApi({
        requestDialogSync: (type, params) => {
          expect(type).toBe('askCard');
          expect(params).toHaveProperty('properties');
          return 65537;
        },
      });
      const result = api.AskCard({ 'Type': ['Piece'] }, 'and', 'Choose a card');
      expect(result).toBe(65537);
    });

    it('returns null when no handler', () => {
      const { api } = createApi();
      expect(api.AskCard({}, null, 'Choose')).toBeNull();
    });
  });

  describe('QueryCard()', () => {
    it('returns matching card IDs from dialog', () => {
      const { api } = createApi({
        requestDialogSync: (type) => {
          if (type === 'queryCard') return ['65537', '65538'];
          return [];
        },
      });
      const result = api.QueryCard({ 'Type': ['Piece'] }, true);
      expect(result).toEqual(['65537', '65538']);
    });

    it('returns empty array when no handler', () => {
      const { api } = createApi();
      expect(api.QueryCard({}, false)).toEqual([]);
    });
  });

  describe('Focus / ClearFocus / GetFocusedCards', () => {
    it('Focus sends card IDs to renderer', () => {
      const { api, deps } = createApi();
      api.Focus([65537, 65538]);
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith('ScriptFocus', { cards: [65537, 65538] });
    });

    it('ClearFocus clears the focus', () => {
      const { api, deps } = createApi();
      api.ClearFocus();
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith('ScriptClearFocus', {});
    });

    it('GetFocusedCards returns null (not trackable in current model)', () => {
      const { api } = createApi();
      expect(api.GetFocusedCards()).toBeNull();
    });
  });

  describe('PlaySound()', () => {
    it('sends sound name to renderer', () => {
      const { api, deps } = createApi();
      api.PlaySound('click');
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith('ScriptPlaySound', { name: 'click' });
    });
  });

  describe('Web_Read()', () => {
    it('returns result from sync HTTP handler', () => {
      const { api } = createApi({
        requestDialogSync: (type, params) => {
          if (type === 'webRead') return { Item1: '<html>OK</html>', Item2: 200 };
          return null;
        },
      });
      const result = api.Web_Read('https://example.com', 5000);
      expect(result).toEqual({ Item1: '<html>OK</html>', Item2: 200 });
    });

    it('returns empty result when no handler', () => {
      const { api } = createApi();
      const result = api.Web_Read('https://example.com', 5000);
      expect(result).toEqual({ Item1: '', Item2: 0 });
    });
  });

  describe('Web_Post()', () => {
    it('returns result from sync HTTP handler', () => {
      const { api } = createApi({
        requestDialogSync: (type, params) => {
          if (type === 'webPost') return { Item1: '{"ok":true}', Item2: 200 };
          return null;
        },
      });
      const result = api.Web_Post('https://example.com/api', 'data=1', 5000);
      expect(result).toEqual({ Item1: '{"ok":true}', Item2: 200 });
    });

    it('returns empty result when no handler', () => {
      const { api } = createApi();
      const result = api.Web_Post('https://example.com/api', 'data=1', 5000);
      expect(result).toEqual({ Item1: '', Item2: 0 });
    });
  });

  describe('RndArray()', () => {
    it('returns array of random integers in range', () => {
      const { api } = createApi();
      const result = api.RndArray(1, 6, 10);
      expect(result).toHaveLength(10);
      for (const val of result) {
        expect(val).toBeGreaterThanOrEqual(1);
        expect(val).toBeLessThanOrEqual(6);
      }
    });

    it('returns empty array for count 0', () => {
      const { api } = createApi();
      expect(api.RndArray(1, 6, 0)).toEqual([]);
    });
  });

  describe('ResetGame() / SoftResetGame()', () => {
    it('ResetGame sends protocol message', () => {
      const { api, deps } = createApi();
      api.ResetGame();
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith('ResetReq', {});
    });

    it('SoftResetGame sends protocol message', () => {
      const { api, deps } = createApi();
      api.SoftResetGame();
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith('SoftResetReq', {});
    });
  });

  describe('ForceDisconnect()', () => {
    it('sends leave protocol message', () => {
      const { api, deps } = createApi();
      api.ForceDisconnect();
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith('Leave', {});
    });
  });

  describe('GroupGetName()', () => {
    it('returns group name', () => {
      const { api } = createApi();
      expect(api.GroupGetName(16842753)).toBe('Hand');
    });

    it('returns Table for table ID', () => {
      const { api } = createApi();
      expect(api.GroupGetName(0x01000000)).toBe('Table');
    });

    it('returns Unknown for invalid group', () => {
      const { api } = createApi();
      expect(api.GroupGetName(99999999)).toBe('Unknown');
    });
  });

  describe('SaveFileDlg() / OpenFileDlg()', () => {
    it('SaveFileDlg delegates to dialog handler', () => {
      const { api } = createApi({
        requestDialogSync: (type) => {
          if (type === 'saveFile') return '/path/to/file.txt';
          return null;
        },
      });
      expect(api.SaveFileDlg()).toBe('/path/to/file.txt');
    });

    it('OpenFileDlg delegates to dialog handler', () => {
      const { api } = createApi({
        requestDialogSync: (type) => {
          if (type === 'openFile') return '/path/to/file.txt';
          return null;
        },
      });
      expect(api.OpenFileDlg()).toBe('/path/to/file.txt');
    });

    it('returns null when no handler', () => {
      const { api } = createApi();
      expect(api.SaveFileDlg()).toBeNull();
      expect(api.OpenFileDlg()).toBeNull();
    });
  });
});
