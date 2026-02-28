import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { OctgnApiClient } from '@main/api/client';

// ---------------------------------------------------------------------------
// Mock node:os and node:crypto for deterministic device ID
// ---------------------------------------------------------------------------
vi.mock('node:os', () => ({
  hostname: () => 'test-host',
  platform: () => 'win32',
  arch: () => 'x64',
}));

vi.mock('node:crypto', () => ({
  createHash: () => ({
    update: () => ({
      digest: () => 'abcdef1234567890abcdef1234567890full',
      // substring(0,32) called by getDeviceId
    }),
  }),
}));

// The actual getDeviceId calls crypto.createHash('sha256').update(raw).digest('hex').substring(0,32)
// With our mock, digest returns a fixed string; substring(0,32) = 'abcdef1234567890abcdef1234567890'
const EXPECTED_DEVICE_ID = 'abcdef1234567890abcdef12345678';

// ---------------------------------------------------------------------------
// fetch mock
// ---------------------------------------------------------------------------
const mockFetch = vi.fn<Parameters<typeof fetch>, ReturnType<typeof fetch>>();
vi.stubGlobal('fetch', mockFetch);

function jsonResponse(body: unknown, status = 200): Response {
  return {
    ok: status >= 200 && status < 300,
    status,
    statusText: status === 500 ? 'Internal Server Error' : 'OK',
    json: async () => body,
    text: async () => JSON.stringify(body),
    headers: new Headers(),
  } as Response;
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('OctgnApiClient', () => {
  let client: OctgnApiClient;

  beforeEach(() => {
    vi.clearAllMocks();
    client = new OctgnApiClient();
  });

  // Recalculate expected device ID from the mock values
  // 'test-host-win32-x64' → sha256 → hex → substring(0,32)
  // Our mock returns 'abcdef1234567890abcdef1234567890full'.substring(0,32)

  describe('login', () => {
    it('should POST to /api/sessions with PascalCase body', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse({
          Result: { Type: 1, Username: 'alice' },
          SessionKey: 'sess-123',
          UserId: 'user-456',
        }),
      );

      await client.login('alice', 'password123');

      expect(mockFetch).toHaveBeenCalledTimes(1);
      const [url, opts] = mockFetch.mock.calls[0];
      expect(url).toBe('https://www.octgn.net/api/sessions');
      expect(opts?.method).toBe('POST');
      expect(opts?.headers).toEqual({ 'Content-Type': 'application/json' });

      const body = JSON.parse(opts?.body as string);
      expect(body).toEqual({
        Username: 'alice',
        Password: 'password123',
        DeviceId: expect.any(String),
      });
      // Verify PascalCase keys
      expect(Object.keys(body)).toEqual(['Username', 'Password', 'DeviceId']);
    });

    it('should return success with user and session when Result.Type === 1', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse({
          Result: { Type: 1, Username: 'alice' },
          SessionKey: 'sess-123',
          UserId: 'user-456',
        }),
      );

      const result = await client.login('alice', 'pw');

      expect(result.success).toBe(true);
      expect(result.user).toEqual({
        id: 'user-456',
        username: 'alice',
        isSubscriber: false,
      });
      expect(result.session).toEqual({
        userId: 'user-456',
        sessionId: 'sess-123',
        deviceId: expect.any(String),
      });
    });

    it('should return "Unknown username" for Result.Type === 3', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse({ Result: { Type: 3, Username: '' }, SessionKey: '', UserId: '' }),
      );

      const result = await client.login('nobody', 'pw');

      expect(result.success).toBe(false);
      expect(result.error).toBe('Unknown username');
    });

    it('should return "Incorrect password" for Result.Type === 4', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse({ Result: { Type: 4, Username: 'alice' }, SessionKey: '', UserId: '' }),
      );

      const result = await client.login('alice', 'wrong');

      expect(result.success).toBe(false);
      expect(result.error).toBe('Incorrect password');
    });

    it('should return email verification error for Result.Type === 2', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse({ Result: { Type: 2, Username: 'bob' }, SessionKey: '', UserId: '' }),
      );

      const result = await client.login('bob', 'pw');

      expect(result.success).toBe(false);
      expect(result.error).toBe('Please verify your email before logging in');
    });

    it('should return not subscribed error for Result.Type === 5', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse({ Result: { Type: 5, Username: 'bob' }, SessionKey: '', UserId: '' }),
      );

      const result = await client.login('bob', 'pw');

      expect(result.success).toBe(false);
      expect(result.error).toBe('Account is not subscribed');
    });

    it('should return no-email error for Result.Type === 6', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse({ Result: { Type: 6, Username: 'bob' }, SessionKey: '', UserId: '' }),
      );

      const result = await client.login('bob', 'pw');

      expect(result.success).toBe(false);
      expect(result.error).toContain('No email associated');
    });

    it('should return unknown error for Result.Type === 0', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse({ Result: { Type: 0, Username: '' }, SessionKey: '', UserId: '' }),
      );

      const result = await client.login('x', 'y');

      expect(result.success).toBe(false);
      expect(result.error).toBe('Unknown server error');
    });

    it('should return server error for HTTP 500', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse('Something broke', 500),
      );

      const result = await client.login('alice', 'pw');

      expect(result.success).toBe(false);
      expect(result.error).toMatch(/^Server error \(500\):/);
    });

    it('should return connection error on network failure', async () => {
      mockFetch.mockRejectedValue(new Error('Failed to fetch'));

      const result = await client.login('alice', 'pw');

      expect(result.success).toBe(false);
      expect(result.error).toBe('Connection error: Failed to fetch');
    });

    it('should return a generic message for unknown result types', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse({ Result: { Type: 99, Username: '' }, SessionKey: '', UserId: '' }),
      );

      const result = await client.login('x', 'y');

      expect(result.success).toBe(false);
      expect(result.error).toBe('Login failed (code 99)');
    });
  });

  describe('logout', () => {
    it('should POST to the session endpoint and clear session', async () => {
      // First login
      mockFetch.mockResolvedValue(
        jsonResponse({
          Result: { Type: 1, Username: 'alice' },
          SessionKey: 'sess-123',
          UserId: 'user-456',
        }),
      );
      await client.login('alice', 'pw');
      expect(client.getSession()).not.toBeNull();

      // Now logout
      mockFetch.mockResolvedValue(jsonResponse({}));
      await client.logout();

      expect(client.getSession()).toBeNull();

      // Verify the logout fetch call
      const logoutCall = mockFetch.mock.calls[1];
      const [url, opts] = logoutCall;
      expect(url).toContain('/api/users/user-456/devices/');
      expect(url).toContain('/session');
      expect(opts?.method).toBe('POST');
      const body = JSON.parse(opts?.body as string);
      expect(body).toEqual({ SessionKey: 'sess-123' });
    });

    it('should be a no-op when not logged in', async () => {
      await client.logout();
      expect(mockFetch).not.toHaveBeenCalled();
      expect(client.getSession()).toBeNull();
    });

    it('should clear session even if logout request fails', async () => {
      // Login first
      mockFetch.mockResolvedValue(
        jsonResponse({
          Result: { Type: 1, Username: 'alice' },
          SessionKey: 'sess-123',
          UserId: 'user-456',
        }),
      );
      await client.login('alice', 'pw');

      // Logout with network error
      mockFetch.mockRejectedValue(new Error('offline'));
      await client.logout();

      expect(client.getSession()).toBeNull();
    });
  });

  describe('getSessionAsLoginResult', () => {
    it('should return { success: false } when no session exists', () => {
      const result = client.getSessionAsLoginResult();
      expect(result.success).toBe(false);
      expect(result.user).toBeUndefined();
      expect(result.session).toBeUndefined();
    });

    it('should return a LoginResult with user and session after login', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse({
          Result: { Type: 1, Username: 'alice' },
          SessionKey: 'sess-123',
          UserId: 'user-456',
        }),
      );
      await client.login('alice', 'pw');

      const result = client.getSessionAsLoginResult();
      expect(result.success).toBe(true);
      expect(result.user).toEqual({
        id: 'user-456',
        username: 'alice',
        isSubscriber: false,
      });
      expect(result.session).toEqual({
        userId: 'user-456',
        sessionId: 'sess-123',
        deviceId: expect.any(String),
      });
    });

    it('should return { success: false } after logout', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse({
          Result: { Type: 1, Username: 'alice' },
          SessionKey: 'sess-123',
          UserId: 'user-456',
        }),
      );
      await client.login('alice', 'pw');

      mockFetch.mockResolvedValue(jsonResponse({}));
      await client.logout();

      const result = client.getSessionAsLoginResult();
      expect(result.success).toBe(false);
    });
  });

  describe('getHostedGames', () => {
    it('should parse nested HostUser object from API response', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse([
          {
            Id: 'game-1',
            Name: 'Friday Night Chess',
            HostUser: { Id: 'user-99', DisplayName: 'ChessMaster42' },
            GameId: 'chess-guid',
            GameName: 'Chess',
            GameVersion: '1.0.0',
            HasPassword: false,
            Spectators: true,
            HostAddress: '192.168.1.10:5000',
            Status: 1,
            DateCreated: '2026-02-28T12:00:00Z',
            PlayerCount: 1,
            MaxPlayers: 2,
            HostUserIconUrl: 'https://octgn.net/icons/user-99.png',
            GameIconUrl: 'https://octgn.net/icons/chess.png',
          },
        ]),
      );

      const games = await client.getHostedGames();

      expect(games).toHaveLength(1);
      expect(games[0].hostUser.id).toBe('user-99');
      expect(games[0].hostUser.username).toBe('ChessMaster42');
    });

    it('should capture HostUserIconUrl as hostUser.iconUrl', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse([
          {
            Id: 'game-2',
            Name: 'Test Game',
            HostUser: { Id: 'user-1', DisplayName: 'Alice' },
            GameId: 'g1',
            GameName: 'TestGame',
            GameVersion: '1.0',
            HasPassword: false,
            Spectators: false,
            HostAddress: '10.0.0.1:9000',
            Status: 2,
            DateCreated: '2026-02-28T10:00:00Z',
            PlayerCount: 2,
            MaxPlayers: 4,
            HostUserIconUrl: 'https://octgn.net/icons/alice.png',
            GameIconUrl: '',
          },
        ]),
      );

      const games = await client.getHostedGames();

      expect(games[0].hostUser.iconUrl).toBe('https://octgn.net/icons/alice.png');
    });

    it('should capture GameIconUrl as gameIconUrl', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse([
          {
            Id: 'game-3',
            Name: 'MTG Draft',
            HostUser: { Id: 'user-5', DisplayName: 'Bob' },
            GameId: 'mtg-guid',
            GameName: 'Magic: The Gathering',
            GameVersion: '2.0',
            HasPassword: true,
            Spectators: true,
            HostAddress: '10.0.0.2:8080',
            Status: 1,
            DateCreated: '2026-02-28T09:00:00Z',
            PlayerCount: 3,
            MaxPlayers: 8,
            HostUserIconUrl: '',
            GameIconUrl: 'https://octgn.net/icons/mtg.png',
          },
        ]),
      );

      const games = await client.getHostedGames();

      expect(games[0].gameIconUrl).toBe('https://octgn.net/icons/mtg.png');
    });

    it('should handle missing HostUser gracefully', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse([
          {
            Id: 'game-4',
            Name: 'No Host Info',
            GameId: 'g1',
            GameName: 'SomeGame',
            GameVersion: '1.0',
            HasPassword: false,
            Spectators: false,
            HostAddress: '10.0.0.1:9000',
            Status: 0,
            DateCreated: '2026-02-28T08:00:00Z',
            PlayerCount: 0,
            MaxPlayers: 2,
          },
        ]),
      );

      const games = await client.getHostedGames();

      expect(games).toHaveLength(1);
      expect(games[0].hostUser.id).toBe('');
      expect(games[0].hostUser.username).toBe('');
      expect(games[0].hostUser.iconUrl).toBeUndefined();
    });

    it('should return empty array on HTTP error', async () => {
      mockFetch.mockResolvedValue(jsonResponse('Server Error', 500));

      const games = await client.getHostedGames();

      expect(games).toEqual([]);
    });

    it('should return empty array on network failure', async () => {
      mockFetch.mockRejectedValue(new Error('Network down'));

      const games = await client.getHostedGames();

      expect(games).toEqual([]);
    });
  });

  describe('getDeviceId', () => {
    it('should produce a deterministic device ID', async () => {
      mockFetch.mockResolvedValue(
        jsonResponse({
          Result: { Type: 1, Username: 'a' },
          SessionKey: 's',
          UserId: 'u',
        }),
      );

      await client.login('a', 'b');
      await client.login('a', 'b');

      // Both calls should use the same DeviceId
      const body1 = JSON.parse(mockFetch.mock.calls[0][1]?.body as string);
      const body2 = JSON.parse(mockFetch.mock.calls[1][1]?.body as string);
      expect(body1.DeviceId).toBe(body2.DeviceId);
      expect(body1.DeviceId).toHaveLength(32);
    });
  });
});
