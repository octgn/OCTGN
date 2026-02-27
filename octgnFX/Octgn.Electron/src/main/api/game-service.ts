import { BrowserWindow } from 'electron';
import { GameConnection } from '../protocol/connection';
import { MessageType } from '../protocol/types';
import type { GameState, ChatMessage, Player, Card, Group } from '../../shared/types';
import { IPC_CHANNELS } from '../../shared/types';

/**
 * Manages the active game connection and bridges protocol messages
 * to the renderer process via IPC.
 */
export class GameService {
  private connection: GameConnection | null = null;
  private gameState: GameState | null = null;
  private localPlayerId: number = 0;
  private chatMessages: ChatMessage[] = [];
  private chatIdCounter: number = 0;

  /**
   * Join a game server and start receiving state updates.
   */
  async joinGame(
    host: string,
    port: number,
    nickname: string,
    userId: string,
    gameId: string,
    gameVersion: string,
    password: string = '',
    spectator: boolean = false,
  ): Promise<{ success: boolean; error?: string }> {
    try {
      this.connection = new GameConnection({ host, port });

      this.setupMessageHandlers();

      await this.connection.connect();

      // Send Hello handshake
      this.connection.sendMessage(MessageType.Hello, 0, {
        nick: nickname,
        userId,
        pkey: BigInt(0),
        client: 'OCTGN-Electron',
        clientVer: '0.1.0',
        octgnVer: '3.4.424.0',
        gameId,
        gameVersion,
        password,
        spectator,
      });

      return { success: true };
    } catch (err) {
      return { success: false, error: (err as Error).message };
    }
  }

  /**
   * Leave the current game.
   */
  async leaveGame(): Promise<void> {
    if (this.connection) {
      this.connection.sendMessage(MessageType.Leave, 0, {
        player: this.localPlayerId,
      });
      this.connection.disconnect();
      this.connection = null;
    }
    this.gameState = null;
    this.chatMessages = [];
  }

  /**
   * Send a chat message.
   */
  sendChat(text: string): void {
    this.connection?.sendMessage(MessageType.ChatReq, 0, { text });
  }

  /**
   * Send a card move request.
   */
  moveCards(cardIds: number[], groupId: number, indices: number[], faceUp: boolean[], isScript: boolean = false): void {
    this.connection?.sendMessage(MessageType.MoveCardReq, 0, {
      id: cardIds,
      group: groupId,
      idx: indices,
      faceUp,
      isScriptMove: isScript,
    });
  }

  /**
   * Send a card move to position request.
   */
  moveCardsAt(cardIds: number[], x: number[], y: number[], indices: number[], faceUp: boolean[], isScript: boolean = false): void {
    this.connection?.sendMessage(MessageType.MoveCardAtReq, 0, {
      id: cardIds,
      x,
      y,
      idx: indices,
      faceUp,
      isScriptMove: isScript,
    });
  }

  /**
   * Request next turn.
   */
  nextTurn(setActive: boolean = true, force: boolean = false): void {
    this.connection?.sendMessage(MessageType.NextTurn, 0, {
      player: this.localPlayerId,
      setActive,
      force,
    });
  }

  /**
   * Flip a card face up/down.
   */
  flipCard(cardId: number, faceUp: boolean): void {
    this.connection?.sendMessage(MessageType.TurnReq, 0, {
      card: cardId,
      up: faceUp,
    });
  }

  /**
   * Rotate a card.
   */
  rotateCard(cardId: number, rotation: number): void {
    this.connection?.sendMessage(MessageType.RotateReq, 0, {
      card: cardId,
      rot: rotation,
    });
  }

  /**
   * Update a counter value.
   */
  setCounter(counterId: number, value: number): void {
    this.connection?.sendMessage(MessageType.CounterReq, 0, {
      counter: counterId,
      value,
      isScriptChange: false,
    });
  }

  /**
   * Load a deck.
   */
  loadDeck(
    ids: number[],
    types: string[],
    groups: number[],
    sizes: string[],
    sleeve: string = '',
    limited: boolean = false,
  ): void {
    this.connection?.sendMessage(MessageType.LoadDeck, 0, {
      id: ids,
      type: types,
      group: groups,
      size: sizes,
      sleeve,
      limited,
    });
  }

  get isConnected(): boolean {
    return this.connection?.isConnected ?? false;
  }

  private setupMessageHandlers(): void {
    if (!this.connection) return;

    // Welcome - we've been assigned a player ID
    this.connection.on('Welcome', (params: Record<string, unknown>) => {
      this.localPlayerId = params.id as number;
      this.initializeGameState(
        params.gameSessionId as string,
        params.gameName as string,
      );
      this.broadcastState();
    });

    // New player joined
    this.connection.on('NewPlayer', (params: Record<string, unknown>) => {
      if (!this.gameState) return;
      const player: Player = {
        id: params.id as number,
        name: params.nick as string,
        color: '#3b82f6',
        isHost: false,
        isSpectator: params.spectator as boolean,
        groups: [],
        counters: [],
        globalVariables: {},
      };
      this.gameState.players.push(player);
      this.broadcastState();
    });

    // Player left
    this.connection.on('Leave', (params: Record<string, unknown>) => {
      if (!this.gameState) return;
      this.gameState.players = this.gameState.players.filter(
        (p) => p.id !== (params.player as number),
      );
      this.broadcastState();
    });

    // Chat message received
    this.connection.on('Chat', (params: Record<string, unknown>) => {
      const msg: ChatMessage = {
        id: String(this.chatIdCounter++),
        playerId: params.player as number,
        playerName: this.getPlayerName(params.player as number),
        message: params.text as string,
        timestamp: Date.now(),
        isSystem: false,
      };
      this.chatMessages.push(msg);
      if (this.gameState) {
        this.gameState.chatMessages = [...this.chatMessages];
      }
      this.broadcastState();
    });

    // Print (system message)
    this.connection.on('Print', (params: Record<string, unknown>) => {
      const msg: ChatMessage = {
        id: String(this.chatIdCounter++),
        playerId: params.player as number,
        playerName: 'System',
        message: params.text as string,
        timestamp: Date.now(),
        isSystem: true,
      };
      this.chatMessages.push(msg);
      if (this.gameState) {
        this.gameState.chatMessages = [...this.chatMessages];
      }
      this.broadcastState();
    });

    // Settings
    this.connection.on('Settings', (_params: Record<string, unknown>) => {
      // Store game settings - for now just acknowledge
      this.broadcastState();
    });

    // Game started
    this.connection.on('Start', () => {
      if (this.gameState) {
        this.gameState.isStarted = true;
      }
      this.broadcastState();
    });

    // Next turn
    this.connection.on('NextTurn', (params: Record<string, unknown>) => {
      if (!this.gameState) return;
      this.gameState.turnNumber++;
      if (params.setActive) {
        this.gameState.activePlayer = params.player as number;
      }
      this.broadcastState();
    });

    // Phase change
    this.connection.on('SetPhase', (params: Record<string, unknown>) => {
      if (!this.gameState) return;
      this.gameState.phase = params.phase as number;
      this.broadcastState();
    });

    // Counter update
    this.connection.on('Counter', (params: Record<string, unknown>) => {
      if (!this.gameState) return;
      const player = this.gameState.players.find(
        (p) => p.id === (params.player as number),
      );
      if (player) {
        const counter = player.counters.find(
          (c) => c.id === (params.counter as number),
        );
        if (counter) {
          counter.value = params.value as number;
        }
      }
      this.broadcastState();
    });

    // Card moved to group
    this.connection.on('MoveCard', (params: Record<string, unknown>) => {
      if (!this.gameState) return;
      const cardIds = params.id as number[];
      const groupId = params.group as number;
      const faceUp = params.faceUp as boolean[];

      for (let i = 0; i < cardIds.length; i++) {
        const card = this.findCard(cardIds[i]);
        if (card) {
          card.groupId = String(groupId);
          card.faceUp = faceUp[i] ?? card.faceUp;
        }
      }
      this.broadcastState();
    });

    // Card moved to position on table
    this.connection.on('MoveCardAt', (params: Record<string, unknown>) => {
      if (!this.gameState) return;
      const cardIds = params.id as number[];
      const xs = params.x as number[];
      const ys = params.y as number[];
      const faceUp = params.faceUp as boolean[];

      for (let i = 0; i < cardIds.length; i++) {
        const card = this.findCard(cardIds[i]);
        if (card) {
          card.position = { x: xs[i], y: ys[i] };
          card.faceUp = faceUp[i] ?? card.faceUp;
        }
      }
      this.broadcastState();
    });

    // Card turned face up/down
    this.connection.on('Turn', (params: Record<string, unknown>) => {
      const card = this.findCard(params.card as number);
      if (card) {
        card.faceUp = params.up as boolean;
      }
      this.broadcastState();
    });

    // Card rotated
    this.connection.on('Rotate', (params: Record<string, unknown>) => {
      const card = this.findCard(params.card as number);
      if (card) {
        card.rotation = (params.rot as number) * 90;
      }
      this.broadcastState();
    });

    // Card highlight
    this.connection.on('Highlight', (params: Record<string, unknown>) => {
      const card = this.findCard(params.card as number);
      if (card) {
        card.highlighted = (params.color as string) || undefined;
      }
      this.broadcastState();
    });

    // Player color
    this.connection.on('SetPlayerColor', (params: Record<string, unknown>) => {
      if (!this.gameState) return;
      const player = this.gameState.players.find(
        (p) => p.id === (params.player as number),
      );
      if (player) {
        player.color = params.color as string;
      }
      this.broadcastState();
    });

    // Error
    this.connection.on('Error', (params: Record<string, unknown>) => {
      this.addSystemMessage(`Error: ${params.msg as string}`);
      this.broadcastState();
    });

    // Player disconnect
    this.connection.on('PlayerDisconnect', (params: Record<string, unknown>) => {
      const name = this.getPlayerName(params.player as number);
      this.addSystemMessage(`${name} disconnected`);
      this.broadcastState();
    });

    // Game state sync (from another player or server)
    this.connection.on('GameState', (params: Record<string, unknown>) => {
      try {
        const stateJson = params.state as string;
        if (stateJson) {
          const parsed = JSON.parse(stateJson);
          // Merge received state with our local state
          if (this.gameState && parsed) {
            Object.assign(this.gameState, parsed);
          }
        }
      } catch {
        // Ignore malformed state
      }
      this.broadcastState();
    });
  }

  private initializeGameState(gameId: string, gameName: string): void {
    this.gameState = {
      gameId,
      gameName,
      players: [],
      localPlayerId: this.localPlayerId,
      table: { cards: [] },
      turnNumber: 0,
      activePlayer: 0,
      phase: 0,
      chatMessages: [],
      isStarted: false,
    };
  }

  private broadcastState(): void {
    if (!this.gameState) return;
    const windows = BrowserWindow.getAllWindows();
    for (const win of windows) {
      win.webContents.send(IPC_CHANNELS.GAME_STATE_UPDATE, this.gameState);
    }
  }

  private findCard(cardId: number): Card | undefined {
    if (!this.gameState) return undefined;

    // Check table cards
    const tableCard = this.gameState.table.cards.find(
      (c) => c.id === String(cardId),
    );
    if (tableCard) return tableCard;

    // Check player group cards
    for (const player of this.gameState.players) {
      for (const group of player.groups) {
        const card = group.cards.find((c) => c.id === String(cardId));
        if (card) return card;
      }
    }
    return undefined;
  }

  private getPlayerName(playerId: number): string {
    const player = this.gameState?.players.find((p) => p.id === playerId);
    return player?.name ?? `Player ${playerId}`;
  }

  private addSystemMessage(text: string): void {
    const msg: ChatMessage = {
      id: String(this.chatIdCounter++),
      playerId: 0,
      playerName: 'System',
      message: text,
      timestamp: Date.now(),
      isSystem: true,
    };
    this.chatMessages.push(msg);
    if (this.gameState) {
      this.gameState.chatMessages = [...this.chatMessages];
    }
  }
}
