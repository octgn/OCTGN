import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ScriptApi, ScriptApiDeps } from '@main/scripting/script-api';
import type { GameState, Player, Card, Group, Counter, GameDefinition } from '../../../src/shared/types';

function makeCard(overrides: Partial<Card> = {}): Card {
  return {
    id: '65537',
    definitionId: 'def-guid-1',
    name: 'Test Card',
    imageUrl: 'octgn-asset://card/1/2/3',
    faceUp: true,
    position: { x: 100, y: 200 },
    rotation: 0,
    groupId: 'table',
    ownerId: '1',
    markers: [],
    properties: { Type: 'Creature', Cost: '3' },
    peekingPlayers: [],
    size: { width: 63, height: 88 },
    ...overrides,
  };
}

function makeCounter(overrides: Partial<Counter> = {}): Counter {
  return {
    id: 100,
    name: 'Life',
    value: 20,
    ...overrides,
  };
}

function makeGroup(overrides: Partial<Group> = {}): Group {
  return {
    id: '16842753', // 0x01010001 = player 1, group index 1
    name: 'Hand',
    cards: [],
    visibility: 2, // Owner
    controller: 1,
    ...overrides,
  };
}

function makePlayer(overrides: Partial<Player> = {}): Player {
  return {
    id: 1,
    name: 'Player One',
    color: '#FF0000',
    isHost: true,
    isSpectator: false,
    groups: [makeGroup()],
    counters: [makeCounter()],
    globalVariables: { score: '10' },
    ...overrides,
  };
}

function makeGameState(overrides: Partial<GameState> = {}): GameState {
  return {
    gameId: 'game-123',
    gameName: 'Test Game',
    players: [
      makePlayer({ id: 1, name: 'Player One', color: '#FF0000' }),
      makePlayer({
        id: 2,
        name: 'Player Two',
        color: '#0000FF',
        isHost: false,
        groups: [makeGroup({ id: '16908289', name: 'Hand', controller: 2 })], // 0x01020001
        counters: [makeCounter({ id: 200, name: 'Life', value: 15 })],
        globalVariables: {},
      }),
    ],
    localPlayerId: 1,
    isSpectator: false,
    table: {
      cards: [makeCard()],
    },
    turnNumber: 3,
    activePlayer: 1,
    phase: 0,
    chatMessages: [],
    isStarted: true,
    globalVariables: { gameMode: 'standard' },
    ...overrides,
  };
}

function makeDeps(gameState?: GameState): ScriptApiDeps & { sendProtocolMessage: ReturnType<typeof vi.fn>; addChatMessage: ReturnType<typeof vi.fn> } {
  const state = gameState ?? makeGameState();
  return {
    getGameState: () => state,
    getLocalPlayerId: () => state.localPlayerId,
    getGameDefinition: () => undefined,
    sendProtocolMessage: vi.fn(),
    addChatMessage: vi.fn(),
  };
}

describe('ScriptApi', () => {
  let deps: ReturnType<typeof makeDeps>;
  let api: ScriptApi;

  beforeEach(() => {
    deps = makeDeps();
    api = new ScriptApi(deps);
  });

  describe('Player API', () => {
    it('LocalPlayerId returns the local player ID', () => {
      expect(api.LocalPlayerId()).toBe(1);
    });

    it('SharedPlayerId returns -1 (no shared player by default)', () => {
      // The shared/global player has id=0 in OCTGN
      const state = makeGameState({
        players: [
          makePlayer({ id: 0, name: 'Global' }),
          makePlayer({ id: 1, name: 'Player One' }),
        ],
      });
      const d = makeDeps(state);
      const a = new ScriptApi(d);
      expect(a.SharedPlayerId()).toBe(0);
    });

    it('AllPlayers returns all non-spectator player IDs', () => {
      const ids = api.AllPlayers();
      expect(ids).toEqual([1, 2]);
    });

    it('PlayerName returns the player name', () => {
      expect(api.PlayerName(1)).toBe('Player One');
      expect(api.PlayerName(2)).toBe('Player Two');
    });

    it('PlayerColor returns the player color', () => {
      expect(api.PlayerColor(1)).toBe('#FF0000');
    });

    it('PlayerCounters returns counter id→name pairs', () => {
      const counters = api.PlayerCounters(1);
      expect(counters).toEqual([{ Key: 100, Value: 'Life' }]);
    });

    it('PlayerPiles returns group id→name pairs', () => {
      const piles = api.PlayerPiles(1);
      expect(piles).toEqual([{ Key: 16842753, Value: 'Hand' }]);
    });
  });

  describe('Card API', () => {
    it('CardName returns the card name', () => {
      expect(api.CardName(65537)).toBe('Test Card');
    });

    it('CardOwner returns the owner player ID', () => {
      expect(api.CardOwner(65537)).toBe(1);
    });

    it('CardGetFaceUp returns face-up state', () => {
      expect(api.CardGetFaceUp(65537)).toBe(true);
    });

    it('CardSetFaceUp sends protocol message', () => {
      api.CardSetFaceUp(65537, false);
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith(
        'TurnReq',
        expect.objectContaining({ card: 65537, up: false })
      );
    });

    it('CardGetOrientation returns rotation as enum value', () => {
      expect(api.CardGetOrientation(65537)).toBe(0); // Rot0
    });

    it('CardSetOrientation sends protocol message', () => {
      api.CardSetOrientation(65537, 1); // Rot90
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith(
        'RotateReq',
        expect.objectContaining({ card: 65537, rot: 90 })
      );
    });

    it('CardPosition returns x,y tuple', () => {
      const pos = api.CardPosition(65537);
      expect(pos).toEqual([100, 200]);
    });

    it('CardGroup returns the group ID', () => {
      // Card on table has groupId 'table'
      const groupId = api.CardGroup(65537);
      expect(groupId).toBe(0x01000000); // TABLE_ID
    });

    it('CardMoveTo sends MoveCardReq', () => {
      api.CardMoveTo(65537, 16842753, 0);
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith(
        'MoveCardReq',
        expect.objectContaining({ id: [65537], group: 16842753, idx: [0] })
      );
    });

    it('CardProperty returns a card property value', () => {
      expect(api.CardProperty(65537, 'Type')).toBe('Creature');
      expect(api.CardProperty(65537, 'Cost')).toBe('3');
    });

    it('CardProperty returns empty string for missing property', () => {
      expect(api.CardProperty(65537, 'NonExistent')).toBe('');
    });

    it('CardModel returns the definition ID', () => {
      expect(api.CardModel(65537)).toBe('def-guid-1');
    });
  });

  describe('Group API', () => {
    it('GroupCount returns card count', () => {
      const state = makeGameState();
      state.players[0].groups[0].cards = [makeCard({ id: '1' }), makeCard({ id: '2' })];
      const d = makeDeps(state);
      const a = new ScriptApi(d);
      expect(a.GroupCount(16842753)).toBe(2);
    });

    it('GroupCards returns card IDs', () => {
      const state = makeGameState();
      state.players[0].groups[0].cards = [makeCard({ id: '111' }), makeCard({ id: '222' })];
      const d = makeDeps(state);
      const a = new ScriptApi(d);
      expect(a.GroupCards(16842753)).toEqual([111, 222]);
    });

    it('GroupCard returns card at index', () => {
      const state = makeGameState();
      state.players[0].groups[0].cards = [makeCard({ id: '111' }), makeCard({ id: '222' })];
      const d = makeDeps(state);
      const a = new ScriptApi(d);
      expect(a.GroupCard(16842753, 0)).toBe(111);
      expect(a.GroupCard(16842753, 1)).toBe(222);
    });

    it('GroupShuffle sends protocol message', () => {
      api.GroupShuffle(16842753);
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith(
        'ShuffleDeprecated',
        expect.objectContaining({ group: 16842753 })
      );
    });
  });

  describe('Counter API', () => {
    it('CounterGet returns counter value', () => {
      expect(api.CounterGet(100)).toBe(20);
    });

    it('CounterSet sends protocol message', () => {
      api.CounterSet(100, 25);
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith(
        'CounterReq',
        expect.objectContaining({ counter: 100, value: 25 })
      );
    });
  });

  describe('Marker API', () => {
    it('MarkerGetCount returns 0 for no markers', () => {
      expect(api.MarkerGetCount(65537, 'marker-1', 'Damage')).toBe(0);
    });

    it('MarkerGetCount returns count for existing marker', () => {
      const state = makeGameState();
      state.table.cards[0].markers = [
        { id: 'marker-1', name: 'Damage', iconUrl: '', count: 3 },
      ];
      const d = makeDeps(state);
      const a = new ScriptApi(d);
      expect(a.MarkerGetCount(65537, 'marker-1', 'Damage')).toBe(3);
    });

    it('MarkerSetCount sends AddMarkerReq for increase', () => {
      api.MarkerSetCount(65537, 2, 'marker-1', 'Damage');
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith(
        'AddMarkerReq',
        expect.objectContaining({
          card: 65537,
          id: 'marker-1',
          name: 'Damage',
          count: 2,
        })
      );
    });
  });

  describe('Variable API', () => {
    it('GetGlobalVariable returns game-level variable', () => {
      expect(api.GetGlobalVariable('gameMode')).toBe('standard');
    });

    it('GetGlobalVariable returns empty string for missing', () => {
      expect(api.GetGlobalVariable('missing')).toBe('');
    });

    it('SetGlobalVariable sends protocol message with oldval and val', () => {
      api.SetGlobalVariable('gameMode', 'draft');
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith(
        'SetGlobalVariable',
        expect.objectContaining({ name: 'gameMode', oldval: 'standard', val: 'draft' })
      );
    });

    it('SetGlobalVariable sends empty oldval for new variable', () => {
      api.SetGlobalVariable('newVar', 'hello');
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith(
        'SetGlobalVariable',
        expect.objectContaining({ name: 'newVar', oldval: '', val: 'hello' })
      );
    });

    it('PlayerGetGlobalVariable returns player variable', () => {
      expect(api.PlayerGetGlobalVariable(1, 'score')).toBe('10');
    });

    it('PlayerSetGlobalVariable sends protocol message with oldval and val', () => {
      api.PlayerSetGlobalVariable(1, 'score', '20');
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith(
        'PlayerSetGlobalVariable',
        expect.objectContaining({ player: 1, name: 'score', oldval: '10', val: '20' })
      );
    });

    it('PlayerSetGlobalVariable sends empty oldval for new variable', () => {
      api.PlayerSetGlobalVariable(1, 'newVar', 'value');
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith(
        'PlayerSetGlobalVariable',
        expect.objectContaining({ player: 1, name: 'newVar', oldval: '', val: 'value' })
      );
    });
  });

  describe('Notification API', () => {
    it('Notify sends PrintReq and adds local system chat message', () => {
      api.Notify('Hello everyone');
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith('PrintReq', { text: 'Hello everyone' });
      expect(deps.addChatMessage).toHaveBeenCalledWith('Hello everyone', true);
    });

    it('Whisper adds a local-only system chat message', () => {
      api.Whisper('Secret message');
      expect(deps.addChatMessage).toHaveBeenCalledWith('Secret message', true);
      expect(deps.sendProtocolMessage).not.toHaveBeenCalled();
    });
  });

  describe('Mute API', () => {
    it('Mute toggles muted state', () => {
      expect(api.isMuted()).toBe(false);
      api.Mute(true);
      expect(api.isMuted()).toBe(true);
      api.Mute(false);
      expect(api.isMuted()).toBe(false);
    });
  });

  describe('Game Flow API', () => {
    it('TurnNumber returns current turn', () => {
      expect(api.TurnNumber()).toBe(3);
    });

    it('GetActivePlayer returns active player ID', () => {
      expect(api.GetActivePlayer()).toBe(1);
    });

    it('IsTwoSided returns useTwoSidedTable', () => {
      const state = makeGameState({ useTwoSidedTable: true });
      const d = makeDeps(state);
      const a = new ScriptApi(d);
      expect(a.IsTwoSided()).toBe(true);
    });
  });

  describe('Card Properties API', () => {
    it('CardProperties returns list of property names', () => {
      const def: GameDefinition = {
        id: 'game-1',
        name: 'Test',
        version: '1.0',
        description: '',
        cardWidth: 63,
        cardHeight: 88,
        cardBack: '',
        deckSections: [],
        sharedDeckSections: [],
        players: [],
        globalVariables: [],
        phases: [],
      };
      const d = makeDeps();
      d.getGameDefinition = () => def;
      const a = new ScriptApi(d);
      // When no card property definitions exist, returns empty
      expect(a.CardProperties()).toEqual([]);
    });
  });

  describe('Group Constructor API', () => {
    it('GroupCtor returns Python constructor string for a pile', () => {
      const ctor = api.GroupCtor(16842753);
      expect(ctor).toContain('Pile(');
      expect(ctor).toContain('16842753');
    });

    it('GroupCtor returns Table() for table group', () => {
      const ctor = api.GroupCtor(0x01000000);
      expect(ctor).toBe('Table()');
    });
  });

  describe('Version API', () => {
    it('OCTGN_Version returns a version string', () => {
      expect(api.OCTGN_Version()).toBeTruthy();
      expect(typeof api.OCTGN_Version()).toBe('string');
    });

    it('GameDef_Version returns game version', () => {
      // No definition loaded
      expect(api.GameDef_Version()).toBe('0.0.0');
    });
  });

  describe('Random API', () => {
    it('Random sends RandomReq and returns a Promise', () => {
      const result = api.Random(1, 6);
      expect(result).toBeInstanceOf(Promise);
      expect(deps.sendProtocolMessage).toHaveBeenCalledWith('RandomReq', { min: 1, max: 6 });
    });

    it('Random resolves when handleRandomResult is called', async () => {
      const promise = api.Random(1, 6);
      api.handleRandomResult(4);
      const result = await promise;
      expect(result).toBe(4);
    });

    it('falls back to local random when no requestRandom dep', () => {
      // For backward compatibility / offline mode
      const localDeps = makeDeps(makeGameState());
      // Remove sendProtocolMessage to simulate offline
      localDeps.sendProtocolMessage = undefined as unknown as typeof localDeps.sendProtocolMessage;
      const localApi = new ScriptApi(localDeps);
      const result = localApi.Random(1, 6);
      // Should return a number directly (not a Promise) as fallback
      expect(typeof result).toBe('number');
      expect(result).toBeGreaterThanOrEqual(1);
      expect(result).toBeLessThanOrEqual(6);
    });
  });

  describe('Table-specific API', () => {
    it('GroupCount for table returns table card count', () => {
      expect(api.GroupCount(0x01000000)).toBe(1);
    });

    it('GroupCards for table returns table card IDs', () => {
      expect(api.GroupCards(0x01000000)).toEqual([65537]);
    });
  });
});
