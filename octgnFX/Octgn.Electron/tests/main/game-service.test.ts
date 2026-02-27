import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { GameService } from '@main/api/game-service';
import { MessageType, type ProtocolMessage } from '@main/protocol/types';

// ---------------------------------------------------------------------------
// Mocks
// ---------------------------------------------------------------------------

// Mock the GameConnection class
const mockSendMessage = vi.fn();
const mockConnect = vi.fn().mockResolvedValue(undefined);
const mockDisconnect = vi.fn();
const mockOn = vi.fn();
let mockIsConnected = false;

vi.mock('@main/protocol/connection', () => ({
  GameConnection: vi.fn().mockImplementation(() => ({
    sendMessage: mockSendMessage,
    connect: mockConnect,
    disconnect: mockDisconnect,
    on: mockOn,
    get isConnected() {
      return mockIsConnected;
    },
  })),
}));

// Mock BrowserWindow.getAllWindows
const mockSend = vi.fn();
vi.mock('electron', () => ({
  BrowserWindow: {
    getAllWindows: () => [
      { webContents: { send: mockSend } },
    ],
  },
}));

// Mock CardResolver — loadGame is a no-op, resolve returns undefined by default
const mockResolve = vi.fn().mockReturnValue(undefined);
const mockLoadGame = vi.fn().mockResolvedValue(undefined);
const mockResolveGroupName = vi.fn().mockReturnValue(undefined);
const mockGetGameDefinition = vi.fn().mockReturnValue(undefined);

vi.mock('@main/games/card-resolver', () => ({
  CardResolver: vi.fn().mockImplementation(() => ({
    resolve: mockResolve,
    loadGame: mockLoadGame,
    resolveGroupName: mockResolveGroupName,
    getGameDefinition: mockGetGameDefinition,
  })),
}));

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/**
 * Create a ProtocolMessage from params.
 */
function msg(type: MessageType, params: Record<string, unknown>): ProtocolMessage {
  return { isMuted: 0, type, params };
}

/**
 * Extract the handler registered for a given event name via connection.on().
 */
function getHandler(eventName: string): (msg: ProtocolMessage) => void {
  const call = mockOn.mock.calls.find((c) => c[0] === eventName);
  if (!call) throw new Error(`No handler registered for event '${eventName}'`);
  return call[1];
}

/**
 * Join a game and return the service, triggering handler registration.
 */
async function joinedService(): Promise<GameService> {
  const service = new GameService();
  await service.joinGame('localhost', 1234, 'Nick', 'uid', 'gid', '1.0');
  return service;
}

/**
 * Join a game and then trigger the Welcome handler so gameState is initialized.
 */
async function serviceWithState(): Promise<GameService> {
  const service = await joinedService();
  const welcome = getHandler('Welcome');
  welcome(msg(MessageType.Welcome, { id: 42, gameSessionId: 'sess-1', gameName: 'Test Game' }));
  return service;
}

/**
 * Get the last broadcast game state.
 */
function lastState() {
  return mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
}

/**
 * Add a card to the table in the current state for testing card operations.
 */
function addTableCard(cardId: string) {
  const state = lastState();
  state.table.cards.push({
    id: cardId,
    definitionId: 'd1',
    name: `Card ${cardId}`,
    imageUrl: '',
    faceUp: false,
    position: { x: 0, y: 0 },
    rotation: 0,
    groupId: 'table',
    ownerId: '42',
    markers: [],
    properties: {},
    peekingPlayers: [],
    size: { width: 100, height: 140 },
  });
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('GameService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockIsConnected = false;
  });

  // -------------------------------------------------------------------------
  // joinGame
  // -------------------------------------------------------------------------

  describe('joinGame', () => {
    it('sends a Hello message with the correct params', async () => {
      const service = new GameService();
      const result = await service.joinGame(
        '127.0.0.1',
        9000,
        'Alice',
        'user-1',
        'game-1',
        '2.0.0',
        'pw',
        true,
      );

      expect(result).toEqual({ success: true });
      expect(mockConnect).toHaveBeenCalled();
      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.Hello, 0, {
        nick: 'Alice',
        userId: 'user-1',
        pkey: BigInt(0),
        client: 'OCTGN-Electron',
        clientVer: '3.4.426.0',
        octgnVer: '3.4.426.0',
        gameId: 'game-1',
        gameVersion: '2.0.0',
        password: 'pw',
        spectator: true,
      });
    });

    it('uses default values for password and spectator', async () => {
      const service = new GameService();
      await service.joinGame('host', 1234, 'Bob', 'u2', 'g2', '1.0');

      const params = mockSendMessage.mock.calls[0][2];
      expect(params.password).toBe('');
      expect(params.spectator).toBe(false);
    });

    it('returns an error result when connect() rejects', async () => {
      mockConnect.mockRejectedValueOnce(new Error('timeout'));

      const service = new GameService();
      const result = await service.joinGame('host', 1234, 'X', 'u', 'g', 'v');

      expect(result).toEqual({ success: false, error: 'timeout' });
    });
  });

  // -------------------------------------------------------------------------
  // leaveGame
  // -------------------------------------------------------------------------

  describe('leaveGame', () => {
    it('sends Leave message with local player id and disconnects', async () => {
      const service = await serviceWithState();
      mockSendMessage.mockClear();

      await service.leaveGame();

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.Leave, 0, {
        player: 42,
      });
      expect(mockDisconnect).toHaveBeenCalled();
    });

    it('clears state even when no connection exists', async () => {
      const service = new GameService();
      await service.leaveGame(); // should not throw
      expect(service.isConnected).toBe(false);
    });
  });

  // -------------------------------------------------------------------------
  // sendChat
  // -------------------------------------------------------------------------

  describe('sendChat', () => {
    it('forwards chat text to the connection', async () => {
      const service = await joinedService();
      mockSendMessage.mockClear();

      service.sendChat('hello world');

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.ChatReq, 0, {
        text: 'hello world',
      });
    });
  });

  // -------------------------------------------------------------------------
  // Game actions
  // -------------------------------------------------------------------------

  describe('moveCards', () => {
    it('sends MoveCardReq with correct params', async () => {
      const service = await joinedService();
      mockSendMessage.mockClear();

      service.moveCards([1, 2], 5, [0, 1], [true, false], true);

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.MoveCardReq, 0, {
        id: [1, 2],
        group: 5,
        idx: [0, 1],
        faceUp: [true, false],
        isScriptMove: true,
      });
    });
  });

  describe('moveCardsAt', () => {
    it('sends MoveCardAtReq with correct params', async () => {
      const service = await joinedService();
      mockSendMessage.mockClear();

      service.moveCardsAt([10], [100], [200], [0], [true]);

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.MoveCardAtReq, 0, {
        id: [10],
        x: [100],
        y: [200],
        idx: [0],
        faceUp: [true],
        isScriptMove: false,
      });
    });
  });

  describe('nextTurn', () => {
    it('sends NextTurn with localPlayerId', async () => {
      const service = await serviceWithState();
      mockSendMessage.mockClear();

      service.nextTurn();

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.NextTurn, 0, {
        player: 42,
        setActive: true,
        force: false,
      });
    });
  });

  describe('flipCard', () => {
    it('sends TurnReq with card id and faceUp', async () => {
      const service = await joinedService();
      mockSendMessage.mockClear();

      service.flipCard(7, true);

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.TurnReq, 0, {
        card: 7,
        up: true,
      });
    });
  });

  describe('rotateCard', () => {
    it('sends RotateReq with card id and rotation', async () => {
      const service = await joinedService();
      mockSendMessage.mockClear();

      service.rotateCard(7, 2);

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.RotateReq, 0, {
        card: 7,
        rot: 2,
      });
    });
  });

  describe('setCounter', () => {
    it('sends CounterReq with counter id and value', async () => {
      const service = await joinedService();
      mockSendMessage.mockClear();

      service.setCounter(3, 20);

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.CounterReq, 0, {
        counter: 3,
        value: 20,
        isScriptChange: false,
      });
    });
  });

  describe('loadDeck', () => {
    it('sends LoadDeck with correct params', async () => {
      const service = await joinedService();
      mockSendMessage.mockClear();

      service.loadDeck([1, 2], ['typeA', 'typeB'], [10, 20], ['s', 'm'], 'sleeve-url', true);

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.LoadDeck, 0, {
        id: [1, 2],
        type: ['typeA', 'typeB'],
        group: [10, 20],
        size: ['s', 'm'],
        sleeve: 'sleeve-url',
        limited: true,
      });
    });
  });

  // -------------------------------------------------------------------------
  // isConnected
  // -------------------------------------------------------------------------

  describe('isConnected', () => {
    it('returns false when no connection exists', () => {
      const service = new GameService();
      expect(service.isConnected).toBe(false);
    });

    it('delegates to connection.isConnected', async () => {
      mockIsConnected = true;
      const service = await joinedService();
      expect(service.isConnected).toBe(true);
    });
  });

  // -------------------------------------------------------------------------
  // Message handlers -> gameState mutations
  // -------------------------------------------------------------------------

  describe('message handlers', () => {
    describe('Welcome', () => {
      it('sets localPlayerId and initializes game state', async () => {
        const service = await joinedService();
        const welcome = getHandler('Welcome');

        welcome(msg(MessageType.Welcome, { id: 7, gameSessionId: 'sess-x', gameName: 'My Game' }));

        // State was broadcast
        expect(mockSend).toHaveBeenCalled();
        const state = lastState();
        expect(state.localPlayerId).toBe(7);
        expect(state.gameId).toBe('sess-x');
        expect(state.gameName).toBe('My Game');
        expect(state.players).toEqual([]);
        expect(state.turnNumber).toBe(0);
        expect(state.isStarted).toBe(false);
        expect(state.isSpectator).toBe(false);
        expect(state.connectionStatus).toBe('connected');
      });

      it('sets isSpectator when joining as spectator', async () => {
        const service = new GameService();
        await service.joinGame('localhost', 1234, 'Nick', 'uid', 'gid', '1.0', '', true);
        const welcome = getHandler('Welcome');

        welcome(msg(MessageType.Welcome, { id: 7, gameSessionId: 'sess-x', gameName: 'My Game' }));

        const state = lastState();
        expect(state.isSpectator).toBe(true);
      });
    });

    describe('NewPlayer', () => {
      it('adds a player to the players array', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('NewPlayer');
        handler(msg(MessageType.NewPlayer, { id: 10, nick: 'Bob', spectator: false }));

        const state = lastState();
        expect(state.players).toHaveLength(1);
        expect(state.players[0]).toMatchObject({
          id: 10,
          name: 'Bob',
          isSpectator: false,
        });
      });

      it('stores tableSide as invertedTable on the player', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('NewPlayer');
        handler(msg(MessageType.NewPlayer, { id: 10, nick: 'Bob', spectator: false, tableSide: true }));

        const state = lastState();
        expect(state.players[0].invertedTable).toBe(true);
      });

      it('defaults invertedTable to false when tableSide not provided', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('NewPlayer');
        handler(msg(MessageType.NewPlayer, { id: 10, nick: 'Bob', spectator: false }));

        const state = lastState();
        expect(state.players[0].invertedTable).toBe(false);
      });
    });

    describe('PlayerSettings', () => {
      it('updates player spectator status', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer(msg(MessageType.NewPlayer, { id: 10, nick: 'Bob', spectator: false }));
        mockSend.mockClear();

        const handler = getHandler('PlayerSettings');
        handler(msg(MessageType.PlayerSettings, { playerId: 10, invertedTable: false, spectator: true }));

        const state = lastState();
        expect(state.players[0].isSpectator).toBe(true);
      });

      it('stores invertedTable on the player', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer(msg(MessageType.NewPlayer, { id: 10, nick: 'Bob', spectator: false }));
        mockSend.mockClear();

        const handler = getHandler('PlayerSettings');
        handler(msg(MessageType.PlayerSettings, { playerId: 10, invertedTable: true, spectator: false }));

        const state = lastState();
        expect(state.players[0].invertedTable).toBe(true);
      });
    });

    describe('Leave', () => {
      it('removes a player from the players array', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer(msg(MessageType.NewPlayer, { id: 10, nick: 'Bob', spectator: false }));
        newPlayer(msg(MessageType.NewPlayer, { id: 11, nick: 'Carol', spectator: false }));
        mockSend.mockClear();

        const leave = getHandler('Leave');
        leave(msg(MessageType.Leave, { player: 10 }));

        const state = lastState();
        expect(state.players).toHaveLength(1);
        expect(state.players[0].id).toBe(11);
      });
    });

    describe('Chat', () => {
      it('adds a chat message to chatMessages', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer(msg(MessageType.NewPlayer, { id: 10, nick: 'Alice', spectator: false }));
        mockSend.mockClear();

        const chat = getHandler('Chat');
        chat(msg(MessageType.Chat, { player: 10, text: 'hi there' }));

        const state = lastState();
        expect(state.chatMessages).toHaveLength(1);
        expect(state.chatMessages[0]).toMatchObject({
          playerId: 10,
          playerName: 'Alice',
          message: 'hi there',
          isSystem: false,
        });
      });
    });

    describe('Print', () => {
      it('adds a system message', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('Print');
        handler(msg(MessageType.Print, { player: 0, text: 'Game reset' }));

        const state = lastState();
        expect(state.chatMessages[0]).toMatchObject({
          message: 'Game reset',
          isSystem: true,
        });
      });
    });

    describe('Settings', () => {
      it('stores twoSidedTable in gameState', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('Settings');
        handler(msg(MessageType.Settings, { twoSidedTable: true, allowSpectators: true, muteSpectators: false }));

        const state = lastState();
        expect(state.useTwoSidedTable).toBe(true);
      });

      it('stores false when twoSidedTable is false', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('Settings');
        handler(msg(MessageType.Settings, { twoSidedTable: false, allowSpectators: true, muteSpectators: false }));

        const state = lastState();
        expect(state.useTwoSidedTable).toBe(false);
      });
    });

    describe('Start', () => {
      it('sets isStarted to true', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const start = getHandler('Start');
        start(msg(MessageType.Start, {}));

        const state = lastState();
        expect(state.isStarted).toBe(true);
      });
    });

    describe('NextTurn', () => {
      it('increments turn and sets active player when setActive is true', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('NextTurn');
        handler(msg(MessageType.NextTurn, { player: 42, setActive: true }));

        const state = lastState();
        expect(state.turnNumber).toBe(1);
        expect(state.activePlayer).toBe(42);
      });

      it('increments turn without changing active player when setActive is false', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('NextTurn');
        handler(msg(MessageType.NextTurn, { player: 99, setActive: false }));

        const state = lastState();
        expect(state.turnNumber).toBe(1);
        expect(state.activePlayer).toBe(0); // unchanged from initial
      });
    });

    describe('SetActivePlayer', () => {
      it('sets the active player', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('SetActivePlayer');
        handler(msg(MessageType.SetActivePlayer, { player: 10 }));

        const state = lastState();
        expect(state.activePlayer).toBe(10);
      });
    });

    describe('ClearActivePlayer', () => {
      it('clears the active player', async () => {
        const service = await serviceWithState();
        // Set an active player first
        const setHandler = getHandler('SetActivePlayer');
        setHandler(msg(MessageType.SetActivePlayer, { player: 10 }));
        mockSend.mockClear();

        const handler = getHandler('ClearActivePlayer');
        handler(msg(MessageType.ClearActivePlayer, {}));

        const state = lastState();
        expect(state.activePlayer).toBe(0);
      });
    });

    describe('Counter', () => {
      it('updates counter value for the correct player', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer(msg(MessageType.NewPlayer, { id: 10, nick: 'Alice', spectator: false }));

        // Manually add a counter to the player so it can be found
        const state1 = lastState();
        state1.players[0].counters.push({ id: 5, name: 'Life', value: 20 });

        mockSend.mockClear();

        const counter = getHandler('Counter');
        counter(msg(MessageType.Counter, { player: 10, counter: 5, value: 15 }));

        const state2 = lastState();
        expect(state2.players[0].counters[0].value).toBe(15);
      });

      it('creates counter if it does not exist', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer(msg(MessageType.NewPlayer, { id: 10, nick: 'Alice', spectator: false }));
        mockSend.mockClear();

        const counter = getHandler('Counter');
        counter(msg(MessageType.Counter, { player: 10, counter: 99, value: 30 }));

        const state = lastState();
        expect(state.players[0].counters).toHaveLength(1);
        expect(state.players[0].counters[0]).toMatchObject({ id: 99, value: 30 });
      });
    });

    describe('CreateCard', () => {
      it('creates cards and adds them to a group', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer(msg(MessageType.NewPlayer, { id: 10, nick: 'Alice', spectator: false }));
        mockSend.mockClear();

        const handler = getHandler('CreateCard');
        handler(msg(MessageType.CreateCard, {
          id: [100, 101],
          type: ['guid-a', 'guid-b'],
          size: ['100,140', '100,140'],
          group: 5,
        }));

        const state = lastState();
        // Cards should be in a group (created under the non-spectator player)
        const player = state.players.find((p: any) => p.id === 10);
        expect(player).toBeDefined();
        const group = player.groups.find((g: any) => g.id === '5');
        expect(group).toBeDefined();
        expect(group.cards).toHaveLength(2);
        expect(group.cards[0].id).toBe('100');
        expect(group.cards[1].id).toBe('101');
      });
    });

    describe('CreateCardAt', () => {
      it('creates cards at table positions', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('CreateCardAt');
        handler(msg(MessageType.CreateCardAt, {
          id: [200, 201],
          modelId: ['guid-c', 'guid-d'],
          x: [50, 150],
          y: [75, 200],
          faceUp: true,
          persist: false,
        }));

        const state = lastState();
        expect(state.table.cards).toHaveLength(2);
        expect(state.table.cards[0].position).toEqual({ x: 50, y: 75 });
        expect(state.table.cards[0].faceUp).toBe(true);
        expect(state.table.cards[1].position).toEqual({ x: 150, y: 200 });
      });
    });

    describe('DeleteCard', () => {
      it('removes a card from the table', async () => {
        const service = await serviceWithState();
        addTableCard('300');
        mockSend.mockClear();

        const handler = getHandler('DeleteCard');
        handler(msg(MessageType.DeleteCard, { card: 300, player: 0 }));

        const state = lastState();
        expect(state.table.cards).toHaveLength(0);
      });
    });

    describe('MoveCard', () => {
      it('moves card from table to a group', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer(msg(MessageType.NewPlayer, { id: 10, nick: 'Alice', spectator: false }));
        addTableCard('100');
        mockSend.mockClear();

        const handler = getHandler('MoveCard');
        handler(msg(MessageType.MoveCard, { id: [100], group: 7, faceUp: [true] }));

        const state = lastState();
        expect(state.table.cards).toHaveLength(0);
        const player = state.players.find((p: any) => p.id === 10);
        const group = player.groups.find((g: any) => g.id === '7');
        expect(group).toBeDefined();
        expect(group.cards[0].faceUp).toBe(true);
      });
    });

    describe('MoveCardAt', () => {
      it('moves card to table position', async () => {
        const service = await serviceWithState();
        addTableCard('200');
        mockSend.mockClear();

        const handler = getHandler('MoveCardAt');
        handler(msg(MessageType.MoveCardAt, { id: [200], x: [50], y: [75], faceUp: [true] }));

        const state = lastState();
        expect(state.table.cards).toHaveLength(1);
        expect(state.table.cards[0].position).toEqual({ x: 50, y: 75 });
        expect(state.table.cards[0].faceUp).toBe(true);
      });
    });

    describe('Turn', () => {
      it('updates card faceUp property', async () => {
        const service = await serviceWithState();
        addTableCard('300');
        mockSend.mockClear();

        const handler = getHandler('Turn');
        handler(msg(MessageType.Turn, { card: 300, up: true }));

        const state = lastState();
        expect(state.table.cards[0].faceUp).toBe(true);
      });
    });

    describe('Rotate', () => {
      it('updates card rotation (multiplied by 90)', async () => {
        const service = await serviceWithState();
        addTableCard('400');
        mockSend.mockClear();

        const handler = getHandler('Rotate');
        handler(msg(MessageType.Rotate, { card: 400, rot: 2 }));

        const state = lastState();
        expect(state.table.cards[0].rotation).toBe(180);
      });
    });

    describe('Target', () => {
      it('marks a card as targeted', async () => {
        const service = await serviceWithState();
        addTableCard('500');
        mockSend.mockClear();

        const handler = getHandler('Target');
        handler(msg(MessageType.Target, { player: 10, card: 500, isScriptChange: false }));

        const state = lastState();
        expect(state.table.cards[0].targetedBy).toBe('10');
      });
    });

    describe('Untarget', () => {
      it('removes targeting from a card', async () => {
        const service = await serviceWithState();
        addTableCard('500');
        // Target it first
        const targetHandler = getHandler('Target');
        targetHandler(msg(MessageType.Target, { player: 10, card: 500, isScriptChange: false }));
        mockSend.mockClear();

        const handler = getHandler('Untarget');
        handler(msg(MessageType.Untarget, { player: 10, card: 500, isScriptChange: false }));

        const state = lastState();
        expect(state.table.cards[0].targetedBy).toBeUndefined();
      });
    });

    describe('Peek', () => {
      it('adds player to peeking list', async () => {
        const service = await serviceWithState();
        addTableCard('600');
        mockSend.mockClear();

        const handler = getHandler('Peek');
        handler(msg(MessageType.Peek, { player: 10, card: 600 }));

        const state = lastState();
        expect(state.table.cards[0].peekingPlayers).toContain('10');
      });
    });

    describe('AddMarker', () => {
      it('adds a marker to a card', async () => {
        const service = await serviceWithState();
        addTableCard('700');
        mockSend.mockClear();

        const handler = getHandler('AddMarker');
        handler(msg(MessageType.AddMarker, {
          player: 10, card: 700, id: 'mark-1', name: 'Damage',
          count: 3, origCount: 0, isScriptChange: false,
        }));

        const state = lastState();
        expect(state.table.cards[0].markers).toHaveLength(1);
        expect(state.table.cards[0].markers[0]).toMatchObject({
          id: 'mark-1', name: 'Damage', count: 3,
        });
      });
    });

    describe('RemoveMarker', () => {
      it('removes a marker from a card', async () => {
        const service = await serviceWithState();
        addTableCard('700');
        // Add marker first
        const addHandler = getHandler('AddMarker');
        addHandler(msg(MessageType.AddMarker, {
          player: 10, card: 700, id: 'mark-1', name: 'Damage',
          count: 3, origCount: 0, isScriptChange: false,
        }));
        mockSend.mockClear();

        const handler = getHandler('RemoveMarker');
        handler(msg(MessageType.RemoveMarker, {
          player: 10, card: 700, id: 'mark-1', name: 'Damage',
          count: 3, origCount: 3, isScriptChange: false,
        }));

        const state = lastState();
        expect(state.table.cards[0].markers).toHaveLength(0);
      });
    });

    describe('SetCardProperty', () => {
      it('sets a custom property on a card', async () => {
        const service = await serviceWithState();
        addTableCard('800');
        mockSend.mockClear();

        const handler = getHandler('SetCardProperty');
        handler(msg(MessageType.SetCardProperty, {
          id: 800, player: 10, name: 'Power', val: '5', valtype: 'string',
        }));

        const state = lastState();
        expect(state.table.cards[0].properties.Power).toBe('5');
      });
    });

    describe('Highlight', () => {
      it('sets highlight color on a card', async () => {
        const service = await serviceWithState();
        addTableCard('900');
        mockSend.mockClear();

        const handler = getHandler('Highlight');
        handler(msg(MessageType.Highlight, { card: 900, color: '#ff0000' }));

        const state = lastState();
        expect(state.table.cards[0].highlighted).toBe('#ff0000');
      });
    });

    describe('Error', () => {
      it('adds a system message with the error text', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('Error');
        handler(msg(MessageType.Error, { msg: 'something went wrong' }));

        const state = lastState();
        expect(state.chatMessages).toHaveLength(1);
        expect(state.chatMessages[0]).toMatchObject({
          message: 'Error: something went wrong',
          isSystem: true,
        });
      });
    });

    describe('PlayerDisconnect', () => {
      it('adds a system message noting the disconnected player', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer(msg(MessageType.NewPlayer, { id: 10, nick: 'Eve', spectator: false }));
        mockSend.mockClear();

        const handler = getHandler('PlayerDisconnect');
        handler(msg(MessageType.PlayerDisconnect, { player: 10 }));

        const state = lastState();
        const lastMsg = state.chatMessages[state.chatMessages.length - 1];
        expect(lastMsg.message).toBe('Eve disconnected');
        expect(lastMsg.isSystem).toBe(true);
      });
    });

    describe('SetBoard', () => {
      it('sets the table board', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('SetBoard');
        handler(msg(MessageType.SetBoard, { player: 10, name: 'board.png' }));

        const state = lastState();
        expect(state.table.board).toBeDefined();
        expect(state.table.board.name).toBe('board.png');
      });
    });

    describe('GroupVis', () => {
      it('is registered as a handler', async () => {
        const service = await serviceWithState();
        expect(() => getHandler('GroupVis')).not.toThrow();
      });
    });

    describe('PlaySound', () => {
      it('adds a system message for sound', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('PlaySound');
        handler(msg(MessageType.PlaySound, { player: 10, name: 'coin.wav' }));

        const state = lastState();
        expect(state.chatMessages[0].message).toContain('coin.wav');
      });
    });
  });

  // -------------------------------------------------------------------------
  // Card resolver integration
  // -------------------------------------------------------------------------

  describe('card resolver', () => {
    it('uses resolved card name when creating cards', async () => {
      mockResolve.mockReturnValue({
        id: 'guid-a',
        name: 'Lightning Bolt',
        setId: 'set-1',
        properties: { Type: 'Instant', Cost: 'R' },
      });

      const service = await serviceWithState();
      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 10, nick: 'Alice', spectator: false }));
      mockSend.mockClear();

      const handler = getHandler('CreateCard');
      handler(msg(MessageType.CreateCard, {
        id: [100],
        type: ['guid-a'],
        size: ['100,140'],
        group: 5,
      }));

      const state = lastState();
      const player = state.players.find((p: any) => p.id === 10);
      const group = player.groups.find((g: any) => g.id === '5');
      expect(group.cards[0].name).toBe('Lightning Bolt');
      expect(group.cards[0].properties).toEqual({ Type: 'Instant', Cost: 'R' });

      mockResolve.mockReturnValue(undefined);
    });

    it('falls back to Card N when resolver returns undefined', async () => {
      mockResolve.mockReturnValue(undefined);

      const service = await serviceWithState();
      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 10, nick: 'Alice', spectator: false }));
      mockSend.mockClear();

      const handler = getHandler('CreateCard');
      handler(msg(MessageType.CreateCard, {
        id: [200],
        type: ['unknown-guid'],
        size: ['100,140'],
        group: 5,
      }));

      const state = lastState();
      const player = state.players.find((p: any) => p.id === 10);
      const group = player.groups.find((g: any) => g.id === '5');
      expect(group.cards[0].name).toBe('Card 200');
      expect(group.cards[0].properties).toEqual({});
    });

    it('uses resolved group name when creating groups', async () => {
      mockResolveGroupName.mockReturnValue('Library');

      const service = await serviceWithState();
      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 10, nick: 'Alice', spectator: false }));
      mockSend.mockClear();

      const handler = getHandler('CreateCard');
      handler(msg(MessageType.CreateCard, {
        id: [300],
        type: ['guid-x'],
        size: ['100,140'],
        group: 2,
      }));

      const state = lastState();
      const player = state.players.find((p: any) => p.id === 10);
      const group = player.groups.find((g: any) => g.id === '2');
      expect(group.name).toBe('Library');

      mockResolveGroupName.mockReturnValue(undefined);
    });
  });
});
