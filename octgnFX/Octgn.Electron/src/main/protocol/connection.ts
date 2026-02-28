/**
 * OCTGN Game Binary Protocol - TCP Connection Manager
 *
 * Manages a TCP connection to an OCTGN game server, handling:
 *   - Message framing (4-byte little-endian length prefix)
 *   - Incoming data buffering and message extraction
 *   - Typed event emission for each deserialized message
 *   - Outbound message serialization and sending
 *   - Ping/pong keepalive
 *   - Automatic reconnection with exponential backoff
 */

import { EventEmitter } from 'events';
import * as net from 'net';

import { deserializeMessage, serializeMessage, getMessageTypeName } from './serializer';
import { MessageType, type ProtocolMessage } from './types';

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

/** Options for creating a {@link GameConnection}. */
export interface GameConnectionOptions {
  /** Remote host name or IP address. */
  host: string;
  /** Remote TCP port. */
  port: number;
  /** Interval in ms between outgoing Ping messages (default 15 000). */
  pingIntervalMs?: number;
  /** If true, automatically attempt to reconnect on disconnect (default true). */
  autoReconnect?: boolean;
  /** Maximum number of consecutive reconnection attempts (default 10). */
  maxReconnectAttempts?: number;
  /** Base delay in ms for exponential backoff reconnection (default 1000). */
  reconnectBaseDelayMs?: number;
  /** Maximum reconnection delay in ms (default 30 000). */
  reconnectMaxDelayMs?: number;
}

// ---------------------------------------------------------------------------
// Typed events
// ---------------------------------------------------------------------------

/**
 * Events emitted by {@link GameConnection}.
 *
 * In addition to the events listed below, every {@link MessageType} name
 * is emitted as an event when a message of that type is received (e.g.
 * `'Hello'`, `'Chat'`, `'MoveCard'`).  The listener receives the
 * full {@link ProtocolMessage} as its sole argument.
 */
export interface GameConnectionEvents {
  /** Emitted when the TCP connection is established. */
  connected: () => void;
  /** Emitted when a dropped connection is re-established via auto-reconnect. */
  reconnected: () => void;
  /** Emitted when the connection is closed (gracefully or due to error). */
  disconnected: (hadError: boolean) => void;
  /** Emitted on socket or protocol errors. */
  error: (err: Error) => void;
  /** Emitted when a reconnection attempt begins. */
  reconnecting: (attempt: number) => void;
  /** Emitted for every successfully deserialized incoming message. */
  message: (msg: ProtocolMessage) => void;
}

// ---------------------------------------------------------------------------
// GameConnection class
// ---------------------------------------------------------------------------

/**
 * A TCP connection to an OCTGN game server that speaks the binary
 * message protocol.
 *
 * @example
 * ```ts
 * const conn = new GameConnection({ host: '127.0.0.1', port: 36963 });
 * conn.on('message', (msg) => console.log(msg.type, msg.params));
 * conn.on('Chat', (msg) => console.log('chat:', msg.params.text));
 * await conn.connect();
 * conn.sendMessage(MessageType.Hello, 0, { ... });
 * ```
 */
export class GameConnection extends EventEmitter {
  private readonly options: Required<GameConnectionOptions>;
  private socket: net.Socket | null = null;
  private receiveBuffer: Buffer = Buffer.alloc(0);
  private pingTimer: ReturnType<typeof setInterval> | null = null;
  private reconnectTimer: ReturnType<typeof setTimeout> | null = null;
  private reconnectAttempts = 0;
  private intentionalDisconnect = false;
  private _isConnected = false;

  /**
   * Create a new GameConnection.
   * @param options - Connection options including host and port.
   */
  constructor(options: GameConnectionOptions) {
    super();
    this.options = {
      pingIntervalMs: 15_000,
      autoReconnect: true,
      maxReconnectAttempts: 10,
      reconnectBaseDelayMs: 1_000,
      reconnectMaxDelayMs: 30_000,
      ...options,
    };
  }

  // -------------------------------------------------------------------------
  // Public API
  // -------------------------------------------------------------------------

  /** Whether the connection is currently established and open. */
  get isConnected(): boolean {
    return this._isConnected;
  }

  /**
   * Open the TCP connection to the game server.
   *
   * @returns A promise that resolves when the connection is established,
   *          or rejects on connection error.
   */
  connect(): Promise<void> {
    return new Promise<void>((resolve, reject) => {
      if (this.socket) {
        this.cleanup();
      }

      this.intentionalDisconnect = false;
      this.receiveBuffer = Buffer.alloc(0);

      const socket = new net.Socket();
      this.socket = socket;

      const onConnect = (): void => {
        socket.removeListener('error', onError);
        this._isConnected = true;
        this.reconnectAttempts = 0;
        this.startPingTimer();
        this.emit('connected');
        resolve();
      };

      const onError = (err: Error): void => {
        socket.removeListener('connect', onConnect);
        reject(err);
      };

      socket.once('connect', onConnect);
      socket.once('error', onError);

      socket.on('data', (data: Buffer) => this.onData(data));
      socket.on('close', (hadError: boolean) => this.onClose(hadError));
      socket.on('error', (err: Error) => this.emit('error', err));

      socket.connect(this.options.port, this.options.host);
    });
  }

  /**
   * Gracefully close the connection.
   * No reconnection will be attempted after an intentional disconnect.
   */
  disconnect(): void {
    this.intentionalDisconnect = true;
    this.cleanup();
  }

  /**
   * Serialize and send a protocol message to the server.
   *
   * @param type     - The message type to send.
   * @param isMuted  - The IsMuted state (usually 0).
   * @param params   - The message parameters.
   * @throws If the connection is not currently open.
   */
  sendMessage(type: MessageType, isMuted: number, params: Record<string, unknown>): void {
    if (!this.socket || !this._isConnected) {
      throw new Error('Cannot send message: not connected');
    }
    const buf = serializeMessage({ isMuted, type, params });
    this.socket.write(buf);
  }

  /**
   * Send a raw pre-serialized buffer to the server.
   * @param data - The complete framed message buffer.
   */
  sendRaw(data: Buffer): void {
    if (!this.socket || !this._isConnected) {
      throw new Error('Cannot send data: not connected');
    }
    this.socket.write(data);
  }

  // -------------------------------------------------------------------------
  // Socket event handlers
  // -------------------------------------------------------------------------

  /** Handle incoming TCP data. */
  private onData(data: Buffer): void {
    // Append new data to the receive buffer.
    this.receiveBuffer = Buffer.concat([this.receiveBuffer, data]);

    // Extract as many complete messages as possible.
    while (this.receiveBuffer.length >= 4) {
      const totalLength = this.receiveBuffer.readInt32LE(0);

      if (totalLength < 4) {
        // Protocol error: length must be at least 4 (the length field itself).
        this.emit('error', new Error(`Invalid message length: ${totalLength}`));
        this.receiveBuffer = Buffer.alloc(0);
        return;
      }

      if (this.receiveBuffer.length < totalLength) {
        // Not enough data yet; wait for more.
        break;
      }

      // Extract the payload (everything after the 4-byte length prefix).
      const payload = this.receiveBuffer.subarray(4, totalLength);
      this.receiveBuffer = Buffer.from(this.receiveBuffer.subarray(totalLength));

      try {
        const message = deserializeMessage(Buffer.from(payload));

        // Handle ping internally.
        if (message.type === MessageType.Ping) {
          this.handlePing(message);
        }

        // Emit on the generic 'message' event.
        this.emit('message', message);

        // Emit on the specific message-type event (e.g. 'Chat', 'Welcome').
        const typeName = getMessageTypeName(message.type);
        if (typeName !== 'Unknown') {
          this.emit(typeName, message);
        }
      } catch (err) {
        this.emit('error', err instanceof Error ? err : new Error(String(err)));
      }
    }
  }

  /** Handle TCP socket close. */
  private onClose(hadError: boolean): void {
    this._isConnected = false;
    this.stopPingTimer();
    this.emit('disconnected', hadError);

    if (!this.intentionalDisconnect && this.options.autoReconnect) {
      this.scheduleReconnect();
    }
  }

  // -------------------------------------------------------------------------
  // Ping / keepalive
  // -------------------------------------------------------------------------

  /** Respond to an incoming Ping with a Ping reply. */
  private handlePing(_message: ProtocolMessage): void {
    try {
      this.sendMessage(MessageType.Ping, 0, {});
    } catch {
      // Connection may have been lost; swallow and let onClose handle it.
    }
  }

  /** Start the periodic outbound ping timer. */
  private startPingTimer(): void {
    this.stopPingTimer();
    this.pingTimer = setInterval(() => {
      try {
        if (this._isConnected) {
          this.sendMessage(MessageType.Ping, 0, {});
        }
      } catch {
        // Swallow; if the socket is dead, onClose will fire.
      }
    }, this.options.pingIntervalMs);
  }

  /** Stop the periodic outbound ping timer. */
  private stopPingTimer(): void {
    if (this.pingTimer !== null) {
      clearInterval(this.pingTimer);
      this.pingTimer = null;
    }
  }

  // -------------------------------------------------------------------------
  // Reconnection logic
  // -------------------------------------------------------------------------

  /** Schedule a reconnection attempt with exponential backoff. */
  private scheduleReconnect(): void {
    if (this.reconnectAttempts >= this.options.maxReconnectAttempts) {
      this.emit(
        'error',
        new Error(
          `Maximum reconnection attempts (${this.options.maxReconnectAttempts}) exceeded`,
        ),
      );
      return;
    }

    this.reconnectAttempts++;
    const delay = Math.min(
      this.options.reconnectBaseDelayMs * Math.pow(2, this.reconnectAttempts - 1),
      this.options.reconnectMaxDelayMs,
    );

    this.emit('reconnecting', this.reconnectAttempts);

    this.reconnectTimer = setTimeout(async () => {
      try {
        await this.connect();
        // Emit reconnected to distinguish from initial connect
        this.emit('reconnected');
      } catch {
        // connect() failure will trigger onClose which will reschedule.
      }
    }, delay);
  }

  // -------------------------------------------------------------------------
  // Cleanup
  // -------------------------------------------------------------------------

  /** Tear down the socket and all timers. */
  private cleanup(): void {
    this.stopPingTimer();

    if (this.reconnectTimer !== null) {
      clearTimeout(this.reconnectTimer);
      this.reconnectTimer = null;
    }

    if (this.socket) {
      this.socket.removeAllListeners();
      this.socket.destroy();
      this.socket = null;
    }

    this._isConnected = false;
  }
}
