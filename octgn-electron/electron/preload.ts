import { contextBridge, ipcRenderer } from 'electron';

// Expose protected methods that allow the renderer process to use
// the ipcRenderer without exposing the entire object
contextBridge.exposeInMainWorld('electronAPI', {
  // ============================================
  // Game Server
  // ============================================
  startServer: (port: number) => ipcRenderer.invoke('start-server', port),
  stopServer: () => ipcRenderer.invoke('stop-server'),
  getServerStatus: () => ipcRenderer.invoke('get-server-status'),

  // ============================================
  // Game Installation
  // ============================================
  installGame: (downloadUrl: string, gameId: string) =>
    ipcRenderer.invoke('install-game', downloadUrl, gameId),
  uninstallGame: (gameId: string) =>
    ipcRenderer.invoke('uninstall-game', gameId),
  getInstalledGames: () => ipcRenderer.invoke('get-installed-games'),

  // ============================================
  // Deck Management
  // ============================================
  saveDeck: (deckName: string, deckData: string) =>
    ipcRenderer.invoke('save-deck', deckName, deckData),
  loadDeck: (deckPath: string) =>
    ipcRenderer.invoke('load-deck', deckPath),
  listDecks: () => ipcRenderer.invoke('list-decks'),

  // ============================================
  // File System
  // ============================================
  openFileDialog: (options: Electron.OpenDialogOptions) =>
    ipcRenderer.invoke('open-file-dialog', options),
  saveFileDialog: (options: Electron.SaveDialogOptions) =>
    ipcRenderer.invoke('save-file-dialog', options),
  readFile: (filePath: string) =>
    ipcRenderer.invoke('read-file', filePath),
  writeFile: (filePath: string, data: string) =>
    ipcRenderer.invoke('write-file', filePath, data),
  deleteFile: (filePath: string) =>
    ipcRenderer.invoke('delete-file', filePath),
  listFiles: (dirPath: string, extension?: string) =>
    ipcRenderer.invoke('list-files', dirPath, extension),

  // ============================================
  // App Info
  // ============================================
  getAppPath: () => ipcRenderer.invoke('get-app-path'),
  getGamesPath: () => ipcRenderer.invoke('get-games-path'),
  getDecksPath: () => ipcRenderer.invoke('get-decks-path'),
  getVersion: () => ipcRenderer.invoke('get-version'),

  // ============================================
  // Utilities
  // ============================================
  openExternal: (url: string) => ipcRenderer.invoke('open-external', url),
  
  // Window controls
  toggleFullscreen: () => ipcRenderer.invoke('toggle-fullscreen'),
  setFullscreen: (fullscreen: boolean) => ipcRenderer.invoke('set-fullscreen', fullscreen),
  minimize: () => ipcRenderer.invoke('minimize-window'),
  maximize: () => ipcRenderer.invoke('maximize-window'),
  quit: () => ipcRenderer.invoke('quit-app'),
  
  // ============================================
  // OCTGN API (bypasses CORS)
  // ============================================
  octgnLogin: (username: string, password: string) =>
    ipcRenderer.invoke('octgn-login', username, password),
  octgnCreateSession: (username: string, password: string, deviceId: string) =>
    ipcRenderer.invoke('octgn-create-session', username, password, deviceId),
  octgnValidateSession: (userId: string, deviceId: string, sessionKey: string) =>
    ipcRenderer.invoke('octgn-validate-session', userId, deviceId, sessionKey),
  octgnClearSession: (userId: string, deviceId: string, sessionKey: string) =>
    ipcRenderer.invoke('octgn-clear-session', userId, deviceId, sessionKey),
  octgnGetGames: () => ipcRenderer.invoke('octgn-get-games'),
  octgnGetStats: () => ipcRenderer.invoke('octgn-get-stats'),
  octgnGetReleaseInfo: () => ipcRenderer.invoke('octgn-get-release-info'),
  
  // Platform info
  platform: process.platform,
  isMac: process.platform === 'darwin',
  isWindows: process.platform === 'win32',
  isLinux: process.platform === 'linux',
  
  // WebSocket bridge port (internal - for dev tools)
  wsBridgePort: 32457,
  
  // Vite dev server port (internal - for dev tools)
  vitePort: 32456,
  
  // Game server port (OCTGN standard - for interop)
  gameServerPort: 8888,
  
  // LAN discovery port (OCTGN standard - for interop)
  lanBroadcastPort: 21234,
  
  // Development mode
  isDev: process.env.NODE_ENV === 'development',
});
