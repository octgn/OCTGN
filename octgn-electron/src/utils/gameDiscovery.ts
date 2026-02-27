import { EventEmitter } from 'events';

export interface DiscoveredGame {
  id: string;
  name: string;
  host: string;
  port: number;
  gameName: string;
  playerCount: number;
  maxPlayers: number;
  hasPassword: boolean;
  discoveredAt: Date;
}

/**
 * LAN Game Discovery Service
 * 
 * Listens for UDP broadcasts from OCTGN game servers on the local network.
 * This matches the existing broadcast protocol used by the WPF client.
 */
export class GameDiscoveryService extends EventEmitter {
  private socket: UDPWebSocket | null = null;
  private broadcastPort: number;
  private isListening = false;
  private discoveredGames: Map<string, DiscoveredGame> = new Map();
  private cleanupInterval: NodeJS.Timeout | null = null;

  constructor(broadcastPort: number = 21234) {
    super();
    this.broadcastPort = broadcastPort;
  }

  /**
   * Start listening for game broadcasts
   */
  async start(): Promise<void> {
    if (this.isListening) return;

    // In browser context, we can't use UDP sockets directly
    // This would be implemented via the Electron main process
    // For now, we'll use a WebSocket-based approach

    this.isListening = true;
    this.startCleanupTimer();

    this.emit('started');
  }

  /**
   * Stop listening for broadcasts
   */
  stop(): void {
    if (!this.isListening) return;

    if (this.socket) {
      this.socket.close();
      this.socket = null;
    }

    if (this.cleanupInterval) {
      clearInterval(this.cleanupInterval);
      this.cleanupInterval = null;
    }

    this.isListening = false;
    this.emit('stopped');
  }

  /**
   * Handle incoming broadcast message
   */
  handleBroadcast(data: Buffer, from: string): void {
    try {
      const message = this.parseBroadcast(data);
      if (message) {
        const game: DiscoveredGame = {
          ...message,
          host: from,
          discoveredAt: new Date(),
        };

        // Update or add game
        const existingKey = `${game.host}:${game.port}`;
        this.discoveredGames.set(existingKey, game);

        this.emit('game-discovered', game);
      }
    } catch (error) {
      console.error('Failed to parse broadcast:', error);
    }
  }

  /**
   * Parse broadcast message format
   * Format matches existing OCTGN protocol
   */
  private parseBroadcast(data: Buffer): Partial<DiscoveredGame> | null {
    // Broadcast format:
    // [4 bytes] magic number
    // [2 bytes] port
    // [string] game name (null-terminated)
    // [string] session name (null-terminated)
    // [byte] player count
    // [byte] max players
    // [byte] has password

    if (data.length < 8) return null;

    const magic = data.readUInt32LE(0);
    if (magic !== 0x4f435447) return null; // 'OCTG'

    const port = data.readUInt16LE(4);
    
    let offset = 6;
    const gameName = this.readNullTerminatedString(data, offset);
    offset += gameName.length + 1;
    
    const sessionName = this.readNullTerminatedString(data, offset);
    offset += sessionName.length + 1;

    const playerCount = data.readUInt8(offset++);
    const maxPlayers = data.readUInt8(offset++);
    const hasPassword = data.readUInt8(offset++) !== 0;

    return {
      id: `${gameName}-${port}`,
      name: sessionName || gameName,
      port,
      gameName,
      playerCount,
      maxPlayers,
      hasPassword,
    };
  }

  private readNullTerminatedString(buffer: Buffer, offset: number): string {
    let end = offset;
    while (end < buffer.length && buffer[end] !== 0) {
      end++;
    }
    return buffer.toString('utf8', offset, end);
  }

  /**
   * Clean up old discovered games
   */
  private startCleanupTimer(): void {
    this.cleanupInterval = setInterval(() => {
      const now = Date.now();
      const maxAge = 30000; // 30 seconds

      for (const [key, game] of this.discoveredGames) {
        if (now - game.discoveredAt.getTime() > maxAge) {
          this.discoveredGames.delete(key);
          this.emit('game-expired', game);
        }
      }
    }, 10000);
  }

  /**
   * Get all currently discovered games
   */
  getDiscoveredGames(): DiscoveredGame[] {
    return Array.from(this.discoveredGames.values());
  }

  /**
   * Check if service is currently listening
   */
  get isActive(): boolean {
    return this.isListening;
  }
}

// Stub for UDP WebSocket (would use dgram in Node.js/Electron main)
interface UDPWebSocket {
  close(): void;
}

/**
 * React hook for game discovery
 */
export function useGameDiscovery(broadcastPort?: number) {
  const service = useRef<GameDiscoveryService | null>(null);
  const [games, setGames] = useState<DiscoveredGame[]>([]);
  const [isScanning, setIsScanning] = useState(false);

  useEffect(() => {
    service.current = new GameDiscoveryService(broadcastPort);

    service.current.on('game-discovered', (game: DiscoveredGame) => {
      setGames(service.current?.getDiscoveredGames() || []);
    });

    service.current.on('game-expired', (game: DiscoveredGame) => {
      setGames(service.current?.getDiscoveredGames() || []);
    });

    return () => {
      service.current?.stop();
    };
  }, [broadcastPort]);

  const startScanning = async () => {
    if (!isScanning) {
      setIsScanning(true);
      await service.current?.start();
    }
  };

  const stopScanning = () => {
    setIsScanning(false);
    service.current?.stop();
  };

  return {
    games,
    isScanning,
    startScanning,
    stopScanning,
  };
}

// Need useState and useRef for the hook
import { useState, useEffect, useRef } from 'react';
