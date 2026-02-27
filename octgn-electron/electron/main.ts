import { app, BrowserWindow, ipcMain, dialog, shell } from 'electron';
import * as fs from 'fs';
import * as path from 'path';
import * as https from 'https';
import * as http from 'http';
import { createWriteStream } from 'fs';
import { mkdir, rm, readdir } from 'fs/promises';
import AdmZip from 'adm-zip';
import { GameServer } from './server/GameServer';
import { WebSocketBridge } from './server/WebSocketBridge';

let mainWindow: BrowserWindow | null = null;
let gameServer: GameServer | null = null;
let wsBridge: WebSocketBridge | null = null;

const isDev = process.env.NODE_ENV === 'development' || !app.isPackaged;
const WS_BRIDGE_PORT = 8889;

// Data paths
function getDataPath(): string {
  const userDataPath = app.getPath('userData');
  const dataPath = path.join(userDataPath, 'octgn-data');
  
  if (!fs.existsSync(dataPath)) {
    fs.mkdirSync(dataPath, { recursive: true });
  }
  
  return dataPath;
}

function getGamesPath(): string {
  const dataPath = getDataPath();
  const gamesPath = path.join(dataPath, 'games');
  
  if (!fs.existsSync(gamesPath)) {
    fs.mkdirSync(gamesPath, { recursive: true });
  }
  
  return gamesPath;
}

function getDecksPath(): string {
  const dataPath = getDataPath();
  const decksPath = path.join(dataPath, 'decks');
  
  if (!fs.existsSync(decksPath)) {
    fs.mkdirSync(decksPath, { recursive: true });
  }
  
  return decksPath;
}

function createWindow() {
  // Check for icon file (may not exist in dev)
  const iconPath = path.join(__dirname, 'assets', 'icon.png');
  const iconExists = fs.existsSync(iconPath);

  mainWindow = new BrowserWindow({
    width: 1400,
    height: 900,
    minWidth: 1024,
    minHeight: 768,
    title: 'OCTGN',
    ...(iconExists ? { icon: iconPath } : {}),
    backgroundColor: '#171717',
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

  // Handle external links
  mainWindow.webContents.setWindowOpenHandler(({ url }) => {
    if (url.startsWith('http://') || url.startsWith('https://')) {
      shell.openExternal(url);
    }
    return { action: 'deny' };
  });

  // Set Content Security Policy for dev mode
  if (isDev) {
    mainWindow.webContents.session.webRequest.onHeadersReceived((details, callback) => {
      callback({
        responseHeaders: {
          ...details.responseHeaders,
          'Content-Security-Policy': [
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' http://localhost:5173; " +
            "style-src 'self' 'unsafe-inline' http://localhost:5173; " +
            "img-src 'self' data: https: http:; " +
            "font-src 'self' data:; " +
            "connect-src 'self' ws://localhost:* http://localhost:* https://www.octgn.net https://*.myget.org"
          ],
        },
      });
    });
  }
}

// ============================================
// IPC Handlers
// ============================================

// Server management
ipcMain.handle('start-server', async (_, port: number) => {
  if (gameServer) {
    return { success: false, error: 'Server already running' };
  }
  
  try {
    gameServer = new GameServer({ port });
    await gameServer.start();
    return { success: true, port };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

ipcMain.handle('stop-server', async () => {
  if (gameServer) {
    await gameServer.stop();
    gameServer = null;
    return { success: true };
  }
  return { success: false, error: 'No server running' };
});

ipcMain.handle('get-server-status', () => {
  return {
    running: gameServer !== null,
    port: gameServer?.['port'] || null,
  };
});

// File dialogs
ipcMain.handle('open-file-dialog', async (_, options: Electron.OpenDialogOptions) => {
  const result = await dialog.showOpenDialog(mainWindow!, options);
  return result;
});

ipcMain.handle('save-file-dialog', async (_, options: Electron.SaveDialogOptions) => {
  const result = await dialog.showSaveDialog(mainWindow!, options);
  return result;
});

// File operations
ipcMain.handle('read-file', async (_, filePath: string) => {
  try {
    const data = await fs.promises.readFile(filePath, 'utf-8');
    return { success: true, data };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

ipcMain.handle('write-file', async (_, filePath: string, data: string) => {
  try {
    // Ensure directory exists
    const dir = path.dirname(filePath);
    await mkdir(dir, { recursive: true });
    
    await fs.promises.writeFile(filePath, data, 'utf-8');
    return { success: true };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

ipcMain.handle('delete-file', async (_, filePath: string) => {
  try {
    await rm(filePath, { force: true });
    return { success: true };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

ipcMain.handle('list-files', async (_, dirPath: string, extension?: string) => {
  try {
    let files = await readdir(dirPath, { withFileTypes: true });
    
    if (extension) {
      files = files.filter(f => f.isFile() && f.name.endsWith(extension));
    }
    
    return {
      success: true,
      files: files.map(f => ({
        name: f.name,
        path: path.join(dirPath, f.name),
        isDirectory: f.isDirectory(),
      })),
    };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

// Game installation
ipcMain.handle('install-game', async (_, downloadUrl: string, gameId: string) => {
  const gamesPath = getGamesPath();
  const gamePath = path.join(gamesPath, gameId);
  const tempPath = path.join(gamesPath, `${gameId}.tmp`);
  
  try {
    // Create temp directory
    await mkdir(tempPath, { recursive: true });
    
    // Download file
    const fileName = downloadUrl.split('/').pop() || `${gameId}.nupkg`;
    const filePath = path.join(tempPath, fileName);
    
    await downloadFile(downloadUrl, filePath);
    
    // Extract (for .nupkg files, they're just zip files)
    const extractPath = path.join(gamePath);
    await extractZip(filePath, extractPath);
    
    // Clean up temp
    await rm(tempPath, { recursive: true, force: true });
    
    return { success: true, path: gamePath };
  } catch (error: any) {
    // Clean up on error
    try {
      await rm(tempPath, { recursive: true, force: true });
      await rm(gamePath, { recursive: true, force: true });
    } catch {}
    
    return { success: false, error: error.message };
  }
});

ipcMain.handle('uninstall-game', async (_, gameId: string) => {
  const gamesPath = getGamesPath();
  const gamePath = path.join(gamesPath, gameId);
  
  try {
    await rm(gamePath, { recursive: true, force: true });
    return { success: true };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

ipcMain.handle('get-installed-games', async () => {
  const gamesPath = getGamesPath();
  
  try {
    const dirs = await readdir(gamesPath, { withFileTypes: true });
    const games = dirs
      .filter(d => d.isDirectory())
      .map(d => ({
        id: d.name,
        path: path.join(gamesPath, d.name),
      }));
    
    return { success: true, games };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

// Deck operations
ipcMain.handle('save-deck', async (_, deckName: string, deckData: string) => {
  const decksPath = getDecksPath();
  const fileName = deckName.endsWith('.o8d') ? deckName : `${deckName}.o8d`;
  const filePath = path.join(decksPath, fileName);
  
  try {
    await fs.promises.writeFile(filePath, deckData, 'utf-8');
    return { success: true, path: filePath };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

ipcMain.handle('load-deck', async (_, deckPath: string) => {
  try {
    const data = await fs.promises.readFile(deckPath, 'utf-8');
    return { success: true, data };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

ipcMain.handle('list-decks', async () => {
  const decksPath = getDecksPath();
  
  try {
    const files = await readdir(decksPath, { withFileTypes: true });
    const decks = files
      .filter(f => f.isFile() && f.name.endsWith('.o8d'))
      .map(f => ({
        name: f.name,
        path: path.join(decksPath, f.name),
      }));
    
    return { success: true, decks };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
});

// App info
ipcMain.handle('get-app-path', () => getDataPath());
ipcMain.handle('get-games-path', () => getGamesPath());
ipcMain.handle('get-decks-path', () => getDecksPath());
ipcMain.handle('get-version', () => app.getVersion());

// Open external URL
ipcMain.handle('open-external', async (_, url: string) => {
  await shell.openExternal(url);
  return { success: true };
});

// Window controls
ipcMain.handle('toggle-fullscreen', async () => {
  if (mainWindow) {
    mainWindow.setFullScreen(!mainWindow.isFullScreen());
    return { success: true, fullscreen: mainWindow.isFullScreen() };
  }
  return { success: false, error: 'No window' };
});

ipcMain.handle('set-fullscreen', async (_, fullscreen: boolean) => {
  if (mainWindow) {
    mainWindow.setFullScreen(fullscreen);
    return { success: true };
  }
  return { success: false, error: 'No window' };
});

ipcMain.handle('minimize-window', async () => {
  if (mainWindow) {
    mainWindow.minimize();
    return { success: true };
  }
  return { success: false };
});

ipcMain.handle('maximize-window', async () => {
  if (mainWindow) {
    if (mainWindow.isMaximized()) {
      mainWindow.unmaximize();
    } else {
      mainWindow.maximize();
    }
    return { success: true, maximized: mainWindow.isMaximized() };
  }
  return { success: false };
});

ipcMain.handle('quit-app', async () => {
  app.quit();
  return { success: true };
});

// Helper functions
function downloadFile(url: string, destPath: string): Promise<void> {
  return new Promise((resolve, reject) => {
    const protocol = url.startsWith('https') ? https : http;
    
    const file = createWriteStream(destPath);
    
    protocol.get(url, (response) => {
      // Handle redirects
      if (response.statusCode === 301 || response.statusCode === 302) {
        const redirectUrl = response.headers.location;
        if (redirectUrl) {
          file.close();
          downloadFile(redirectUrl, destPath).then(resolve).catch(reject);
          return;
        }
      }
      
      if (response.statusCode !== 200) {
        file.close();
        reject(new Error(`HTTP ${response.statusCode}`));
        return;
      }
      
      response.pipe(file);
      
      file.on('finish', () => {
        file.close();
        resolve();
      });
    }).on('error', (err) => {
      file.close();
      reject(err);
    });
  });
}

async function extractZip(zipPath: string, destPath: string): Promise<void> {
  const zip = new AdmZip(zipPath);
  zip.extractAllTo(destPath, true);
}

// App lifecycle
app.whenReady().then(async () => {
  // Initialize data directories
  getDataPath();
  getGamesPath();
  getDecksPath();
  
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
