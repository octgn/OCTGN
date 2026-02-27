/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_APP_TITLE: string;
  readonly VITE_WS_BRIDGE_PORT: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

// Window extensions
declare global {
  interface Window {
    electronAPI?: {
      startServer: (port: number) => Promise<{ success: boolean; port?: number; error?: string }>;
      stopServer: () => Promise<{ success: boolean }>;
      connectToServer: (host: string, port: number) => Promise<{ success: boolean }>;
      platform: string;
      isMac: boolean;
      isWindows: boolean;
      isLinux: boolean;
      wsBridgePort: number;
      openFileDialog: (options: Electron.OpenDialogOptions) => Promise<Electron.OpenDialogReturnValue>;
      saveFileDialog: (options: Electron.SaveDialogOptions) => Promise<Electron.SaveDialogReturnValue>;
      readFile: (path: string) => Promise<{ success: boolean; data?: string; error?: string }>;
      writeFile: (path: string, data: string) => Promise<{ success: boolean; error?: string }>;
      getAppPath: () => Promise<string>;
      getVersion: () => Promise<string>;
    };
  }
}

export {};
