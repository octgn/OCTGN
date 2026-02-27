import * as net from 'net';
import { EventEmitter } from 'events';
import { BinaryProtocol, METHODS } from './BinaryProtocol';
import { GameState } from './GameState';

export interface PlayerInfo {
  id: string;
  nick: string;
  userId: string;
  publicKey: bigint;
  tableSide: boolean;
  spectator: boolean;
  invertedTable: boolean;
  ready: boolean;
}

export class Player extends EventEmitter {
  private socket: net.Socket;
  private protocol: BinaryProtocol;
  private gameState: GameState;
  private info: PlayerInfo;
  private buffer: Buffer[] = [];
  private connected = true;
  private saidHello = false;

  constructor(
    id: string,
    socket: net.Socket,
    protocol: BinaryProtocol,
    gameState: GameState
  ) {
    super();
    this.socket = socket;
    this.protocol = protocol;
    this.gameState = gameState;
    this.info = {
      id,
      nick: '',
      userId: '',
      publicKey: 0n,
      tableSide: false,
      spectator: false,
      invertedTable: false,
      ready: false,
    };

    this.setupSocket();
  }

  private setupSocket(): void {
    this.socket.on('data', (data: Buffer) => {
      this.buffer.push(data);
      this.processBuffer();
    });

    this.socket.on('close', () => {
      this.connected = false;
      this.emit('disconnect');
    });

    this.socket.on('error', (err) => {
      console.error(`Socket error for player ${this.info.id}:`, err);
      this.connected = false;
      this.emit('disconnect');
    });
  }

  private processBuffer(): void {
    const allData = Buffer.concat(this.buffer);
    this.buffer = [];

    let offset = 0;
    while (offset < allData.length) {
      if (allData.length - offset < 4) {
        // Not enough data for length prefix
        break;
      }

      const messageLength = allData.readInt32LE(offset);
      if (allData.length - offset < messageLength + 4) {
        // Incomplete message
        break;
      }

      const messageData = allData.slice(offset + 4, offset + 4 + messageLength);
      offset += 4 + messageLength;

      try {
        const parsed = this.protocol.parse(messageData);
        this.handleMessage(parsed);
      } catch (err) {
        console.error('Failed to parse message:', err);
      }
    }

    // Store any remaining partial data
    if (offset < allData.length) {
      this.buffer = [allData.slice(offset)];
    }
  }

  private handleMessage(message: any): void {
    const { methodId, methodName, data } = message;

    switch (methodId) {
      case METHODS.Hello:
        this.handleHello(data);
        break;

      case METHODS.Ping:
        // Respond to ping
        this.sendPingResponse();
        break;

      case METHODS.Ready:
        this.info.ready = true;
        this.emit('ready');
        break;

      default:
        // Forward to game logic
        this.emit('message', message);
    }
  }

  private handleHello(data: any): void {
    this.info.nick = data.nick;
    this.info.userId = data.userId;
    this.info.publicKey = data.pkey;
    this.info.spectator = data.spectator;
    this.saidHello = true;

    // Send welcome message
    this.sendWelcome();

    this.emit('hello', data);
  }

  // Outgoing messages
  sendWelcome(): void {
    const message = this.protocol.createMessage('Welcome', {
      id: parseInt(this.info.id, 10) % 256, // byte
      gameSessionId: '00000000-0000-0000-0000-000000000000',
      gameName: 'OCTGN Electron',
      waitForGameState: false,
    });
    this.send(message);
  }

  sendError(msg: string): void {
    const message = this.protocol.createMessage('Error', { msg });
    this.send(message);
  }

  sendKick(reason: string): void {
    const message = this.protocol.createMessage('Kick', { reason });
    this.send(message);
  }

  sendPingResponse(): void {
    const message = this.protocol.createMessage('Ping', {});
    this.send(message);
  }

  sendNewPlayer(player: Player): void {
    const message = this.protocol.createMessage('NewPlayer', {
      id: parseInt(player.id, 10) % 256,
      nick: player.nickname,
      userId: player.userId,
      pkey: player.publicKey,
      tableSide: player.tableSide,
      spectator: player.isSpectator,
    });
    this.send(message);
  }

  sendPlayerLeave(playerId: string): void {
    const message = this.protocol.createMessage('Leave', {
      player: parseInt(playerId, 10) % 256,
    });
    this.send(message);
  }

  sendChat(playerId: string, text: string): void {
    const message = this.protocol.createMessage('Chat', {
      player: parseInt(playerId, 10) % 256,
      text,
    });
    this.send(message);
  }

  send(data: Buffer): void {
    if (!this.connected) return;

    // Prepend message length
    const lengthBuffer = Buffer.alloc(4);
    lengthBuffer.writeInt32LE(data.length, 0);

    try {
      this.socket.write(Buffer.concat([lengthBuffer, data]));
    } catch (err) {
      console.error('Failed to send data:', err);
    }
  }

  disconnect(): void {
    this.connected = false;
    try {
      this.socket.destroy();
    } catch (err) {
      // Ignore
    }
  }

  // Getters
  get id(): string {
    return this.info.id;
  }

  get nickname(): string {
    return this.info.nick;
  }

  get userId(): string {
    return this.info.userId;
  }

  get publicKey(): bigint {
    return this.info.publicKey;
  }

  get tableSide(): boolean {
    return this.info.tableSide;
  }

  get isSpectator(): boolean {
    return this.info.spectator;
  }

  get isInvertedTable(): boolean {
    return this.info.invertedTable;
  }

  get isReady(): boolean {
    return this.info.ready;
  }

  get saidHello(): boolean {
    return this._saidHello;
  }

  private set saidHello(value: boolean) {
    this._saidHello = value;
  }
  private _saidHello = false;
}
