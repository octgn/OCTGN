import { useEffect, useState, useCallback, useMemo } from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import { useGameStore } from '../stores/gameStore';
import { useGameClient } from '../hooks/useGameClient';
import { useKeyboardShortcuts } from '../hooks/useKeyboardShortcuts';
import { useCardSelection, useCardDrag, useSelectionBox } from '../hooks/useCardDrag';
import { useContextMenu, getCardContextMenuItems } from '../components/ContextMenu';
import {
  GameCanvas,
  PlayerHand,
  PlayerList,
  TurnIndicator,
  CounterPanel,
  CardZoom,
  Modal,
  Button,
  Badge,
  ContextMenu,
} from '../components';
import { Card as CardType, Player } from '../types/game';

interface ChatMsg {
  id: string;
  playerId: string;
  playerName: string;
  message: string;
  timestamp: number;
  isSystem?: boolean;
}
import { soundManager } from '../utils';

type GamePhase = 'connecting' | 'lobby' | 'playing' | 'finished';

export default function GameTablePage() {
  const { gameId } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  // URL params
  const isHost = searchParams.get('host') === 'true';
  const hostAddress = searchParams.get('host') || 'localhost';
  const port = parseInt(searchParams.get('port') || '8888', 10);
  const gameName = searchParams.get('gameName') || 'Unknown Game';
  const serverName = searchParams.get('name') || 'Game';
  const password = searchParams.get('password') || '';

  // Game store
  const {
    playerId,
    playerName,
    players,
    cards,
    groups,
    counters,
    connected,
    connecting,
    activePlayerId,
    turnNumber,
    phase,
    phases,
    selectedCards,
    panOffset,
    zoom,
    chatMessages,
    showChat,
    showHand,
    zoomedCard,
    setConnected,
    setConnecting,
    setPlayerId,
    addChatMessage,
    addPlayer,
    removePlayer,
    updateCard,
    selectCards,
    clearSelection,
    setZoomedCard,
    toggleChat,
    toggleHand,
    setPanOffset,
    setZoom,
    saveGame,
    loadGame,
    resetGame,
    nextTurn,
    setPhase,
  } = useGameStore();

  // Game phase
  const [gamePhase, setGamePhase] = useState<GamePhase>('connecting');
  const [error, setError] = useState<string | null>(null);

  // Player info
  const [nickname, setNickname] = useState('');
  const [showNameModal, setShowNameModal] = useState(false);

  // Selection box state
  const selectionBox = useSelectionBox();
  const [isSelecting, setIsSelecting] = useState(false);
  const [selectStart, setSelectStart] = useState({ x: 0, y: 0 });

  // Card selection
  const cardSelection = useCardSelection(
    Array.from(cards.values()),
    selectCards
  );

  // Drag state
  const cardDrag = useCardDrag({
    onDragStart: (cardIds) => {
      soundManager.play('cardmove');
    },
    onDragMove: (cardIds, dx, dy) => {
      cardIds.forEach((id) => {
        const card = cards.get(id);
        if (card) {
          updateCard(id, { x: card.x + dx, y: card.y + dy });
        }
      });
    },
    onDragEnd: (cardIds, x, y) => {
      // Send to server via game client
    },
  });

  // Context menu
  const contextMenu = useContextMenu();

  // Game client hook
  const gameClient = useGameClient({
    onConnected: () => {
      setConnected(true);
      setGamePhase('lobby');
      addChatMessage({
        id: crypto.randomUUID(),
        playerId: 'system',
        playerName: 'System',
        message: 'Connected to game server',
        timestamp: Date.now(),
        isSystem: true,
      });
    },
    onDisconnected: (data) => {
      setConnected(false);
      setGamePhase('connecting');
      addChatMessage({
        id: crypto.randomUUID(),
        playerId: 'system',
        playerName: 'System',
        message: data?.reason || 'Disconnected from server',
        timestamp: Date.now(),
        isSystem: true,
      });
    },
    onError: (err) => {
      setError('Connection error');
      setGamePhase('connecting');
    },
    onChat: (data) => {
      addChatMessage({
        id: crypto.randomUUID(),
        playerId: data.playerId || data.sender,
        playerName: data.sender || data.playerName,
        message: data.text || data.message,
        timestamp: Date.now(),
      });
    },
    onPlayerJoined: (data) => {
      addPlayer({
        id: data.playerId,
        name: data.name,
        userId: data.userId || data.playerId,
        publicKey: BigInt(0),
        tableSide: true,
        spectator: data.spectating || false,
        ready: false,
        color: data.color || '#9370DB',
        hand: { id: -1, name: 'Hand', type: 'hand', visibility: 'owner', visibleTo: [], cards: [], controllerId: data.playerId },
        deck: { id: -2, name: 'Deck', type: 'deck', visibility: 'nobody', visibleTo: [], cards: [], controllerId: data.playerId },
        discard: { id: -3, name: 'Discard', type: 'discard', visibility: 'all', visibleTo: [], cards: [], controllerId: data.playerId },
        counters: [],
        disconnected: false,
        invertedTable: false,
      });
    },
    onPlayerLeft: (data) => {
      removePlayer(data.playerId);
    },
    onTurnChanged: (data) => {
      // Update turn state
    },
  });

  // Chat input
  const [chatInput, setChatInput] = useState('');

  // Check if we need to ask for nickname
  useEffect(() => {
    if (!playerName && !showNameModal) {
      setShowNameModal(true);
    }
  }, [playerName, showNameModal]);

  // Auto-connect when we have all info
  useEffect(() => {
    if (playerName && !connected && !connecting && gameId !== 'local') {
      handleConnect();
    }
  }, [playerName, connected, connecting, gameId]);

  // Handle connecting to server
  const handleConnect = useCallback(async () => {
    if (!playerName) return;
    
    setConnecting(true);
    setError(null);
    
    try {
      await gameClient.connect(hostAddress, port, playerName, false);
    } catch (err: any) {
      setError(err.message || 'Failed to connect');
      setConnecting(false);
    }
  }, [playerName, hostAddress, port, gameClient]);

  // Handle name submission
  const handleNameSubmit = useCallback(() => {
    if (nickname.trim()) {
      setPlayerId(crypto.randomUUID());
      // Store nickname in game store
      setShowNameModal(false);
    }
  }, [nickname]);

  // Handle hosting game
  const handleHostGame = useCallback(async () => {
    if (!window.electronAPI?.startServer) {
      setError('Server not available - running in browser mode');
      return;
    }

    try {
      const result = await window.electronAPI.startServer(port);
      if (!result.success) {
        setError(result.error || 'Failed to start server');
        return;
      }
      
      // Now connect to our own server
      await handleConnect();
    } catch (err: any) {
      setError(err.message);
    }
  }, [port, handleConnect]);

  // Handle disconnect
  const handleDisconnect = useCallback(() => {
    gameClient.disconnect();
    navigate('/play');
  }, [gameClient, navigate]);

  // Handle sending chat
  const handleSendChat = useCallback(() => {
    if (chatInput.trim()) {
      gameClient.sendChat(chatInput.trim());
      setChatInput('');
    }
  }, [chatInput, gameClient]);

  // Handle card context menu
  const handleCardContextMenu = useCallback((card: CardType, event: React.MouseEvent) => {
    event.preventDefault();
    const items = getCardContextMenuItems(card, {
      onFlip: () => gameClient.turnCard(parseInt(card.id), !card.faceUp),
      onRotate: (rotation) => gameClient.rotateCard(parseInt(card.id), rotation),
      onMove: (groupId) => {
        // Move card to group
      },
    });
    contextMenu.show(event.clientX, event.clientY, items);
  }, [gameClient, contextMenu]);

  // Handle canvas click
  const handleCanvasClick = useCallback((x: number, y: number) => {
    clearSelection();
    contextMenu.hide();
  }, [clearSelection, contextMenu]);

  // Handle canvas right-click
  const handleCanvasContextMenu = useCallback((x: number, y: number, event: React.MouseEvent) => {
    event.preventDefault();
    // Show table context menu
    contextMenu.show(event.clientX, event.clientY, [
      { id: 'create-card', label: 'Create Card', icon: '🃏', action: () => {} },
      { id: 'divider', type: 'separator' },
      { id: 'reset-zoom', label: 'Reset View', icon: '🔍', action: () => { setZoom(1); setPanOffset({ x: 0, y: 0 }); } },
    ]);
  }, [contextMenu, setZoom, setPanOffset]);

  // Keyboard shortcuts
  useKeyboardShortcuts({
    onUndo: () => {},
    onRedo: () => {},
    onDelete: () => {},
    onCopy: () => {},
    onPaste: () => {},
    onSelectAll: () => {},
    onZoomIn: () => setZoom(Math.min(3, zoom * 1.2)),
    onZoomOut: () => setZoom(Math.max(0.3, zoom / 1.2)),
    onResetZoom: () => { setZoom(1); setPanOffset({ x: 0, y: 0 }); },
    onToggleChat: toggleChat,
    onToggleHand: toggleHand,
    onQuit: handleDisconnect,
  });

  // Convert store cards to canvas format
  const canvasCards = useMemo(() => {
    return Array.from(cards.values()).map((card) => ({
      id: card.id,
      x: card.x,
      y: card.y,
      width: 100,
      height: 140,
      faceUp: card.faceUp,
      rotation: card.rotation || 0,
      name: card.name,
      imageUrl: card.imageUrl,
      selected: selectedCards.has(card.id),
    }));
  }, [cards, selectedCards]);

  // Player list for display
  const playerList = useMemo(() => {
    return Array.from(players.values());
  }, [players]);

  // Render connecting modal
  if (gamePhase === 'connecting' && !showNameModal) {
    return (
      <div className="h-full flex items-center justify-center">
        <div className="glass rounded-2xl p-8 text-center max-w-md">
          <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-octgn-accent animate-pulse flex items-center justify-center">
            <span className="text-3xl">🎮</span>
          </div>
          <h2 className="text-xl font-bold text-white mb-2">Connecting...</h2>
          <p className="text-gray-400 mb-4">
            {isHost ? 'Starting game server...' : `Connecting to ${hostAddress}:${port}...`}
          </p>
          {error && (
            <p className="text-red-400 mb-4">{error}</p>
          )}
          <div className="flex justify-center space-x-3">
            {isHost && (
              <Button variant="primary" onClick={handleHostGame}>
                Start Server
              </Button>
            )}
            <Button variant="secondary" onClick={handleDisconnect}>
              Cancel
            </Button>
          </div>
        </div>
      </div>
    );
  }

  // Render name input modal
  if (showNameModal) {
    return (
      <div className="h-full flex items-center justify-center">
        <div className="glass rounded-2xl p-8 max-w-md w-full">
          <h2 className="text-xl font-bold text-white mb-4">Enter Your Name</h2>
          <input
            type="text"
            value={nickname}
            onChange={(e) => setNickname(e.target.value)}
            placeholder="Your nickname..."
            className="input w-full mb-4"
            autoFocus
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                handleNameSubmit();
              }
            }}
          />
          <div className="flex justify-end space-x-3">
            <Button variant="secondary" onClick={() => navigate('/play')}>
              Cancel
            </Button>
            <Button variant="primary" onClick={handleNameSubmit}>
              {isHost ? 'Host Game' : 'Join Game'}
            </Button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="h-full flex flex-col">
      {/* Top Bar */}
      <div className="h-12 bg-octgn-primary border-b border-octgn-accent/30 flex items-center justify-between px-4">
        <div className="flex items-center space-x-4">
          <h2 className="text-lg font-bold text-white">{serverName}</h2>
          <Badge variant={connected ? 'success' : 'error'}>
            {connected ? 'Connected' : 'Disconnected'}
          </Badge>
          <span className="text-sm text-gray-400">{gameName}</span>
        </div>
        
        <div className="flex items-center space-x-3">
          {gamePhase === 'lobby' && (
            <Button variant="primary" size="sm" onClick={() => gameClient.ready()}>
              Ready
            </Button>
          )}
          <Button variant="ghost" size="sm" onClick={toggleChat}>
            💬
          </Button>
          <Button variant="ghost" size="sm" onClick={toggleHand}>
            🃏
          </Button>
          <Button variant="danger" size="sm" onClick={handleDisconnect}>
            Leave
          </Button>
        </div>
      </div>

      {/* Main Game Area */}
      <div className="flex-1 flex overflow-hidden">
        {/* Game Canvas */}
        <div className="flex-1 relative">
          <GameCanvas
            cards={canvasCards}
            panOffset={panOffset}
            zoom={zoom}
            onPanChange={setPanOffset}
            onZoomChange={setZoom}
            onClick={handleCanvasClick}
            onContextMenu={handleCanvasContextMenu}
            onCardClick={(card) => selectCards([card.id], false)}
            onCardDoubleClick={(card) => gameClient.turnCard(parseInt(card.id), !card.faceUp)}
            onCardContextMenu={handleCardContextMenu}
            onCardDragStart={(card, e) => cardDrag.startDrag([card.id], e)}
            onSelectionChange={() => {}}
          />

          {/* Zoom Controls */}
          <div className="absolute bottom-4 left-4 glass rounded-lg p-2 flex items-center space-x-2">
            <button
              onClick={() => setZoom(Math.max(0.3, zoom / 1.2))}
              className="w-8 h-8 rounded hover:bg-octgn-accent flex items-center justify-center text-white"
            >
              ➖
            </button>
            <span className="text-white text-sm w-16 text-center">{Math.round(zoom * 100)}%</span>
            <button
              onClick={() => setZoom(Math.min(3, zoom * 1.2))}
              className="w-8 h-8 rounded hover:bg-octgn-accent flex items-center justify-center text-white"
            >
              ➕
            </button>
          </div>
        </div>

        {/* Right Sidebar */}
        <div className="w-64 bg-octgn-primary border-l border-octgn-accent/30 flex flex-col">
          {/* Player List */}
          <div className="p-4 border-b border-octgn-accent/30">
            <h3 className="font-bold text-white mb-3">Players</h3>
            <PlayerList
              players={playerList}
              activePlayerId={activePlayerId}
              localPlayerId={playerId}
            />
          </div>

          {/* Turn Info */}
          {gamePhase === 'playing' && (
            <div className="p-4 border-b border-octgn-accent/30">
              <TurnIndicator
                turnNumber={turnNumber}
                phase={phase || ''}
                phases={phases || []}
                activePlayerId={activePlayerId}
                onNextTurn={nextTurn}
                onSetPhase={setPhase}
              />
            </div>
          )}

          {/* Chat */}
          {showChat && (
            <div className="flex-1 flex flex-col min-h-0">
              <div className="p-4 border-b border-octgn-accent/30">
                <h3 className="font-bold text-white">Chat</h3>
              </div>
              <div className="flex-1 overflow-y-auto p-2 space-y-2">
                {chatMessages.map((msg) => (
                  <div key={msg.id} className="text-sm">
                    <span className="text-octgn-highlight font-medium">{msg.playerName}:</span>{' '}
                    <span className="text-gray-300">{msg.message}</span>
                  </div>
                ))}
              </div>
              <div className="p-2 border-t border-octgn-accent/30">
                <div className="flex space-x-2">
                  <input
                    type="text"
                    value={chatInput}
                    onChange={(e) => setChatInput(e.target.value)}
                    onKeyDown={(e) => e.key === 'Enter' && handleSendChat()}
                    placeholder="Type a message..."
                    className="input flex-1 text-sm"
                  />
                  <Button size="sm" onClick={handleSendChat}>
                    Send
                  </Button>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Player Hand (Bottom) */}
      {showHand && (
        <div className="h-48 bg-octgn-primary border-t border-octgn-accent/30">
          <PlayerHand
            cards={Array.from(cards.values()).filter((c) => c.group === 'hand')}
            selectedCards={selectedCards}
            onCardClick={(card) => selectCards([card.id], false)}
            onCardDoubleClick={(card) => gameClient.turnCard(parseInt(card.id), !card.faceUp)}
            onCardContextMenu={handleCardContextMenu}
          />
        </div>
      )}

      {/* Card Zoom Modal */}
      {zoomedCard && (
        <CardZoom
          card={zoomedCard}
          onClose={() => setZoomedCard(null)}
        />
      )}

      {/* Context Menu */}
      {contextMenu.state.visible && (
        <ContextMenu
          items={contextMenu.state.items}
          position={{ x: contextMenu.state.x, y: contextMenu.state.y }}
          onClose={contextMenu.hide}
        />
      )}
    </div>
  );
}
