import type { LoginResult, HostedGame, Deck } from '../../shared/types';

const API_BASE = 'https://www.octgn.net';

interface ApiSessionData {
  userId: string;
  sessionId: string;
  deviceId: string;
  username: string;
}

export class OctgnApiClient {
  private session: ApiSessionData | null = null;

  async login(username: string, password: string): Promise<LoginResult> {
    try {
      // Generate a device ID for this installation
      const deviceId = await this.getDeviceId();

      const response = await fetch(`${API_BASE}/api/user/createsession`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password, deviceId }),
      });

      if (!response.ok) {
        return { success: false, error: 'Invalid username or password' };
      }

      const data = await response.json();

      if (data.result === 'Ok') {
        this.session = {
          userId: data.userId,
          sessionId: data.sessionKey,
          deviceId,
          username,
        };

        return {
          success: true,
          user: {
            id: data.userId,
            username,
            isSubscriber: data.isSubscriber || false,
          },
          session: {
            userId: data.userId,
            sessionId: data.sessionKey,
            deviceId,
          },
        };
      }

      return { success: false, error: data.message || 'Login failed' };
    } catch (err) {
      return { success: false, error: `Connection error: ${(err as Error).message}` };
    }
  }

  async logout(): Promise<void> {
    if (this.session) {
      try {
        await fetch(`${API_BASE}/api/user/clearsession`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            userId: this.session.userId,
            deviceId: this.session.deviceId,
            sessionId: this.session.sessionId,
          }),
        });
      } catch {
        // Ignore logout errors
      }
      this.session = null;
    }
  }

  getSession(): ApiSessionData | null {
    return this.session;
  }

  async getHostedGames(): Promise<HostedGame[]> {
    try {
      const response = await fetch(`${API_BASE}/api/game/list`);
      if (!response.ok) return [];
      const data = await response.json();
      return data.map(this.mapHostedGame);
    } catch {
      return [];
    }
  }

  async hostGame(options: Record<string, unknown>): Promise<{ success: boolean; error?: string }> {
    // TODO: Implement game hosting via API
    console.log('Host game:', options);
    return { success: false, error: 'Not yet implemented' };
  }

  async joinGame(gameId: string, _password?: string): Promise<{ success: boolean; error?: string }> {
    // TODO: Implement game joining - connect to game server TCP
    console.log('Join game:', gameId);
    return { success: false, error: 'Not yet implemented' };
  }

  async leaveGame(): Promise<void> {
    // TODO: Disconnect from game server
  }

  async sendGameAction(action: Record<string, unknown>): Promise<void> {
    // TODO: Send action via game protocol
    console.log('Game action:', action);
  }

  async sendChatMessage(message: string): Promise<void> {
    // TODO: Send via game protocol
    console.log('Chat:', message);
  }

  async loadDeck(deck: Deck): Promise<{ success: boolean; error?: string }> {
    // TODO: Send deck load via game protocol
    console.log('Load deck:', deck);
    return { success: false, error: 'Not yet implemented' };
  }

  private async getDeviceId(): Promise<string> {
    // Generate a stable device ID based on machine info
    const os = await import('node:os');
    const crypto = await import('node:crypto');
    const raw = `${os.hostname()}-${os.platform()}-${os.arch()}`;
    return crypto.createHash('sha256').update(raw).digest('hex').substring(0, 32);
  }

  private mapHostedGame(data: Record<string, unknown>): HostedGame {
    const hostAddress = (data.HostAddress as string) || '';
    const parts = hostAddress.split(':');
    return {
      id: (data.Id as string) || '',
      name: (data.Name as string) || '',
      hostUser: {
        id: (data.HostUserId as string) || '',
        username: (data.HostUserName as string) || '',
        isSubscriber: false,
      },
      gameId: (data.GameId as string) || '',
      gameName: (data.GameName as string) || '',
      gameVersion: (data.GameVersion as string) || '',
      hasPassword: (data.HasPassword as boolean) || false,
      spectators: (data.Spectators as boolean) || false,
      hostAddress: parts[0] || '',
      port: parseInt(parts[1] || '0', 10),
      status: (data.Status as number) || 0,
      dateCreated: (data.DateCreated as string) || '',
      playerCount: (data.PlayerCount as number) || 0,
      maxPlayers: (data.MaxPlayers as number) || 0,
    };
  }
}
