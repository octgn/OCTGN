import { BrowserWindow, app } from 'electron';
import { writeFile, readFile, unlink } from 'fs/promises';
import { join } from 'path';
import { GameConnection } from '../protocol/connection';
import { MessageType, type ProtocolMessage } from '../protocol/types';
import type { GameState, ChatMessage, Player, Card, Group, Counter, Marker, Deck, DeckSectionDef } from '../../shared/types';
import { IPC_CHANNELS, GroupVisibility } from '../../shared/types';
import { ScriptEngine } from '../scripting/script-engine';
import { CardResolver } from '../games/card-resolver';
import { ImageResolver } from '../games/image-resolver';
import { log, logError } from '../logger';

export interface SavedConnectionInfo {
  host: string;
  port: number;
  nickname: string;
  userId: string;
  playerId: number;
  pkey: string;
  gameId: string;
  gameVersion: string;
  password: string;
  isSpectator: boolean;
  connectedAt: number;
}

/**
 * Player color palette matching the WPF OCTGN client (Player.cs _playerColors).
 * 14 colors, indexed 0-13. Player ID maps via (id - 1) % 14.
 * IDs 0 and 255 get black (global player / spectator).
 */
const PLAYER_COLORS = [
  '#008000', // 0: dark green
  '#cc0000', // 1: dark red
  '#000080', // 2: dark blue
  '#800080', // 3: dark magenta
  '#cc6600', // 4: dark orange
  '#008080', // 5: dark cyan
  '#664b32', // 6: brown
  '#502060', // 7: dark purple
  '#808000', // 8: olive
  '#ff0000', // 9: bright red
  '#808080', // 10: gray
  '#206020', // 11: dark green
  '#ff00ff', // 12: magenta
  '#0000ff', // 13: bright blue
];

function playerColorById(id: number): string {
  if (id === 0 || id === 255) return '#000000';
  return PLAYER_COLORS[(id - 1) % PLAYER_COLORS.length];
}

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
  private scriptEngine: ScriptEngine = new ScriptEngine();
  private isSpectator: boolean = false;
  private cardResolver: CardResolver = new CardResolver();
  private imageResolver: ImageResolver = new ImageResolver();
  private currentGameId: string = '';
  private pendingBoardName: string | undefined;
  private nextCardUniqueId: number = 1;
  private joinParams: { host: string; port: number; nickname: string; userId: string; gameVersion: string; password: string } | null = null;

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
      log('GAME', `Joining game at ${host}:${port} as ${nickname} (spectator=${spectator})`);
      this.isSpectator = spectator;
      this.currentGameId = gameId;
      this.joinParams = { host, port, nickname, userId, gameVersion, password };
      this.connection = new GameConnection({ host, port });

      this.setupMessageHandlers();

      await this.connection.connect();
      log('GAME', 'TCP connection established, sending Hello handshake');

      // Send Hello handshake
      log('GAME', `Sending Hello: nick=${nickname} userId=${userId} gameId=${gameId} gameVer=${gameVersion} spectator=${spectator}`);
      this.connection.sendMessage(MessageType.Hello, 0, {
        nick: nickname,
        userId,
        pkey: BigInt(0),
        client: 'OCTGN-Electron',
        clientVer: '3.4.426.0',
        octgnVer: '3.4.426.0',
        gameId,
        gameVersion,
        password,
        spectator,
      });

      log('GAME', 'Hello handshake sent, join returning success');
      return { success: true };
    } catch (err) {
      logError('GAME', err);
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
    this.joinParams = null;
    this.clearConnectionInfo();
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
    log('MOVE', `moveCards: cardIds=[${cardIds}] groupId=${groupId} faceUp=[${faceUp}] connected=${!!this.connection}`);
    if (!this.connection) {
      log('MOVE', 'WARNING: moveCards called but connection is null — message not sent');
      return;
    }
    this.connection.sendMessage(MessageType.MoveCardReq, 0, {
      id: cardIds,
      group: groupId,
      idx: indices,
      faceUp,
      isScriptMove: isScript,
    });
    // Optimistic local update — move cards immediately before server echo
    if (this.gameState) {
      for (let i = 0; i < cardIds.length; i++) {
        const card = this.findCard(cardIds[i]);
        if (card) {
          this.removeCard(cardIds[i]);
          card.groupId = String(groupId);
          card.faceUp = faceUp[i] ?? card.faceUp;
          this.addCardToGroup(card, groupId);
        }
      }
      this.broadcastState();
    }
  }

  /**
   * Send a card move to position request.
   */
  moveCardsAt(cardIds: number[], x: number[], y: number[], indices: number[], faceUp: boolean[], isScript: boolean = false): void {
    log('MOVE', `moveCardsAt: cardIds=[${cardIds}] x=[${x}] y=[${y}] faceUp=[${faceUp}] connected=${!!this.connection}`);
    if (!this.connection) {
      log('MOVE', 'WARNING: moveCardsAt called but connection is null — message not sent');
      return;
    }
    this.connection.sendMessage(MessageType.MoveCardAtReq, 0, {
      id: cardIds,
      x,
      y,
      idx: indices,
      faceUp,
      isScriptMove: isScript,
    });
    // Optimistic local update — move cards to table position immediately
    if (this.gameState) {
      for (let i = 0; i < cardIds.length; i++) {
        const card = this.findCard(cardIds[i]);
        if (card) {
          this.removeCard(cardIds[i]);
          card.position = { x: x[i], y: y[i] };
          card.faceUp = faceUp[i] ?? card.faceUp;
          card.groupId = 'table';
          this.gameState.table.cards.push(card);
        }
      }
      this.broadcastState();
    }
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
   * Peek at a card (view it without revealing).
   */
  peekCard(cardId: number): void {
    this.connection?.sendMessage(MessageType.PeekReq, 0, {
      card: cardId,
    });
  }

  /**
   * Stop peeking at a card.
   */
  unpeekCard(cardId: number): void {
    this.connection?.sendMessage(MessageType.UntargetReq, 0, {
      card: cardId,
      isScriptChange: false,
    });
  }

  /**
   * Target a card.
   */
  targetCard(cardId: number, _playerId: number, isScriptChange: boolean = false): void {
    this.connection?.sendMessage(MessageType.TargetReq, 0, {
      card: cardId,
      isScriptChange,
    });
  }

  /**
   * Highlight a card with a color.
   */
  highlightCard(cardId: number, color: string): void {
    this.connection?.sendMessage(MessageType.Highlight, 0, {
      card: cardId,
      color,
    });
  }

  /**
   * Add a marker to a card.
   */
  addMarker(cardId: number, markerId: string, markerName: string, count: number, origCount: number = 0): void {
    this.connection?.sendMessage(MessageType.AddMarkerReq, 0, {
      card: cardId,
      id: markerId,
      name: markerName,
      count,
      origCount,
      isScriptChange: false,
    });
  }

  /**
   * Remove a marker from a card.
   */
  removeMarker(cardId: number, markerId: string, markerName: string, count: number, origCount: number = 0): void {
    this.connection?.sendMessage(MessageType.RemoveMarkerReq, 0, {
      card: cardId,
      id: markerId,
      name: markerName,
      count,
      origCount,
      isScriptChange: false,
    });
  }

  /**
   * Shuffle a group (e.g., deck).
   */
  shuffleGroup(groupId: number): void {
    this.connection?.sendMessage(MessageType.ShuffleDeprecated, 0, {
      group: groupId,
      card: [],
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

  /**
   * Generate a unique card ID matching WPF's GenerateCardId.
   */
  private generateCardId(): number {
    return (this.localPlayerId << 16) | (this.nextCardUniqueId++ & 0xFFFF);
  }

  /**
   * Load a deck from a parsed Deck object.
   * Maps deck sections to player groups using game definition, generates card IDs,
   * and sends the LoadDeck protocol message.
   */
  loadDeckFromFile(deck: Deck): void {
    const gameDef = this.currentGameId
      ? this.cardResolver.getGameDefinition(this.currentGameId)
      : undefined;

    const playerDeckSections: DeckSectionDef[] = gameDef?.deckSections ?? [];
    const sharedDeckSections: DeckSectionDef[] = gameDef?.sharedDeckSections ?? [];

    const ids: number[] = [];
    const types: string[] = [];
    const groups: number[] = [];
    const sizes: string[] = [];

    for (const section of deck.sections) {
      // WPF uses the deck file's shared flag to pick which definition dictionary to search
      const isShared = section.shared ?? false;
      const sectionDefs = isShared ? sharedDeckSections : playerDeckSections;
      // Fallback: if not found in the primary list, try the other
      const allDefs = [...sectionDefs, ...(isShared ? playerDeckSections : sharedDeckSections)];
      const groupId = this.resolveGroupIdForSection(section.name, allDefs, gameDef);

      for (const card of section.cards) {
        for (let i = 0; i < card.quantity; i++) {
          ids.push(this.generateCardId());
          types.push(card.id); // card definition ID (GUID)
          groups.push(groupId);
          sizes.push(''); // default size
        }
      }
    }

    if (ids.length === 0) {
      log('GAME', 'loadDeckFromFile: no cards to load');
      return;
    }

    log('GAME', `loadDeckFromFile: loading ${ids.length} cards into ${new Set(groups).size} groups`);
    this.loadDeck(ids, types, groups, sizes, deck.sleeveUrl ?? '', false);
  }

  /**
   * Resolve the numeric group ID for a deck section name.
   * Uses game definition deck sections to map section name → group name → player group ID.
   * Shared sections map to the global player (ID 0).
   */
  private resolveGroupIdForSection(
    sectionName: string,
    deckSectionDefs: DeckSectionDef[],
    gameDef?: { players: { groups: { name: string }[] }[]; globalPlayer?: { groups: { name: string }[] } },
  ): number {
    // Step 1: find the deck section definition matching this section name
    const sectionDef = deckSectionDefs.find(
      (d) => d.name.toLowerCase() === sectionName.toLowerCase(),
    );
    const targetGroupName = sectionDef?.group ?? sectionName;
    const isShared = sectionDef?.shared ?? false;

    // Step 2: for non-shared sections, find the local player's group matching that group name
    if (!isShared) {
      const localPlayer = this.gameState?.players.find((p) => p.id === this.localPlayerId);
      if (localPlayer) {
        const group = localPlayer.groups.find(
          (g) => g.name.toLowerCase() === targetGroupName.toLowerCase(),
        );
        if (group) return Number(group.id);
      }
    }

    // Step 3: look up group index in the appropriate group list from game definition
    if (isShared && gameDef?.globalPlayer) {
      // Shared sections use the global player (ID 0)
      const groupIndex = gameDef.globalPlayer.groups.findIndex(
        (g) => g.name.toLowerCase() === targetGroupName.toLowerCase(),
      );
      if (groupIndex >= 0) {
        // WPF Group.Id encoding: 0x01000000 | (Owner.Id << 16) | Def.Id
        // Global player ID is 0
        return 0x01000000 | (0 << 16) | (groupIndex + 1);
      }
    } else if (gameDef?.players && gameDef.players.length > 0) {
      const playerDef = gameDef.players[0];
      const groupIndex = playerDef.groups.findIndex(
        (g) => g.name.toLowerCase() === targetGroupName.toLowerCase(),
      );
      if (groupIndex >= 0) {
        // WPF Group.Id encoding: 0x01000000 | (Owner.Id << 16) | Def.Id
        // Def.Id is 1-based group index
        return 0x01000000 | (this.localPlayerId << 16) | (groupIndex + 1);
      }
    }

    // Final fallback: first group of local player, or table (0x01000000)
    const localPlayer = this.gameState?.players.find((p) => p.id === this.localPlayerId);
    if (localPlayer && localPlayer.groups.length > 0) {
      return Number(localPlayer.groups[0].id);
    }
    return 0x01000000;
  }

  /**
   * Execute a script function via RemoteCall.
   */
  executeScript(functionName: string, args: string = ''): void {
    this.connection?.sendMessage(MessageType.RemoteCall, 0, {
      player: this.localPlayerId,
      function: functionName,
      args,
    });
  }

  /**
   * Send game settings (host only).
   */
  sendSettings(twoSidedTable: boolean, allowSpectators: boolean, muteSpectators: boolean, allowCardList: boolean): void {
    this.connection?.sendMessage(MessageType.Settings, 0, {
      twoSidedTable,
      allowSpectators,
      muteSpectators,
      allowCardList,
    });
  }

  /**
   * Send player settings (side/spectator toggle).
   */
  sendPlayerSettings(playerId: number, invertedTable: boolean, spectator: boolean): void {
    this.connection?.sendMessage(MessageType.PlayerSettings, 0, {
      playerId,
      invertedTable,
      spectator,
    });
  }

  /**
   * Boot (kick) a player from the game (host only).
   */
  bootPlayer(playerId: number, reason: string = ''): void {
    this.connection?.sendMessage(MessageType.Boot, 0, {
      player: playerId,
      reason,
    });
  }

  /**
   * Start the game (host only).
   */
  startGame(): void {
    this.connection?.sendMessage(MessageType.Start, 0, {});
  }

  /**
   * Get the script engine instance.
   */
  getScriptEngine(): ScriptEngine {
    return this.scriptEngine;
  }

  get isConnected(): boolean {
    return this.connection?.isConnected ?? false;
  }

  /**
   * Get the ImageResolver instance (used by the asset protocol handler).
   */
  getImageResolver(): ImageResolver {
    return this.imageResolver;
  }

  /**
   * Get the current game state (used by GET_APP_STATE for renderer recovery).
   */
  getState(): GameState | null {
    return this.gameState;
  }

  /**
   * Reconnect to a game server using HelloAgain after a main process restart.
   */
  async reconnectGame(info: SavedConnectionInfo): Promise<{ success: boolean; error?: string }> {
    try {
      log('GAME', `Reconnecting to game at ${info.host}:${info.port} as ${info.nickname} (pid=${info.playerId})`);
      this.isSpectator = info.isSpectator;
      this.currentGameId = info.gameId;
      this.localPlayerId = info.playerId;
      this.connection = new GameConnection({ host: info.host, port: info.port });

      this.setupMessageHandlers();

      await this.connection.connect();
      log('GAME', 'TCP connection established, sending HelloAgain handshake');

      this.connection.sendMessage(MessageType.HelloAgain, 0, {
        pid: info.playerId,
        nick: info.nickname,
        userId: info.userId,
        pkey: BigInt(info.pkey),
        client: 'OCTGN-Electron',
        clientVer: '3.4.426.0',
        octgnVer: '3.4.426.0',
        gameId: info.gameId,
        gameVersion: info.gameVersion,
        password: info.password,
      });

      log('GAME', 'HelloAgain sent, reconnect returning success');
      return { success: true };
    } catch (err) {
      logError('GAME', err);
      await this.clearConnectionInfo();
      return { success: false, error: (err as Error).message };
    }
  }

  private getConnectionInfoPath(): string {
    return join(app.getPath('userData'), 'game-connection.json');
  }

  private async saveConnectionInfo(host: string, port: number, nickname: string, userId: string, gameId: string, gameVersion: string, password: string): Promise<void> {
    const info: SavedConnectionInfo = {
      host,
      port,
      nickname,
      userId,
      playerId: this.localPlayerId,
      pkey: '0',
      gameId,
      gameVersion,
      password,
      isSpectator: this.isSpectator,
      connectedAt: Date.now(),
    };
    try {
      await writeFile(this.getConnectionInfoPath(), JSON.stringify(info), 'utf-8');
      log('GAME', 'Saved connection info for potential reconnection');
    } catch (err) {
      logError('GAME', err);
    }
  }

  async clearConnectionInfo(): Promise<void> {
    try {
      await unlink(this.getConnectionInfoPath());
    } catch {
      // File may not exist
    }
  }

  static async loadConnectionInfo(): Promise<SavedConnectionInfo | null> {
    try {
      const filePath = join(app.getPath('userData'), 'game-connection.json');
      const data = await readFile(filePath, 'utf-8');
      const info: SavedConnectionInfo = JSON.parse(data);
      // Only use if less than 90 seconds old
      if (Date.now() - info.connectedAt > 90_000) {
        log('GAME', 'Connection info too old, ignoring');
        await unlink(filePath).catch(() => {});
        return null;
      }
      return info;
    } catch {
      return null;
    }
  }

  // ---------------------------------------------------------------------------
  // Helper: extract params from ProtocolMessage
  // ---------------------------------------------------------------------------

  private p(msg: ProtocolMessage): Record<string, unknown> {
    return msg.params;
  }

  // ---------------------------------------------------------------------------
  // Message handler setup
  // ---------------------------------------------------------------------------

  private setupMessageHandlers(): void {
    if (!this.connection) return;

    // Log all incoming messages (BigInt-safe serialization)
    this.connection.on('message', (msg: ProtocolMessage) => {
      const safeParams = JSON.stringify(msg.params, (_k, v) => typeof v === 'bigint' ? v.toString() : v);
      const truncated = safeParams.length > 500 ? safeParams.slice(0, 500) + '...' : safeParams;
      log('GAME', `<-- type=${msg.type} (${MessageType[msg.type] ?? '?'}) ${truncated}`);
    });

    // Log connection lifecycle events
    this.connection.on('connected', () => log('GAME', 'Connection established'));
    this.connection.on('disconnected', (hadError: boolean) => {
      log('GAME', `Disconnected (hadError=${hadError})`);
      if (this.gameState) {
        this.gameState.connectionStatus = 'reconnecting';
        this.broadcastState();
      }
    });
    this.connection.on('error', (err: Error) => logError('GAME', err));
    this.connection.on('reconnecting', (attempt: number) => log('GAME', `Reconnecting attempt ${attempt}`));

    // On TCP auto-reconnect, re-send HelloAgain to restore the game session
    this.connection.on('reconnected', () => {
      log('GAME', 'TCP reconnected, sending HelloAgain');
      if (this.joinParams && this.connection) {
        this.connection.sendMessage(MessageType.HelloAgain, 0, {
          pid: this.localPlayerId,
          nick: this.joinParams.nickname,
          userId: this.joinParams.userId,
          pkey: BigInt(0),
          client: 'OCTGN-Electron',
          clientVer: '3.4.426.0',
          octgnVer: '3.4.426.0',
          gameId: this.currentGameId,
          gameVersion: this.joinParams.gameVersion,
          password: this.joinParams.password,
        });
      }
    });

    // Kick — server rejected us
    this.connection.on('Kick', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const reason = (params.reason as string) || 'Unknown reason';
      log('GAME', `KICKED: ${reason}`);
      // Broadcast an error state to the renderer
      if (this.gameState) {
        this.gameState.connectionStatus = 'disconnected';
      } else {
        this.initializeGameState('', '');
        this.gameState!.connectionStatus = 'disconnected';
      }
      this.addSystemMessage(`Kicked from game: ${reason}`);
      this.broadcastState();
      this.clearConnectionInfo();
      // Disconnect
      this.connection?.disconnect();
      this.connection = null;
    });

    // Welcome - we've been assigned a player ID
    this.connection.on('Welcome', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      this.localPlayerId = params.id as number;
      this.initializeGameState(
        params.gameSessionId as string,
        params.gameName as string,
      );
      // Player ID 1 is the host
      if (this.localPlayerId === 1) {
        const localPlayer = this.gameState!.players.find(p => p.id === this.localPlayerId);
        if (localPlayer) localPlayer.isHost = true;
      }
      this.broadcastState();

      // Persist connection info for reconnection after main process restart
      if (this.joinParams) {
        this.saveConnectionInfo(
          this.joinParams.host,
          this.joinParams.port,
          this.joinParams.nickname,
          this.joinParams.userId,
          this.currentGameId,
          this.joinParams.gameVersion,
          this.joinParams.password,
        );
      }

      // Load card definitions in the background
      if (this.currentGameId) {
        this.cardResolver.loadGame(this.currentGameId).then(() => {
          const gameDef = this.cardResolver.getGameDefinition(this.currentGameId);
          if (gameDef && this.gameState) {
            this.gameState.gameDefinition = { name: gameDef.name, id: gameDef.id };
            // Populate table dimensions, background style, and background image from game definition
            if (gameDef.table) {
              this.gameState.table.width = gameDef.table.width || undefined;
              this.gameState.table.height = gameDef.table.height || undefined;
              this.gameState.table.backgroundStyle = gameDef.table.backgroundStyle;
              // Resolve table background image (e.g. wood texture) to an asset URL
              if (gameDef.table.background) {
                const encodedBgPath = encodeURIComponent(gameDef.table.background);
                this.gameState.table.backgroundUrl = `octgn-asset://game-file/${this.currentGameId}/${encodedBgPath}`;
              }
            }
            // Populate card sizes from game definition
            if (gameDef.defaultCardSize || (gameDef.cardWidth && gameDef.cardHeight)) {
              this.gameState.cardSize = gameDef.defaultCardSize
                ? { width: gameDef.defaultCardSize.width, height: gameDef.defaultCardSize.height }
                : { width: gameDef.cardWidth, height: gameDef.cardHeight };
            }
            if (gameDef.cardSizes) {
              const sizes: Record<string, { width: number; height: number }> = {};
              for (const [name, sizeDef] of Object.entries(gameDef.cardSizes)) {
                sizes[name] = { width: sizeDef.width, height: sizeDef.height };
              }
              this.gameState.cardSizes = sizes;
            }
            // Update existing card sizes to use game definition values
            this.updateCardSizesFromDefinition();
            // Re-resolve board now that definition is loaded
            if (this.pendingBoardName) {
              this.resolveBoardImage(this.pendingBoardName);
              this.pendingBoardName = undefined;
            }
            // Set default board from game definition if no board has been set yet
            // (WPF client does this at init via GameBoards[""] — the default board)
            if (!this.gameState.table.board) {
              const defaultBoard = gameDef.boards?.[0] ?? gameDef.table?.board;
              if (defaultBoard?.source) {
                this.resolveBoardImage(defaultBoard.name || '');
              }
            }
            // Initialize player groups from game definition for players that don't have groups yet
            this.initializePlayerGroupsFromDefinition(gameDef);
            // Initialize global player (shared groups) from game definition
            this.initializeGlobalPlayer(gameDef);
            // Update card names and images from now-loaded definitions
            this.updateCardNamesAndImages();
            this.broadcastState();
          }
        }).catch((err) => logError('GAME', err));
      }
    });

    // New player joined
    this.connection.on('NewPlayer', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      const playerId = params.id as number;
      const player: Player = {
        id: playerId,
        name: params.nick as string,
        color: playerColorById(playerId),
        isHost: playerId === 1,
        isSpectator: (params.spectator as boolean) ?? false,
        invertedTable: (params.tableSide as boolean) ?? false,
        groups: [],
        counters: [],
        globalVariables: {},
      };
      this.gameState.players.push(player);
      this.broadcastState();
    });

    // Player settings (inverted table, spectator status)
    this.connection.on('PlayerSettings', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      const player = this.gameState.players.find(
        (p) => p.id === (params.playerId as number),
      );
      if (player) {
        player.isSpectator = (params.spectator as boolean) ?? player.isSpectator;
        player.invertedTable = (params.invertedTable as boolean) ?? player.invertedTable;
      }
      this.broadcastState();
    });

    // Player left
    this.connection.on('Leave', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      this.gameState.players = this.gameState.players.filter(
        (p) => p.id !== (params.player as number),
      );
      this.broadcastState();
    });

    // Chat message received
    this.connection.on('Chat', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const playerId = params.player as number;
      const playerColor = this.gameState?.players.find(p => p.id === playerId)?.color;
      const chatMsg: ChatMessage = {
        id: String(this.chatIdCounter++),
        playerId,
        playerName: this.getPlayerName(playerId),
        message: params.text as string,
        timestamp: Date.now(),
        isSystem: false,
        color: playerColor,
      };
      this.chatMessages.push(chatMsg);
      if (this.gameState) {
        this.gameState.chatMessages = [...this.chatMessages];
      }
      this.broadcastState();
    });

    // Print (system message)
    this.connection.on('Print', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const printPlayerId = params.player as number;
      const playerColor = printPlayerId
        ? this.gameState?.players.find(p => p.id === printPlayerId)?.color ?? playerColorById(printPlayerId)
        : undefined;
      const chatMsg: ChatMessage = {
        id: String(this.chatIdCounter++),
        playerId: printPlayerId,
        playerName: 'System',
        message: params.text as string,
        timestamp: Date.now(),
        isSystem: true,
        color: playerColor,
      };
      this.chatMessages.push(chatMsg);
      if (this.gameState) {
        this.gameState.chatMessages = [...this.chatMessages];
      }
      this.broadcastState();
    });

    // Settings
    this.connection.on('Settings', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      // For spectators, initialize game state on Settings since Welcome may not come
      if (!this.gameState) {
        this.initializeGameState('', '');
      }
      log('GAME', `Settings: twoSided=${params.twoSidedTable} spectators=${params.allowSpectators} mute=${params.muteSpectators} cardList=${params.allowCardList}`);
      this.gameState!.useTwoSidedTable = params.twoSidedTable as boolean;
      this.gameState!.allowSpectators = (params.allowSpectators as boolean) ?? false;
      this.gameState!.muteSpectators = (params.muteSpectators as boolean) ?? false;
      this.gameState!.allowCardList = (params.allowCardList as boolean) ?? false;
      this.broadcastState();
    });

    // Game started
    this.connection.on('Start', (_msg: ProtocolMessage) => {
      if (!this.gameState) {
        this.initializeGameState('', '');
      }
      this.gameState!.isStarted = true;
      log('GAME', 'Game started');
      this.broadcastState();

      // Signal readiness to the server — the WPF host shows a "Waiting for Other
      // Users" dialog until every player sends Ready (#90). Without this, the host
      // waits indefinitely for the Electron player.
      this.connection?.sendMessage(MessageType.Ready, 0, { player: this.localPlayerId });
    });

    // A player signalled ready (echoed by the server to all players)
    this.connection.on('Ready', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const playerId = params.player as number;
      const player = this.gameState?.players.find(p => p.id === playerId);
      const name = player?.name || `Player ${playerId}`;
      log('GAME', `${name} is ready`);
      this.addSystemMessage(`${name} is ready`, playerId);
      this.broadcastState();
    });

    // Full GameState snapshot (sent to spectators joining mid-game)
    this.connection.on('GameState', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const stateJson = params.state as string;
      log('GAME', `GameState snapshot received (${stateJson.length} chars)`);
      try {
        this.parseGameStateSnapshot(stateJson);
        this.broadcastState();
      } catch (err) {
        logError('GAME', err);
      }
    });

    // Next turn
    this.connection.on('NextTurn', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      this.gameState.turnNumber++;
      if (params.setActive) {
        this.gameState.activePlayer = params.player as number;
      }
      this.addSystemMessage(`Turn ${this.gameState.turnNumber} — ${this.getPlayerName(params.player as number)}`, params.player as number);
      this.broadcastState();
    });

    // Phase change
    this.connection.on('SetPhase', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      this.gameState.phase = params.phase as number;
      this.broadcastState();
    });

    // SetActivePlayer
    this.connection.on('SetActivePlayer', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      this.gameState.activePlayer = params.player as number;
      this.broadcastState();
    });

    // ClearActivePlayer
    this.connection.on('ClearActivePlayer', (_msg: ProtocolMessage) => {
      if (!this.gameState) return;
      this.gameState.activePlayer = 0;
      this.broadcastState();
    });

    // Counter update
    this.connection.on('Counter', (msg: ProtocolMessage) => {
      const params = this.p(msg);
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
        } else {
          // Counter doesn't exist yet — create it
          player.counters.push({
            id: params.counter as number,
            name: `Counter ${params.counter}`,
            value: params.value as number,
          });
        }
      }
      this.broadcastState();
    });

    // LoadDeck — creates groups and cards for a player
    this.connection.on('LoadDeck', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      const ids = params.id as number[];
      const types = params.type as string[];
      const groups = params.group as number[];
      const sizes = params.size as string[];

      for (let i = 0; i < ids.length; i++) {
        const groupId = groups[i];
        const card = this.createCard(
          ids[i],
          types[i],
          sizes[i] ?? '',
          String(groupId),
        );
        this.addCardToGroup(card, groupId);
      }
      this.broadcastState();
    });

    // CreateCard — creates card(s) in a player group
    this.connection.on('CreateCard', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      const ids = params.id as number[];
      const types = params.type as string[];
      const sizes = params.size as string[];
      const groupId = params.group as number;

      for (let i = 0; i < ids.length; i++) {
        const card = this.createCard(
          ids[i],
          types[i],
          sizes[i] ?? '',
          String(groupId),
        );
        this.addCardToGroup(card, groupId);
      }
      this.broadcastState();
    });

    // CreateCardAt — creates card(s) at a table position
    this.connection.on('CreateCardAt', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      const ids = params.id as number[];
      const modelIds = params.modelId as string[];
      const xs = params.x as number[];
      const ys = params.y as number[];
      const faceUp = params.faceUp as boolean;

      for (let i = 0; i < ids.length; i++) {
        const card = this.createCard(ids[i], modelIds[i], '', 'table');
        card.position = { x: xs[i], y: ys[i] };
        card.faceUp = faceUp;
        this.gameState.table.cards.push(card);
      }
      this.broadcastState();
    });

    // DeleteCard — remove a card from the game
    this.connection.on('DeleteCard', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      const cardId = params.card as number;
      this.removeCard(cardId);
      this.broadcastState();
    });

    // Card moved to group
    this.connection.on('MoveCard', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      const cardIds = params.id as number[];
      const groupId = params.group as number;
      const faceUp = params.faceUp as boolean[];
      log('MOVE', `MoveCard response: cardIds=[${cardIds}] groupId=${groupId} faceUp=[${faceUp}]`);

      for (let i = 0; i < cardIds.length; i++) {
        const card = this.findCard(cardIds[i]);
        if (card) {
          // Remove from current location
          this.removeCard(cardIds[i]);
          // Update properties
          card.groupId = String(groupId);
          card.faceUp = faceUp[i] ?? card.faceUp;
          // Add to new group
          this.addCardToGroup(card, groupId);
        }
      }
      this.broadcastState();
    });

    // Card moved to position on table
    this.connection.on('MoveCardAt', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      const cardIds = params.id as number[];
      const xs = params.x as number[];
      const ys = params.y as number[];
      const faceUp = params.faceUp as boolean[];
      log('MOVE', `MoveCardAt response: cardIds=[${cardIds}] x=[${xs}] y=[${ys}] faceUp=[${faceUp}]`);

      for (let i = 0; i < cardIds.length; i++) {
        let card = this.findCard(cardIds[i]);
        if (card) {
          // Remove from current location
          this.removeCard(cardIds[i]);
          card.position = { x: xs[i], y: ys[i] };
          card.faceUp = faceUp[i] ?? card.faceUp;
          card.groupId = 'table';
          // Add to table
          this.gameState.table.cards.push(card);
        }
      }
      this.broadcastState();
    });

    // Card turned face up/down
    this.connection.on('Turn', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const card = this.findCard(params.card as number);
      if (card) {
        card.faceUp = params.up as boolean;
      }
      this.broadcastState();
    });

    // Card rotated
    this.connection.on('Rotate', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const card = this.findCard(params.card as number);
      if (card) {
        card.rotation = (params.rot as number) * 90;
      }
      this.broadcastState();
    });

    // Card highlight
    this.connection.on('Highlight', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const card = this.findCard(params.card as number);
      if (card) {
        card.highlighted = (params.color as string) || undefined;
      }
      this.broadcastState();
    });

    // Target — mark a card as targeted by a player
    this.connection.on('Target', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const card = this.findCard(params.card as number);
      if (card) {
        card.targetedBy = String(params.player);
      }
      this.broadcastState();
    });

    // Untarget — remove targeting from a card
    this.connection.on('Untarget', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const card = this.findCard(params.card as number);
      if (card) {
        card.targetedBy = undefined;
      }
      this.broadcastState();
    });

    // Peek — a player is peeking at a card
    this.connection.on('Peek', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const card = this.findCard(params.card as number);
      if (card) {
        const playerId = String(params.player);
        if (!card.peekingPlayers.includes(playerId)) {
          card.peekingPlayers.push(playerId);
        }
      }
      this.broadcastState();
    });

    // AddMarker — add a marker to a card
    this.connection.on('AddMarker', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const card = this.findCard(params.card as number);
      if (card) {
        const markerId = params.id as string;
        const existing = card.markers.find((m) => m.id === markerId);
        if (existing) {
          existing.count = params.count as number;
        } else {
          card.markers.push({
            id: markerId,
            name: params.name as string,
            iconUrl: '',
            count: params.count as number,
          });
        }
      }
      this.broadcastState();
    });

    // RemoveMarker — remove a marker from a card
    this.connection.on('RemoveMarker', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const card = this.findCard(params.card as number);
      if (card) {
        const markerId = params.id as string;
        const count = params.count as number;
        const existing = card.markers.find((m) => m.id === markerId);
        if (existing) {
          existing.count -= count;
          if (existing.count <= 0) {
            card.markers = card.markers.filter((m) => m.id !== markerId);
          }
        }
      }
      this.broadcastState();
    });

    // Shuffled — reorder cards in a group
    this.connection.on('Shuffled', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      const groupId = params.group as number;
      const cardIds = params.card as number[];

      // Find the group and reorder cards
      const group = this.findGroup(groupId);
      if (group && cardIds.length > 0) {
        const cardMap = new Map(group.cards.map((c) => [c.id, c]));
        const reordered: Card[] = [];
        for (const id of cardIds) {
          const card = cardMap.get(String(id));
          if (card) reordered.push(card);
        }
        // Any cards not in the new order stay at end
        for (const card of group.cards) {
          if (!reordered.includes(card)) reordered.push(card);
        }
        group.cards = reordered;
      }

      this.addSystemMessage(`${this.getPlayerName(params.player as number)} shuffled a pile`, params.player as number);
      this.broadcastState();
    });

    // SetCardProperty — update a card's custom property
    this.connection.on('SetCardProperty', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const card = this.findCard(params.id as number);
      if (card) {
        card.properties[params.name as string] = params.val as string;
      }
      this.broadcastState();
    });

    // GroupVis — group visibility change
    this.connection.on('GroupVis', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      const group = this.findGroup(params.group as number);
      if (group) {
        if (params.defined as boolean) {
          group.visibility = (params.visible as boolean)
            ? GroupVisibility.Everybody
            : GroupVisibility.Nobody;
        } else {
          group.visibility = GroupVisibility.Undefined;
        }
      }
      this.broadcastState();
    });

    // SetBoard — set table background image
    this.connection.on('SetBoard', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      const boardName = params.name as string;
      this.resolveBoardImage(boardName);
    });

    // PlaySound — play a game sound effect
    this.connection.on('PlaySound', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      this.addSystemMessage(`♪ Sound: ${params.name as string}`);
      this.broadcastState();
    });

    // Player color
    this.connection.on('SetPlayerColor', (msg: ProtocolMessage) => {
      const params = this.p(msg);
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
    this.connection.on('Error', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      this.addSystemMessage(`Error: ${params.msg as string}`);
      this.broadcastState();
    });

    // Player disconnect
    this.connection.on('PlayerDisconnect', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const disconnectPlayerId = params.player as number;
      const name = this.getPlayerName(disconnectPlayerId);
      this.addSystemMessage(`${name} disconnected`, disconnectPlayerId);
      this.broadcastState();
    });

    // RemoteCall - script function execution from server
    this.connection.on('RemoteCall', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      const playerId = params.player as number;
      const functionName = params.function as string;
      const args = params.args as string;

      const result = this.scriptEngine.handleRemoteCall(playerId, functionName, args);

      if (!result.success) {
        this.addSystemMessage(`Script error: ${result.error}`);
        this.broadcastState();
      }
    });

    // Note: GameState handler is registered above (line ~436) — no duplicate needed

    // PlayerState — player global variable updates
    this.connection.on('PlayerState', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      const player = this.gameState.players.find(
        (p) => p.id === (params.toPlayer as number),
      );
      if (player && params.state) {
        try {
          const vars = JSON.parse(params.state as string);
          Object.assign(player.globalVariables, vars);
        } catch {
          // ignore
        }
      }
      this.broadcastState();
    });

    // SetGlobalVariable — game-level global variable
    this.connection.on('SetGlobalVariable', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      if (!this.gameState.globalVariables) {
        this.gameState.globalVariables = {};
      }
      this.gameState.globalVariables[params.name as string] = params.value as string;
      this.broadcastState();
    });

    // PlayerSetGlobalVariable — player-level global variable
    this.connection.on('PlayerSetGlobalVariable', (msg: ProtocolMessage) => {
      const params = this.p(msg);
      if (!this.gameState) return;
      const player = this.gameState.players.find(
        (p) => p.id === (params.player as number),
      );
      if (player) {
        player.globalVariables[params.name as string] = params.value as string;
      }
      this.broadcastState();
    });

    // Connection lifecycle events
    this.connection.on('disconnected', () => {
      this.addSystemMessage('Disconnected from game server');
      if (this.gameState) {
        this.gameState.connectionStatus = 'disconnected';
      }
      this.broadcastState();
    });

    this.connection.on('reconnecting', () => {
      this.addSystemMessage('Reconnecting...');
      if (this.gameState) {
        this.gameState.connectionStatus = 'reconnecting';
      }
      this.broadcastState();
    });

    this.connection.on('connected', () => {
      if (this.gameState) {
        this.gameState.connectionStatus = 'connected';
        this.broadcastState();
      }
    });
  }

  private initializeGameState(gameId: string, gameName: string): void {
    this.gameState = {
      gameId,
      gameName,
      players: [],
      localPlayerId: this.localPlayerId,
      isSpectator: this.isSpectator,
      table: { cards: [] },
      turnNumber: 0,
      activePlayer: 0,
      phase: 0,
      chatMessages: [],
      isStarted: false,
      connectionStatus: 'connected',
    };
  }

  /**
   * Initialize player groups from game definition for players that don't have groups yet.
   * WPF creates groups from definition.xml when the game engine starts; we do it after
   * the game definition is loaded asynchronously.
   */
  private initializePlayerGroupsFromDefinition(gameDef: { players: { groups: { name: string; visibility?: number }[] }[] }): void {
    if (!this.gameState || !gameDef?.players || gameDef.players.length === 0) return;
    const playerGroupDefs = gameDef.players[0].groups;
    if (!playerGroupDefs || playerGroupDefs.length === 0) return;

    for (const player of this.gameState.players) {
      if (player.isSpectator) continue;
      if (player.groups.length > 0) continue; // already has groups (e.g. from GameState snapshot)

      player.groups = playerGroupDefs.map((gDef, index) => ({
        id: String(0x01000000 | (player.id << 16) | (index + 1)),
        name: gDef.name,
        cards: [],
        visibility: (gDef as any).visibility ?? GroupVisibility.Owner,
        controller: player.id,
      }));
      log('GAME', `Initialized ${player.groups.length} groups for player ${player.id} (${player.name})`);
    }
  }

  /**
   * Initialize global player (ID 0) from game definition's globalPlayer/shared groups.
   * The global player holds shared piles accessible to all players.
   */
  private initializeGlobalPlayer(gameDef: { globalPlayer?: { groups: { name: string; visibility?: number }[] } }): void {
    if (!this.gameState || !gameDef?.globalPlayer?.groups?.length) return;

    // Don't create if global player already exists
    if (this.gameState.players.some((p) => p.id === 0)) return;

    const globalGroups: Group[] = gameDef.globalPlayer.groups.map((gDef, index) => ({
      id: String(0x01000000 | (0 << 16) | (index + 1)),
      name: gDef.name,
      cards: [],
      visibility: (gDef as any).visibility ?? GroupVisibility.Everybody,
      controller: 0,
    }));

    this.gameState.players.push({
      id: 0,
      name: 'Global',
      color: '#000000',
      isHost: false,
      isSpectator: false,
      invertedTable: false,
      groups: globalGroups,
      counters: [],
      globalVariables: {},
    });
    log('GAME', `Initialized global player with ${globalGroups.length} shared groups`);
  }

  /**
   * Parse a full GameState JSON snapshot from the server.
   * This is sent to spectators joining a game already in progress.
   */
  private parseGameStateSnapshot(json: string): void {
    const data = JSON.parse(json);

    if (!this.gameState) {
      this.initializeGameState(data.SessionId ?? '', '');
    }

    // Parse players
    const players: Player[] = (data.Players ?? []).map((p: any) => {
      const groups: Group[] = (p.Groups ?? []).map((g: any) => ({
        id: String(g.Id),
        name: this.resolveGroupName(g.Id, g.Visiblity),
        cards: (g.Cards ?? []).map((c: any) => this.parseSnapshotCard(c)),
        visibility: g.Visiblity ?? 0,
        controller: p.Id,
      }));

      const counters: Counter[] = (p.Counters ?? []).map((c: any) => ({
        id: c.Id ?? 0,
        name: c.Name ?? 'Counter',
        value: c.Value ?? 0,
      }));

      return {
        id: p.Id,
        name: p.Nickname ?? `Player ${p.Id}`,
        color: p.Color ?? playerColorById(p.Id),
        isHost: false,
        isSpectator: false,
        invertedTable: false,
        groups,
        counters,
        globalVariables: p.GlobalVariables ?? {},
      } as Player;
    });

    // Parse table cards
    const tableCards: Card[] = data.Table?.Cards
      ? data.Table.Cards.map((c: any) => this.parseSnapshotCard(c))
      : [];

    this.gameState!.players = players;
    this.gameState!.table.cards = tableCards;
    this.gameState!.turnNumber = data.TurnNumber ?? 0;
    this.gameState!.activePlayer = data.ActivePlayer ?? 0;
    this.gameState!.isStarted = true;
    this.gameState!.globalVariables = data.GlobalVariables ?? {};

    if (data.GameBoard) {
      // Use resolveBoardImage to properly resolve the board from game definition
      this.resolveBoardImage(data.GameBoard);
    }

    log('GAME', `Parsed snapshot: ${players.length} players, ${tableCards.length} table cards, turn=${data.TurnNumber}`);
  }

  private parseSnapshotCard(c: any): Card {
    const markers: Marker[] = (c.Markers ?? []).map((m: any) => ({
      id: m.Id ?? '',
      name: m.Name ?? '',
      iconUrl: '',
      count: m.Count ?? 1,
    }));

    const definitionId = c.Type ?? '';
    const def = this.cardResolver.resolve(definitionId);
    const imageUrl = def?.setId
      ? this.imageResolver.buildAssetUrl(this.currentGameId, def.setId, definitionId)
      : '';

    // Merge: definition properties as base, protocol overrides on top
    const properties = def?.properties
      ? { ...def.properties, ...(c.PropertyOverrides ?? {}) }
      : (c.PropertyOverrides ?? {});

    return {
      id: String(c.Id),
      definitionId,
      name: def?.name ?? `Card ${c.Id}`,
      imageUrl,
      faceUp: c.FaceUp ?? false,
      position: { x: c.X ?? 0, y: c.Y ?? 0 },
      size: this.resolveCardSize(c.Size ?? ''),
      ownerId: String(c.Owner ?? 0),
      groupId: '',
      markers,
      rotation: (c.Orientation ?? 0) * 90,
      highlighted: c.HighlightColor ?? undefined,
      targetedBy: c.TargetedBy ? String(c.TargetedBy) : undefined,
      properties,
      peekingPlayers: c.PeekingPlayers ? c.PeekingPlayers.split(',').filter(Boolean) : [],
    };
  }

  /** Resolve group name from game definition, falling back to heuristic guess. */
  private resolveGroupName(groupId: number, visibility: number): string {
    // Try to resolve from loaded game definition first
    if (this.currentGameId) {
      const name = this.cardResolver.resolveGroupName(this.currentGameId, groupId);
      if (name) return name;
    }

    // Fallback: OCTGN group IDs encode player + group index in low byte
    const idx = groupId & 0xFF;
    switch (idx) {
      case 1: return 'Hand';
      case 2: return visibility === 1 ? 'Deck' : 'Library';
      case 3: return 'Discard';
      case 4: return 'Special';
      case 5: return 'Extra';
      default: return `Zone ${idx}`;
    }
  }

  private broadcastState(): void {
    if (!this.gameState) return;
    const windows = BrowserWindow.getAllWindows();
    for (const win of windows) {
      win.webContents.send(IPC_CHANNELS.GAME_STATE_UPDATE, this.gameState);
    }
  }

  /**
   * Create a Card object from protocol data.
   */
  private createCard(id: number, definitionId: string, size: string, groupId: string): Card {
    const cardSize = this.resolveCardSize(size);

    const def = this.cardResolver.resolve(definitionId);
    const imageUrl = def?.setId
      ? this.imageResolver.buildAssetUrl(this.currentGameId, def.setId, definitionId)
      : '';

    return {
      id: String(id),
      definitionId,
      name: def?.name ?? `Card ${id}`,
      imageUrl,
      faceUp: false,
      position: { x: 0, y: 0 },
      rotation: 0,
      groupId,
      ownerId: '',
      markers: [],
      properties: def?.properties ?? {},
      peekingPlayers: [],
      size: cardSize,
    };
  }

  /** Resolve card size from protocol size string, game definition, or fallback. */
  private resolveCardSize(size: string): { width: number; height: number } {
    if (size.includes(',')) {
      const [w, h] = size.split(',').map(Number);
      if (w > 0 && h > 0) return { width: w, height: h };
    }
    if (size && this.gameState?.cardSizes?.[size]) {
      return { ...this.gameState.cardSizes[size] };
    }
    if (this.gameState?.cardSize) {
      return { ...this.gameState.cardSize };
    }
    return { width: 100, height: 140 };
  }

  /** Update all existing cards with fallback size to use game definition values. */
  private updateCardSizesFromDefinition(): void {
    if (!this.gameState?.cardSize) return;
    const defaultSize = this.gameState.cardSize;
    const updateCard = (card: Card) => {
      if (card.size.width === 100 && card.size.height === 140) {
        card.size = { ...defaultSize };
      }
    };
    for (const card of this.gameState.table.cards) {
      updateCard(card);
    }
    for (const player of this.gameState.players) {
      for (const group of player.groups) {
        for (const card of group.cards) {
          updateCard(card);
        }
      }
    }
  }

  /**
   * Retroactively update card names and image URLs from now-loaded game definitions.
   * Called after cardResolver.loadGame() completes so that cards which arrived
   * before definitions were ready get their proper name and image.
   */
  private updateCardNamesAndImages(): void {
    if (!this.gameState || !this.currentGameId) return;

    const allCards = [
      ...this.gameState.table.cards,
      ...this.gameState.players.flatMap(p =>
        p.groups.flatMap(g => g.cards || [])
      ),
    ];

    let updated = 0;
    for (const card of allCards) {
      const def = this.cardResolver.resolve(card.definitionId);
      if (def) {
        if (def.name) card.name = def.name;
        if (def.setId) {
          card.imageUrl = this.imageResolver.buildAssetUrl(
            this.currentGameId, def.setId, card.definitionId
          );
        }
        updated++;
      }
    }

    if (updated > 0) {
      log('GAME', `Retroactively updated ${updated} card(s) with names/images from definitions`);
    }
  }

  /**
   * Add a card to the appropriate group or table.
   * WPF Group ID encoding: 0x01000000 | (Owner.Id << 16) | Def.Id
   * Special case: 0x01000000 (no owner, Def.Id=0) = Table
   */
  private addCardToGroup(card: Card, groupId: number): void {
    if (!this.gameState) return;

    // Table: group id 0 or 0x01000000 (WPF Table.Id has no owner and Def.Id=0)
    if (groupId === 0 || groupId === 0x01000000) {
      this.gameState.table.cards.push(card);
      return;
    }

    // Find the group across all players
    const group = this.findGroup(groupId);
    if (group) {
      group.cards.push(card);
      return;
    }

    // Group doesn't exist yet — decode the owner player ID from the group ID
    // WPF: Player.Find((byte)(id >> 16))
    const ownerId = (groupId >> 16) & 0xFF;
    const targetPlayer = this.gameState.players.find((p) => p.id === ownerId)
      ?? this.gameState.players.find((p) => !p.isSpectator);

    if (targetPlayer) {
      const newGroup: Group = {
        id: String(groupId),
        name: this.resolveGroupName(groupId, GroupVisibility.Owner),
        cards: [card],
        visibility: GroupVisibility.Owner,
        controller: targetPlayer.id,
      };
      targetPlayer.groups.push(newGroup);
    } else {
      // Fallback to table
      this.gameState.table.cards.push(card);
    }
  }

  /**
   * Find a group by numeric ID across all players.
   */
  private findGroup(groupId: number): Group | undefined {
    if (!this.gameState) return undefined;
    for (const player of this.gameState.players) {
      const group = player.groups.find((g) => g.id === String(groupId));
      if (group) return group;
    }
    return undefined;
  }

  /**
   * Remove a card from wherever it currently exists.
   */
  private removeCard(cardId: number): void {
    if (!this.gameState) return;
    const idStr = String(cardId);

    // Check table
    const tableIdx = this.gameState.table.cards.findIndex((c) => c.id === idStr);
    if (tableIdx >= 0) {
      this.gameState.table.cards.splice(tableIdx, 1);
      return;
    }

    // Check player groups
    for (const player of this.gameState.players) {
      for (const group of player.groups) {
        const idx = group.cards.findIndex((c) => c.id === idStr);
        if (idx >= 0) {
          group.cards.splice(idx, 1);
          return;
        }
      }
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

  private addSystemMessage(text: string, playerId?: number): void {
    const color = playerId != null
      ? this.gameState?.players.find(p => p.id === playerId)?.color ?? playerColorById(playerId)
      : undefined;
    const msg: ChatMessage = {
      id: String(this.chatIdCounter++),
      playerId: 0,
      playerName: 'System',
      message: text,
      timestamp: Date.now(),
      isSystem: true,
      color,
    };
    this.chatMessages.push(msg);
    if (this.gameState) {
      this.gameState.chatMessages = [...this.chatMessages];
    }
  }

  /**
   * Resolve a board name to an image URL and update game state.
   * The board name from the protocol is matched against the game definition's
   * boards list to find the source file and positioning info.
   * The image is served via the octgn-asset:// protocol.
   */
  private resolveBoardImage(boardName: string): void {
    if (!this.gameState) return;

    const gameDef = this.cardResolver.getGameDefinition(this.currentGameId);

    // Try to find the board in the game definition
    let boardDef = gameDef?.boards?.find(
      (b) => b.name.toLowerCase() === boardName.toLowerCase(),
    );

    // If not found by name, try the first board (common case: single board)
    if (!boardDef && gameDef?.boards?.length) {
      boardDef = gameDef.boards[0];
    }

    // Also check the table's embedded board definition
    if (!boardDef && gameDef?.table?.board) {
      boardDef = gameDef.table.board;
    }

    if (boardDef && boardDef.source) {
      // Build an octgn-asset:// URL for the board image file
      const encodedPath = encodeURIComponent(boardDef.source);
      const imageUrl = `octgn-asset://game-file/${this.currentGameId}/${encodedPath}`;

      this.gameState.table.board = {
        name: boardName,
        imageUrl,
        width: boardDef.width,
        height: boardDef.height,
        x: boardDef.x,
        y: boardDef.y,
      };

      log('GAME', `Board resolved: ${boardName} -> ${imageUrl} (${boardDef.width}x${boardDef.height} at ${boardDef.x},${boardDef.y})`);
    } else {
      // Fallback: use the board name as-is (may be a direct path)
      // Store pending name so we can re-resolve after loadGame completes
      this.pendingBoardName = boardName;
      const encodedPath = encodeURIComponent(boardName);
      const imageUrl = `octgn-asset://game-file/${this.currentGameId}/${encodedPath}`;

      this.gameState.table.board = {
        name: boardName,
        imageUrl,
        width: 0,
        height: 0,
      };

      log('GAME', `Board fallback: ${boardName} -> ${imageUrl} (no definition found, will retry after loadGame)`);
    }

    // Also apply table metadata from game definition if available
    if (gameDef?.table) {
      this.gameState.table.width = gameDef.table.width || undefined;
      this.gameState.table.height = gameDef.table.height || undefined;
      this.gameState.table.backgroundStyle = gameDef.table.backgroundStyle;
      // Resolve table background image (e.g. wood texture) to an asset URL
      if (gameDef.table.background) {
        const encodedBgPath = encodeURIComponent(gameDef.table.background);
        this.gameState.table.backgroundUrl = `octgn-asset://game-file/${this.currentGameId}/${encodedBgPath}`;
      }
    }

    this.broadcastState();
  }
}
