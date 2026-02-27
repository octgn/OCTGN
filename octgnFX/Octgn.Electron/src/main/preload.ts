import { contextBridge, ipcRenderer } from 'electron';
import { IPC_CHANNELS } from '../shared/types';

const api = {
  // Auth
  login: (username: string, password: string) =>
    ipcRenderer.invoke(IPC_CHANNELS.LOGIN, username, password),
  logout: () => ipcRenderer.invoke(IPC_CHANNELS.LOGOUT),
  getSession: () => ipcRenderer.invoke(IPC_CHANNELS.GET_SESSION),

  // Lobby
  getGames: () => ipcRenderer.invoke(IPC_CHANNELS.GET_GAMES),
  hostGame: (options: Record<string, unknown>) =>
    ipcRenderer.invoke(IPC_CHANNELS.HOST_GAME, options),
  joinGame: (gameId: string, password?: string) =>
    ipcRenderer.invoke(IPC_CHANNELS.JOIN_GAME, gameId, password),
  leaveGame: () => ipcRenderer.invoke(IPC_CHANNELS.LEAVE_GAME),

  // Game
  onGameStateUpdate: (callback: (state: unknown) => void) => {
    const listener = (_event: unknown, state: unknown) => callback(state);
    ipcRenderer.on(IPC_CHANNELS.GAME_STATE_UPDATE, listener);
    return () => ipcRenderer.removeListener(IPC_CHANNELS.GAME_STATE_UPDATE, listener);
  },
  gameAction: (action: Record<string, unknown>) =>
    ipcRenderer.invoke(IPC_CHANNELS.GAME_ACTION, action),
  gameChat: (message: string) =>
    ipcRenderer.invoke(IPC_CHANNELS.GAME_CHAT, message),
  loadDeck: (deck: unknown) =>
    ipcRenderer.invoke(IPC_CHANNELS.LOAD_DECK, deck),

  // Window controls
  minimize: () => ipcRenderer.invoke(IPC_CHANNELS.APP_MINIMIZE),
  maximize: () => ipcRenderer.invoke(IPC_CHANNELS.APP_MAXIMIZE),
  quit: () => ipcRenderer.invoke(IPC_CHANNELS.APP_QUIT),
  getVersion: () => ipcRenderer.invoke(IPC_CHANNELS.APP_VERSION),

  // File dialog
  openFileDialog: (filters?: { name: string; extensions: string[] }[]) =>
    ipcRenderer.invoke(IPC_CHANNELS.OPEN_FILE_DIALOG, filters),
};

contextBridge.exposeInMainWorld('octgn', api);

export type OctgnAPI = typeof api;
