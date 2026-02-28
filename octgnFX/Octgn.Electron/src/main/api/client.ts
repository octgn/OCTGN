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
      // Generate a stable device ID for this installation (persisted across sessions)
      const deviceId = await this.getDeviceId();

      const response = await fetch(`${API_BASE}/api/sessions`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Username: username, Password: password, DeviceId: deviceId }),
      });

      if (!response.ok) {
        const body = await response.text().catch(() => '');
        return { success: false, error: `Server error (${response.status}): ${body || response.statusText}` };
      }

      const data = await response.json();

      // Response: { Result: { Type: 0|1|2|3|4|5|6, Username: '...' }, SessionKey: '...', UserId: '...' }
      // Type enum: 0=UnknownError, 1=Ok, 2=EmailUnverified, 3=UnknownUsername,
      //            4=PasswordWrong, 5=NotSubscribed, 6=NoEmailAssociated
      const resultType = data.Result?.Type ?? data.result?.type ?? -1;
      const returnedUsername: string = data.Result?.Username ?? data.result?.username ?? username;

      if (resultType === 1 || resultType === 'Ok') {
        this.session = {
          userId: data.UserId ?? data.userId,
          sessionId: data.SessionKey ?? data.sessionKey,
          deviceId,
          username: returnedUsername,
        };

        return {
          success: true,
          user: {
            id: this.session.userId,
            username: returnedUsername,
            isSubscriber: false,
          },
          session: {
            userId: this.session.userId,
            sessionId: this.session.sessionId,
            deviceId,
          },
        };
      }

      // Map server error types to user-friendly messages
      const errorMessages: Record<string | number, string> = {
        0: 'Unknown server error',
        2: 'Please verify your email before logging in',
        3: 'Unknown username',
        4: 'Incorrect password',
        5: 'Account is not subscribed',
        6: 'No email associated with your account — visit octgn.net to add one',
        UnknownUsername: 'Unknown username',
        PasswordWrong: 'Incorrect password',
        EmailUnverified: 'Please verify your email before logging in',
      };
      const errorMsg = errorMessages[resultType] ?? `Login failed (code ${resultType})`;
      return { success: false, error: errorMsg };
    } catch (err) {
      return { success: false, error: `Connection error: ${(err as Error).message}` };
    }
  }

  async logout(): Promise<void> {
    if (this.session) {
      try {
        // POST /api/users/{userId}/devices/{deviceId}/session to clear
        await fetch(
          `${API_BASE}/api/users/${this.session.userId}/devices/${this.session.deviceId}/session`,
          {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ SessionKey: this.session.sessionId }),
          },
        );
      } catch {
        // Ignore logout errors
      }
      this.session = null;
    }
  }

  getSession(): ApiSessionData | null {
    return this.session;
  }

  getSessionAsLoginResult(): LoginResult {
    if (!this.session) {
      return { success: false };
    }
    return {
      success: true,
      user: {
        id: this.session.userId,
        username: this.session.username,
        isSubscriber: false,
      },
      session: {
        userId: this.session.userId,
        sessionId: this.session.sessionId,
        deviceId: this.session.deviceId,
      },
    };
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
