import * as net from 'net';
import { EventEmitter } from 'events';
import { BinaryProtocol } from './BinaryProtocol';
import { GameState } from './GameState';
import { Player } from './Player';

export interface ServerConfig {
  port: number;
  maxPlayers?: number;
  broadcastPort?: number;
}

export class GameServer extends EventEmitter {
  private server: net.Server | null = null;
  private port: number;
  private maxPlayers: number;
  private broadcastPort: number;
  private protocol: BinaryProtocol;
  private gameState: GameState;
  private players: Map<string, Player> = new Map();
  private isRunning = false;

  constructor(config: ServerConfig) {
    super();
    this.port = config.port || 32458;
    this.maxPlayers = config.maxPlayers || 8;
    this.broadcastPort = config.broadcastPort || 32459;
    this.protocol = new BinaryProtocol();
    this.gameState = new GameState();
  }

  async start(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.server = net.createServer((socket) => {
        this.handleConnection(socket);
      });

      this.server.on('error', (err) => {
        this.emit('error', err);
        reject(err);
      });

      // Enable SO_REUSEADDR for quick restarts
      this.server.listen(this.port, () => {
        this.isRunning = true;
        this.emit('started', { port: this.port });
        resolve();
      });
    });
  }

  async stop(): Promise<void> {
    return new Promise((resolve) => {
      if (!this.server) {
        resolve();
        return;
      }

      // Disconnect all players
      for (const player of this.players.values()) {
        player.disconnect();
      }
      this.players.clear();

      this.server.close(() => {
        this.isRunning = false;
        this.emit('stopped');
        resolve();
      });
    });
  }

  private handleConnection(socket: net.Socket): void {
    const playerId = this.generatePlayerId();
    const player = new Player(playerId, socket, this.protocol, this.gameState);

    player.on('disconnect', () => {
      this.players.delete(playerId);
      this.broadcastPlayerLeave(player);
      this.emit('player-disconnected', player);
    });

    player.on('message', (msg) => {
      this.handlePlayerMessage(player, msg);
    });

    player.on('ready', () => {
      this.players.set(playerId, player);
      this.broadcastPlayerJoin(player);
      this.emit('player-joined', player);
    });

    if (this.players.size >= this.maxPlayers) {
      player.sendError('Server is full');
      player.disconnect();
      return;
    }
  }

  private handlePlayerMessage(player: Player, message: any): void {
    const { type, data } = message;

    switch (type) {
      case 'Chat':
        this.broadcastChat(player, data.text);
        break;
      case 'MoveCard':
        this.handleMoveCard(player, data);
        break;
      case 'Turn':
        this.handleTurnCard(player, data);
        break;
      // ... more message types
      default:
        console.log(`Unhandled message type: ${type}`);
    }
  }

  private broadcastChat(player: Player, text: string): void {
    const message = this.protocol.createMessage('Chat', {
      player: player.id,
      text,
    });
    this.broadcast(message, player);
  }

  private handleMoveCard(player: Player, data: any): void {
    // Update game state
    this.gameState.moveCard(data.cardIds, data.toGroup, data.toIndex, data.faceUp);

    // Broadcast to all other players
    const message = this.protocol.createMessage('MoveCard', {
      player: player.id,
      ...data,
    });
    this.broadcast(message);
  }

  private handleTurnCard(player: Player, data: any): void {
    this.gameState.turnCard(data.cardId, data.faceUp);

    const message = this.protocol.createMessage('Turn', {
      player: player.id,
      card: data.cardId,
      up: data.faceUp,
    });
    this.broadcast(message);
  }

  private broadcastPlayerJoin(player: Player): void {
    const message = this.protocol.createMessage('NewPlayer', {
      id: player.id,
      nick: player.nickname,
      userId: player.userId,
      pkey: player.publicKey,
      tableSide: player.tableSide,
      spectator: player.isSpectator,
    });
    this.broadcast(message, player);
  }

  private broadcastPlayerLeave(player: Player): void {
    const message = this.protocol.createMessage('Leave', {
      player: player.id,
    });
    this.broadcast(message);
  }

  private broadcast(data: Buffer, excludePlayer?: Player): void {
    for (const player of this.players.values()) {
      if (player !== excludePlayer) {
        player.send(data);
      }
    }
  }

  private generatePlayerId(): string {
    return Math.random().toString(36).substring(2, 10);
  }

  get isServerRunning(): boolean {
    return this.isRunning;
  }

  get playerCount(): number {
    return this.players.size;
  }
}
