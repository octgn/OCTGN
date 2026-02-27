import { ipcMain, IpcMainInvokeEvent } from 'electron';
import WebSocket, { WebSocketServer } from 'ws';
import { GameServer } from './GameServer';
import { GameClient } from './GameClient';

interface BridgeConnection {
  ws: WebSocket;
  client: GameClient | null;
}

export class WebSocketBridge {
  private wss: WebSocketServer | null = null;
  private gameServer: GameServer | null = null;
  private connections: Set<BridgeConnection> = new Set();
  private port: number;

  constructor(port: number = 8889) {
    this.port = port;
    this.setupIPCHandlers();
  }

  start(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.wss = new WebSocketServer({ port: this.port });

      this.wss.on('connection', (ws: WebSocket) => {
        const conn: BridgeConnection = { ws, client: null };
        this.connections.add(conn);

        ws.on('message', (data: Buffer) => {
          this.handleMessage(conn, data);
        });

        ws.on('close', () => {
          if (conn.client) {
            conn.client.disconnect();
          }
          this.connections.delete(conn);
        });

        ws.on('error', (error) => {
          console.error('WebSocket error:', error);
        });
      });

      this.wss.on('error', (error) => {
        reject(error);
      });

      this.wss.on('listening', () => {
        console.log(`WebSocket bridge listening on port ${this.port}`);
        resolve();
      });
    });
  }

  stop(): void {
    for (const conn of this.connections) {
      conn.ws.close();
    }
    this.connections.clear();

    if (this.wss) {
      this.wss.close();
      this.wss = null;
    }
  }

  private handleMessage(conn: BridgeConnection, data: Buffer): void {
    try {
      const message = JSON.parse(data.toString());
      this.processCommand(conn, message);
    } catch (error) {
      console.error('Failed to parse bridge message:', error);
    }
  }

  private async processCommand(conn: BridgeConnection, message: any): Promise<void> {
    const { type, payload, id } = message;

    const sendResponse = (response: any) => {
      conn.ws.send(JSON.stringify({ id, ...response }));
    };

    switch (type) {
      case 'connect': {
        const { host, port, nick, spectator } = payload;
        conn.client = new GameClient({ host, port, nick, spectator: spectator || false });

        conn.client.on('connected', () => {
          sendResponse({ success: true });
        });

        conn.client.on('disconnected', (data) => {
          conn.ws.send(JSON.stringify({ type: 'disconnected', data }));
        });

        conn.client.on('error', (error) => {
          sendResponse({ success: false, error: error.message });
        });

        conn.client.on('chat', (data) => {
          conn.ws.send(JSON.stringify({ type: 'chat', data }));
        });

        conn.client.on('player-joined', (data) => {
          conn.ws.send(JSON.stringify({ type: 'player-joined', data }));
        });

        conn.client.on('player-left', (data) => {
          conn.ws.send(JSON.stringify({ type: 'player-left', data }));
        });

        conn.client.on('card-created', (data) => {
          conn.ws.send(JSON.stringify({ type: 'card-created', data }));
        });

        conn.client.on('cards-moved', (data) => {
          conn.ws.send(JSON.stringify({ type: 'cards-moved', data }));
        });

        conn.client.on('card-turned', (data) => {
          conn.ws.send(JSON.stringify({ type: 'card-turned', data }));
        });

        conn.client.on('turn-changed', (data) => {
          conn.ws.send(JSON.stringify({ type: 'turn-changed', data }));
        });

        try {
          await conn.client.connect();
        } catch (error: any) {
          sendResponse({ success: false, error: error.message });
        }
        break;
      }

      case 'disconnect': {
        if (conn.client) {
          conn.client.disconnect();
          conn.client = null;
        }
        sendResponse({ success: true });
        break;
      }

      case 'chat': {
        if (conn.client) {
          conn.client.sendChat(payload.text);
          sendResponse({ success: true });
        } else {
          sendResponse({ success: false, error: 'Not connected' });
        }
        break;
      }

      case 'moveCards': {
        if (conn.client) {
          conn.client.moveCards(payload.cardIds, payload.toGroup, payload.toIndex, payload.faceUp);
          sendResponse({ success: true });
        } else {
          sendResponse({ success: false, error: 'Not connected' });
        }
        break;
      }

      case 'turnCard': {
        if (conn.client) {
          conn.client.turnCard(payload.cardId, payload.faceUp);
          sendResponse({ success: true });
        } else {
          sendResponse({ success: false, error: 'Not connected' });
        }
        break;
      }

      case 'rotateCard': {
        if (conn.client) {
          conn.client.rotateCard(payload.cardId, payload.rotation);
          sendResponse({ success: true });
        } else {
          sendResponse({ success: false, error: 'Not connected' });
        }
        break;
      }

      case 'ready': {
        if (conn.client) {
          conn.client.ready();
          sendResponse({ success: true });
        } else {
          sendResponse({ success: false, error: 'Not connected' });
        }
        break;
      }

      case 'leave': {
        if (conn.client) {
          conn.client.leave();
          conn.client = null;
          sendResponse({ success: true });
        }
        break;
      }

      default:
        sendResponse({ success: false, error: `Unknown command: ${type}` });
    }
  }

  private setupIPCHandlers(): void {
    ipcMain.handle('ws-bridge-connect', async (event: IpcMainInvokeEvent, config: any) => {
      // This is handled via the WebSocket bridge
      return { success: true, port: this.port };
    });
  }
}
