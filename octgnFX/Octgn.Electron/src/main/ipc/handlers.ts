import { IpcMain, BrowserWindow, app } from 'electron';
import { IPC_CHANNELS } from '../../shared/types';
import { OctgnApiClient } from '../api/client';

const apiClient = new OctgnApiClient();

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
    return apiClient.joinGame(gameId, password);
  });

  ipcMain.handle(IPC_CHANNELS.LEAVE_GAME, async () => {
    return apiClient.leaveGame();
  });

  // Game handlers
  ipcMain.handle(IPC_CHANNELS.GAME_ACTION, async (_event, action) => {
    return apiClient.sendGameAction(action);
  });

  ipcMain.handle(IPC_CHANNELS.GAME_CHAT, async (_event, message: string) => {
    return apiClient.sendChatMessage(message);
  });

  ipcMain.handle(IPC_CHANNELS.LOAD_DECK, async (_event, deck) => {
    return apiClient.loadDeck(deck);
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

  ipcMain.handle(IPC_CHANNELS.APP_QUIT, () => {
    app.quit();
  });

  ipcMain.handle(IPC_CHANNELS.APP_VERSION, () => {
    return app.getVersion();
  });
}
