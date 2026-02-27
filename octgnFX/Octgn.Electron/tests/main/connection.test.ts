import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { EventEmitter } from 'events';
import { GameConnection } from '@main/protocol/connection';
import { MessageType } from '@main/protocol/types';
import { serializeMessage } from '@main/protocol/serializer';

// ---------------------------------------------------------------------------
// Mock net.Socket
// ---------------------------------------------------------------------------

let mockSocketInstance: EventEmitter & {
  connect: ReturnType<typeof vi.fn>;
  write: ReturnType<typeof vi.fn>;
  destroy: ReturnType<typeof vi.fn>;
};

function createMockSocket() {
  const socket = new EventEmitter() as typeof mockSocketInstance;
  socket.connect = vi.fn((_port: number, _host: string) => {
    process.nextTick(() => socket.emit('connect'));
    return socket;
  });
  socket.write = vi.fn();
  socket.destroy = vi.fn();
  mockSocketInstance = socket;
  return socket;
}

vi.mock('net', () => ({
  Socket: vi.fn(() => createMockSocket()),
}));

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/**
 * Create a framed message buffer (4-byte LE length prefix + payload).
 */
function framedMessage(
  type: MessageType,
  params: Record<string, unknown> = {},
  isMuted = 0,
): Buffer {
  return serializeMessage({ isMuted, type, params });
}

async function connectedConnection(
  opts: Partial<ConstructorParameters<typeof GameConnection>[0]> = {},
): Promise<GameConnection> {
  const conn = new GameConnection({ host: 'localhost', port: 1234, ...opts });
  const promise = conn.connect();
  await vi.advanceTimersByTimeAsync(0);
  await promise;
  return conn;
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('GameConnection', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.restoreAllMocks();
  });

  // -------------------------------------------------------------------------
  // connect()
  // -------------------------------------------------------------------------

  describe('connect()', () => {
    it('creates a socket and connects to the given host and port', async () => {
      const conn = new GameConnection({ host: '10.0.0.1', port: 5555 });
      const promise = conn.connect();
      await vi.advanceTimersByTimeAsync(0);
      await promise;

      expect(mockSocketInstance.connect).toHaveBeenCalledWith(5555, '10.0.0.1');
      expect(conn.isConnected).toBe(true);
    });

    it('emits "connected" event on success', async () => {
      const connectedSpy = vi.fn();
      const conn = new GameConnection({ host: 'localhost', port: 1234 });
      conn.on('connected', connectedSpy);

      const promise = conn.connect();
      await vi.advanceTimersByTimeAsync(0);
      await promise;

      expect(connectedSpy).toHaveBeenCalledOnce();
    });

    it('rejects when the socket emits an error before connecting', async () => {
      // Make the next socket emit 'error' instead of 'connect'
      const { Socket } = await import('net');
      (Socket as unknown as ReturnType<typeof vi.fn>).mockImplementationOnce(() => {
        const socket = new EventEmitter() as typeof mockSocketInstance;
        socket.connect = vi.fn((_port: number, _host: string) => {
          // Emit error on next microtask (not via setTimeout to avoid fake-timer issues)
          Promise.resolve().then(() => socket.emit('error', new Error('ECONNREFUSED')));
          return socket;
        });
        socket.write = vi.fn();
        socket.destroy = vi.fn();
        mockSocketInstance = socket;
        return socket;
      });

      const conn = new GameConnection({ host: 'localhost', port: 1234 });
      // Prevent unhandled 'error' event on the EventEmitter
      conn.on('error', () => {});

      await expect(conn.connect()).rejects.toThrow('ECONNREFUSED');
      expect(conn.isConnected).toBe(false);
    });
  });

  // -------------------------------------------------------------------------
  // disconnect()
  // -------------------------------------------------------------------------

  describe('disconnect()', () => {
    it('destroys the socket and clears timers', async () => {
      const conn = await connectedConnection();
      const socket = mockSocketInstance;

      conn.disconnect();

      expect(socket.destroy).toHaveBeenCalled();
      expect(conn.isConnected).toBe(false);
    });

    it('prevents reconnection after intentional disconnect', async () => {
      const conn = await connectedConnection({ autoReconnect: true });
      const reconnectingSpy = vi.fn();
      conn.on('reconnecting', reconnectingSpy);

      conn.disconnect();

      // Even after a long wait, no reconnection should be attempted
      await vi.advanceTimersByTimeAsync(60_000);
      expect(reconnectingSpy).not.toHaveBeenCalled();
    });
  });

  // -------------------------------------------------------------------------
  // sendMessage()
  // -------------------------------------------------------------------------

  describe('sendMessage()', () => {
    it('serializes and writes to the socket', async () => {
      const conn = await connectedConnection();

      conn.sendMessage(MessageType.ChatReq, 0, { text: 'hello' });

      expect(mockSocketInstance.write).toHaveBeenCalledOnce();
      const buf: Buffer = mockSocketInstance.write.mock.calls[0][0];
      expect(Buffer.isBuffer(buf)).toBe(true);
      expect(buf.length).toBeGreaterThan(4);
    });

    it('throws when not connected', () => {
      const conn = new GameConnection({ host: 'localhost', port: 1234 });
      expect(() => conn.sendMessage(MessageType.Ping, 0, {})).toThrow(
        'Cannot send message: not connected',
      );
    });
  });

  // -------------------------------------------------------------------------
  // onData() - message framing
  // -------------------------------------------------------------------------

  describe('onData()', () => {
    it('parses a complete framed message and emits "message"', async () => {
      const conn = await connectedConnection();
      const messageSpy = vi.fn();
      conn.on('message', messageSpy);

      const frame = framedMessage(MessageType.Chat, { player: 1, text: 'hi' });
      mockSocketInstance.emit('data', frame);

      expect(messageSpy).toHaveBeenCalledOnce();
      const msg = messageSpy.mock.calls[0][0];
      expect(msg.type).toBe(MessageType.Chat);
      expect(msg.params.text).toBe('hi');
    });

    it('handles partial messages by buffering', async () => {
      const conn = await connectedConnection();
      const messageSpy = vi.fn();
      conn.on('message', messageSpy);

      const frame = framedMessage(MessageType.Chat, { player: 1, text: 'buffered' });
      const mid = Math.floor(frame.length / 2);

      // Send first half
      mockSocketInstance.emit('data', frame.subarray(0, mid));
      expect(messageSpy).not.toHaveBeenCalled();

      // Send second half
      mockSocketInstance.emit('data', frame.subarray(mid));
      expect(messageSpy).toHaveBeenCalledOnce();
      expect(messageSpy.mock.calls[0][0].params.text).toBe('buffered');
    });

    it('handles multiple messages in one chunk', async () => {
      const conn = await connectedConnection();
      const messageSpy = vi.fn();
      conn.on('message', messageSpy);

      const frame1 = framedMessage(MessageType.Chat, { player: 1, text: 'msg1' });
      const frame2 = framedMessage(MessageType.Chat, { player: 2, text: 'msg2' });
      const combined = Buffer.concat([frame1, frame2]);

      mockSocketInstance.emit('data', combined);

      expect(messageSpy).toHaveBeenCalledTimes(2);
      expect(messageSpy.mock.calls[0][0].params.text).toBe('msg1');
      expect(messageSpy.mock.calls[1][0].params.text).toBe('msg2');
    });

    it('emits both "message" and typed event (e.g. "Chat")', async () => {
      const conn = await connectedConnection();
      const messageSpy = vi.fn();
      const chatSpy = vi.fn();
      conn.on('message', messageSpy);
      conn.on('Chat', chatSpy);

      const frame = framedMessage(MessageType.Chat, { player: 5, text: 'typed event' });
      mockSocketInstance.emit('data', frame);

      expect(messageSpy).toHaveBeenCalledOnce();
      expect(chatSpy).toHaveBeenCalledOnce();
      expect(chatSpy.mock.calls[0][0].type).toBe(MessageType.Chat);
    });
  });

  // -------------------------------------------------------------------------
  // Ping handling
  // -------------------------------------------------------------------------

  describe('ping handling', () => {
    it('responds to an incoming Ping with a Ping reply', async () => {
      const conn = await connectedConnection();
      mockSocketInstance.write.mockClear();

      const pingFrame = framedMessage(MessageType.Ping, {});
      mockSocketInstance.emit('data', pingFrame);

      // The connection should have sent a Ping reply
      expect(mockSocketInstance.write).toHaveBeenCalled();
    });

    it('sends periodic pings via the ping timer', async () => {
      const conn = await connectedConnection({ pingIntervalMs: 5000 });
      mockSocketInstance.write.mockClear();

      // Advance past one ping interval
      vi.advanceTimersByTime(5001);

      expect(mockSocketInstance.write).toHaveBeenCalled();
    });
  });

  // -------------------------------------------------------------------------
  // Reconnection logic
  // -------------------------------------------------------------------------

  describe('reconnection', () => {
    it('schedules reconnect with exponential backoff on unexpected close', async () => {
      const conn = await connectedConnection({
        autoReconnect: true,
        reconnectBaseDelayMs: 100,
        maxReconnectAttempts: 5,
      });
      const reconnectingSpy = vi.fn();
      conn.on('reconnecting', reconnectingSpy);

      // Simulate unexpected close
      mockSocketInstance.emit('close', false);

      expect(reconnectingSpy).toHaveBeenCalledWith(1);

      // First reconnect delay: 100 * 2^0 = 100ms
      await vi.advanceTimersByTimeAsync(100);

      // A new socket should have been created and connect called
      expect(mockSocketInstance.connect).toHaveBeenCalled();
    });

    it('does not reconnect when intentionalDisconnect is set', async () => {
      const conn = await connectedConnection({ autoReconnect: true });
      const reconnectingSpy = vi.fn();
      conn.on('reconnecting', reconnectingSpy);

      conn.disconnect();

      await vi.advanceTimersByTimeAsync(60_000);
      expect(reconnectingSpy).not.toHaveBeenCalled();
    });

    it('does not reconnect when autoReconnect is false', async () => {
      const conn = await connectedConnection({ autoReconnect: false });
      const reconnectingSpy = vi.fn();
      conn.on('reconnecting', reconnectingSpy);

      mockSocketInstance.emit('close', false);

      await vi.advanceTimersByTimeAsync(60_000);
      expect(reconnectingSpy).not.toHaveBeenCalled();
    });

    it('emits error when max reconnect attempts exceeded', async () => {
      const conn = await connectedConnection({
        autoReconnect: true,
        maxReconnectAttempts: 2,
        reconnectBaseDelayMs: 50,
      });
      const errorSpy = vi.fn();
      const reconnectingSpy = vi.fn();
      conn.on('error', errorSpy);
      conn.on('reconnecting', reconnectingSpy);

      // Make subsequent connect() calls fail so reconnectAttempts is not reset
      const { Socket } = await import('net');
      (Socket as unknown as ReturnType<typeof vi.fn>).mockImplementation(() => {
        const socket = new EventEmitter() as typeof mockSocketInstance;
        socket.connect = vi.fn((_port: number, _host: string) => {
          process.nextTick(() => {
            socket.emit('error', new Error('ECONNREFUSED'));
            socket.emit('close', true);
          });
          return socket;
        });
        socket.write = vi.fn();
        socket.destroy = vi.fn();
        mockSocketInstance = socket;
        return socket;
      });

      // First unexpected close -> schedules attempt 1
      mockSocketInstance.emit('close', false);
      expect(reconnectingSpy).toHaveBeenCalledWith(1);

      // Wait for attempt 1 delay (50ms) and let its connect fail
      await vi.advanceTimersByTimeAsync(50);
      await vi.advanceTimersByTimeAsync(0);
      // The failed connect triggers 'close' -> schedules attempt 2
      expect(reconnectingSpy).toHaveBeenCalledWith(2);

      // Wait for attempt 2 delay (100ms) and let its connect fail
      await vi.advanceTimersByTimeAsync(100);
      await vi.advanceTimersByTimeAsync(0);
      // The failed connect triggers 'close' -> attempts (2) >= max (2) -> error

      const maxExceededError = errorSpy.mock.calls.find(
        (c) => c[0]?.message?.includes('Maximum reconnection attempts'),
      );
      expect(maxExceededError).toBeTruthy();
    });
  });
});
