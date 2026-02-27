import { IpcMain, BrowserWindow, app, dialog } from 'electron';
import { readFile } from 'fs/promises';
import { IPC_CHANNELS } from '../../shared/types';
import { OctgnApiClient } from '../api/client';
import { GameService } from '../api/game-service';

const apiClient = new OctgnApiClient();
const gameService = new GameService();

export function setupIpcHandlers(ipcMain: IpcMain): void {
  // Auth handlers
  ipcMain.handle(IPC_CHANNELS.LOGIN, async (_event, username: string, password: string) => {
    return apiClient.login(username, password);
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

  ipcMain.handle(IPC_CHANNELS.JOIN_GAME, async (_event, gameId: string, password?: string) => {
    // Get game details to find host:port, then connect via GameService
    const games = await apiClient.getHostedGames();
    const game = games.find((g) => g.id === gameId);
    if (!game) {
      return { success: false, error: 'Game not found' };
    }
    const session = apiClient.getSession();
    if (!session) {
      return { success: false, error: 'Not logged in' };
    }
    return gameService.joinGame(
      game.hostAddress,
      game.port,
      session.username,
      session.userId,
      game.gameId,
      game.gameVersion,
      password ?? '',
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
}
