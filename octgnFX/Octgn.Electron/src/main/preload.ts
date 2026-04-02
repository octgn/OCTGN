import { contextBridge, ipcRenderer } from 'electron';
import { IPC_CHANNELS } from '../shared/types';

const api = {
  // Auth
  login: (username: string, password: string, rememberMe?: boolean) =>
    ipcRenderer.invoke(IPC_CHANNELS.LOGIN, username, password, rememberMe),
  logout: () => ipcRenderer.invoke(IPC_CHANNELS.LOGOUT),
  getSession: () => ipcRenderer.invoke(IPC_CHANNELS.GET_SESSION),

  // Credentials
  loadCredentials: () => ipcRenderer.invoke(IPC_CHANNELS.CREDS_LOAD),
  saveCredentials: (username: string, password: string) =>
    ipcRenderer.invoke(IPC_CHANNELS.CREDS_SAVE, username, password),
  clearCredentials: () => ipcRenderer.invoke(IPC_CHANNELS.CREDS_CLEAR),

  // Clipboard
  writeClipboard: (text: string) => ipcRenderer.invoke(IPC_CHANNELS.CLIPBOARD_WRITE, text),

  // Lobby
  getGames: () => ipcRenderer.invoke(IPC_CHANNELS.GET_GAMES),
  hostGame: (options: Record<string, unknown>) =>
    ipcRenderer.invoke(IPC_CHANNELS.HOST_GAME, options),
  joinGame: (gameId: string, password?: string, spectator?: boolean) =>
    ipcRenderer.invoke(IPC_CHANNELS.JOIN_GAME, gameId, password ?? '', spectator ?? false),
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
  gameSettings: (twoSidedTable: boolean, allowSpectators: boolean, muteSpectators: boolean, allowCardList: boolean) =>
    ipcRenderer.invoke(IPC_CHANNELS.GAME_SETTINGS, twoSidedTable, allowSpectators, muteSpectators, allowCardList),
  gamePlayerSettings: (playerId: number, invertedTable: boolean, spectator: boolean) =>
    ipcRenderer.invoke(IPC_CHANNELS.GAME_PLAYER_SETTINGS, playerId, invertedTable, spectator),
  bootPlayer: (playerId: number, reason: string) =>
    ipcRenderer.invoke(IPC_CHANNELS.GAME_BOOT_PLAYER, playerId, reason ?? ''),
  startGame: () =>
    ipcRenderer.invoke(IPC_CHANNELS.GAME_START),
  loadDeck: (deck: unknown) =>
    ipcRenderer.invoke(IPC_CHANNELS.LOAD_DECK, deck),

  // App state recovery
  getAppState: () => ipcRenderer.invoke(IPC_CHANNELS.GET_APP_STATE),

  // Window controls
  minimize: () => ipcRenderer.invoke(IPC_CHANNELS.APP_MINIMIZE),
  maximize: () => ipcRenderer.invoke(IPC_CHANNELS.APP_MAXIMIZE),
  quit: () => ipcRenderer.invoke(IPC_CHANNELS.APP_QUIT),
  getVersion: () => ipcRenderer.invoke(IPC_CHANNELS.APP_VERSION),

  // File dialog
  openFileDialog: (filters?: { name: string; extensions: string[] }[], defaultPath?: string) =>
    ipcRenderer.invoke(IPC_CHANNELS.OPEN_FILE_DIALOG, filters, defaultPath),

  // Deck paths
  getDeckPaths: (gameId?: string) =>
    ipcRenderer.invoke(IPC_CHANNELS.DECK_PATHS, gameId),

  // Scripting
  executeScript: (functionName: string, args?: string) =>
    ipcRenderer.invoke(IPC_CHANNELS.SCRIPT_EXECUTE, functionName, args ?? ''),
  executeAction: (request: { type: 'card' | 'group'; action: unknown; cardId?: number; cardIds?: number[]; groupId: number }) =>
    ipcRenderer.invoke(IPC_CHANNELS.SCRIPT_EXECUTE_ACTION, request),
  evaluateMenu: (request: { actions: Array<{ showIf?: string; getName?: string; id: string; action: unknown }>; cardOrGroupId: number }) =>
    ipcRenderer.invoke(IPC_CHANNELS.SCRIPT_EVALUATE_MENU, request),
  onScriptEvent: (callback: (event: unknown) => void) => {
    const listener = (_event: unknown, data: unknown) => callback(data);
    ipcRenderer.on(IPC_CHANNELS.SCRIPT_EVENT, listener);
    return () => ipcRenderer.removeListener(IPC_CHANNELS.SCRIPT_EVENT, listener);
  },

  // Game definitions
  listInstalledGames: () => ipcRenderer.invoke(IPC_CHANNELS.GAMES_LIST_INSTALLED),
  listAvailableGames: () => ipcRenderer.invoke(IPC_CHANNELS.GAMES_LIST_AVAILABLE),
  installGame: (gameId: string, downloadUrl: string) =>
    ipcRenderer.invoke(IPC_CHANNELS.GAMES_INSTALL, gameId, downloadUrl),
  uninstallGame: (gameId: string) =>
    ipcRenderer.invoke(IPC_CHANNELS.GAMES_UNINSTALL, gameId),
  onInstallProgress: (callback: (progress: unknown) => void) => {
    const listener = (_event: unknown, data: unknown) => callback(data);
    ipcRenderer.on(IPC_CHANNELS.GAMES_INSTALL_PROGRESS, listener);
    return () => ipcRenderer.removeListener(IPC_CHANNELS.GAMES_INSTALL_PROGRESS, listener);
  },

  // Script dialogs
  onDialogRequest: (callback: (request: { requestId: string; type: string; params: Record<string, unknown> }) => void) => {
    const listener = (_event: unknown, data: { requestId: string; type: string; params: Record<string, unknown> }) => callback(data);
    ipcRenderer.on(IPC_CHANNELS.SCRIPT_DIALOG_REQUEST, listener);
    return () => ipcRenderer.removeListener(IPC_CHANNELS.SCRIPT_DIALOG_REQUEST, listener);
  },
  sendDialogResponse: (requestId: string, result: unknown) =>
    ipcRenderer.send(IPC_CHANNELS.SCRIPT_DIALOG_RESPONSE, { requestId, result }),

  // Game window management
  closeGameWindow: () => ipcRenderer.invoke(IPC_CHANNELS.CLOSE_GAME_WINDOW),

  // Feeds
  listFeeds: () => ipcRenderer.invoke(IPC_CHANNELS.FEEDS_LIST),
  addFeed: (name: string, url: string, username?: string, password?: string) =>
    ipcRenderer.invoke(IPC_CHANNELS.FEEDS_ADD, name, url, username, password),
  removeFeed: (name: string) => ipcRenderer.invoke(IPC_CHANNELS.FEEDS_REMOVE, name),
  setFeedEnabled: (name: string, enabled: boolean) =>
    ipcRenderer.invoke(IPC_CHANNELS.FEEDS_SET_ENABLED, name, enabled),
};

contextBridge.exposeInMainWorld('octgn', api);

export type OctgnAPI = typeof api;
