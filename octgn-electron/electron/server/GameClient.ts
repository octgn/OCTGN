import { EventEmitter } from 'events';
import { BinaryProtocol, METHODS, ProtocolMessage } from './BinaryProtocol';
import { GameState, Card, Group } from './GameState';

export interface ClientConfig {
  host: string;
  port: number;
  nick: string;
  userId?: string;
  spectator?: boolean;
}

export class GameClient extends EventEmitter {
  private socket: WebSocket | null = null;
  private protocol: BinaryProtocol;
  private config: ClientConfig;
  private connected = false;
  private playerId: string | null = null;
  private gameState: GameState;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectDelay = 1000;

  constructor(config: ClientConfig) {
    super();
    this.config = config;
    this.protocol = new BinaryProtocol();
    this.gameState = new GameState();

    // Forward game state events
    this.gameState.on('card-created', (card) => this.emit('card-created', card));
    this.gameState.on('cards-moved', (data) => this.emit('cards-moved', data));
    this.gameState.on('card-turned', (data) => this.emit('card-turned', data));
    this.gameState.on('card-rotated', (data) => this.emit('card-rotated', data));
    this.gameState.on('counter-changed', (data) => this.emit('counter-changed', data));
    this.gameState.on('turn-changed', (data) => this.emit('turn-changed', data));
  }

  async connect(): Promise<void> {
    return new Promise((resolve, reject) => {
      try {
        // Use WebSocket for browser/Electron renderer
        const wsUrl = `ws://${this.config.host}:${this.config.port}`;
        this.socket = new WebSocket(wsUrl, 'octgn');

        this.socket.binaryType = 'arraybuffer';

        this.socket.onopen = () => {
          this.connected = true;
          this.reconnectAttempts = 0;
          this.emit('connected');
          
          // Send hello message
          this.sendHello();
          resolve();
        };

        this.socket.onmessage = (event) => {
          const data = new Uint8Array(event.data);
          this.handleMessage(data);
        };

        this.socket.onclose = (event) => {
          this.connected = false;
          this.emit('disconnected', { code: event.code, reason: event.reason });
          
          // Attempt reconnect if not intentional
          if (event.code !== 1000 && this.reconnectAttempts < this.maxReconnectAttempts) {
            setTimeout(() => this.reconnect(), this.reconnectDelay * Math.pow(2, this.reconnectAttempts));
          }
        };

        this.socket.onerror = (error) => {
          this.emit('error', error);
          reject(error);
        };
      } catch (error) {
        reject(error);
      }
    });
  }

  private async reconnect(): Promise<void> {
    this.reconnectAttempts++;
    this.emit('reconnecting', { attempt: this.reconnectAttempts });
    
    try {
      await this.connect();
    } catch (error) {
      // Will try again if under max attempts
    }
  }

  disconnect(): void {
    if (this.socket) {
      this.socket.close(1000, 'Client disconnect');
      this.socket = null;
      this.connected = false;
    }
  }

  private sendHello(): void {
    const message = this.protocol.createMessage('Hello', {
      nick: this.config.nick,
      userId: this.config.userId || '',
      pkey: BigInt(0),
      client: 'OCTGN Electron',
      version: '3.5.0.0',
      gameVersion: '1.0.0.0',
      gameId: '00000000-0000-0000-0000-000000000000',
      scriptVersion: '3.1.0.2',
      password: '',
      spectator: this.config.spectator || false,
    });
    this.send(message);
  }

  private handleMessage(data: Uint8Array): void {
    try {
      const buffer = Buffer.from(data);
      const message = this.protocol.parse(buffer);
      this.processMessage(message);
    } catch (error) {
      console.error('Failed to parse message:', error);
    }
  }

  private processMessage(message: ProtocolMessage): void {
    const { methodId, methodName, data } = message;

    switch (methodId) {
      case METHODS.Error:
        this.emit('error', { message: data.msg });
        break;

      case METHODS.Welcome:
        // Not in protocol - server sends this
        this.playerId = data.id?.toString() || null;
        this.emit('welcome', data);
        break;

      case METHODS.NewPlayer:
        this.emit('player-joined', data);
        break;

      case METHODS.Leave:
        this.emit('player-left', data);
        break;

      case METHODS.Chat:
        this.emit('chat', data);
        break;

      case METHODS.MoveCard:
        this.gameState.moveCard(data.cardIds, data.toGroup, data.toIndex, data.faceUp);
        break;

      case METHODS.Turn:
        this.gameState.turnCard(data.card, data.up);
        break;

      case METHODS.Rotate:
        this.gameState.rotateCard(data.card, data.rot);
        break;

      case METHODS.Start:
        this.emit('game-start');
        break;

      case METHODS.Reset:
        this.emit('game-reset', data);
        break;

      case METHODS.NextTurn:
        this.gameState.nextTurn(data.player, data.force);
        break;

      case METHODS.Counter:
        this.gameState.setCounter(data.counter?.toString() || '', data.value);
        break;

      case METHODS.LoadDeck:
        this.handleLoadDeck(data);
        break;

      case METHODS.Ping:
        // Respond to ping
        this.sendPing();
        break;

      default:
        console.log(`Unhandled message: ${methodName}`, data);
    }
  }

  private handleLoadDeck(data: any): void {
    const { ids, types, groups, sizes, sleeve, limited } = data;
    
    for (let i = 0; i < ids.length; i++) {
      const card = this.gameState.createCard(types[i], groups[i], sizes[i]);
      // Set additional properties
    }
    
    this.emit('deck-loaded', data);
  }

  // Public methods for sending game actions

  sendChat(text: string): void {
    const message = this.protocol.createMessage('ChatReq', { text });
    this.send(message);
  }

  moveCards(cardIds: number[], toGroup: number, toIndex: number[], faceUp: boolean[]): void {
    const message = this.protocol.createMessage('MoveCardReq', {
      cardIds,
      toGroup,
      toIndex,
      faceUp,
      isScriptMove: false,
    });
    this.send(message);
  }

  turnCard(cardId: number, faceUp: boolean): void {
    const message = this.protocol.createMessage('TurnReq', {
      card: cardId,
      up: faceUp,
    });
    this.send(message);
  }

  rotateCard(cardId: number, rotation: number): void {
    const message = this.protocol.createMessage('RotateReq', {
      card: cardId,
      rot: rotation,
    });
    this.send(message);
  }

  peekCard(cardId: number): void {
    const message = this.protocol.createMessage('PeekReq', { card: cardId });
    this.send(message);
  }

  targetCard(cardId: number, targeted: boolean): void {
    const message = targeted
      ? this.protocol.createMessage('TargetReq', { card: cardId, isScriptChange: false })
      : this.protocol.createMessage('UntargetReq', { card: cardId, isScriptChange: false });
    this.send(message);
  }

  highlightCard(cardId: number, color: string): void {
    const message = this.protocol.createMessage('Highlight', { card: cardId, color });
    this.send(message);
  }

  shuffleGroup(groupId: number): void {
    // Request shuffle - server will respond with Shuffled message
    const positions = this.gameState.shuffleGroup(groupId);
    const message = this.protocol.createMessage('Shuffled', {
      player: parseInt(this.playerId || '0', 10),
      group: groupId,
      card: positions.map((_, i) => i),
      pos: positions,
    });
    this.send(message);
  }

  addMarker(cardId: number, markerId: string, name: string, count: number): void {
    const message = this.protocol.createMessage('AddMarkerReq', {
      card: cardId,
      id: markerId,
      name,
      count,
      origCount: 0,
      isScriptChange: false,
    });
    this.send(message);
  }

  removeMarker(cardId: number, markerId: string, name: string, count: number): void {
    const message = this.protocol.createMessage('RemoveMarkerReq', {
      card: cardId,
      id: markerId,
      name,
      count,
      origCount: count,
      isScriptChange: false,
    });
    this.send(message);
  }

  setCounter(name: string, value: number): void {
    const message = this.protocol.createMessage('CounterReq', {
      counter: name,
      value,
      isScriptChange: false,
    });
    this.send(message);
  }

  loadDeck(deckData: any): void {
    const message = this.protocol.createMessage('LoadDeck', deckData);
    this.send(message);
  }

  ready(): void {
    const message = this.protocol.createMessage('Ready', {
      player: parseInt(this.playerId || '0', 10),
    });
    this.send(message);
  }

  leave(): void {
    const message = this.protocol.createMessage('Leave', {
      player: parseInt(this.playerId || '0', 10),
    });
    this.send(message);
    this.disconnect();
  }

  private sendPing(): void {
    const message = this.protocol.createMessage('Ping', {});
    this.send(message);
  }

  private send(data: Buffer): void {
    if (!this.socket || !this.connected) {
      console.warn('Cannot send: not connected');
      return;
    }

    // Prepend length
    const lengthBuffer = Buffer.alloc(4);
    lengthBuffer.writeInt32LE(data.length, 0);
    const fullMessage = Buffer.concat([lengthBuffer, data]);

    this.socket.send(fullMessage);
  }

  // Getters
  get isConnected(): boolean {
    return this.connected;
  }

  get currentGameState(): GameState {
    return this.gameState;
  }

  get currentPlayerId(): string | null {
    return this.playerId;
  }
}
