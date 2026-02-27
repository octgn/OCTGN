/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_APP_TITLE: string;
  readonly VITE_WS_BRIDGE_PORT: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

// Electron dialog types
declare namespace Electron {
  interface OpenDialogOptions {
    title?: string;
    defaultPath?: string;
    buttonLabel?: string;
    filters?: Array<{ name: string; extensions: string[] }>;
    properties?: Array<
      | 'openFile'
      | 'openDirectory'
      | 'multiSelections'
      | 'showHiddenFiles'
      | 'createDirectory'
      | 'promptToCreate'
      | 'noResolveAliases'
      | 'treatPackageAsDirectory'
      | 'dontAddToRecent'
    >;
  }

  interface SaveDialogOptions {
    title?: string;
    defaultPath?: string;
    buttonLabel?: string;
    filters?: Array<{ name: string; extensions: string[] }>;
    properties?: Array<
      | 'showHiddenFiles'
      | 'createDirectory'
      | 'treatPackageAsDirectory'
      | 'showOverwriteConfirmation'
      | 'dontAddToRecent'
    >;
  }

  interface OpenDialogReturnValue {
    canceled: boolean;
    filePaths: string[];
  }

  interface SaveDialogReturnValue {
    canceled: boolean;
    filePath?: string;
  }
}

// Full Electron API interface
interface ElectronAPI {
  // Game Server
  startServer: (port: number) => Promise<{ success: boolean; port?: number; error?: string }>;
  stopServer: () => Promise<{ success: boolean; error?: string }>;
  getServerStatus: () => Promise<{ running: boolean; port?: number }>;

  // Game Installation
  installGame: (
    downloadUrl: string,
    gameId: string
  ) => Promise<{ success: boolean; path?: string; error?: string }>;
  uninstallGame: (gameId: string) => Promise<{ success: boolean; error?: string }>;
  getInstalledGames: () => Promise<{
    success: boolean;
    games?: Array<{ id: string; path: string }>;
    error?: string;
  }>;

  // Deck Management
  saveDeck: (
    deckName: string,
    deckData: string
  ) => Promise<{ success: boolean; path?: string; error?: string }>;
  loadDeck: (deckPath: string) => Promise<{ success: boolean; data?: string; error?: string }>;
  listDecks: () => Promise<{
    success: boolean;
    decks?: Array<{ name: string; path: string }>;
    error?: string;
  }>;

  // File System
  openFileDialog: (
    options: Electron.OpenDialogOptions
  ) => Promise<Electron.OpenDialogReturnValue>;
  saveFileDialog: (
    options: Electron.SaveDialogOptions
  ) => Promise<Electron.SaveDialogReturnValue>;
  readFile: (filePath: string) => Promise<{ success: boolean; data?: string; error?: string }>;
  writeFile: (filePath: string, data: string) => Promise<{ success: boolean; error?: string }>;
  deleteFile: (filePath: string) => Promise<{ success: boolean; error?: string }>;
  listFiles: (
    dirPath: string,
    extension?: string
  ) => Promise<{
    success: boolean;
    files?: Array<{ name: string; path: string; isDirectory: boolean }>;
    error?: string;
  }>;

  // App Info
  getAppPath: () => Promise<string>;
  getGamesPath: () => Promise<string>;
  getDecksPath: () => Promise<string>;
  getVersion: () => Promise<string>;

  // Utilities
  openExternal: (url: string) => Promise<{ success: boolean }>;

  // Window Controls
  toggleFullscreen: () => Promise<{ success: boolean; fullscreen?: boolean; error?: string }>;
  setFullscreen: (fullscreen: boolean) => Promise<{ success: boolean; error?: string }>;
  minimize: () => Promise<{ success: boolean }>;
  maximize: () => Promise<{ success: boolean; maximized?: boolean }>;
  quit: () => Promise<{ success: boolean }>;

  // Platform
  platform: 'win32' | 'darwin' | 'linux' | string;
  isMac: boolean;
  isWindows: boolean;
  isLinux: boolean;

  // Config
  wsBridgePort: 32457;      // Internal - dev tools
  vitePort: 32456;          // Internal - dev tools
  gameServerPort: 8888;     // OCTGN standard - for interop
  lanBroadcastPort: 21234;  // OCTGN standard - for interop
  isDev: boolean;
}

// Window extensions
declare global {
  interface Window {
    electronAPI?: ElectronAPI;
  }
}

export {};
