/**
 * Credential storage for the Electron app.
 *
 * Reads/writes credentials from the same location the Windows WPF client uses:
 *   %LOCALAPPDATA%\Octgn\Config\settings.json  (username)
 *   %LOCALAPPDATA%\Octgn\Config\Password.dat   (encrypted password)
 *
 * Uses Electron safeStorage for password encryption (DPAPI on Windows,
 * Keychain on macOS, libsecret on Linux).
 */
import { safeStorage } from 'electron';
import { readFile, writeFile, mkdir } from 'fs/promises';
import { existsSync } from 'fs';
import path from 'path';
import os from 'os';
import { log, logError } from '../logger';

export interface SavedCredentials {
  username: string;
  password: string;
}

/** Resolve the OCTGN config directory — same path the WPF client uses. */
function getConfigDir(): string {
  const octgnData = process.env.OCTGN_DATA;
  if (octgnData) {
    return path.join(octgnData, 'Config');
  }
  // Default: %LOCALAPPDATA%\Octgn\Config  (Windows)
  // On non-Windows: ~/.local/share/Octgn/Config  (best-effort match)
  const localAppData =
    process.env.LOCALAPPDATA ?? path.join(os.homedir(), '.local', 'share');
  return path.join(localAppData, 'Octgn', 'Config');
}

const SETTINGS_FILE = 'settings.json';
const PASSWORD_FILE = 'Password.dat';

async function ensureConfigDir(): Promise<string> {
  const dir = getConfigDir();
  if (!existsSync(dir)) {
    await mkdir(dir, { recursive: true });
  }
  return dir;
}

/** Read the settings.json file; returns an empty object on any failure. */
async function readSettings(): Promise<Record<string, unknown>> {
  try {
    const dir = getConfigDir();
    const raw = await readFile(path.join(dir, SETTINGS_FILE), 'utf-8');
    return JSON.parse(raw);
  } catch {
    return {};
  }
}

/** Write a single key into the settings.json file. */
async function writeSetting(key: string, value: string): Promise<void> {
  const dir = await ensureConfigDir();
  const settings = await readSettings();
  settings[key] = value;
  await writeFile(
    path.join(dir, SETTINGS_FILE),
    JSON.stringify(settings, null, 2),
    'utf-8',
  );
}

/** Encrypt a password string using Electron safeStorage. */
function encryptPassword(password: string): Buffer {
  if (!safeStorage.isEncryptionAvailable()) {
    // Fallback: base64 (not secure, but functional on systems without keyring)
    return Buffer.from(password, 'utf-8');
  }
  return safeStorage.encryptString(password);
}

/** Decrypt a previously-encrypted password buffer. */
function decryptPassword(encrypted: Buffer): string {
  if (!safeStorage.isEncryptionAvailable()) {
    return encrypted.toString('utf-8');
  }
  return safeStorage.decryptString(encrypted);
}

/**
 * Save credentials to the OCTGN config directory.
 */
export async function saveCredentials(
  username: string,
  password: string,
): Promise<void> {
  try {
    const dir = await ensureConfigDir();
    // Save username into settings.json
    await writeSetting('Username', username);
    // Save encrypted password into Password.dat
    const encrypted = encryptPassword(password);
    await writeFile(path.join(dir, PASSWORD_FILE), encrypted);
    log('CRED', `Saved credentials for "${username}"`);
  } catch (err) {
    logError('CRED', err);
    throw new Error('Failed to save credentials');
  }
}

/**
 * Load previously saved credentials. Returns null if none exist.
 */
export async function loadCredentials(): Promise<SavedCredentials | null> {
  try {
    const dir = getConfigDir();
    const settings = await readSettings();
    const username = (settings.Username as string) ?? '';
    if (!username) return null;

    const passwordPath = path.join(dir, PASSWORD_FILE);
    if (!existsSync(passwordPath)) return { username, password: '' };

    const encrypted = await readFile(passwordPath);
    if (encrypted.length === 0) return { username, password: '' };

    const password = decryptPassword(encrypted);
    return { username, password };
  } catch (err) {
    logError('CRED', err);
    return null;
  }
}

/**
 * Clear saved credentials.
 */
export async function clearCredentials(): Promise<void> {
  try {
    const dir = await ensureConfigDir();
    await writeSetting('Username', '');
    // Overwrite password file with empty content
    await writeFile(path.join(dir, PASSWORD_FILE), Buffer.alloc(0));
    log('CRED', 'Cleared saved credentials');
  } catch (err) {
    logError('CRED', err);
  }
}
