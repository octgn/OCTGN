/**
 * OCTGN API Service
 * 
 * Connects to the real OCTGN.net backend services
 * In Electron, routes through main process to bypass CORS
 * API endpoints from Octgn.Site.Api.ApiClient
 */

import { useState, useEffect, useCallback } from 'react';

// API Configuration
export const API_BASE = 'https://www.octgn.net';
export const CHAT_HOST = 'chat.octgn.net';
export const GAME_SERVER_PORT = 8888;

// Types
export interface User {
  id: string;
  username: string;
  email?: string;
  iconUrl?: string;
  isSubscribed?: boolean;
}

export interface Session {
  userId: string;
  deviceId: string;
  sessionKey: string;
  username: string;
}

export interface HostedGame {
  id: string;
  name: string;
  hostUser: {
    id: string;
    username: string;
    iconUrl?: string;
  };
  gameName: string;
  gameId: string;
  gameVersion: string;
  octgnVersion: string;
  hasPassword: boolean;
  spectators: boolean;
  gameIconUrl?: string;
  hostUserIconUrl?: string;
  hostAddress: string;
  port?: number;
  status: 'Staging' | 'Playing' | 'Hosting';
  source: 'Online' | 'LAN';
  dateCreated: string;
  dateStarted?: string;
  playerCount?: number;
  maxPlayers?: number;
}

export interface LoginResult {
  type: 'Ok' | 'UnknownError' | 'EmailUnverified' | 'UnknownUsername' | 'PasswordWrong' | 'NotSubscribed' | 'NoEmailAssociated';
  username?: string;
  sessionKey?: string;
  userId?: string;
}

export interface GameDefinition {
  id: string;
  name: string;
  uuid: string;
  version: string;
  description?: string;
  iconUrl?: string;
  authors?: string[];
  tags?: string[];
  cardCount?: number;
  cardSize?: { width: number; height: number };
  installed: boolean;
  installPath?: string;
}

/**
 * OCTGN API Client
 * Uses Electron IPC when available to bypass CORS
 */
export class OctgnApiClient {
  private baseUrl: string;
  private session: Session | null = null;

  constructor(baseUrl: string = API_BASE) {
    this.baseUrl = baseUrl;
  }

  /**
   * Check if running in Electron
   */
  private isElectron(): boolean {
    return typeof window !== 'undefined' && !!window.electronAPI;
  }

  /**
   * Login with username and password (legacy endpoint)
   */
  async loginLegacy(username: string, password: string): Promise<LoginResult> {
    try {
      // Use Electron IPC if available (bypasses CORS)
      if (this.isElectron() && window.electronAPI?.octgnLogin) {
        const result = await window.electronAPI.octgnLogin(username, password);
        if (result.success && result.data) {
          const data = result.data;
          if (data.Type === 'Ok' || data.type === 'Ok') {
            return {
              type: 'Ok',
              username: data.Username || data.username,
            };
          }
          return {
            type: data.Type || data.type || 'UnknownError',
            username: data.Username || data.username,
          };
        }
        return { type: 'UnknownError' };
      }

      // Fallback to fetch (for browser mode, will have CORS issues)
      const response = await fetch(
        `${this.baseUrl}/api/user/loginandusername?username2=${encodeURIComponent(username)}&password2=${encodeURIComponent(password)}`
      );

      if (!response.ok) {
        return { type: 'UnknownError' };
      }

      const data = await response.json();
      
      if (data.Type === 'Ok' || data.type === 'Ok') {
        return {
          type: 'Ok',
          username: data.Username || data.username,
        };
      }

      return {
        type: data.Type || data.type || 'UnknownError',
        username: data.Username || data.username,
      };
    } catch (error) {
      console.error('Login error:', error);
      return { type: 'UnknownError' };
    }
  }

  /**
   * Create a modern session (preferred)
   */
  async createSession(username: string, password: string, deviceId: string): Promise<LoginResult & { sessionKey?: string; userId?: string }> {
    try {
      // Use Electron IPC if available (bypasses CORS)
      if (this.isElectron() && window.electronAPI?.octgnCreateSession) {
        const result = await window.electronAPI.octgnCreateSession(username, password, deviceId);
        
        if (result.success && result.data) {
          const data = result.data;
          const loginResult = data.Result || data.result;
          
          if (loginResult?.Type === 'Ok' || loginResult?.type === 'Ok') {
            const session: Session = {
              userId: data.UserId || data.userId,
              deviceId,
              sessionKey: data.SessionKey || data.sessionKey,
              username: loginResult.Username || loginResult.username || username,
            };
            
            this.session = session;
            
            return {
              type: 'Ok',
              username: session.username,
              sessionKey: session.sessionKey,
              userId: session.userId,
            };
          }

          return {
            type: loginResult?.Type || loginResult?.type || 'UnknownError',
          };
        }
        
        return { type: 'UnknownError' };
      }

      // Fallback to fetch (for browser mode)
      const response = await fetch(`${this.baseUrl}/api/sessions`, {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          Username: username,
          Password: password,
          DeviceId: deviceId,
        }),
      });

      if (!response.ok) {
        if (response.status === 401 || response.status === 403) {
          return { type: 'PasswordWrong' };
        }
        return { type: 'UnknownError' };
      }

      const data = await response.json();
      const resResult = data.Result || data.result;
      
      if (resResult?.Type === 'Ok' || resResult?.type === 'Ok') {
        const session: Session = {
          userId: data.UserId || data.userId,
          deviceId,
          sessionKey: data.SessionKey || data.sessionKey,
          username: resResult.Username || resResult.username || username,
        };
        
        this.session = session;
        
        return {
          type: 'Ok',
          username: session.username,
          sessionKey: session.sessionKey,
          userId: session.userId,
        };
      }

      return {
        type: resResult?.Type || resResult?.type || 'UnknownError',
      };
    } catch (error) {
      console.error('Create session error:', error);
      return { type: 'UnknownError' };
    }
  }

  /**
   * Validate an existing session
   */
  async validateSession(userId: string, deviceId: string, sessionKey: string): Promise<boolean> {
    try {
      // Use Electron IPC if available
      if (this.isElectron() && window.electronAPI?.octgnValidateSession) {
        const result = await window.electronAPI.octgnValidateSession(userId, deviceId, sessionKey);
        return result.success && result.data === 'ok';
      }

      // Fallback to fetch
      const url = `${this.baseUrl}/api/users/${encodeURIComponent(userId)}/devices/${encodeURIComponent(deviceId)}/session/validate`;
      
      const response = await fetch(url, {
        method: 'PUT',
        headers: { 
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(sessionKey),
      });

      if (response.status === 404) return false;
      if (!response.ok) return false;

      const result = await response.json();
      return result === 'ok' || result === true;
    } catch (error) {
      console.error('Validate session error:', error);
      return false;
    }
  }

  /**
   * Clear/logout session
   */
  async clearSession(userId: string, deviceId: string, sessionKey: string): Promise<void> {
    try {
      // Use Electron IPC if available
      if (this.isElectron() && window.electronAPI?.octgnClearSession) {
        await window.electronAPI.octgnClearSession(userId, deviceId, sessionKey);
        return;
      }

      // Fallback to fetch
      const url = `${this.baseUrl}/api/users/${encodeURIComponent(userId)}/devices/${encodeURIComponent(deviceId)}/session`;
      
      await fetch(url, {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(sessionKey),
      });
    } catch (error) {
      console.error('Clear session error:', error);
    }
  }

  /**
   * Get list of hosted games
   */
  async getHostedGames(): Promise<HostedGame[]> {
    try {
      // Use Electron IPC if available
      if (this.isElectron() && window.electronAPI?.octgnGetGames) {
        const result = await window.electronAPI.octgnGetGames();
        if (result.success && result.data) {
          return result.data.map((g: any) => this.mapHostedGame(g));
        }
        return [];
      }

      // Fallback to fetch
      const response = await fetch(`${this.baseUrl}/api/game`, {
        headers: { 'Accept': 'application/json' }
      });

      if (!response.ok) {
        throw new Error(`Failed to get games: ${response.status}`);
      }

      const games = await response.json();
      return games.map((g: any) => this.mapHostedGame(g));
    } catch (error) {
      console.error('Get hosted games error:', error);
      return [];
    }
  }

  /**
   * Get stats (users online, sub percentage)
   */
  async getStats(): Promise<{ usersOnline: number; subPercent: number }> {
    try {
      // Use Electron IPC if available
      if (this.isElectron() && window.electronAPI?.octgnGetStats) {
        const result = await window.electronAPI.octgnGetStats();
        if (result.success) {
          const value = typeof result.data === 'string' 
            ? parseInt(result.data.trim(), 10) 
            : typeof result.data === 'number' ? result.data : 0;
          return { usersOnline: 0, subPercent: isNaN(value) ? 0 : value };
        }
        return { usersOnline: 0, subPercent: 0 };
      }

      // Fallback to fetch
      const response = await fetch(`${this.baseUrl}/api/stats/UsersOnlineNow?type=SubPercent`);
      const text = await response.text();
      const value = parseInt(text.trim(), 10);
      return { usersOnline: 0, subPercent: isNaN(value) ? 0 : value };
    } catch (error) {
      console.error('Get stats error:', error);
      return { usersOnline: 0, subPercent: 0 };
    }
  }

  /**
   * Get current session
   */
  getSession(): Session | null {
    return this.session;
  }

  /**
   * Set session
   */
  setSession(session: Session | null) {
    this.session = session;
  }

  /**
   * Map API response to HostedGame
   */
  private mapHostedGame(g: any): HostedGame {
    const hostAddress = g.HostAddress || g.hostAddress || '';
    const [host, portStr] = hostAddress.split(':');
    
    return {
      id: g.Id || g.id,
      name: g.Name || g.name || 'Unknown Game',
      hostUser: {
        id: g.HostUser?.Id || g.hostUser?.id || '',
        username: g.HostUser?.Username || g.hostUser?.username || 'Unknown',
        iconUrl: g.HostUserIconUrl || g.hostUserIconUrl,
      },
      gameName: g.GameName || g.gameName || 'Unknown',
      gameId: g.GameId || g.gameId || '',
      gameVersion: g.GameVersion || g.gameVersion || '0.0.0',
      octgnVersion: g.OctgnVersion || g.octgnVersion || '3.0.0',
      hasPassword: g.HasPassword ?? g.hasPassword ?? false,
      spectators: g.Spectators ?? g.spectators ?? true,
      gameIconUrl: g.GameIconUrl || g.gameIconUrl,
      hostUserIconUrl: g.HostUserIconUrl || g.hostUserIconUrl,
      hostAddress: host,
      port: portStr ? parseInt(portStr, 10) : GAME_SERVER_PORT,
      status: g.Status || g.status || 'Staging',
      source: g.Source || g.source || 'Online',
      dateCreated: g.DateCreated || g.dateCreated,
      dateStarted: g.DateStarted || g.dateStarted,
    };
  }
}

// Singleton instance
export const octgnApi = new OctgnApiClient();

/**
 * React hook for OCTGN authentication
 */
export function useOctgnAuth() {
  const [session, setSession] = useState<Session | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Try to restore session from storage
  useEffect(() => {
    const stored = localStorage.getItem('octgn_session');
    if (stored) {
      try {
        const parsed = JSON.parse(stored);
        setSession(parsed);
        octgnApi.setSession(parsed);
        
        // Validate session
        octgnApi.validateSession(parsed.userId, parsed.deviceId, parsed.sessionKey)
          .then((valid) => {
            if (!valid) {
              setSession(null);
              localStorage.removeItem('octgn_session');
            }
          });
      } catch (e) {
        localStorage.removeItem('octgn_session');
      }
    }
  }, []);

  const login = useCallback(async (username: string, password: string) => {
    setLoading(true);
    setError(null);

    try {
      const deviceId = getDeviceId();
      const result = await octgnApi.createSession(username, password, deviceId);

      if (result.type === 'Ok' && result.sessionKey && result.userId) {
        const newSession: Session = {
          userId: result.userId,
          deviceId,
          sessionKey: result.sessionKey,
          username: result.username!,
        };

        setSession(newSession);
        localStorage.setItem('octgn_session', JSON.stringify(newSession));
        return true;
      } else {
        setError(getLoginErrorMessage(result.type));
        return false;
      }
    } catch (e: any) {
      setError(e.message || 'Unknown error');
      return false;
    } finally {
      setLoading(false);
    }
  }, []);

  const logout = useCallback(async () => {
    if (session) {
      await octgnApi.clearSession(session.userId, session.deviceId, session.sessionKey);
    }
    setSession(null);
    octgnApi.setSession(null);
    localStorage.removeItem('octgn_session');
  }, [session]);

  return {
    session,
    user: session ? { id: session.userId, username: session.username } : null,
    loading,
    error,
    login,
    logout,
    isAuthenticated: !!session,
    clearError: () => setError(null),
  };
}

/**
 * React hook for hosted games
 */
export function useHostedGames() {
  const [games, setGames] = useState<HostedGame[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const result = await octgnApi.getHostedGames();
      setGames(result);
    } catch (e: any) {
      setError(e.message || 'Failed to load games');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    refresh();
    const interval = setInterval(refresh, 30000); // Refresh every 30s
    return () => clearInterval(interval);
  }, [refresh]);

  return { games, loading, error, refresh };
}

/**
 * React hook for stats
 */
export function useOctgnStats() {
  const [stats, setStats] = useState({ usersOnline: 0, subPercent: 0 });

  useEffect(() => {
    octgnApi.getStats().then(setStats);
    const interval = setInterval(() => {
      octgnApi.getStats().then(setStats);
    }, 60000);
    return () => clearInterval(interval);
  }, []);

  return stats;
}

// Helper functions
function getDeviceId(): string {
  let deviceId = localStorage.getItem('octgn_device_id');
  if (!deviceId) {
    deviceId = crypto.randomUUID();
    localStorage.setItem('octgn_device_id', deviceId);
  }
  return deviceId;
}

function getLoginErrorMessage(type: string): string {
  switch (type) {
    case 'UnknownUsername':
      return 'Username not found';
    case 'PasswordWrong':
      return 'Incorrect password';
    case 'EmailUnverified':
      return 'Please verify your email address';
    case 'NotSubscribed':
      return 'Subscription required for this feature';
    case 'NoEmailAssociated':
      return 'No email associated with this account';
    default:
      return 'Login failed. Please try again.';
  }
}

export default octgnApi;
