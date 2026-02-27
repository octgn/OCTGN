import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { GameService } from '@main/api/game-service';
import { MessageType } from '@main/protocol/types';

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

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/**
 * Extract the handler registered for a given event name via connection.on().
 */
function getHandler(eventName: string): (params: Record<string, unknown>) => void {
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
  welcome({ id: 42, gameSessionId: 'sess-1', gameName: 'Test Game' });
  return service;
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
        clientVer: '0.1.0',
        octgnVer: '3.4.424.0',
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

        welcome({ id: 7, gameSessionId: 'sess-x', gameName: 'My Game' });

        // State was broadcast
        expect(mockSend).toHaveBeenCalled();
        const state = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        expect(state.localPlayerId).toBe(7);
        expect(state.gameId).toBe('sess-x');
        expect(state.gameName).toBe('My Game');
        expect(state.players).toEqual([]);
        expect(state.turnNumber).toBe(0);
        expect(state.isStarted).toBe(false);
      });
    });

    describe('NewPlayer', () => {
      it('adds a player to the players array', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('NewPlayer');
        handler({ id: 10, nick: 'Bob', spectator: false });

        const state = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        expect(state.players).toHaveLength(1);
        expect(state.players[0]).toMatchObject({
          id: 10,
          name: 'Bob',
          isSpectator: false,
        });
      });
    });

    describe('Leave', () => {
      it('removes a player from the players array', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer({ id: 10, nick: 'Bob', spectator: false });
        newPlayer({ id: 11, nick: 'Carol', spectator: false });
        mockSend.mockClear();

        const leave = getHandler('Leave');
        leave({ player: 10 });

        const state = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        expect(state.players).toHaveLength(1);
        expect(state.players[0].id).toBe(11);
      });
    });

    describe('Chat', () => {
      it('adds a chat message to chatMessages', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer({ id: 10, nick: 'Alice', spectator: false });
        mockSend.mockClear();

        const chat = getHandler('Chat');
        chat({ player: 10, text: 'hi there' });

        const state = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        expect(state.chatMessages).toHaveLength(1);
        expect(state.chatMessages[0]).toMatchObject({
          playerId: 10,
          playerName: 'Alice',
          message: 'hi there',
          isSystem: false,
        });
      });
    });

    describe('Start', () => {
      it('sets isStarted to true', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const start = getHandler('Start');
        start({});

        const state = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        expect(state.isStarted).toBe(true);
      });
    });

    describe('NextTurn', () => {
      it('increments turn and sets active player when setActive is true', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('NextTurn');
        handler({ player: 42, setActive: true });

        const state = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        expect(state.turnNumber).toBe(1);
        expect(state.activePlayer).toBe(42);
      });

      it('increments turn without changing active player when setActive is false', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('NextTurn');
        handler({ player: 99, setActive: false });

        const state = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        expect(state.turnNumber).toBe(1);
        expect(state.activePlayer).toBe(0); // unchanged from initial
      });
    });

    describe('Counter', () => {
      it('updates counter value for the correct player', async () => {
        const service = await serviceWithState();
        const newPlayer = getHandler('NewPlayer');
        newPlayer({ id: 10, nick: 'Alice', spectator: false });

        // Manually add a counter to the player so it can be found
        const state1 = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        state1.players[0].counters.push({ id: 5, name: 'Life', value: 20 });

        mockSend.mockClear();

        const counter = getHandler('Counter');
        counter({ player: 10, counter: 5, value: 15 });

        const state2 = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        expect(state2.players[0].counters[0].value).toBe(15);
      });
    });

    describe('MoveCard', () => {
      it('updates card group and faceUp status', async () => {
        const service = await serviceWithState();

        // Add a card to the table
        const state = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        state.table.cards.push({
          id: '100',
          definitionId: 'd1',
          name: 'Card A',
          imageUrl: '',
          faceUp: false,
          position: { x: 0, y: 0 },
          rotation: 0,
          groupId: '1',
          ownerId: '42',
          markers: [],
          properties: {},
          peekingPlayers: [],
          size: { width: 100, height: 140 },
        });

        mockSend.mockClear();

        const handler = getHandler('MoveCard');
        handler({ id: [100], group: 7, faceUp: [true] });

        const state2 = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        expect(state2.table.cards[0].groupId).toBe('7');
        expect(state2.table.cards[0].faceUp).toBe(true);
      });
    });

    describe('MoveCardAt', () => {
      it('updates card position and faceUp', async () => {
        const service = await serviceWithState();

        const state = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        state.table.cards.push({
          id: '200',
          definitionId: 'd2',
          name: 'Card B',
          imageUrl: '',
          faceUp: false,
          position: { x: 0, y: 0 },
          rotation: 0,
          groupId: '1',
          ownerId: '42',
          markers: [],
          properties: {},
          peekingPlayers: [],
          size: { width: 100, height: 140 },
        });

        mockSend.mockClear();

        const handler = getHandler('MoveCardAt');
        handler({ id: [200], x: [50], y: [75], faceUp: [true] });

        const state2 = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        expect(state2.table.cards[0].position).toEqual({ x: 50, y: 75 });
        expect(state2.table.cards[0].faceUp).toBe(true);
      });
    });

    describe('Turn', () => {
      it('updates card faceUp property', async () => {
        const service = await serviceWithState();

        const state = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        state.table.cards.push({
          id: '300',
          definitionId: 'd3',
          name: 'Card C',
          imageUrl: '',
          faceUp: false,
          position: { x: 0, y: 0 },
          rotation: 0,
          groupId: '1',
          ownerId: '42',
          markers: [],
          properties: {},
          peekingPlayers: [],
          size: { width: 100, height: 140 },
        });

        mockSend.mockClear();

        const handler = getHandler('Turn');
        handler({ card: 300, up: true });

        const state2 = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        expect(state2.table.cards[0].faceUp).toBe(true);
      });
    });

    describe('Rotate', () => {
      it('updates card rotation (multiplied by 90)', async () => {
        const service = await serviceWithState();

        const state = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        state.table.cards.push({
          id: '400',
          definitionId: 'd4',
          name: 'Card D',
          imageUrl: '',
          faceUp: true,
          position: { x: 0, y: 0 },
          rotation: 0,
          groupId: '1',
          ownerId: '42',
          markers: [],
          properties: {},
          peekingPlayers: [],
          size: { width: 100, height: 140 },
        });

        mockSend.mockClear();

        const handler = getHandler('Rotate');
        handler({ card: 400, rot: 2 });

        const state2 = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        expect(state2.table.cards[0].rotation).toBe(180);
      });
    });

    describe('Error', () => {
      it('adds a system message with the error text', async () => {
        const service = await serviceWithState();
        mockSend.mockClear();

        const handler = getHandler('Error');
        handler({ msg: 'something went wrong' });

        const state = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
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
        newPlayer({ id: 10, nick: 'Eve', spectator: false });
        mockSend.mockClear();

        const handler = getHandler('PlayerDisconnect');
        handler({ player: 10 });

        const state = mockSend.mock.calls[mockSend.mock.calls.length - 1][1];
        const lastMsg = state.chatMessages[state.chatMessages.length - 1];
        expect(lastMsg.message).toBe('Eve disconnected');
        expect(lastMsg.isSystem).toBe(true);
      });
    });
  });
});
