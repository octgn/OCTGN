import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { GameService } from '@main/api/game-service';
import { ScriptEngine } from '@main/scripting/script-engine';
import { MessageType } from '@main/protocol/types';

// ---------------------------------------------------------------------------
// Mocks
// ---------------------------------------------------------------------------

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

function getHandler(eventName: string): (params: Record<string, unknown>) => void {
  const call = mockOn.mock.calls.find((c) => c[0] === eventName);
  if (!call) throw new Error(`No handler registered for event '${eventName}'`);
  return call[1];
}

function getLatestState() {
  const calls = mockSend.mock.calls.filter((c) => c[0] === 'game:state-update');
  if (calls.length === 0) throw new Error('No state broadcasts found');
  return calls[calls.length - 1][1];
}

async function joinedService(): Promise<GameService> {
  const service = new GameService();
  await service.joinGame('localhost', 1234, 'Nick', 'uid', 'gid', '1.0');
  return service;
}

async function serviceWithState(): Promise<GameService> {
  const service = await joinedService();
  const welcome = getHandler('Welcome');
  welcome({ id: 42, gameSessionId: 'sess-1', gameName: 'Test Game' });
  return service;
}

function addCardToState(cardId: string, opts: Partial<{ faceUp: boolean; groupId: string; x: number; y: number }> = {}) {
  const state = getLatestState();
  state.table.cards.push({
    id: cardId,
    definitionId: `def-${cardId}`,
    name: `Card ${cardId}`,
    imageUrl: '',
    faceUp: opts.faceUp ?? false,
    position: { x: opts.x ?? 0, y: opts.y ?? 0 },
    rotation: 0,
    groupId: opts.groupId ?? '1',
    ownerId: '42',
    markers: [],
    properties: {},
    peekingPlayers: [],
    size: { width: 100, height: 140 },
  });
}

// ---------------------------------------------------------------------------
// Integration Tests
// ---------------------------------------------------------------------------

describe('Integration: GameService + ScriptEngine', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockIsConnected = false;
  });

  // -------------------------------------------------------------------------
  // Full game lifecycle: join -> welcome -> players -> chat -> leave
  // -------------------------------------------------------------------------

  describe('full game lifecycle flow', () => {
    it('progresses through join -> welcome -> new players -> start -> chat -> leave', async () => {
      // 1. Join the game
      const service = new GameService();
      const joinResult = await service.joinGame(
        'localhost', 9000, 'Alice', 'user-1', 'game-1', '1.0.0',
      );
      expect(joinResult).toEqual({ success: true });
      expect(mockConnect).toHaveBeenCalled();
      expect(mockSendMessage).toHaveBeenCalledWith(
        MessageType.Hello,
        0,
        expect.objectContaining({ nick: 'Alice', userId: 'user-1' }),
      );

      // 2. Receive Welcome - assigns local player ID
      const welcome = getHandler('Welcome');
      welcome({ id: 1, gameSessionId: 'sess-abc', gameName: 'My Game' });

      let state = getLatestState();
      expect(state.localPlayerId).toBe(1);
      expect(state.gameId).toBe('sess-abc');
      expect(state.gameName).toBe('My Game');
      expect(state.players).toEqual([]);
      expect(state.isStarted).toBe(false);

      // 3. Two players join
      const newPlayer = getHandler('NewPlayer');
      newPlayer({ id: 1, nick: 'Alice', spectator: false });
      newPlayer({ id: 2, nick: 'Bob', spectator: false });

      state = getLatestState();
      expect(state.players).toHaveLength(2);
      expect(state.players[0]).toMatchObject({ id: 1, name: 'Alice', isSpectator: false });
      expect(state.players[1]).toMatchObject({ id: 2, name: 'Bob', isSpectator: false });

      // 4. Game starts
      const start = getHandler('Start');
      start({});

      state = getLatestState();
      expect(state.isStarted).toBe(true);

      // 5. Chat messages exchanged
      const chat = getHandler('Chat');
      chat({ player: 1, text: 'Hello everyone!' });
      chat({ player: 2, text: 'Hi Alice!' });

      state = getLatestState();
      expect(state.chatMessages).toHaveLength(2);
      expect(state.chatMessages[0]).toMatchObject({
        playerId: 1,
        playerName: 'Alice',
        message: 'Hello everyone!',
        isSystem: false,
      });
      expect(state.chatMessages[1]).toMatchObject({
        playerId: 2,
        playerName: 'Bob',
        message: 'Hi Alice!',
        isSystem: false,
      });

      // 6. Bob leaves
      const leave = getHandler('Leave');
      leave({ player: 2 });

      state = getLatestState();
      expect(state.players).toHaveLength(1);
      expect(state.players[0].id).toBe(1);

      // 7. Alice leaves the game
      mockSendMessage.mockClear();
      await service.leaveGame();

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.Leave, 0, { player: 1 });
      expect(mockDisconnect).toHaveBeenCalled();
    });

    it('handles player disconnect with system message', async () => {
      const service = await serviceWithState();
      const newPlayer = getHandler('NewPlayer');
      newPlayer({ id: 10, nick: 'DisconnectingUser', spectator: false });
      mockSend.mockClear();

      const handler = getHandler('PlayerDisconnect');
      handler({ player: 10 });

      const state = getLatestState();
      const lastMsg = state.chatMessages[state.chatMessages.length - 1];
      expect(lastMsg.message).toBe('DisconnectingUser disconnected');
      expect(lastMsg.isSystem).toBe(true);
    });

    it('handles spectator joining', async () => {
      const service = await serviceWithState();
      const newPlayer = getHandler('NewPlayer');
      newPlayer({ id: 99, nick: 'Spectator1', spectator: true });

      const state = getLatestState();
      expect(state.players).toHaveLength(1);
      expect(state.players[0]).toMatchObject({
        id: 99,
        name: 'Spectator1',
        isSpectator: true,
      });
    });
  });

  // -------------------------------------------------------------------------
  // Card operations: move, flip, rotate -> state update -> broadcast
  // -------------------------------------------------------------------------

  describe('card operations flow', () => {
    it('moves a card to a group and updates state on MoveCard handler', async () => {
      const service = await serviceWithState();
      addCardToState('500', { groupId: '1', faceUp: false });
      mockSend.mockClear();

      // Simulate server response with MoveCard
      const moveCard = getHandler('MoveCard');
      moveCard({ id: [500], group: 3, faceUp: [true] });

      const state = getLatestState();
      expect(state.table.cards[0].groupId).toBe('3');
      expect(state.table.cards[0].faceUp).toBe(true);
    });

    it('moves a card to a position and updates state on MoveCardAt handler', async () => {
      const service = await serviceWithState();
      addCardToState('600', { x: 0, y: 0, faceUp: false });
      mockSend.mockClear();

      const moveCardAt = getHandler('MoveCardAt');
      moveCardAt({ id: [600], x: [150], y: [250], faceUp: [true] });

      const state = getLatestState();
      expect(state.table.cards[0].position).toEqual({ x: 150, y: 250 });
      expect(state.table.cards[0].faceUp).toBe(true);
    });

    it('flips a card and updates faceUp in state', async () => {
      const service = await serviceWithState();
      addCardToState('700', { faceUp: false });
      mockSend.mockClear();

      const turn = getHandler('Turn');
      turn({ card: 700, up: true });

      const state = getLatestState();
      expect(state.table.cards[0].faceUp).toBe(true);
    });

    it('rotates a card and updates rotation in state (multiplied by 90)', async () => {
      const service = await serviceWithState();
      addCardToState('800');
      mockSend.mockClear();

      const rotate = getHandler('Rotate');
      rotate({ card: 800, rot: 3 });

      const state = getLatestState();
      expect(state.table.cards[0].rotation).toBe(270);
    });

    it('highlights a card with a color', async () => {
      const service = await serviceWithState();
      addCardToState('900');
      mockSend.mockClear();

      const highlight = getHandler('Highlight');
      highlight({ card: 900, color: '#ff0000' });

      const state = getLatestState();
      expect(state.table.cards[0].highlighted).toBe('#ff0000');
    });

    it('handles batch card moves', async () => {
      const service = await serviceWithState();
      addCardToState('101', { groupId: '1', faceUp: false });
      addCardToState('102', { groupId: '1', faceUp: false });
      addCardToState('103', { groupId: '1', faceUp: true });
      mockSend.mockClear();

      const moveCard = getHandler('MoveCard');
      moveCard({ id: [101, 102, 103], group: 5, faceUp: [true, false, true] });

      const state = getLatestState();
      expect(state.table.cards[0].groupId).toBe('5');
      expect(state.table.cards[0].faceUp).toBe(true);
      expect(state.table.cards[1].groupId).toBe('5');
      expect(state.table.cards[1].faceUp).toBe(false);
      expect(state.table.cards[2].groupId).toBe('5');
      expect(state.table.cards[2].faceUp).toBe(true);
    });

    it('sends move request from client API then handles server response', async () => {
      const service = await serviceWithState();
      addCardToState('200');
      mockSendMessage.mockClear();

      // Client sends move request
      service.moveCards([200], 7, [0], [true], false);

      expect(mockSendMessage).toHaveBeenCalledWith(MessageType.MoveCardReq, 0, {
        id: [200],
        group: 7,
        idx: [0],
        faceUp: [true],
        isScriptMove: false,
      });

      // Server responds with MoveCard
      mockSend.mockClear();
      const moveCard = getHandler('MoveCard');
      moveCard({ id: [200], group: 7, faceUp: [true] });

      const state = getLatestState();
      expect(state.table.cards[0].groupId).toBe('7');
      expect(state.table.cards[0].faceUp).toBe(true);
    });
  });

  // -------------------------------------------------------------------------
  // Turn and counter management
  // -------------------------------------------------------------------------

  describe('turn and counter management', () => {
    it('advances through multiple turns updating active player', async () => {
      const service = await serviceWithState();
      const newPlayer = getHandler('NewPlayer');
      newPlayer({ id: 1, nick: 'Alice', spectator: false });
      newPlayer({ id: 2, nick: 'Bob', spectator: false });

      const nextTurn = getHandler('NextTurn');

      // Turn 1 - Alice active
      nextTurn({ player: 1, setActive: true });
      let state = getLatestState();
      expect(state.turnNumber).toBe(1);
      expect(state.activePlayer).toBe(1);

      // Turn 2 - Bob active
      nextTurn({ player: 2, setActive: true });
      state = getLatestState();
      expect(state.turnNumber).toBe(2);
      expect(state.activePlayer).toBe(2);

      // Turn 3 - no active change
      nextTurn({ player: 1, setActive: false });
      state = getLatestState();
      expect(state.turnNumber).toBe(3);
      expect(state.activePlayer).toBe(2); // unchanged
    });

    it('updates counter value for a player', async () => {
      const service = await serviceWithState();
      const newPlayer = getHandler('NewPlayer');
      newPlayer({ id: 10, nick: 'Alice', spectator: false });

      // Add a counter to the player
      const state1 = getLatestState();
      state1.players[0].counters.push({ id: 1, name: 'Life', value: 20 });

      mockSend.mockClear();
      const counter = getHandler('Counter');
      counter({ player: 10, counter: 1, value: 15 });

      const state2 = getLatestState();
      expect(state2.players[0].counters[0].value).toBe(15);
    });

    it('changes phase via SetPhase handler', async () => {
      const service = await serviceWithState();
      mockSend.mockClear();

      const setPhase = getHandler('SetPhase');
      setPhase({ phase: 3 });

      const state = getLatestState();
      expect(state.phase).toBe(3);
    });
  });

  // -------------------------------------------------------------------------
  // RemoteCall -> ScriptEngine validation
  // -------------------------------------------------------------------------

  describe('RemoteCall through GameService to ScriptEngine', () => {
    it('allows valid script function names through the full path', async () => {
      const service = await serviceWithState();
      mockSend.mockClear();

      const remoteCall = getHandler('RemoteCall');
      remoteCall({ player: 42, function: 'OnGameStart', args: '' });

      // A successful RemoteCall does NOT call broadcastState (no game state update),
      // it only sends a script:event to the renderer.
      const scriptEvents = mockSend.mock.calls.filter((c) => c[0] === 'script:event');
      expect(scriptEvents.length).toBeGreaterThan(0);

      // Verify it was a remoteCall event, not an error
      const lastScriptEvent = scriptEvents[scriptEvents.length - 1][1];
      expect(lastScriptEvent.type).toBe('remoteCall');
      expect(lastScriptEvent.function).toBe('OnGameStart');

      // No game:state-update calls should exist (no error system message added)
      const stateUpdates = mockSend.mock.calls.filter((c) => c[0] === 'game:state-update');
      expect(stateUpdates).toHaveLength(0);
    });

    it('blocks dangerous function names and adds system error message', async () => {
      const service = await serviceWithState();
      mockSend.mockClear();

      const remoteCall = getHandler('RemoteCall');
      remoteCall({ player: 42, function: 'eval', args: '' });

      // Should have broadcast a script error event
      const scriptEvents = mockSend.mock.calls.filter((c) => c[0] === 'script:event');
      expect(scriptEvents.length).toBeGreaterThan(0);
      const errorEvent = scriptEvents.find((c) => c[1].type === 'error');
      expect(errorEvent).toBeTruthy();
      expect(errorEvent![1].result.error).toContain('Script security violation');

      // Should also add a system message to the game state
      const stateUpdates = mockSend.mock.calls.filter((c) => c[0] === 'game:state-update');
      expect(stateUpdates.length).toBeGreaterThan(0);
      const state = stateUpdates[stateUpdates.length - 1][1];
      expect(state.chatMessages.length).toBeGreaterThan(0);
      const errorMsg = state.chatMessages.find((m: { message: string }) =>
        m.message.includes('Script error'),
      );
      expect(errorMsg).toBeTruthy();
      expect(errorMsg.isSystem).toBe(true);
    });

    it('blocks exec function through the full GameService path', async () => {
      const service = await serviceWithState();
      mockSend.mockClear();

      const remoteCall = getHandler('RemoteCall');
      remoteCall({ player: 42, function: 'exec', args: '' });

      const stateUpdates = mockSend.mock.calls.filter((c) => c[0] === 'game:state-update');
      const state = stateUpdates[stateUpdates.length - 1][1];
      const errorMsg = state.chatMessages.find((m: { message: string }) =>
        m.message.includes('exec'),
      );
      expect(errorMsg).toBeTruthy();
    });

    it('blocks __import__ dunder function through the full path', async () => {
      const service = await serviceWithState();
      mockSend.mockClear();

      const remoteCall = getHandler('RemoteCall');
      remoteCall({ player: 42, function: '__import__', args: '' });

      const stateUpdates = mockSend.mock.calls.filter((c) => c[0] === 'game:state-update');
      const state = stateUpdates[stateUpdates.length - 1][1];
      expect(state.chatMessages.some((m: { message: string }) =>
        m.message.includes('Script error'),
      )).toBe(true);
    });

    it('blocks arbitrary dunder names (e.g. __init__)', async () => {
      const service = await serviceWithState();
      mockSend.mockClear();

      const remoteCall = getHandler('RemoteCall');
      remoteCall({ player: 42, function: '__init__', args: '' });

      const stateUpdates = mockSend.mock.calls.filter((c) => c[0] === 'game:state-update');
      const state = stateUpdates[stateUpdates.length - 1][1];
      expect(state.chatMessages.some((m: { message: string }) =>
        m.message.includes('Script error'),
      )).toBe(true);
    });

    it('blocks function names with expression operators in args', async () => {
      const service = await serviceWithState();
      mockSend.mockClear();

      const remoteCall = getHandler('RemoteCall');
      remoteCall({ player: 42, function: 'OnGameStart', args: '1+1' });

      const stateUpdates = mockSend.mock.calls.filter((c) => c[0] === 'game:state-update');
      const state = stateUpdates[stateUpdates.length - 1][1];
      expect(state.chatMessages.some((m: { message: string }) =>
        m.message.includes('Script error'),
      )).toBe(true);
    });

    it('allows valid arguments: numbers, strings, booleans, OCTGN constructors', async () => {
      const service = await serviceWithState();

      const remoteCall = getHandler('RemoteCall');

      // Numeric arg
      mockSend.mockClear();
      remoteCall({ player: 42, function: 'OnCardClick', args: '42' });
      let scriptEvents = mockSend.mock.calls.filter((c) => c[0] === 'script:event');
      let remoteCallEvent = scriptEvents.find((c) => c[1].type === 'remoteCall');
      expect(remoteCallEvent).toBeTruthy();

      // String arg
      mockSend.mockClear();
      remoteCall({ player: 42, function: 'OnCardClick', args: '"hello"' });
      scriptEvents = mockSend.mock.calls.filter((c) => c[0] === 'script:event');
      remoteCallEvent = scriptEvents.find((c) => c[1].type === 'remoteCall');
      expect(remoteCallEvent).toBeTruthy();

      // Boolean arg
      mockSend.mockClear();
      remoteCall({ player: 42, function: 'OnCardClick', args: 'True' });
      scriptEvents = mockSend.mock.calls.filter((c) => c[0] === 'script:event');
      remoteCallEvent = scriptEvents.find((c) => c[1].type === 'remoteCall');
      expect(remoteCallEvent).toBeTruthy();

      // OCTGN constructor arg
      mockSend.mockClear();
      remoteCall({ player: 42, function: 'OnCardClick', args: 'Card(123)' });
      scriptEvents = mockSend.mock.calls.filter((c) => c[0] === 'script:event');
      remoteCallEvent = scriptEvents.find((c) => c[1].type === 'remoteCall');
      expect(remoteCallEvent).toBeTruthy();

      // Multiple args
      mockSend.mockClear();
      remoteCall({ player: 42, function: 'OnCardClick', args: '42, "test", True' });
      scriptEvents = mockSend.mock.calls.filter((c) => c[0] === 'script:event');
      remoteCallEvent = scriptEvents.find((c) => c[1].type === 'remoteCall');
      expect(remoteCallEvent).toBeTruthy();
    });

    it('logs script calls in the script engine call log', async () => {
      const service = await serviceWithState();
      const engine = service.getScriptEngine();

      expect(engine.getCallLog()).toHaveLength(0);

      const remoteCall = getHandler('RemoteCall');
      remoteCall({ player: 42, function: 'OnGameStart', args: '' });
      remoteCall({ player: 42, function: 'OnTurn', args: '1' });

      const log = engine.getCallLog();
      expect(log).toHaveLength(2);
      expect(log[0].function).toBe('OnGameStart');
      expect(log[1].function).toBe('OnTurn');
      expect(log[1].args).toBe('1');
    });

    it('logs blocked calls as well', async () => {
      const service = await serviceWithState();
      const engine = service.getScriptEngine();

      const remoteCall = getHandler('RemoteCall');
      remoteCall({ player: 42, function: 'eval', args: '' });

      const log = engine.getCallLog();
      expect(log).toHaveLength(1);
      expect(log[0].function).toBe('eval');
    });
  });

  // -------------------------------------------------------------------------
  // ScriptEngine standalone security tests
  // -------------------------------------------------------------------------

  describe('ScriptEngine security validation', () => {
    let engine: ScriptEngine;

    beforeEach(() => {
      engine = new ScriptEngine();
    });

    it('blocks all dangerous function names', () => {
      const dangerousFunctions = [
        'exec', 'eval', 'compile', '__import__', 'open', 'file',
        'input', 'raw_input', 'reload', 'execfile', 'apply',
        'getattr', 'setattr', 'delattr', 'hasattr',
        'globals', 'locals', 'vars', 'dir', 'exit', 'quit',
      ];

      for (const fn of dangerousFunctions) {
        const result = engine.handleRemoteCall(1, fn, '');
        expect(result.success, `Expected '${fn}' to be blocked`).toBe(false);
        expect(result.error).toContain('Script security violation');
      }
    });

    it('blocks function names that are not valid identifiers', () => {
      const invalidNames = [
        'func name',        // space
        '123func',          // starts with number
        'func.attr',        // dot notation
        'func()',           // parentheses
        '',                 // empty
        'func;drop',        // semicolon
      ];

      for (const fn of invalidNames) {
        const result = engine.handleRemoteCall(1, fn, '');
        expect(result.success, `Expected '${fn}' to be blocked`).toBe(false);
      }
    });

    it('allows valid game event functions', () => {
      const validFunctions = [
        'OnGameStart', 'OnCardClick', 'OnTurn', 'OnLoadDeck',
        'OnPlayerConnect', 'OnEndTurn', 'OnMarkerChanged',
      ];

      for (const fn of validFunctions) {
        const result = engine.handleRemoteCall(1, fn, '');
        expect(result.success, `Expected '${fn}' to be allowed`).toBe(true);
      }
    });

    it('blocks expression operators in arguments', () => {
      const dangerousArgs = [
        '__import__("os")',   // has parens but not OCTGN constructor pattern
        'x + y',
        'a; b',
        'x = 5',
        'a > b',
        'a < b',
      ];

      for (const arg of dangerousArgs) {
        const result = engine.handleRemoteCall(1, 'OnCardClick', arg);
        expect(result.success, `Expected arg '${arg}' to be blocked`).toBe(false);
      }
    });

    it('allows sandboxing to be disabled', () => {
      engine.setSandboxing(false);
      expect(engine.isSandboxed).toBe(false);

      // Now dangerous calls should pass through
      const result = engine.handleRemoteCall(1, 'eval', 'anything');
      expect(result.success).toBe(true);
    });

    it('re-enables sandboxing correctly', () => {
      engine.setSandboxing(false);
      engine.setSandboxing(true);
      expect(engine.isSandboxed).toBe(true);

      const result = engine.handleRemoteCall(1, 'eval', '');
      expect(result.success).toBe(false);
    });
  });

  // -------------------------------------------------------------------------
  // Error and edge case handling
  // -------------------------------------------------------------------------

  describe('error and edge case handling', () => {
    it('handles Error message and adds system message', async () => {
      const service = await serviceWithState();
      mockSend.mockClear();

      const error = getHandler('Error');
      error({ msg: 'Server error occurred' });

      const state = getLatestState();
      expect(state.chatMessages).toHaveLength(1);
      expect(state.chatMessages[0].message).toBe('Error: Server error occurred');
      expect(state.chatMessages[0].isSystem).toBe(true);
    });

    it('handles GameState sync from server', async () => {
      const service = await serviceWithState();
      mockSend.mockClear();

      const gameState = getHandler('GameState');
      gameState({
        state: JSON.stringify({ turnNumber: 10, activePlayer: 5 }),
      });

      const state = getLatestState();
      expect(state.turnNumber).toBe(10);
      expect(state.activePlayer).toBe(5);
    });

    it('handles malformed GameState gracefully', async () => {
      const service = await serviceWithState();
      mockSend.mockClear();

      const gameState = getHandler('GameState');
      // Should not throw
      gameState({ state: 'not valid json{{' });

      // State should still be broadcast (unchanged)
      expect(mockSend).toHaveBeenCalled();
    });

    it('handles connection failure gracefully', async () => {
      mockConnect.mockRejectedValueOnce(new Error('Connection refused'));

      const service = new GameService();
      const result = await service.joinGame('localhost', 9999, 'User', 'u', 'g', '1.0');

      expect(result).toEqual({ success: false, error: 'Connection refused' });
    });

    it('handles leave when not connected', async () => {
      const service = new GameService();
      // Should not throw
      await service.leaveGame();
      expect(service.isConnected).toBe(false);
    });

    it('handles chat for unknown player ID gracefully', async () => {
      const service = await serviceWithState();
      mockSend.mockClear();

      const chat = getHandler('Chat');
      chat({ player: 999, text: 'ghost message' });

      const state = getLatestState();
      expect(state.chatMessages[0].playerName).toBe('Player 999');
      expect(state.chatMessages[0].message).toBe('ghost message');
    });

    it('handles MoveCard for nonexistent card without crashing', async () => {
      const service = await serviceWithState();
      mockSend.mockClear();

      const moveCard = getHandler('MoveCard');
      // Should not throw even though card does not exist
      moveCard({ id: [99999], group: 3, faceUp: [true] });

      // State still broadcast
      expect(mockSend).toHaveBeenCalled();
    });

    it('handles Print message as system chat', async () => {
      const service = await serviceWithState();
      mockSend.mockClear();

      const print = getHandler('Print');
      print({ player: 0, text: 'Game is loading...' });

      const state = getLatestState();
      expect(state.chatMessages).toHaveLength(1);
      expect(state.chatMessages[0]).toMatchObject({
        playerName: 'System',
        message: 'Game is loading...',
        isSystem: true,
      });
    });
  });

  // -------------------------------------------------------------------------
  // Combined flow: game actions + script calls interleaved
  // -------------------------------------------------------------------------

  describe('interleaved game actions and script calls', () => {
    it('processes card moves and script calls in sequence', async () => {
      const service = await serviceWithState();
      const newPlayer = getHandler('NewPlayer');
      newPlayer({ id: 1, nick: 'Alice', spectator: false });

      addCardToState('50', { faceUp: false });
      addCardToState('51', { faceUp: false });
      mockSend.mockClear();

      // Script call for OnGameStarted
      const remoteCall = getHandler('RemoteCall');
      remoteCall({ player: 1, function: 'OnGameStarted', args: '' });

      // Card move
      const moveCard = getHandler('MoveCard');
      moveCard({ id: [50], group: 2, faceUp: [true] });

      // Another script call
      remoteCall({ player: 1, function: 'OnMoveCard', args: 'Card(50)' });

      // Verify card state updated
      const state = getLatestState();
      expect(state.table.cards[0].groupId).toBe('2');
      expect(state.table.cards[0].faceUp).toBe(true);

      // Verify script log has both calls
      const engine = service.getScriptEngine();
      const log = engine.getCallLog();
      expect(log).toHaveLength(2);
      expect(log[0].function).toBe('OnGameStarted');
      expect(log[1].function).toBe('OnMoveCard');
    });

    it('blocked script call does not affect game state progression', async () => {
      const service = await serviceWithState();

      addCardToState('60', { faceUp: false });

      // Dangerous script call
      const remoteCall = getHandler('RemoteCall');
      remoteCall({ player: 42, function: 'exec', args: '' });

      // Card operation should still work after blocked call
      mockSend.mockClear();
      const moveCard = getHandler('MoveCard');
      moveCard({ id: [60], group: 5, faceUp: [true] });

      const state = getLatestState();
      expect(state.table.cards[0].groupId).toBe('5');
      expect(state.table.cards[0].faceUp).toBe(true);
    });

    it('SetPlayerColor updates player color in state', async () => {
      const service = await serviceWithState();
      const newPlayer = getHandler('NewPlayer');
      newPlayer({ id: 10, nick: 'Alice', spectator: false });
      mockSend.mockClear();

      const setColor = getHandler('SetPlayerColor');
      setColor({ player: 10, color: '#ff5733' });

      const state = getLatestState();
      expect(state.players[0].color).toBe('#ff5733');
    });
  });
});
