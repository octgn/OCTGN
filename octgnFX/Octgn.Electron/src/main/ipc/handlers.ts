import { IpcMain, BrowserWindow, app, dialog, clipboard } from 'electron';
import { readFile } from 'fs/promises';
import { IPC_CHANNELS, type Deck } from '../../shared/types';
import { OctgnApiClient } from '../api/client';
import { GameService, type SavedConnectionInfo } from '../api/game-service';
import { GameWindowManager } from '../game-window-manager';
import { listInstalledGames, uninstallGame, getUserDecksDir, getPrebuiltDecksDir } from '../games/game-store';
import { fetchAvailableGames } from '../games/game-feed';
import { installGame } from '../games/game-installer';
import { listFeeds, addFeed, addDirectRepo, addRepoFeed, removeFeed, setFeedEnabled } from '../games/feed-manager';
import { resolveAndCacheIcons } from '../games/icon-cache';
import { fetchManifest, fetchFeedIndex, normalizeRepoUrl } from '../games/repo-feed';
import { mergeWithLegacy } from '../games/source-resolver';
import { installFromRepo } from '../games/repo-installer';
import { constructZipballUrl } from '../games/repo-feed-types';
import { saveCredentials, loadCredentials, clearCredentials } from '../auth/credential-store';
import { setImageResolver } from '../asset-protocol';
import { log, logError } from '../logger';

const apiClient = new OctgnApiClient();
const gameWindowManager = new GameWindowManager();

// Wire the shared ImageResolver to the asset protocol handler
setImageResolver(gameWindowManager.getImageResolver());

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
    return apiClient.getSessionAsLoginResult();
  });

  ipcMain.handle(IPC_CHANNELS.GET_APP_STATE, async (event) => {
    const service = gameWindowManager.getServiceForEvent(event);
    return {
      session: apiClient.getSessionAsLoginResult(),
      gameState: service?.getState() ?? null,
    };
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
    // Get game details to find host:port, then open a game window
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
    log('JOIN', `Creating game window for ${game.hostAddress}:${game.port} as ${session.username} spectator=${spectator}`);
    return gameWindowManager.createGameWindow({
      host: game.hostAddress,
      port: game.port,
      nickname: session.username,
      userId: session.userId,
      gameId: game.gameId,
      gameVersion: game.gameVersion,
      password: password ?? '',
      spectator: spectator ?? false,
    });
  });

  ipcMain.handle(IPC_CHANNELS.LEAVE_GAME, async (event) => {
    const win = BrowserWindow.fromWebContents(event.sender);
    if (win) {
      await gameWindowManager.closeGameWindow(win.id);
    }
  });

  // Game handlers - routed to the correct GameService via sender
  ipcMain.handle(IPC_CHANNELS.GAME_ACTION, async (event, action: Record<string, unknown>) => {
    const service = gameWindowManager.getServiceForEvent(event);
    if (!service) return;
    const type = action.type as string;
    if (type === 'moveCards' || type === 'moveCardsAt') {
      log('IPC', `GAME_ACTION: ${type} ${JSON.stringify(action)}`);
    }
    switch (type) {
      case 'moveCards':
        service.moveCards(
          action.cardIds as number[],
          action.groupId as number,
          action.indices as number[],
          action.faceUp as boolean[],
        );
        break;
      case 'moveCardsAt':
        service.moveCardsAt(
          action.cardIds as number[],
          action.x as number[],
          action.y as number[],
          action.indices as number[],
          action.faceUp as boolean[],
        );
        break;
      case 'nextTurn':
        service.nextTurn();
        break;
      case 'flipCard':
        service.flipCard(action.cardId as number, action.faceUp as boolean);
        break;
      case 'rotateCard':
        service.rotateCard(action.cardId as number, action.rotation as number);
        break;
      case 'setCounter':
        service.setCounter(action.counterId as number, action.value as number);
        break;
      case 'peekCard':
        service.peekCard(action.cardId as number);
        break;
      case 'targetCard':
        service.targetCard(
          action.cardId as number,
          action.playerId as number,
          action.active as boolean,
        );
        break;
      case 'highlightCard':
        service.highlightCard(action.cardId as number, action.color as string);
        break;
      case 'addMarker':
        service.addMarker(
          action.cardId as number,
          action.markerId as string,
          action.markerName as string,
          action.count as number,
        );
        break;
      case 'removeMarker':
        service.removeMarker(
          action.cardId as number,
          action.markerId as string,
          action.markerName as string,
          action.count as number,
        );
        break;
      case 'shuffleGroup':
        service.shuffleGroup(action.groupId as number);
        break;
    }
  });

  ipcMain.handle(IPC_CHANNELS.GAME_CHAT, async (event, message: string) => {
    const service = gameWindowManager.getServiceForEvent(event);
    service?.sendChat(message);
  });

  ipcMain.handle(IPC_CHANNELS.GAME_SETTINGS, async (event, twoSidedTable: boolean, allowSpectators: boolean, muteSpectators: boolean, allowCardList: boolean) => {
    const service = gameWindowManager.getServiceForEvent(event);
    service?.sendSettings(twoSidedTable, allowSpectators, muteSpectators, allowCardList);
  });

  ipcMain.handle(IPC_CHANNELS.GAME_PLAYER_SETTINGS, async (event, playerId: number, invertedTable: boolean, spectator: boolean) => {
    const service = gameWindowManager.getServiceForEvent(event);
    service?.sendPlayerSettings(playerId, invertedTable, spectator);
  });

  ipcMain.handle(IPC_CHANNELS.GAME_BOOT_PLAYER, async (event, playerId: number, reason: string) => {
    const service = gameWindowManager.getServiceForEvent(event);
    service?.bootPlayer(playerId, reason ?? '');
  });

  ipcMain.handle(IPC_CHANNELS.GAME_START, async (event) => {
    const service = gameWindowManager.getServiceForEvent(event);
    service?.startGame();
  });

  ipcMain.handle(IPC_CHANNELS.LOAD_DECK, async (event, deck: Deck) => {
    const service = gameWindowManager.getServiceForEvent(event);
    service?.loadDeckFromFile(deck);
  });

  // File dialog handler
  ipcMain.handle(
    IPC_CHANNELS.OPEN_FILE_DIALOG,
    async (_event, filters?: { name: string; extensions: string[] }[], defaultPath?: string) => {
      const defaultFilters = filters ?? [
        { name: 'OCTGN Deck Files', extensions: ['o8d'] },
        { name: 'All Files', extensions: ['*'] },
      ];

      const result = await dialog.showOpenDialog({
        title: 'Open Deck File',
        filters: defaultFilters,
        properties: ['openFile'],
        defaultPath: defaultPath || undefined,
      });

      if (result.canceled || result.filePaths.length === 0) {
        return null;
      }

      const filePath = result.filePaths[0];
      const content = await readFile(filePath, 'utf-8');
      return { filePath, content };
    },
  );

  // Deck paths handler
  ipcMain.handle(IPC_CHANNELS.DECK_PATHS, async (_event, gameId?: string) => {
    log('DECK_PATHS', `Resolving deck paths for gameId=${gameId}`);
    const userDecksPath = await getUserDecksDir();
    const prebuiltDecksPath = gameId ? await getPrebuiltDecksDir(gameId) : null;
    log('DECK_PATHS', `userDecksPath=${userDecksPath}, prebuiltDecksPath=${prebuiltDecksPath}`);
    return { userDecksPath, prebuiltDecksPath };
  });

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

  ipcMain.handle(IPC_CHANNELS.APP_QUIT, async (event) => {
    const sender = BrowserWindow.fromWebContents(event.sender);
    const isGameWindow = sender && gameWindowManager.has(sender.id);
    if (isGameWindow) {
      // Game window close — just close this game window, don't quit the app
      await gameWindowManager.closeGameWindow(sender.id);
      return;
    }
    // Main window close = quit app (close all game windows too)
    await gameWindowManager.closeAll();
    app.quit();
  });

  ipcMain.handle(IPC_CHANNELS.APP_VERSION, () => {
    return app.getVersion();
  });

  // Close game window handler
  ipcMain.handle(IPC_CHANNELS.CLOSE_GAME_WINDOW, async (event) => {
    const win = BrowserWindow.fromWebContents(event.sender);
    if (win) {
      await gameWindowManager.closeGameWindow(win.id);
    }
  });

  // Script execution handler
  ipcMain.handle(
    IPC_CHANNELS.SCRIPT_EXECUTE,
    async (event, functionName: string, args: string = '') => {
      const service = gameWindowManager.getServiceForEvent(event);
      service?.executeScript(functionName, args);
    },
  );

  // Action execution handler (for game-defined context menu actions)
  ipcMain.handle(
    IPC_CHANNELS.SCRIPT_EXECUTE_ACTION,
    async (event, request: { type: 'card' | 'group'; action: any; cardId?: number; cardIds?: number[]; groupId: number }) => {
      const service = gameWindowManager.getServiceForEvent(event);
      if (!service) return { success: false, error: 'No game service for this window' };
      const executor = service.getActionExecutor();
      if (!executor) {
        return { success: false, error: 'Script engine not initialized' };
      }
      if (request.type === 'card') {
        if (request.cardIds && request.cardIds.length > 1) {
          return executor.executeBatchCardAction(request.action, request.cardIds);
        }
        const cardId = request.cardId ?? request.cardIds?.[0];
        if (cardId == null) return { success: false, error: 'No card ID provided' };
        return executor.executeCardAction(request.action, cardId);
      } else {
        return executor.executeGroupAction(request.action, request.groupId);
      }
    },
  );

  // Menu evaluation handler (showIf/getName for game-defined actions)
  ipcMain.handle(
    IPC_CHANNELS.SCRIPT_EVALUATE_MENU,
    async (event, request: { actions: Array<{ showIf?: string; getName?: string; id: string; action: any }>; cardOrGroupId: number }) => {
      const service = gameWindowManager.getServiceForEvent(event);
      if (!service) {
        return request.actions.map((a) => ({ id: a.id, visible: true, name: a.action.name }));
      }
      const executor = service.getActionExecutor();
      if (!executor) {
        // No script engine — show all actions with default names
        return request.actions.map((a) => ({ id: a.id, visible: true, name: a.action.name }));
      }
      const results = [];
      for (const a of request.actions) {
        const visible = await executor.evaluateShowIf(a.action, request.cardOrGroupId);
        const name = await executor.evaluateGetName(a.action, request.cardOrGroupId);
        results.push({ id: a.id, visible, name });
      }
      return results;
    },
  );

  // Game definitions handlers
  ipcMain.handle(IPC_CHANNELS.GAMES_LIST_INSTALLED, async () => {
    return listInstalledGames();
  });

  ipcMain.handle(IPC_CHANNELS.GAMES_LIST_AVAILABLE, async () => {
    try {
      const feeds = await listFeeds();
      log('GAMES', `Feeds: ${feeds.map(f => `${f.name}(${f.feedType}/${f.enabled ? 'ON' : 'OFF'})`).join(', ')}`);

      // Split feeds by type
      const nugetFeeds = feeds.filter(f => (f.feedType === 'nuget' || !f.feedType) && f.enabled);
      const repoIndexFeeds = feeds.filter(f => f.feedType === 'repo-index' && f.enabled);
      const directRepoFeeds = feeds.filter(f => f.feedType === 'direct-repo' && f.enabled);

      // Fetch from all sources in parallel
      const settledResults = await Promise.allSettled([
        fetchAvailableGames(nugetFeeds),
        ...repoIndexFeeds.map(f => fetchFeedIndex(f.url)),
        ...directRepoFeeds.map(f => {
          const { owner, repo } = normalizeRepoUrl(f.url);
          return fetchManifest(owner, repo, f.branch || 'main');
        }),
      ]);

      // Extract NuGet games
      const nugetResult = settledResults[0];
      const nugetGames = nugetResult.status === 'fulfilled' ? nugetResult.value : [];
      if (nugetResult.status === 'rejected') {
        logError('GAMES', `NuGet fetch failed: ${nugetResult.reason}`);
      }

      // Collect repo games from feed index results
      const repoGames: import('../../shared/types').AvailableGame[] = [];
      const repoOffset = 1; // index 0 is nuget
      for (let i = 0; i < repoIndexFeeds.length; i++) {
        const result = settledResults[repoOffset + i];
        if (result.status === 'fulfilled') {
          const feedResult = result.value as { games: import('../../shared/types').AvailableGame[]; errors: string[] };
          repoGames.push(...feedResult.games);
          for (const err of feedResult.errors) {
            log('GAMES', `Repo index warning (${repoIndexFeeds[i].name}): ${err}`);
          }
        } else {
          logError('GAMES', `Repo index feed "${repoIndexFeeds[i].name}" failed: ${result.reason}`);
        }
      }

      // Collect repo games from direct repo results
      const directOffset = repoOffset + repoIndexFeeds.length;
      for (let i = 0; i < directRepoFeeds.length; i++) {
        const result = settledResults[directOffset + i];
        if (result.status === 'fulfilled') {
          const manifestResult = result.value as { manifest?: import('../../shared/types').GameManifest; error?: string };
          if (manifestResult.manifest) {
            const m = manifestResult.manifest;
            const feed = directRepoFeeds[i];
            const { owner, repo } = normalizeRepoUrl(feed.url);
            repoGames.push({
              id: m.guid,
              name: m.name,
              version: m.version,
              description: m.description,
              authors: m.authors.join(', '),
              tags: (m.tags || []).join(' '),
              downloadUrl: constructZipballUrl(owner, repo, feed.branch || 'main'),
              sourceType: 'repo',
              sourceInfo: `${owner}/${repo}`,
            });
          } else if (manifestResult.error) {
            log('GAMES', `Direct repo "${directRepoFeeds[i].name}" warning: ${manifestResult.error}`);
          }
        } else {
          logError('GAMES', `Direct repo "${directRepoFeeds[i].name}" failed: ${result.reason}`);
        }
      }

      // Merge repo games with NuGet games (repo takes priority for same GUID)
      const merged = repoGames.length > 0
        ? mergeWithLegacy(repoGames, nugetGames)
        : nugetGames;

      log('GAMES', `Total games: ${merged.length} (${repoGames.length} repo + ${nugetGames.length} nuget, merged)`);
      if (merged.length <= 20) {
        for (const g of merged) log('GAMES', `  ${g.name} v${g.version} [${g.sourceType ?? 'nuget'}]`);
      } else {
        log('GAMES', `  First 5: ${merged.slice(0, 5).map(g => g.name).join(', ')}`);
        log('GAMES', `  Last 5: ${merged.slice(-5).map(g => g.name).join(', ')}`);
      }
      return resolveAndCacheIcons(merged);
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

  // Repo-based game install
  ipcMain.handle(
    IPC_CHANNELS.GAMES_INSTALL_FROM_REPO,
    async (event, owner: string, repo: string, branch: string, gamePath: string, gameId: string) => {
      return installFromRepo(owner, repo, branch, gamePath, gameId, (progress) => {
        event.sender.send(IPC_CHANNELS.GAMES_INSTALL_PROGRESS, progress);
      });
    },
  );

  // Repo feed management handlers
  ipcMain.handle(
    IPC_CHANNELS.REPO_FEEDS_ADD_REPO,
    async (_event, name: string, repoUrl: string, branch?: string) => {
      return addDirectRepo(name, repoUrl, branch);
    },
  );

  ipcMain.handle(
    IPC_CHANNELS.REPO_FEEDS_ADD_FEED,
    async (_event, name: string, indexUrl: string) => {
      return addRepoFeed(name, indexUrl);
    },
  );

  ipcMain.handle(
    IPC_CHANNELS.REPO_FEEDS_FETCH_MANIFEST,
    async (_event, owner: string, repo: string, branch: string, manifestPath?: string) => {
      return fetchManifest(owner, repo, branch, manifestPath);
    },
  );

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

  // Attempt game reconnection on startup (main process restart scenario)
  GameService.loadConnectionInfo().then(async (info: SavedConnectionInfo | null) => {
    if (info) {
      log('RECONNECT', `Found recent connection info, attempting reconnection to ${info.host}:${info.port}`);
      const reconnectService = new GameService();
      const result = await reconnectService.reconnectGame(info);
      if (result.success) {
        log('RECONNECT', 'Game reconnection successful, creating game window');
        await gameWindowManager.adoptService(reconnectService);
      } else {
        log('RECONNECT', `Game reconnection failed: ${result.error}`);
        await reconnectService.clearConnectionInfo();
      }
    }
  }).catch((err) => {
    logError('RECONNECT', err);
  });
}
