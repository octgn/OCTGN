import { useEffect, useRef, useCallback, useState } from 'react';

interface GameEvent {
  type: string;
  data: any;
}

interface UseGameClientOptions {
  onConnected?: () => void;
  onDisconnected?: (data: any) => void;
  onError?: (error: any) => void;
  onChat?: (data: any) => void;
  onPlayerJoined?: (data: any) => void;
  onPlayerLeft?: (data: any) => void;
  onCardCreated?: (data: any) => void;
  onCardsMoved?: (data: any) => void;
  onCardTurned?: (data: any) => void;
  onTurnChanged?: (data: any) => void;
}

interface UseGameClientReturn {
  connected: boolean;
  connecting: boolean;
  error: string | null;
  connect: (host: string, port: number, nick: string, spectator?: boolean) => Promise<void>;
  disconnect: () => void;
  sendChat: (text: string) => void;
  moveCards: (cardIds: number[], toGroup: number, toIndex: number[], faceUp: boolean[]) => void;
  turnCard: (cardId: number, faceUp: boolean) => void;
  rotateCard: (cardId: number, rotation: number) => void;
  ready: () => void;
  leave: () => void;
}

export function useGameClient(options: UseGameClientOptions = {}): UseGameClientReturn {
  const wsRef = useRef<WebSocket | null>(null);
  const pendingRequests = useRef<Map<string, { resolve: Function; reject: Function }>>(new Map());
  
  const [connected, setConnected] = useState(false);
  const [connecting, setConnecting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Get WebSocket bridge port from Electron or use default
  const bridgePort = window.electronAPI?.wsBridgePort || 8889;

  const sendCommand = useCallback((type: string, payload: any): Promise<any> => {
    return new Promise((resolve, reject) => {
      if (!wsRef.current || wsRef.current.readyState !== WebSocket.OPEN) {
        reject(new Error('Not connected'));
        return;
      }

      const id = `${Date.now()}-${Math.random().toString(36).substring(2, 9)}`;
      pendingRequests.current.set(id, { resolve, reject });

      wsRef.current.send(JSON.stringify({ type, payload, id }));

      // Timeout after 10 seconds
      setTimeout(() => {
        if (pendingRequests.current.has(id)) {
          pendingRequests.current.delete(id);
          reject(new Error('Request timeout'));
        }
      }, 10000);
    });
  }, []);

  const connect = useCallback(async (host: string, port: number, nick: string, spectator = false) => {
    if (wsRef.current?.readyState === WebSocket.OPEN) {
      throw new Error('Already connected');
    }

    setConnecting(true);
    setError(null);

    return new Promise<void>((resolve, reject) => {
      try {
        // Connect to the WebSocket bridge
        wsRef.current = new WebSocket(`ws://localhost:${bridgePort}`);

        wsRef.current.onopen = () => {
          // Now send connect command to the actual game server
          sendCommand('connect', { host, port, nick, spectator })
            .then(() => {
              setConnected(true);
              setConnecting(false);
              options.onConnected?.();
              resolve();
            })
            .catch((err) => {
              setConnecting(false);
              setError(err.message);
              reject(err);
            });
        };

        wsRef.current.onmessage = (event) => {
          try {
            const message = JSON.parse(event.data);
            
            if (message.id && pendingRequests.current.has(message.id)) {
              const pending = pendingRequests.current.get(message.id)!;
              pendingRequests.current.delete(message.id);
              
              if (message.success) {
                pending.resolve(message);
              } else {
                pending.reject(new Error(message.error));
              }
            } else if (message.type) {
              // This is an event from the server
              handleEvent(message);
            }
          } catch (err) {
            console.error('Failed to parse message:', err);
          }
        };

        wsRef.current.onclose = () => {
          setConnected(false);
          setConnecting(false);
          options.onDisconnected?.({});
        };

        wsRef.current.onerror = (err) => {
          setConnecting(false);
          setError('Connection failed');
          options.onError?.(err);
          reject(err);
        };
      } catch (err: any) {
        setConnecting(false);
        setError(err.message);
        reject(err);
      }
    });
  }, [bridgePort, options, sendCommand]);

  const handleEvent = useCallback((event: GameEvent) => {
    const { type, data } = event;

    switch (type) {
      case 'disconnected':
        setConnected(false);
        options.onDisconnected?.(data);
        break;
      case 'chat':
        options.onChat?.(data);
        break;
      case 'player-joined':
        options.onPlayerJoined?.(data);
        break;
      case 'player-left':
        options.onPlayerLeft?.(data);
        break;
      case 'card-created':
        options.onCardCreated?.(data);
        break;
      case 'cards-moved':
        options.onCardsMoved?.(data);
        break;
      case 'card-turned':
        options.onCardTurned?.(data);
        break;
      case 'turn-changed':
        options.onTurnChanged?.(data);
        break;
    }
  }, [options]);

  const disconnect = useCallback(() => {
    if (wsRef.current) {
      sendCommand('disconnect', {}).catch(() => {});
      wsRef.current.close();
      wsRef.current = null;
    }
    setConnected(false);
  }, [sendCommand]);

  const sendChat = useCallback((text: string) => {
    sendCommand('chat', { text }).catch(console.error);
  }, [sendCommand]);

  const moveCards = useCallback((cardIds: number[], toGroup: number, toIndex: number[], faceUp: boolean[]) => {
    sendCommand('moveCards', { cardIds, toGroup, toIndex, faceUp }).catch(console.error);
  }, [sendCommand]);

  const turnCard = useCallback((cardId: number, faceUp: boolean) => {
    sendCommand('turnCard', { cardId, faceUp }).catch(console.error);
  }, [sendCommand]);

  const rotateCard = useCallback((cardId: number, rotation: number) => {
    sendCommand('rotateCard', { cardId, rotation }).catch(console.error);
  }, [sendCommand]);

  const ready = useCallback(() => {
    sendCommand('ready', {}).catch(console.error);
  }, [sendCommand]);

  const leave = useCallback(() => {
    sendCommand('leave', {}).catch(console.error);
    setConnected(false);
  }, [sendCommand]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (wsRef.current) {
        wsRef.current.close();
      }
    };
  }, []);

  return {
    connected,
    connecting,
    error,
    connect,
    disconnect,
    sendChat,
    moveCards,
    turnCard,
    rotateCard,
    ready,
    leave,
  };
}
