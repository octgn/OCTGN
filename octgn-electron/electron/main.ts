import { app, BrowserWindow, ipcMain } from 'electron';
import * as path from 'path';
import { GameServer } from './server/GameServer';

let mainWindow: BrowserWindow | null = null;
let gameServer: GameServer | null = null;

const isDev = process.env.NODE_ENV === 'development' || !app.isPackaged;

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
  gameServer = new GameServer(port);
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

app.whenReady().then(() => {
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
    app.quit();
  }
});
