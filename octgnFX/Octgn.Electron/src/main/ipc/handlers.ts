import { IpcMain, BrowserWindow, app, dialog, clipboard } from 'electron';
import { readFile } from 'fs/promises';
import { IPC_CHANNELS } from '../../shared/types';
import { OctgnApiClient } from '../api/client';
import { GameService } from '../api/game-service';
import { listInstalledGames, uninstallGame } from '../games/game-store';
import { fetchAvailableGames } from '../games/game-feed';
import { installGame } from '../games/game-installer';
import { listFeeds, addFeed, removeFeed, setFeedEnabled } from '../games/feed-manager';
import { resolveAndCacheIcons } from '../games/icon-cache';
import { saveCredentials, loadCredentials, clearCredentials } from '../auth/credential-store';
import { setImageResolver } from '../asset-protocol';
import { log, logError } from '../logger';

const apiClient = new OctgnApiClient();
const gameService = new GameService();

// Wire the GameService's ImageResolver to the asset protocol handler
setImageResolver(gameService.getImageResolver());

export function setupIpcHandlers(ipcMain: IpcMain): void {
  // Auth handlers
  ipcMain.handle(IPC_CHANNELS.LOGIN, async (_event, username: string, password: string, rememberMe?: boolean) => {
    log('LOGIN', `Attempting login for user: ${username}`);
    try {
      const result = await apiClient.login(username, password);
      log('LOGIN', `Result: ${JSON.stringify(result)}`);
      // Auto-save credentials on successful login when rememberMe is true
      if (result.success && rememberMe) {
        await saveCredentials(username, password).catch((err) =>
          logError('LOGIN', err),
        );
      }
      return result;
    } catch (err) {
      logError('LOGIN', err);
      throw err;
    }
  });

  ipcMain.handle(IPC_CHANNELS.LOGOUT, async () => {
    return apiClient.logout();
  });

  ipcMain.handle(IPC_CHANNELS.GET_SESSION, async () => {
    return apiClient.getSession();
  });

  // Lobby handlers
  ipcMain.handle(IPC_CHANNELS.GET_GAMES, async () => {
    return apiClient.getHostedGames();
  });

  ipcMain.handle(IPC_CHANNELS.HOST_GAME, async (_event, options) => {
    return apiClient.hostGame(options);
  });

  ipcMain.handle(IPC_CHANNELS.JOIN_GAME, async (_event, gameId: string, password?: string, spectator?: boolean) => {
    log('JOIN', `JOIN_GAME called: gameId=${gameId} spectator=${spectator}`);
    // Get game details to find host:port, then connect via GameService
    const games = await apiClient.getHostedGames();
    log('JOIN', `Got ${games.length} games from API`);
    const game = games.find((g) => g.id === gameId);
    log('JOIN', `Found game: ${game ? `${game.name} at ${game.hostAddress}:${game.port}` : 'NOT FOUND'}`);
    if (!game) {
      return { success: false, error: 'Game not found' };
    }
    const session = apiClient.getSession();
    log('JOIN', `Session: ${session ? session.username : 'NULL'}`);
    if (!session) {
      return { success: false, error: 'Not logged in' };
    }
    log('JOIN', `Calling gameService.joinGame(${game.hostAddress}, ${game.port}, ${session.username}, spectator=${spectator})`);
    return gameService.joinGame(
      game.hostAddress,
      game.port,
      session.username,
      session.userId,
      game.gameId,
      game.gameVersion,
      password ?? '',
      spectator ?? false,
    );
  });

  ipcMain.handle(IPC_CHANNELS.LEAVE_GAME, async () => {
    return gameService.leaveGame();
  });

  // Game handlers - now wired to GameService
  ipcMain.handle(IPC_CHANNELS.GAME_ACTION, async (_event, action: Record<string, unknown>) => {
    const type = action.type as string;
    switch (type) {
      case 'moveCards':
        gameService.moveCards(
          action.cardIds as number[],
          action.groupId as number,
          action.indices as number[],
          action.faceUp as boolean[],
        );
        break;
      case 'moveCardsAt':
        gameService.moveCardsAt(
          action.cardIds as number[],
          action.x as number[],
          action.y as number[],
          action.indices as number[],
          action.faceUp as boolean[],
        );
        break;
      case 'nextTurn':
        gameService.nextTurn();
        break;
      case 'flipCard':
        gameService.flipCard(action.cardId as number, action.faceUp as boolean);
        break;
      case 'rotateCard':
        gameService.rotateCard(action.cardId as number, action.rotation as number);
        break;
      case 'setCounter':
        gameService.setCounter(action.counterId as number, action.value as number);
        break;
      case 'peekCard':
        gameService.peekCard(action.cardId as number);
        break;
      case 'targetCard':
        gameService.targetCard(
          action.cardId as number,
          action.playerId as number,
          action.active as boolean,
        );
        break;
      case 'highlightCard':
        gameService.highlightCard(action.cardId as number, action.color as string);
        break;
      case 'addMarker':
        gameService.addMarker(
          action.cardId as number,
          action.markerId as string,
          action.markerName as string,
          action.count as number,
        );
        break;
      case 'removeMarker':
        gameService.removeMarker(
          action.cardId as number,
          action.markerId as string,
          action.markerName as string,
          action.count as number,
        );
        break;
      case 'shuffleGroup':
        gameService.shuffleGroup(action.groupId as number);
        break;
    }
  });

  ipcMain.handle(IPC_CHANNELS.GAME_CHAT, async (_event, message: string) => {
    gameService.sendChat(message);
  });

  ipcMain.handle(IPC_CHANNELS.LOAD_DECK, async (_event, deck: Record<string, unknown>) => {
    gameService.loadDeck(
      deck.ids as number[],
      deck.types as string[],
      deck.groups as number[],
      deck.sizes as string[],
      (deck.sleeve as string) ?? '',
      (deck.limited as boolean) ?? false,
    );
  });

  // File dialog handler
  ipcMain.handle(
    IPC_CHANNELS.OPEN_FILE_DIALOG,
    async (_event, filters?: { name: string; extensions: string[] }[]) => {
      const defaultFilters = filters ?? [
        { name: 'OCTGN Deck Files', extensions: ['o8d'] },
        { name: 'All Files', extensions: ['*'] },
      ];

      const result = await dialog.showOpenDialog({
        title: 'Open Deck File',
        filters: defaultFilters,
        properties: ['openFile'],
      });

      if (result.canceled || result.filePaths.length === 0) {
        return null;
      }

      const filePath = result.filePaths[0];
      const content = await readFile(filePath, 'utf-8');
      return { filePath, content };
    },
  );

  // Window control handlers
  ipcMain.handle(IPC_CHANNELS.APP_MINIMIZE, () => {
    BrowserWindow.getFocusedWindow()?.minimize();
  });

  ipcMain.handle(IPC_CHANNELS.APP_MAXIMIZE, () => {
    const win = BrowserWindow.getFocusedWindow();
    if (win?.isMaximized()) {
      win.unmaximize();
    } else {
      win?.maximize();
    }
  });

  ipcMain.handle(IPC_CHANNELS.APP_QUIT, () => {
    app.quit();
  });

  ipcMain.handle(IPC_CHANNELS.APP_VERSION, () => {
    return app.getVersion();
  });

  // Script execution handler
  ipcMain.handle(
    IPC_CHANNELS.SCRIPT_EXECUTE,
    async (_event, functionName: string, args: string = '') => {
      gameService.executeScript(functionName, args);
    },
  );

  // Game definitions handlers
  ipcMain.handle(IPC_CHANNELS.GAMES_LIST_INSTALLED, async () => {
    return listInstalledGames();
  });

  ipcMain.handle(IPC_CHANNELS.GAMES_LIST_AVAILABLE, async () => {
    try {
      const feeds = await listFeeds();
      log('GAMES', `Feeds: ${feeds.map(f => `${f.name}(${f.enabled ? 'ON' : 'OFF'})`).join(', ')}`);
      const games = await fetchAvailableGames(feeds);
      log('GAMES', `fetchAvailableGames returned ${games.length} games`);
      if (games.length <= 20) {
        for (const g of games) log('GAMES', `  ${g.name} v${g.version} (${g.downloadCount})`);
      } else {
        log('GAMES', `  First 5: ${games.slice(0, 5).map(g => g.name).join(', ')}`);
        log('GAMES', `  Last 5: ${games.slice(-5).map(g => g.name).join(', ')}`);
      }
      return resolveAndCacheIcons(games);
    } catch (err) {
      logError('GAMES', err);
      throw err;
    }
  });

  ipcMain.handle(
    IPC_CHANNELS.GAMES_INSTALL,
    async (event, gameId: string, downloadUrl: string) => {
      return installGame(gameId, downloadUrl, (progress) => {
        event.sender.send(IPC_CHANNELS.GAMES_INSTALL_PROGRESS, progress);
      });
    },
  );

  ipcMain.handle(IPC_CHANNELS.GAMES_UNINSTALL, async (_event, gameId: string) => {
    return uninstallGame(gameId);
  });

  // Feed management handlers
  ipcMain.handle(IPC_CHANNELS.FEEDS_LIST, async () => {
    return listFeeds();
  });

  ipcMain.handle(IPC_CHANNELS.FEEDS_ADD, async (_event, name: string, url: string, username?: string, password?: string) => {
    return addFeed(name, url, username, password);
  });

  ipcMain.handle(IPC_CHANNELS.FEEDS_REMOVE, async (_event, name: string) => {
    return removeFeed(name);
  });

  ipcMain.handle(IPC_CHANNELS.FEEDS_SET_ENABLED, async (_event, name: string, enabled: boolean) => {
    return setFeedEnabled(name, enabled);
  });

  // Credential handlers
  ipcMain.handle(IPC_CHANNELS.CREDS_LOAD, async () => {
    return loadCredentials();
  });

  ipcMain.handle(IPC_CHANNELS.CREDS_SAVE, async (_event, username: string, password: string) => {
    return saveCredentials(username, password);
  });

  ipcMain.handle(IPC_CHANNELS.CREDS_CLEAR, async () => {
    return clearCredentials();
  });

  // Clipboard
  ipcMain.handle(IPC_CHANNELS.CLIPBOARD_WRITE, (_event, text: string) => {
    clipboard.writeText(text);
  });
}
