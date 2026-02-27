import { contextBridge, ipcRenderer } from 'electron';

// Expose protected methods that allow the renderer process to use
// the ipcRenderer without exposing the entire object
contextBridge.exposeInMainWorld('electronAPI', {
  // Game server
  startServer: (port: number) => ipcRenderer.invoke('start-server', port),
  stopServer: () => ipcRenderer.invoke('stop-server'),
  
  // Legacy connect (handled via WebSocket bridge now)
  connectToServer: (host: string, port: number) =>
    ipcRenderer.invoke('connect-to-server', host, port),

  // Platform info
  platform: process.platform,
  isMac: process.platform === 'darwin',
  isWindows: process.platform === 'win32',
  isLinux: process.platform === 'linux',
  
  // WebSocket bridge port
  wsBridgePort: 8889,

  // File system operations (for deck loading/saving)
  openFileDialog: (options: any) => ipcRenderer.invoke('open-file-dialog', options),
  saveFileDialog: (options: any) => ipcRenderer.invoke('save-file-dialog', options),
  readFile: (path: string) => ipcRenderer.invoke('read-file', path),
  writeFile: (path: string, data: string) => ipcRenderer.invoke('write-file', path, data),

  // App info
  getAppPath: () => ipcRenderer.invoke('get-app-path'),
  getVersion: () => ipcRenderer.invoke('get-version'),
});
