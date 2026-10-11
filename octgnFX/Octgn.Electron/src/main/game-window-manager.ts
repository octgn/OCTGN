import { BrowserWindow, type IpcMainInvokeEvent } from 'electron';
import * as path from 'path';
import { GameService, type SavedConnectionInfo } from './api/game-service';
import { ImageResolver } from './games/image-resolver';
import { log, logError } from './logger';

export interface JoinParams {
  host: string;
  port: number;
  nickname: string;
  userId: string;
  gameId: string;
  gameVersion: string;
  password: string;
  spectator: boolean;
}

interface GameWindowEntry {
  window: BrowserWindow;
  service: GameService;
}

/**
 * Manages game BrowserWindows and their associated GameService instances.
 * Each game runs in its own window with its own TCP connection.
 */
export class GameWindowManager {
  private windows = new Map<number, GameWindowEntry>();
  private sharedImageResolver = new ImageResolver();

  /**
   * Get the shared ImageResolver used by the asset protocol.
   */
  getImageResolver(): ImageResolver {
    return this.sharedImageResolver;
  }

  /**
   * Create a new game window and connect to the game server.
   */
  async createGameWindow(params: JoinParams): Promise<{ success: boolean; error?: string; windowId?: number }> {
    const win = new BrowserWindow({
      width: 1280,
      height: 800,
      minWidth: 900,
      minHeight: 600,
      title: 'OCTGN - Game',
      frame: false,
      backgroundColor: '#0a0e17',
      webPreferences: {
        preload: path.join(__dirname, 'preload.js'),
        contextIsolation: true,
        nodeIntegration: false,
      },
    });

    const service = new GameService();
    service.setTargetWebContents(win.webContents);

    // Store in map before joining so event routing works during handshake
    this.windows.set(win.id, { window: win, service });

    // Wire window close to cleanup
    win.on('closed', () => {
      log('GAME_WINDOW', `Game window ${win.id} closed`);
      const entry = this.windows.get(win.id);
      if (entry) {
        entry.service.leaveGame();
        this.windows.delete(win.id);
      }
    });

    // Capture renderer console messages for game windows
    win.webContents.on('console-message', (...args: unknown[]) => {
      const labels = ['DEBUG', 'INFO', 'WARN', 'ERROR'];
      let msg = '';
      if (args.length >= 3) {
        const level = args[1] as number;
        msg = `[${labels[level] ?? 'LOG'}] ${args[2]}`;
      } else if (args.length === 1 && typeof args[0] === 'object' && args[0] !== null) {
        const e = args[0] as Record<string, unknown>;
        const level = (e.level as number) ?? 1;
        msg = `[${labels[level] ?? 'LOG'}] ${e.message ?? ''}`;
      } else {
        msg = String(args);
      }
      log(`RENDERER[game-${win.id}]`, msg);
    });

    // Load the game window URL
    if (process.env.NODE_ENV === 'development') {
      win.loadURL('http://localhost:5173?window=game');
    } else {
      win.loadFile(path.join(__dirname, '../renderer/index.html'), {
        query: { window: 'game' },
      });
    }

    // Join the game
    try {
      const result = await service.joinGame(
        params.host,
        params.port,
        params.nickname,
        params.userId,
        params.gameId,
        params.gameVersion,
        params.password,
        params.spectator,
      );

      if (!result.success) {
        log('GAME_WINDOW', `Join failed: ${result.error}, destroying window ${win.id}`);
        this.windows.delete(win.id);
        win.destroy();
        return { success: false, error: result.error };
      }

      log('GAME_WINDOW', `Game window ${win.id} created and joined successfully`);
      return { success: true, windowId: win.id };
    } catch (err) {
      logError('GAME_WINDOW', err);
      this.windows.delete(win.id);
      win.destroy();
      return { success: false, error: (err as Error).message };
    }
  }

  /**
   * Look up the GameService for an IPC event by its sender webContents.
   */
  getServiceForEvent(event: IpcMainInvokeEvent): GameService | null {
    const win = BrowserWindow.fromWebContents(event.sender);
    if (!win) return null;
    return this.windows.get(win.id)?.service ?? null;
  }

  /**
   * Check if a window ID belongs to a game window.
   */
  has(windowId: number): boolean {
    return this.windows.has(windowId);
  }

  /**
   * Close a specific game window: leave game, destroy window, remove from map.
   */
  async closeGameWindow(windowId: number): Promise<void> {
    const entry = this.windows.get(windowId);
    if (!entry) return;
    log('GAME_WINDOW', `Closing game window ${windowId}`);
    this.windows.delete(windowId);
    await entry.service.leaveGame();
    if (!entry.window.isDestroyed()) {
      entry.window.destroy();
    }
  }

  /**
   * Close all game windows (called on app quit).
   */
  async closeAll(): Promise<void> {
    log('GAME_WINDOW', `Closing all ${this.windows.size} game window(s)`);
    const entries = Array.from(this.windows.entries());
    this.windows.clear();
    for (const [_id, entry] of entries) {
      await entry.service.leaveGame();
      if (!entry.window.isDestroyed()) {
        entry.window.destroy();
      }
    }
  }

  /**
   * Adopt an existing GameService (e.g. from reconnection) by creating a window for it.
   */
  async adoptService(service: GameService): Promise<number | null> {
    const win = new BrowserWindow({
      width: 1280,
      height: 800,
      minWidth: 900,
      minHeight: 600,
      title: 'OCTGN - Game',
      frame: false,
      backgroundColor: '#0a0e17',
      webPreferences: {
        preload: path.join(__dirname, 'preload.js'),
        contextIsolation: true,
        nodeIntegration: false,
      },
    });

    service.setTargetWebContents(win.webContents);
    this.windows.set(win.id, { window: win, service });

    win.on('closed', () => {
      log('GAME_WINDOW', `Adopted game window ${win.id} closed`);
      const entry = this.windows.get(win.id);
      if (entry) {
        entry.service.leaveGame();
        this.windows.delete(win.id);
      }
    });

    if (process.env.NODE_ENV === 'development') {
      win.loadURL('http://localhost:5173?window=game');
    } else {
      win.loadFile(path.join(__dirname, '../renderer/index.html'), {
        query: { window: 'game' },
      });
    }

    log('GAME_WINDOW', `Adopted service into game window ${win.id}`);
    return win.id;
  }
}
