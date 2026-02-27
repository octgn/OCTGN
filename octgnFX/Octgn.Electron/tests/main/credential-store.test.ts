import { describe, it, expect, vi, beforeEach } from 'vitest';

// ---------------------------------------------------------------------------
// Mocks — vi.mock calls are hoisted; factory functions must not reference
// variables defined in the same scope. Use vi.hoisted() for shared state.
// ---------------------------------------------------------------------------

const {
  mockEncryptionAvailable,
  mockEncryptString,
  mockDecryptString,
  mockReadFile,
  mockWriteFile,
  mockMkdir,
  mockExistsSync,
} = vi.hoisted(() => ({
  mockEncryptionAvailable: vi.fn(() => true),
  mockEncryptString: vi.fn((text: string) => Buffer.from(`encrypted:${text}`)),
  mockDecryptString: vi.fn((buf: Buffer) => {
    const str = buf.toString('utf-8');
    return str.startsWith('encrypted:') ? str.slice('encrypted:'.length) : str;
  }),
  mockReadFile: vi.fn(),
  mockWriteFile: vi.fn(),
  mockMkdir: vi.fn(),
  mockExistsSync: vi.fn(() => true),
}));

vi.mock('electron', () => ({
  safeStorage: {
    isEncryptionAvailable: mockEncryptionAvailable,
    encryptString: mockEncryptString,
    decryptString: mockDecryptString,
  },
}));

vi.mock('fs/promises', () => ({
  default: { readFile: mockReadFile, writeFile: mockWriteFile, mkdir: mockMkdir },
  readFile: mockReadFile,
  writeFile: mockWriteFile,
  mkdir: mockMkdir,
}));

vi.mock('fs', () => ({
  default: { existsSync: mockExistsSync },
  existsSync: mockExistsSync,
}));

vi.mock('@main/logger', () => ({
  log: vi.fn(),
  logError: vi.fn(),
}));

// Set a predictable config directory
process.env.LOCALAPPDATA = 'C:\\Users\\test\\AppData\\Local';

import {
  saveCredentials,
  loadCredentials,
  clearCredentials,
} from '@main/auth/credential-store';

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('credential-store', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockExistsSync.mockReturnValue(true);
    mockReadFile.mockReset();
    mockWriteFile.mockResolvedValue(undefined);
    mockMkdir.mockResolvedValue(undefined);
    mockEncryptionAvailable.mockReturnValue(true);
  });

  describe('saveCredentials', () => {
    it('should write username to settings.json and encrypted password to Password.dat', async () => {
      // settings.json doesn't exist yet
      mockReadFile.mockRejectedValueOnce(new Error('ENOENT'));

      await saveCredentials('alice', 'p@ssw0rd');

      // Should have written settings.json
      const settingsCall = mockWriteFile.mock.calls.find((c: unknown[]) =>
        (c[0] as string).includes('settings.json'),
      );
      expect(settingsCall).toBeTruthy();
      const writtenSettings = JSON.parse(settingsCall![1] as string);
      expect(writtenSettings.Username).toBe('alice');

      // Should have written Password.dat
      const passwordCall = mockWriteFile.mock.calls.find((c: unknown[]) =>
        (c[0] as string).includes('Password.dat'),
      );
      expect(passwordCall).toBeTruthy();
      expect(mockEncryptString).toHaveBeenCalledWith('p@ssw0rd');
    });

    it('should merge into existing settings.json', async () => {
      mockReadFile.mockResolvedValueOnce(
        JSON.stringify({ SomeOtherKey: 'value' }),
      );

      await saveCredentials('bob', 'secret');

      const settingsCall = mockWriteFile.mock.calls.find((c: unknown[]) =>
        (c[0] as string).includes('settings.json'),
      );
      const writtenSettings = JSON.parse(settingsCall![1] as string);
      expect(writtenSettings.SomeOtherKey).toBe('value');
      expect(writtenSettings.Username).toBe('bob');
    });

    it('should create config directory if it does not exist', async () => {
      mockExistsSync.mockReturnValue(false);
      mockReadFile.mockRejectedValueOnce(new Error('ENOENT'));

      await saveCredentials('charlie', 'pw');

      expect(mockMkdir).toHaveBeenCalledWith(
        expect.stringContaining('Octgn'),
        { recursive: true },
      );
    });

    it('should use plain buffer when safeStorage is unavailable', async () => {
      mockEncryptionAvailable.mockReturnValue(false);
      mockReadFile.mockRejectedValueOnce(new Error('ENOENT'));

      await saveCredentials('dave', 'mypassword');

      const passwordCall = mockWriteFile.mock.calls.find((c: unknown[]) =>
        (c[0] as string).includes('Password.dat'),
      );
      const written = passwordCall![1] as Buffer;
      expect(written.toString('utf-8')).toBe('mypassword');
      expect(mockEncryptString).not.toHaveBeenCalled();
    });
  });

  describe('loadCredentials', () => {
    it('should return username and decrypted password', async () => {
      mockReadFile
        .mockResolvedValueOnce(JSON.stringify({ Username: 'alice' }))
        .mockResolvedValueOnce(Buffer.from('encrypted:p@ssw0rd'));

      const creds = await loadCredentials();

      expect(creds).toEqual({ username: 'alice', password: 'p@ssw0rd' });
    });

    it('should return null when no username is saved', async () => {
      mockReadFile.mockResolvedValueOnce(JSON.stringify({}));

      const creds = await loadCredentials();

      expect(creds).toBeNull();
    });

    it('should return username with empty password when Password.dat is missing', async () => {
      mockReadFile.mockResolvedValueOnce(
        JSON.stringify({ Username: 'bob' }),
      );
      mockExistsSync.mockImplementation((p: unknown) =>
        !String(p).includes('Password.dat'),
      );

      const creds = await loadCredentials();

      expect(creds).toEqual({ username: 'bob', password: '' });
    });

    it('should return username with empty password when Password.dat is empty', async () => {
      mockReadFile
        .mockResolvedValueOnce(JSON.stringify({ Username: 'charlie' }))
        .mockResolvedValueOnce(Buffer.alloc(0));

      const creds = await loadCredentials();

      expect(creds).toEqual({ username: 'charlie', password: '' });
    });

    it('should return null when settings.json is corrupt', async () => {
      mockReadFile.mockResolvedValueOnce('not json{{{');

      const creds = await loadCredentials();

      expect(creds).toBeNull();
    });

    it('should return null on read error', async () => {
      mockReadFile.mockRejectedValueOnce(new Error('permission denied'));

      const creds = await loadCredentials();

      expect(creds).toBeNull();
    });
  });

  describe('clearCredentials', () => {
    it('should clear username in settings.json and empty Password.dat', async () => {
      mockReadFile.mockResolvedValueOnce(
        JSON.stringify({ Username: 'alice', Other: 'keep' }),
      );

      await clearCredentials();

      // settings.json should have Username cleared but Other preserved
      const settingsCall = mockWriteFile.mock.calls.find((c: unknown[]) =>
        (c[0] as string).includes('settings.json'),
      );
      const writtenSettings = JSON.parse(settingsCall![1] as string);
      expect(writtenSettings.Username).toBe('');
      expect(writtenSettings.Other).toBe('keep');

      // Password.dat should be empty
      const passwordCall = mockWriteFile.mock.calls.find((c: unknown[]) =>
        (c[0] as string).includes('Password.dat'),
      );
      const written = passwordCall![1] as Buffer;
      expect(written.length).toBe(0);
    });
  });
});
