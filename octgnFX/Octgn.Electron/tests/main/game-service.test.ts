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

// Mock ScriptEngine — track initialize/loadGameScript calls
const mockScriptInitialize = vi.fn().mockResolvedValue({ success: true });
const mockLoadGameScript = vi.fn().mockResolvedValue({ success: true });
const mockGetScope = vi.fn().mockReturnValue(null);
const mockScriptIsInitialized = vi.fn().mockReturnValue(false);

vi.mock('@main/scripting/script-engine', () => ({
  ScriptEngine: vi.fn().mockImplementation(() => ({
    initialize: mockScriptInitialize,
    loadGameScript: mockLoadGameScript,
    getScope: mockGetScope,
    isInitialized: mockScriptIsInitialized,
    handleRemoteCall: vi.fn().mockReturnValue({ success: true }),
    getScriptEngine: vi.fn(),
  })),
}));

// Mock findGameDir for script loading
const mockFindGameDir = vi.fn().mockResolvedValue(null);
vi.mock('@main/games/game-store', () => ({
  findGameDir: (...args: unknown[]) => mockFindGameDir(...args),
}));

// Note: fs/promises readdir/readFile are NOT mocked here because vi.mock
// with importOriginal doesn't reliably intercept named imports already bound
// in the module under test. Script loading via readdir is tested at the
// integration level instead.

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

      it('includes the player color when player is known', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer(msg(MessageType.NewPlayer, {
          id: 5, nick: 'Alice', pkey: BigInt(0), inversedTable: false, spectator: false,
        }));
        mockSend.mockClear();

        const handler = getHandler('Print');
        handler(msg(MessageType.Print, { player: 5, text: 'Alice draws a card' }));

        const state = lastState();
        const printMsg = state.chatMessages[state.chatMessages.length - 1];
        expect(printMsg.message).toBe('Alice draws a card');
        expect(printMsg.isSystem).toBe(true);
        // Player 5 → index (5-1) % 14 = 4 → '#cc6600'
        expect(printMsg.color).toBe('#cc6600');
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

      it('sends Ready message with local player ID after Start', async () => {
        const service = await serviceWithState();
        mockSendMessage.mockClear();

        const start = getHandler('Start');
        start(msg(MessageType.Start, {}));

        // Should send Ready (type 90) with the local player's ID (42 from Welcome)
        expect(mockSendMessage).toHaveBeenCalledWith(
          MessageType.Ready,
          0,
          { player: 42 },
        );
      });

      it('sends Ready even if gameState was not initialized before Start', async () => {
        // Edge case: Start arrives without prior Welcome (shouldn't happen normally,
        // but the handler calls initializeGameState as a fallback)
        const service = await joinedService();
        mockSendMessage.mockClear();

        const start = getHandler('Start');
        start(msg(MessageType.Start, {}));

        // localPlayerId defaults to 0 if no Welcome was received
        expect(mockSendMessage).toHaveBeenCalledWith(
          MessageType.Ready,
          0,
          { player: 0 },
        );
      });
    });

    describe('Ready', () => {
      it('adds system message when a player signals ready', async () => {
        const service = await serviceWithState();
        // Add a player first
        const newPlayer = getHandler('NewPlayer');
        newPlayer(msg(MessageType.NewPlayer, {
          id: 5, nick: 'Alice', pkey: BigInt(0), inversedTable: false, spectator: false,
        }));
        mockSend.mockClear();

        const ready = getHandler('Ready');
        ready(msg(MessageType.Ready, { player: 5 }));

        const state = lastState();
        expect(state.chatMessages).toBeDefined();
        const readyMsg = state.chatMessages.find(
          (m: { message: string }) => m.message.includes('ready'),
        );
        expect(readyMsg).toBeDefined();
        expect(readyMsg.message).toContain('Alice');
      });

      it('includes the player color on the ready system message', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer(msg(MessageType.NewPlayer, {
          id: 5, nick: 'Alice', pkey: BigInt(0), inversedTable: false, spectator: false,
        }));
        mockSend.mockClear();

        const ready = getHandler('Ready');
        ready(msg(MessageType.Ready, { player: 5 }));

        const state = lastState();
        const readyMsg = state.chatMessages.find(
          (m: { message: string }) => m.message.includes('ready'),
        );
        expect(readyMsg).toBeDefined();
        // Player 5 → index (5-1) % 14 = 4 → '#cc6600' (dark orange)
        expect(readyMsg.color).toBe('#cc6600');
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

      it('includes the active player color on the turn system message', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('NextTurn');
        handler(msg(MessageType.NextTurn, { player: 42, setActive: true }));

        const state = lastState();
        const turnMsg = state.chatMessages.find(
          (m: { message: string }) => m.message.includes('Turn'),
        );
        expect(turnMsg).toBeDefined();
        // Player 42 → (42-1) % 14 = 41 % 14 = 13 → '#0000ff' (bright blue)
        expect(turnMsg.color).toBe('#0000ff');
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

    describe('Shuffled', () => {
      it('includes the player color on the shuffle system message', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer(msg(MessageType.NewPlayer, { id: 3, nick: 'Bob', spectator: false }));
        mockSend.mockClear();

        const handler = getHandler('Shuffled');
        handler(msg(MessageType.Shuffled, { player: 3, group: 100, card: [] }));

        const state = lastState();
        const shuffleMsg = state.chatMessages.find(
          (m: { message: string }) => m.message.includes('shuffled'),
        );
        expect(shuffleMsg).toBeDefined();
        // Player 3 → (3-1) % 14 = 2 → '#000080' (dark blue)
        expect(shuffleMsg.color).toBe('#000080');
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

      it('includes the player color on the disconnect message', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer(msg(MessageType.NewPlayer, { id: 10, nick: 'Eve', spectator: false }));
        mockSend.mockClear();

        const handler = getHandler('PlayerDisconnect');
        handler(msg(MessageType.PlayerDisconnect, { player: 10 }));

        const state = lastState();
        const lastMsg = state.chatMessages[state.chatMessages.length - 1];
        // Player 10 → (10-1) % 14 = 9 → '#ff0000' (bright red)
        expect(lastMsg.color).toBe('#ff0000');
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

  // -------------------------------------------------------------------------
  // Pre-game lobby: send methods
  // -------------------------------------------------------------------------

  describe('sendSettings', () => {
    it('sends a Settings message with the correct params', async () => {
      const service = await serviceWithState();
      mockSendMessage.mockClear();

      service.sendSettings(true, true, false, true);

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.Settings, 0, {
        twoSidedTable: true,
        allowSpectators: true,
        muteSpectators: false,
        allowCardList: true,
      });
    });
  });

  describe('sendPlayerSettings', () => {
    it('sends a PlayerSettings message with the correct params', async () => {
      const service = await serviceWithState();
      mockSendMessage.mockClear();

      service.sendPlayerSettings(42, true, false);

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.PlayerSettings, 0, {
        playerId: 42,
        invertedTable: true,
        spectator: false,
      });
    });
  });

  describe('bootPlayer', () => {
    it('sends a Boot message with player id and reason', async () => {
      const service = await serviceWithState();
      mockSendMessage.mockClear();

      service.bootPlayer(5, 'AFK');

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.Boot, 0, {
        player: 5,
        reason: 'AFK',
      });
    });

    it('uses empty reason by default', async () => {
      const service = await serviceWithState();
      mockSendMessage.mockClear();

      service.bootPlayer(5);

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.Boot, 0, {
        player: 5,
        reason: '',
      });
    });
  });

  describe('startGame', () => {
    it('sends a Start message', async () => {
      const service = await serviceWithState();
      mockSendMessage.mockClear();

      service.startGame();

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.Start, 0, {});
    });
  });

  // -------------------------------------------------------------------------
  // Pre-game lobby: host detection
  // -------------------------------------------------------------------------

  describe('host detection', () => {
    it('marks player with id 1 as host in NewPlayer', async () => {
      const service = await joinedService();
      const welcome = getHandler('Welcome');
      welcome(msg(MessageType.Welcome, { id: 2, gameSessionId: 'sess', gameName: 'G' }));

      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 1, nick: 'HostUser', spectator: false }));

      const state = lastState();
      const host = state.players.find((p: any) => p.id === 1);
      expect(host.isHost).toBe(true);
    });

    it('assigns player 1 dark green (#008000) matching WPF palette', async () => {
      const service = await joinedService();
      const welcome = getHandler('Welcome');
      welcome(msg(MessageType.Welcome, { id: 1, gameSessionId: 'sess', gameName: 'G' }));

      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 1, nick: 'P1', spectator: false }));

      const state = lastState();
      const p1 = state.players.find((p: any) => p.id === 1);
      expect(p1.color).toBe('#008000');
    });

    it('assigns player 2 dark red (#cc0000) matching WPF palette', async () => {
      const service = await joinedService();
      const welcome = getHandler('Welcome');
      welcome(msg(MessageType.Welcome, { id: 1, gameSessionId: 'sess', gameName: 'G' }));

      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 2, nick: 'P2', spectator: false }));

      const state = lastState();
      const p2 = state.players.find((p: any) => p.id === 2);
      expect(p2.color).toBe('#cc0000');
    });

    it('assigns player 3 dark blue (#000080) matching WPF palette', async () => {
      const service = await joinedService();
      const welcome = getHandler('Welcome');
      welcome(msg(MessageType.Welcome, { id: 1, gameSessionId: 'sess', gameName: 'G' }));

      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 3, nick: 'P3', spectator: false }));

      const state = lastState();
      const p3 = state.players.find((p: any) => p.id === 3);
      expect(p3.color).toBe('#000080');
    });

    it('assigns all 14 WPF palette colors correctly for players 1-14', async () => {
      const service = await joinedService();
      const welcome = getHandler('Welcome');
      welcome(msg(MessageType.Welcome, { id: 1, gameSessionId: 'sess', gameName: 'G' }));

      const expectedColors = [
        '#008000', // 1: dark green
        '#cc0000', // 2: dark red
        '#000080', // 3: dark blue
        '#800080', // 4: dark magenta
        '#cc6600', // 5: dark orange
        '#008080', // 6: dark cyan
        '#664b32', // 7: brown
        '#502060', // 8: dark purple
        '#808000', // 9: olive
        '#ff0000', // 10: bright red
        '#808080', // 11: gray
        '#206020', // 12: dark green
        '#ff00ff', // 13: magenta
        '#0000ff', // 14: bright blue
      ];

      const newPlayer = getHandler('NewPlayer');
      for (let i = 1; i <= 14; i++) {
        newPlayer(msg(MessageType.NewPlayer, { id: i, nick: `P${i}`, spectator: false }));
      }

      const state = lastState();
      for (let i = 1; i <= 14; i++) {
        const player = state.players.find((p: any) => p.id === i);
        expect(player.color).toBe(expectedColors[i - 1]);
      }
    });

    it('wraps around palette for player 15 (same as player 1)', async () => {
      const service = await joinedService();
      const welcome = getHandler('Welcome');
      welcome(msg(MessageType.Welcome, { id: 1, gameSessionId: 'sess', gameName: 'G' }));

      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 15, nick: 'P15', spectator: false }));

      const state = lastState();
      const p15 = state.players.find((p: any) => p.id === 15);
      // (15 - 1) % 14 = 0 → same as player 1
      expect(p15.color).toBe('#008000');
    });

    it('assigns black (#000000) for player ID 0 (global player)', async () => {
      const service = await joinedService();
      const welcome = getHandler('Welcome');
      welcome(msg(MessageType.Welcome, { id: 1, gameSessionId: 'sess', gameName: 'G' }));

      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 0, nick: 'Global', spectator: false }));

      const state = lastState();
      const p0 = state.players.find((p: any) => p.id === 0);
      expect(p0.color).toBe('#000000');
    });

    it('assigns black (#000000) for player ID 255 (spectator)', async () => {
      const service = await joinedService();
      const welcome = getHandler('Welcome');
      welcome(msg(MessageType.Welcome, { id: 1, gameSessionId: 'sess', gameName: 'G' }));

      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 255, nick: 'Spec', spectator: true }));

      const state = lastState();
      const p255 = state.players.find((p: any) => p.id === 255);
      expect(p255.color).toBe('#000000');
    });

    it('SetPlayerColor overrides the default color', async () => {
      const service = await serviceWithState();

      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 5, nick: 'Alice', spectator: false }));

      // Default should be dark orange
      let state = lastState();
      expect(state.players.find((p: any) => p.id === 5).color).toBe('#cc6600');

      // Override via protocol
      const setColor = getHandler('SetPlayerColor');
      setColor(msg(MessageType.SetPlayerColor, { player: 5, color: '#abcdef' }));

      state = lastState();
      expect(state.players.find((p: any) => p.id === 5).color).toBe('#abcdef');
    });

    it('does not mark non-id-1 players as host in NewPlayer', async () => {
      const service = await joinedService();
      const welcome = getHandler('Welcome');
      welcome(msg(MessageType.Welcome, { id: 1, gameSessionId: 'sess', gameName: 'G' }));

      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 3, nick: 'Guest', spectator: false }));

      const state = lastState();
      const guest = state.players.find((p: any) => p.id === 3);
      expect(guest.isHost).toBe(false);
    });
  });

  // -------------------------------------------------------------------------
  // Pre-game lobby: Settings handler stores lobby fields
  // -------------------------------------------------------------------------

  describe('Settings handler lobby fields', () => {
    it('stores allowSpectators, muteSpectators, and allowCardList', async () => {
      const service = await serviceWithState();

      const settings = getHandler('Settings');
      settings(msg(MessageType.Settings, {
        twoSidedTable: true,
        allowSpectators: true,
        muteSpectators: true,
        allowCardList: false,
      }));

      const state = lastState();
      expect(state.useTwoSidedTable).toBe(true);
      expect(state.allowSpectators).toBe(true);
      expect(state.muteSpectators).toBe(true);
      expect(state.allowCardList).toBe(false);
    });
  });

  // -------------------------------------------------------------------------
  // Chat messages include player color
  // -------------------------------------------------------------------------

  describe('chat message color', () => {
    it('includes the sending player color in chat messages', async () => {
      const service = await serviceWithState();

      // Add a player with a custom color
      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 7, nick: 'Red', spectator: false }));

      // Set that player's color
      const setColor = getHandler('SetPlayerColor');
      setColor(msg(MessageType.SetPlayerColor, { player: 7, color: '#ff0000' }));

      // Send a chat from that player
      const chat = getHandler('Chat');
      chat(msg(MessageType.Chat, { player: 7, text: 'hello' }));

      const state = lastState();
      const chatMsg = state.chatMessages.find((m: any) => m.message === 'hello');
      expect(chatMsg).toBeDefined();
      expect(chatMsg.color).toBe('#ff0000');
    });

    it('omits color when player has no custom color set', async () => {
      const service = await serviceWithState();

      // Chat from local player (id 42) who has default color
      const chat = getHandler('Chat');
      chat(msg(MessageType.Chat, { player: 42, text: 'hi' }));

      const state = lastState();
      const chatMsg = state.chatMessages.find((m: any) => m.message === 'hi');
      expect(chatMsg).toBeDefined();
      // Default color is '#3b82f6' — but no player with id=42 was added via NewPlayer,
      // so the player lookup returns undefined, and color will be undefined
      expect(chatMsg.color).toBeUndefined();
    });
  });

  // -------------------------------------------------------------------------
  // Default board from game definition (loaded after Welcome)
  // -------------------------------------------------------------------------

  describe('default board from game definition', () => {
    /**
     * Helper: join a game with a game definition that has boards defined.
     * Triggers Welcome and waits for the async loadGame → then() callback.
     */
    async function serviceWithGameDef(gameDef: Record<string, unknown>): Promise<GameService> {
      // Set up getGameDefinition to return the provided definition
      mockGetGameDefinition.mockReturnValue(gameDef);

      const service = await joinedService();
      const welcome = getHandler('Welcome');
      welcome(msg(MessageType.Welcome, { id: 1, gameSessionId: 'sess', gameName: 'Test' }));

      // Flush the microtask queue so the loadGame().then() callback runs
      await vi.waitFor(() => {
        expect(mockGetGameDefinition).toHaveBeenCalled();
      });

      return service;
    }

    it('sets the default board from game definition boards[] when no SetBoard received', async () => {
      await serviceWithGameDef({
        name: 'Chess',
        id: 'game-1',
        boards: [
          { name: 'default', source: 'boards/chess.png', x: -200, y: -150, width: 400, height: 300 },
        ],
      });

      const state = lastState();
      expect(state.table.board).toBeDefined();
      expect(state.table.board.imageUrl).toContain('octgn-asset://game-file/');
      expect(state.table.board.imageUrl).toContain(encodeURIComponent('boards/chess.png'));
      expect(state.table.board.width).toBe(400);
      expect(state.table.board.height).toBe(300);
      expect(state.table.board.x).toBe(-200);
      expect(state.table.board.y).toBe(-150);
    });

    it('sets the default board from table.board (legacy format) when no boards[] exists', async () => {
      await serviceWithGameDef({
        name: 'Legacy Game',
        id: 'game-2',
        table: {
          board: { name: 'Default', source: 'board.jpg', x: 0, y: 0, width: 500, height: 500 },
          width: 800,
          height: 600,
        },
      });

      const state = lastState();
      expect(state.table.board).toBeDefined();
      expect(state.table.board.imageUrl).toContain(encodeURIComponent('board.jpg'));
    });

    it('does NOT override board if SetBoard was already received', async () => {
      mockGetGameDefinition.mockReturnValue({
        name: 'Game',
        id: 'game-3',
        boards: [
          { name: 'default', source: 'boards/default.png', x: 0, y: 0, width: 100, height: 100 },
        ],
      });

      const service = await joinedService();
      const welcome = getHandler('Welcome');
      welcome(msg(MessageType.Welcome, { id: 1, gameSessionId: 'sess', gameName: 'Test' }));

      // SetBoard arrives before loadGame completes — set a board via protocol
      const setBoard = getHandler('SetBoard');
      setBoard(msg(MessageType.SetBoard, { player: 1, name: 'custom-board' }));

      // The SetBoard handler sets the board (with fallback since def not loaded yet)
      const stateAfterSetBoard = lastState();
      expect(stateAfterSetBoard.table.board).toBeDefined();
      expect(stateAfterSetBoard.table.board.name).toBe('custom-board');

      // Flush the microtask queue for loadGame().then()
      await vi.waitFor(() => {
        expect(mockGetGameDefinition).toHaveBeenCalled();
      });

      // After definition loads, the board should have been re-resolved from pendingBoardName
      // (via the pending board mechanism), NOT replaced by the default board
      const stateAfterLoad = lastState();
      expect(stateAfterLoad.table.board).toBeDefined();
      // The board name should still be 'custom-board' (re-resolved, not replaced)
      expect(stateAfterLoad.table.board.name).toBe('custom-board');
    });

    it('does NOT set board when game definition has no boards', async () => {
      await serviceWithGameDef({
        name: 'No Board Game',
        id: 'game-4',
      });

      const state = lastState();
      expect(state.table.board).toBeUndefined();
    });

    it('does NOT set board when board definition has no source', async () => {
      await serviceWithGameDef({
        name: 'Empty Board',
        id: 'game-5',
        boards: [
          { name: 'default', source: '', x: 0, y: 0, width: 100, height: 100 },
        ],
      });

      const state = lastState();
      expect(state.table.board).toBeUndefined();
    });
  });

  // -------------------------------------------------------------------------
  // loadDeckFromFile
  // -------------------------------------------------------------------------

  describe('loadDeckFromFile', () => {
    /**
     * Helper: create a service with state AND a local player with groups.
     */
    async function serviceWithPlayerGroups(
      groups: { id: string; name: string }[] = [],
    ): Promise<GameService> {
      const service = await serviceWithState();
      // Add local player via NewPlayer
      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 42, nick: 'Nick', spectator: false }));
      // Set up groups on the local player
      const state = lastState();
      const localPlayer = state.players.find((p: any) => p.id === 42);
      if (localPlayer) {
        localPlayer.groups = groups.map((g) => ({
          id: g.id,
          name: g.name,
          cards: [],
          visibility: 2,
          controller: 42,
        }));
      }
      return service;
    }

    it('generates card IDs using (playerId << 16) | counter', async () => {
      const service = await serviceWithPlayerGroups();

      service.loadDeckFromFile({
        gameId: 'gid',
        sections: [
          {
            name: 'Hand',
            cards: [
              { id: 'card-a', name: 'Card A', quantity: 2, properties: {} },
            ],
          },
        ],
      });

      expect(mockSendMessage).toHaveBeenCalledWith(
        MessageType.LoadDeck,
        0,
        expect.objectContaining({
          id: [(42 << 16) | 1, (42 << 16) | 2],
          type: ['card-a', 'card-a'],
        }),
      );
    });

    it('expands cards by quantity', async () => {
      const service = await serviceWithPlayerGroups();

      service.loadDeckFromFile({
        gameId: 'gid',
        sections: [
          {
            name: 'Hand',
            cards: [
              { id: 'c1', name: 'One', quantity: 3, properties: {} },
            ],
          },
        ],
      });

      const call = mockSendMessage.mock.calls.find(
        (c) => c[0] === MessageType.LoadDeck,
      );
      expect(call).toBeDefined();
      expect(call![2].id).toHaveLength(3);
      expect(call![2].type).toEqual(['c1', 'c1', 'c1']);
    });

    it('maps sections to group IDs using game definition deckSections', async () => {
      mockGetGameDefinition.mockReturnValue({
        id: 'gid',
        name: 'Test',
        players: [{
          name: 'Player',
          groups: [
            { name: 'Hand', visibility: 2, ordered: false, cardActions: [], groupActions: [] },
            { name: 'Library', visibility: 1, ordered: false, cardActions: [], groupActions: [] },
          ],
          counters: [],
          globalVariables: [],
        }],
        deckSections: [
          { name: 'Main Deck', group: 'Library', shared: false },
          { name: 'Hand Cards', group: 'Hand', shared: false },
        ],
        sharedDeckSections: [],
      });

      const handGroupId = 0x01000000 | (42 << 16) | 1;
      const libraryGroupId = 0x01000000 | (42 << 16) | 2;
      const service = await serviceWithPlayerGroups([
        { id: String(handGroupId), name: 'Hand' },
        { id: String(libraryGroupId), name: 'Library' },
      ]);

      service.loadDeckFromFile({
        gameId: 'gid',
        sections: [
          { name: 'Main Deck', cards: [{ id: 'c1', name: 'Card', quantity: 1, properties: {} }] },
          { name: 'Hand Cards', cards: [{ id: 'c2', name: 'Card', quantity: 1, properties: {} }] },
        ],
      });

      const call = mockSendMessage.mock.calls.find(
        (c) => c[0] === MessageType.LoadDeck,
      );
      expect(call).toBeDefined();
      // "Main Deck" → group "Library" → libraryGroupId
      // "Hand Cards" → group "Hand" → handGroupId
      expect(call![2].group).toEqual([
        libraryGroupId,
        handGroupId,
      ]);
    });

    it('falls back to matching section name directly to group name', async () => {
      // No game definition loaded
      mockGetGameDefinition.mockReturnValue(undefined);

      const service = await serviceWithPlayerGroups([
        { id: '100', name: 'Hand' },
        { id: '200', name: 'Deck' },
      ]);

      service.loadDeckFromFile({
        gameId: 'gid',
        sections: [
          { name: 'Hand', cards: [{ id: 'c1', name: 'Card', quantity: 1, properties: {} }] },
          { name: 'Deck', cards: [{ id: 'c2', name: 'Card', quantity: 1, properties: {} }] },
        ],
      });

      const call = mockSendMessage.mock.calls.find(
        (c) => c[0] === MessageType.LoadDeck,
      );
      expect(call).toBeDefined();
      expect(call![2].group).toEqual([100, 200]);
    });

    it('does not send LoadDeck for empty deck', async () => {
      const service = await serviceWithPlayerGroups();

      service.loadDeckFromFile({
        gameId: 'gid',
        sections: [],
      });

      const call = mockSendMessage.mock.calls.find(
        (c) => c[0] === MessageType.LoadDeck,
      );
      expect(call).toBeUndefined();
    });

    it('passes sleeve URL from deck', async () => {
      const service = await serviceWithPlayerGroups();

      service.loadDeckFromFile({
        gameId: 'gid',
        sections: [
          { name: 'Hand', cards: [{ id: 'c1', name: 'Card', quantity: 1, properties: {} }] },
        ],
        sleeveUrl: 'http://example.com/sleeve.png',
      });

      const call = mockSendMessage.mock.calls.find(
        (c) => c[0] === MessageType.LoadDeck,
      );
      expect(call).toBeDefined();
      expect(call![2].sleeve).toBe('http://example.com/sleeve.png');
    });

    it('uses WPF-compatible group ID encoding: 0x01000000 | (playerId << 16) | (groupIndex + 1)', async () => {
      // When falling back to game definition for group resolution (no groups in state),
      // the group ID must match WPF format: 0x01000000 | (Owner.Id << 16) | Def.Id
      mockGetGameDefinition.mockReturnValue({
        id: 'gid',
        name: 'Test',
        players: [{
          name: 'Player',
          groups: [
            { name: 'Hand', visibility: 2, ordered: false, cardActions: [], groupActions: [] },
            { name: 'Library', visibility: 1, ordered: false, cardActions: [], groupActions: [] },
          ],
          counters: [],
          globalVariables: [],
        }],
        deckSections: [
          { name: 'Main Deck', group: 'Library', shared: false },
        ],
        sharedDeckSections: [],
      });

      // No pre-existing groups in player state — forces fallback to game definition
      const service = await serviceWithPlayerGroups([]);

      service.loadDeckFromFile({
        gameId: 'gid',
        sections: [
          { name: 'Main Deck', cards: [{ id: 'c1', name: 'Card', quantity: 1, properties: {} }] },
        ],
      });

      const call = mockSendMessage.mock.calls.find(
        (c) => c[0] === MessageType.LoadDeck,
      );
      expect(call).toBeDefined();
      // Library is group index 1 (0-based), so Def.Id = 2 (1-based)
      // playerId = 42, so group ID = 0x01000000 | (42 << 16) | 2
      const expectedGroupId = 0x01000000 | (42 << 16) | 2;
      expect(call![2].group).toEqual([expectedGroupId]);
    });

    it('handles shared deck sections mapped to global player groups', async () => {
      mockGetGameDefinition.mockReturnValue({
        id: 'gid',
        name: 'Test',
        players: [{
          name: 'Player',
          groups: [
            { name: 'Hand', visibility: 2, ordered: false, cardActions: [], groupActions: [] },
          ],
          counters: [],
          globalVariables: [],
        }],
        deckSections: [
          { name: 'Main Deck', group: 'Hand', shared: false },
        ],
        sharedDeckSections: [
          { name: 'Shared Tokens', group: 'Tokens', shared: true },
        ],
        globalPlayer: {
          groups: [
            { name: 'Tokens', visibility: 0, ordered: false, cardActions: [], groupActions: [] },
          ],
        },
      });

      const service = await serviceWithPlayerGroups([
        { id: String(0x01000000 | (42 << 16) | 1), name: 'Hand' },
      ]);

      service.loadDeckFromFile({
        gameId: 'gid',
        sections: [
          { name: 'Main Deck', cards: [{ id: 'c1', name: 'Card', quantity: 1, properties: {} }] },
          { name: 'Shared Tokens', cards: [{ id: 'c2', name: 'Token', quantity: 1, properties: {} }] },
        ],
      });

      const call = mockSendMessage.mock.calls.find(
        (c) => c[0] === MessageType.LoadDeck,
      );
      expect(call).toBeDefined();
      // Main Deck → Hand group of local player
      // Shared Tokens → global player group (player 0), Tokens at index 0, Def.Id = 1
      const handGroupId = 0x01000000 | (42 << 16) | 1;
      const sharedGroupId = 0x01000000 | (0 << 16) | 1; // global player = 0
      expect(call![2].group).toEqual([handGroupId, sharedGroupId]);
    });

    it('uses deck section shared flag to route to global player even when name matches a player group', async () => {
      // Game has a player group "Library" AND a global group "Library"
      // The deck file's shared flag determines which one to use
      mockGetGameDefinition.mockReturnValue({
        id: 'gid',
        name: 'Test',
        players: [{
          name: 'Player',
          groups: [
            { name: 'Hand', visibility: 2, ordered: false, cardActions: [], groupActions: [] },
            { name: 'Library', visibility: 1, ordered: false, cardActions: [], groupActions: [] },
          ],
          counters: [],
          globalVariables: [],
        }],
        deckSections: [
          { name: 'Player Cards', group: 'Library', shared: false },
        ],
        sharedDeckSections: [
          { name: 'Shared Cards', group: 'Library', shared: true },
        ],
        globalPlayer: {
          groups: [
            { name: 'Library', visibility: 0, ordered: false, cardActions: [], groupActions: [] },
          ],
        },
      });

      const service = await serviceWithPlayerGroups([
        { id: String(0x01000000 | (42 << 16) | 1), name: 'Hand' },
        { id: String(0x01000000 | (42 << 16) | 2), name: 'Library' },
      ]);

      service.loadDeckFromFile({
        gameId: 'gid',
        sections: [
          // This section is marked shared in the deck file — should go to global player
          { name: 'Shared Cards', cards: [{ id: 'c1', name: 'Token', quantity: 1, properties: {} }], shared: true },
          // This section is NOT shared — should go to local player
          { name: 'Player Cards', cards: [{ id: 'c2', name: 'Card', quantity: 1, properties: {} }], shared: false },
        ],
      });

      const call = mockSendMessage.mock.calls.find(
        (c) => c[0] === MessageType.LoadDeck,
      );
      expect(call).toBeDefined();
      const globalLibraryId = 0x01000000 | (0 << 16) | 1; // global player, group 1
      const playerLibraryId = 0x01000000 | (42 << 16) | 2; // local player, group 2
      expect(call![2].group).toEqual([globalLibraryId, playerLibraryId]);
    });
  });

  // -------------------------------------------------------------------------
  // LoadDeck handler (receiving cards from server)
  // -------------------------------------------------------------------------

  describe('LoadDeck handler', () => {
    it('adds cards to the correct player group by decoding player from group ID', async () => {
      const service = await serviceWithState();
      // Add a player with a group
      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 1, nick: 'Host', spectator: false }));

      // Manually set up a group on that player matching WPF group ID
      const state = lastState();
      const player = state.players.find((p: any) => p.id === 1);
      const groupId = 0x01000000 | (1 << 16) | 1; // player 1, group def id 1
      player.groups = [{
        id: String(groupId),
        name: 'Hand',
        cards: [],
        visibility: 2,
        controller: 1,
      }];

      // Trigger LoadDeck handler with that group ID
      const loadDeck = getHandler('LoadDeck');
      loadDeck(msg(MessageType.LoadDeck, {
        id: [0x00010001],
        type: ['def-1'],
        group: [groupId],
        size: [''],
      }));

      const finalState = lastState();
      const p = finalState.players.find((p: any) => p.id === 1);
      const g = p.groups.find((g: any) => g.id === String(groupId));
      expect(g).toBeDefined();
      expect(g.cards).toHaveLength(1);
    });

    it('creates group under correct player when group does not exist yet', async () => {
      const service = await serviceWithState();
      // Add two players
      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 1, nick: 'Host', spectator: false }));
      newPlayer(msg(MessageType.NewPlayer, { id: 2, nick: 'Guest', spectator: false }));

      // Trigger LoadDeck with a group ID encoding player 2
      const loadDeck = getHandler('LoadDeck');
      const groupIdForPlayer2 = 0x01000000 | (2 << 16) | 1;
      loadDeck(msg(MessageType.LoadDeck, {
        id: [0x00020001],
        type: ['def-1'],
        group: [groupIdForPlayer2],
        size: [''],
      }));

      const finalState = lastState();
      // Card should be on player 2, not player 1
      const player2 = finalState.players.find((p: any) => p.id === 2);
      expect(player2).toBeDefined();
      const group = player2.groups.find((g: any) => g.id === String(groupIdForPlayer2));
      expect(group).toBeDefined();
      expect(group.cards).toHaveLength(1);

      // Player 1 should NOT have this group
      const player1 = finalState.players.find((p: any) => p.id === 1);
      expect(player1.groups.find((g: any) => g.id === String(groupIdForPlayer2))).toBeUndefined();
    });

    it('treats 0x01000000 as table group', async () => {
      const service = await serviceWithState();
      const newPlayer = getHandler('NewPlayer');
      newPlayer(msg(MessageType.NewPlayer, { id: 1, nick: 'Host', spectator: false }));

      const loadDeck = getHandler('LoadDeck');
      const tableGroupId = 0x01000000; // special table ID
      loadDeck(msg(MessageType.LoadDeck, {
        id: [0x00010001],
        type: ['def-1'],
        group: [tableGroupId],
        size: [''],
      }));

      const finalState = lastState();
      expect(finalState.table.cards).toHaveLength(1);
    });
  });

  // -------------------------------------------------------------------------
  // ScriptEngine initialization
  // -------------------------------------------------------------------------

  describe('ScriptEngine initialization', () => {
    const fakeGameDef = {
      id: 'game-1',
      name: 'Test Game',
      version: '1.0',
      description: '',
      cardWidth: 63,
      cardHeight: 88,
      cardBack: '',
      deckSections: [],
      sharedDeckSections: [],
      players: [{
        name: 'Player',
        groups: [{
          name: 'Hand',
          visibility: 2,
          ordered: false,
          cardActions: [],
          groupActions: [],
        }],
        counters: [],
        globalVariables: [],
      }],
      globalVariables: [],
      phases: [],
      table: {
        name: 'Table',
        width: 800,
        height: 600,
        cardActions: [],
        groupActions: [],
      },
      scriptVersion: '3.1.0.2',
    };

    it('initializes ScriptEngine when game definition loads', async () => {
      // Setup: mockGetGameDefinition returns a valid def after loadGame resolves
      mockScriptInitialize.mockResolvedValue({ success: true });
      mockGetGameDefinition.mockReturnValue(fakeGameDef);

      const service = await serviceWithState();

      // Wait for the async loadGame().then() to resolve
      await vi.waitFor(() => {
        expect(mockScriptInitialize).toHaveBeenCalledTimes(1);
      });

      // Verify initialize was called with the game definition and proper deps
      const [gameDef, deps] = mockScriptInitialize.mock.calls[0];
      expect(gameDef).toBe(fakeGameDef);
      expect(deps).toHaveProperty('getGameState');
      expect(deps).toHaveProperty('getLocalPlayerId');
      expect(deps).toHaveProperty('getGameDefinition');
      expect(deps).toHaveProperty('sendProtocolMessage');
      expect(deps).toHaveProperty('addChatMessage');
    });

    it('looks for game scripts directory after initialization', async () => {
      mockScriptInitialize.mockResolvedValue({ success: true });
      mockGetGameDefinition.mockReturnValue(fakeGameDef);
      mockFindGameDir.mockResolvedValue(null); // No game dir found

      const service = await serviceWithState();

      // Wait for findGameDir to be called — confirms the script loading path runs
      await vi.waitFor(() => {
        expect(mockFindGameDir).toHaveBeenCalled();
      });

      // findGameDir should be called with the current game ID
      expect(mockFindGameDir).toHaveBeenCalledWith('gid');
    });

    it('does not crash when game directory is not found', async () => {
      mockScriptInitialize.mockResolvedValue({ success: true });
      mockGetGameDefinition.mockReturnValue(fakeGameDef);
      mockFindGameDir.mockResolvedValue(null); // No game dir

      const service = await serviceWithState();

      // Wait for async init
      await vi.waitFor(() => {
        expect(mockScriptInitialize).toHaveBeenCalledTimes(1);
      });

      // ScriptEngine should still be initialized even if game dir not found
      expect(mockScriptInitialize).toHaveBeenCalledTimes(1);
      // loadGameScript should NOT be called since there's no game dir
      expect(mockLoadGameScript).not.toHaveBeenCalled();
    });

    it('getActionExecutor returns an executor after initialization', async () => {
      // After initialize, getScope should return a valid scope
      mockScriptInitialize.mockResolvedValue({ success: true });
      const fakeScope = { execute: vi.fn() };
      mockGetScope.mockReturnValue(fakeScope);
      mockScriptIsInitialized.mockReturnValue(true);
      mockGetGameDefinition.mockReturnValue(fakeGameDef);

      const service = await serviceWithState();

      await vi.waitFor(() => {
        expect(mockScriptInitialize).toHaveBeenCalledTimes(1);
      });

      const executor = service.getActionExecutor();
      expect(executor).not.toBeNull();
    });
  });
});
