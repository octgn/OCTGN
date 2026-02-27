import { useState, useEffect, useRef } from 'react';

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

// Simple event emitter that works in both Node and browser
type EventCallback = (...args: unknown[]) => void;

class SimpleEventEmitter {
  private listeners: Map<string, Set<EventCallback>> = new Map();

  on(event: string, callback: EventCallback): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)!.add(callback);
  }

  off(event: string, callback: EventCallback): void {
    this.listeners.get(event)?.delete(callback);
  }

  emit(event: string, ...args: unknown[]): void {
    this.listeners.get(event)?.forEach((cb) => cb(...args));
  }

  removeAllListeners(): void {
    this.listeners.clear();
  }
}

/**
 * LAN Game Discovery Service
 * 
 * Listens for UDP broadcasts from OCTGN game servers on the local network.
 * This matches the existing broadcast protocol used by the WPF client.
 */
export class GameDiscoveryService extends SimpleEventEmitter {
  private broadcastPort: number;
  private isListening = false;
  private discoveredGames: Map<string, DiscoveredGame> = new Map();
  private cleanupInterval: ReturnType<typeof setInterval> | null = null;

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

    if (this.cleanupInterval) {
      clearInterval(this.cleanupInterval);
      this.cleanupInterval = null;
    }

    this.isListening = false;
    this.emit('stopped');
  }

  /**
   * Handle incoming broadcast message (called from main process)
   */
  handleBroadcast(data: { host: string; message: string }): void {
    try {
      const message = JSON.parse(data.message);
      const game: DiscoveredGame = {
        ...message,
        host: data.host,
        discoveredAt: new Date(),
      };

      // Update or add game
      const existingKey = `${game.host}:${game.port}`;
      this.discoveredGames.set(existingKey, game);

      this.emit('game-discovered', game);
    } catch (error) {
      console.error('Failed to parse broadcast:', error);
    }
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

/**
 * React hook for game discovery
 */
export function useGameDiscovery(broadcastPort?: number) {
  const service = useRef<GameDiscoveryService | null>(null);
  const [games, setGames] = useState<DiscoveredGame[]>([]);
  const [isScanning, setIsScanning] = useState(false);

  useEffect(() => {
    service.current = new GameDiscoveryService(broadcastPort);

    service.current.on('game-discovered', () => {
      setGames(service.current?.getDiscoveredGames() || []);
    });

    service.current.on('game-expired', () => {
      setGames(service.current?.getDiscoveredGames() || []);
    });

    return () => {
      service.current?.stop();
      service.current?.removeAllListeners();
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
