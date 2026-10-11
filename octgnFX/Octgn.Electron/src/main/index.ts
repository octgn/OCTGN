import { app, BrowserWindow, ipcMain } from 'electron';
import * as path from 'path';
import { setupIpcHandlers } from './ipc/handlers';
import { registerAssetScheme, registerAssetProtocol } from './asset-protocol';
import { log, logError } from './logger';

// Register custom schemes before app is ready
registerAssetScheme();

let mainWindow: BrowserWindow | null = null;

function createWindow(): void {
  mainWindow = new BrowserWindow({
    width: 1280,
    height: 800,
    minWidth: 900,
    minHeight: 600,
    title: 'OCTGN',
    frame: false,
    backgroundColor: '#0a0e17',
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: false,
    },
  });

  if (process.env.NODE_ENV === 'development') {
    mainWindow.loadURL('http://localhost:5173');
    mainWindow.webContents.openDevTools();
  } else {
    mainWindow.loadFile(path.join(__dirname, '../renderer/index.html'));
  }

  // Capture renderer console messages (errors, warnings, logs) into the main log file
  mainWindow.webContents.on('console-message', (...args: unknown[]) => {
    // Electron 34: (event, level, message, line, sourceId) or (event) with properties
    const labels = ['DEBUG', 'INFO', 'WARN', 'ERROR'];
    let msg = '';
    if (args.length >= 3) {
      // Old-style: (event, level, message, line, sourceId)
      const level = args[1] as number;
      msg = `[${labels[level] ?? 'LOG'}] ${args[2]}`;
    } else if (args.length === 1 && typeof args[0] === 'object' && args[0] !== null) {
      // New-style: single event object
      const e = args[0] as Record<string, unknown>;
      const level = (e.level as number) ?? 1;
      msg = `[${labels[level] ?? 'LOG'}] ${e.message ?? ''}`;
    } else {
      msg = String(args);
    }
    log('RENDERER', msg);
  });

  // Capture renderer crashes
  mainWindow.webContents.on('render-process-gone', (_event, details) => {
    log('RENDERER', `Process gone: reason=${details.reason}, exitCode=${details.exitCode}`);
  });

  mainWindow.on('closed', () => {
    mainWindow = null;
  });
}

log('APP', `Starting OCTGN Electron — pid=${process.pid}, node=${process.version}, electron=${process.versions.electron}`);
log('APP', `userData: ${app.getPath('userData')}`);

app.whenReady().then(() => {
  log('APP', 'App ready, creating window');
  registerAssetProtocol();
  createWindow();
  setupIpcHandlers(ipcMain);
  log('APP', 'IPC handlers registered');

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow();
    }
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

export function getMainWindow(): BrowserWindow | null {
  return mainWindow;
}
