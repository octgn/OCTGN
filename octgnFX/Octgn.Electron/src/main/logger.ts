/**
 * Simple file logger for main process debugging.
 * Writes to octgn-electron.log in Electron's userData directory
 * (e.g. %APPDATA%/octgn-electron/octgn-electron.log on Windows).
 */
import { appendFileSync, mkdirSync } from 'fs';
import { join } from 'path';
import { app } from 'electron';

function getLogPath(): string {
  const dir = app.getPath('userData');
  mkdirSync(dir, { recursive: true });
  return join(dir, 'octgn-electron.log');
}

// Lazily initialized after app is ready
let _logPath: string | null = null;

function logPath(): string {
  if (!_logPath) _logPath = getLogPath();
  return _logPath;
}

function timestamp(): string {
  return new Date().toISOString();
}

export function log(tag: string, ...args: unknown[]): void {
  const msg = args.map((a) => (typeof a === 'string' ? a : JSON.stringify(a, null, 2))).join(' ');
  const line = `[${timestamp()}] [${tag}] ${msg}\n`;
  try {
    appendFileSync(logPath(), line);
  } catch { /* ignore during early startup */ }
  console.log(`[${tag}]`, ...args);
}

export function logError(tag: string, err: unknown): void {
  const msg = err instanceof Error ? `${err.message}\n${err.stack}` : String(err);
  log(tag, `ERROR: ${msg}`);
}

/** Returns the full path to the log file (for display to user). */
export function getLogFilePath(): string {
  return logPath();
}
