import { app, BrowserWindow, ipcMain, dialog } from 'electron';
import * as fs from 'fs';
import * as path from 'path';
import { GameServer } from './server/GameServer';
import { WebSocketBridge } from './server/WebSocketBridge';

let mainWindow: BrowserWindow | null = null;
let gameServer: GameServer | null = null;
let wsBridge: WebSocketBridge | null = null;

const isDev = process.env.NODE_ENV === 'development' || !app.isPackaged;
const WS_BRIDGE_PORT = 8889;

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 1400,
    height: 900,
    minWidth: 1024,
    minHeight: 768,
    title: 'OCTGN',
    icon: path.join(__dirname, 'assets/icon.png'),
    backgroundColor: '#1a1a2e',
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      preload: path.join(__dirname, 'preload.js'),
    },
  });

  if (isDev) {
    mainWindow.loadURL('http://localhost:5173');
    mainWindow.webContents.openDevTools();
  } else {
    mainWindow.loadFile(path.join(__dirname, 'renderer/index.html'));
  }

  mainWindow.on('closed', () => {
    mainWindow = null;
  });
}

// IPC Handlers
ipcMain.handle('start-server', async (_, port: number) => {
  if (gameServer) {
    return { success: false, error: 'Server already running' };
  }
  gameServer = new GameServer({ port });
  await gameServer.start();
  return { success: true, port };
});

ipcMain.handle('stop-server', async () => {
  if (gameServer) {
    await gameServer.stop();
    gameServer = null;
    return { success: true };
  }
  return { success: false, error: 'No server running' };
});

ipcMain.handle('connect-to-server', async (_, host: string, port: number) => {
  // This will be handled by the renderer via WebSocket
  return { success: true, host, port };
});

// File dialog handlers
ipcMain.handle('open-file-dialog', async (_, options: Electron.OpenDialogOptions) => {
  const result = await dialog.showOpenDialog(mainWindow!, options);
  return result;
});

ipcMain.handle('save-file-dialog', async (_, options: Electron.SaveDialogOptions) => {
  const result = await dialog.showSaveDialog(mainWindow!, options);
  return result;
});

ipcMain.handle('read-file', async (_, path: string) => {
  try {
    const data = await fs.promises.readFile(path, 'utf-8');
    return { success: true, data };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

ipcMain.handle('write-file', async (_, path: string, data: string) => {
  try {
    await fs.promises.writeFile(path, data, 'utf-8');
    return { success: true };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

// App info handlers
ipcMain.handle('get-app-path', () => {
  return app.getAppPath();
});

ipcMain.handle('get-version', () => {
  return app.getVersion();
});

app.whenReady().then(async () => {
  // Start WebSocket bridge for renderer communication
  wsBridge = new WebSocketBridge(WS_BRIDGE_PORT);
  try {
    await wsBridge.start();
    console.log(`WebSocket bridge started on port ${WS_BRIDGE_PORT}`);
  } catch (error) {
    console.error('Failed to start WebSocket bridge:', error);
  }

  createWindow();

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow();
    }
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    if (gameServer) {
      gameServer.stop();
    }
    if (wsBridge) {
      wsBridge.stop();
    }
    app.quit();
  }
});
