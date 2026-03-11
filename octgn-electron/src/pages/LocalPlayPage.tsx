import { useEffect, useState, useCallback, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useGameStore } from '../stores/gameStore';
import { useKeyboardShortcuts } from '../hooks/useKeyboardShortcuts';
import { useCardSelection, useCardDrag } from '../hooks/useCardDrag';
import { useContextMenu, getCardContextMenuItems } from '../components/ContextMenu';
import {
  GameCanvas,
  PlayerHand,
  PlayerList,
  TurnIndicator,
  CardZoom,
  Button,
  Badge,
  ContextMenu,
  Modal,
} from '../components';
import { Card, Player } from '../types/game';
import { soundManager } from '../utils';

// Demo cards for local play
function createDemoCards(): Card[] {
  const cards: Card[] = [];
  let id = 0;

  // Create some demo cards on the table
  const demoCardData = [
    { name: 'Lightning Bolt', x: 200, y: 200 },
    { name: 'Giant Growth', x: 320, y: 200 },
    { name: 'Counterspell', x: 440, y: 200 },
    { name: 'Serra Angel', x: 200, y: 360 },
    { name: 'Shivan Dragon', x: 320, y: 360 },
    { name: 'Black Lotus', x: 440, y: 360 },
    { name: 'Sol Ring', x: 200, y: 520 },
    { name: 'Goblin Guide', x: 320, y: 520 },
    { name: 'Tarmogoyf', x: 440, y: 520 },
  ];

  for (const data of demoCardData) {
    cards.push({
      id: id++,
      modelId: `card-${id}`,
      groupId: 0,
      x: data.x,
      y: data.y,
      width: 100,
      height: 140,
      faceUp: Math.random() > 0.3,
      rotation: 0,
      ownerId: 'local-player',
      name: data.name,
      properties: {
        type: 'Card',
        cost: '{1}',
      },
      markers: [],
      anchored: false,
      targeted: false,
    });
  }

  // Create a deck pile
  for (let i = 0; i < 60; i++) {
    cards.push({
      id: id++,
      modelId: `deck-card-${i}`,
      groupId: 1,
      x: 800,
      y: 300,
      width: 100,
      height: 140,
      faceUp: false,
      rotation: 0,
      ownerId: 'local-player',
      name: `Deck Card ${i + 1}`,
      properties: {},
      markers: [],
      anchored: false,
      targeted: false,
    });
  }

  // Create hand cards
  for (let i = 0; i < 7; i++) {
    cards.push({
      id: id++,
      modelId: `hand-card-${i}`,
      groupId: 2,
      x: i * 110,
      y: 0,
      width: 100,
      height: 140,
      faceUp: true,
      rotation: 0,
      ownerId: 'local-player',
      name: `Hand Card ${i + 1}`,
      properties: {
        type: ['Instant', 'Creature', 'Land', 'Sorcery'][i % 4],
      },
      markers: [],
      anchored: false,
      targeted: false,
    });
  }

  return cards;
}

export default function LocalPlayPage() {
  const navigate = useNavigate();

  // Game store
  const {
    playerId,
    playerName,
    players,
    cards,
    groups,
    counters,
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
    setPlayerId,
    addPlayer,
    updateCard,
    selectCards,
    clearSelection,
    setZoomedCard,
    toggleChat,
    toggleHand,
    setPanOffset,
    setZoom,
    setCards,
    nextTurn,
    setPhase,
  } = useGameStore();

  // Game state
  const [gameStarted, setGameStarted] = useState(false);
  const [nickname, setNickname] = useState('');
  const [showNameModal, setShowNameModal] = useState(true);

  // Selection
  const cardSelection = useCardSelection(
    Array.from(cards.values()),
    selectCards
  );

  // Drag
  const cardDrag = useCardDrag({
    onDragStart: () => soundManager.play('cardmove'),
    onDragMove: (cardIds, dx, dy) => {
      cardIds.forEach((id) => {
        const card = cards.get(id);
        if (card) {
          updateCard(id, { x: card.x + dx, y: card.y + dy });
        }
      });
    },
    onDragEnd: () => {},
  });

  // Context menu
  const contextMenu = useContextMenu();

  // Initialize local game
  const startGame = useCallback(() => {
    if (!nickname.trim()) return;

    const pid = 'local-player';
    setPlayerId(pid);
    
    // Add local player
    addPlayer({
      id: pid,
      name: nickname,
      userId: 'local',
      publicKey: BigInt(0),
      tableSide: true,
      spectator: false,
      ready: true,
      color: '#9370DB',
      hand: { id: 2, name: 'Hand', type: 'hand', visibility: 'owner', visibleTo: [], cards: [], controllerId: pid },
      deck: { id: 1, name: 'Deck', type: 'deck', visibility: 'nobody', visibleTo: [], cards: [], controllerId: pid },
      discard: { id: 3, name: 'Discard', type: 'discard', visibility: 'all', visibleTo: [], cards: [], controllerId: pid },
      counters: [],
      disconnected: false,
      invertedTable: false,
    });

    // Load demo cards
    const demoCards = createDemoCards();
    const cardMap = new Map<number, Card>();
    demoCards.forEach(card => cardMap.set(card.id, card));
    setCards(cardMap);

    setConnected(true);
    setGameStarted(true);
    setShowNameModal(false);
  }, [nickname, setPlayerId, addPlayer, setCards, setConnected]);

  // Card context menu
  const handleCardContextMenu = useCallback((card: Card, event: React.MouseEvent) => {
    event.preventDefault();
    const items = getCardContextMenuItems(card, {
      onFlip: () => updateCard(card.id, { faceUp: !card.faceUp }),
      onRotate: (rotation) => updateCard(card.id, { rotation }),
      onMove: () => {},
    });
    contextMenu.show(event.clientX, event.clientY, items);
  }, [updateCard, contextMenu]);

  // Canvas click
  const handleCanvasClick = useCallback((x: number, y: number) => {
    clearSelection();
    contextMenu.hide();
  }, [clearSelection, contextMenu]);

  // Canvas context menu
  const handleCanvasContextMenu = useCallback((x: number, y: number, event: React.MouseEvent) => {
    event.preventDefault();
    contextMenu.show(event.clientX, event.clientY, [
      { id: 'draw-card', label: 'Draw Card', icon: '🃏', action: () => {} },
      { id: 'shuffle', label: 'Shuffle Deck', icon: '🔀', action: () => {} },
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
    onQuit: () => navigate('/play'),
  });

  // Convert cards for canvas
  const canvasCards = useMemo(() => {
    return Array.from(cards.values())
      .filter(c => c.groupId === 0) // Table cards only
      .map((card) => ({
        id: card.id.toString(),
        x: card.x,
        y: card.y,
        width: 100,
        height: 140,
        faceUp: card.faceUp,
        rotation: card.rotation,
        name: card.name,
        selected: selectedCards.includes(card.id),
      }));
  }, [cards, selectedCards]);

  // Hand cards
  const handCards = useMemo(() => {
    return Array.from(cards.values()).filter(c => c.groupId === 2);
  }, [cards]);

  // Render name modal
  if (showNameModal) {
    return (
      <div className="h-full flex items-center justify-center">
        <div className="glass rounded-2xl p-8 max-w-md w-full">
          <div className="text-center mb-6">
            <span className="text-5xl">🎮</span>
            <h2 className="text-2xl font-bold text-white mt-4">Local Game</h2>
            <p className="text-gray-400 mt-2">Play offline with demo cards</p>
          </div>
          
          <input
            type="text"
            value={nickname}
            onChange={(e) => setNickname(e.target.value)}
            placeholder="Your name..."
            className="input w-full mb-4"
            autoFocus
            onKeyDown={(e) => e.key === 'Enter' && startGame()}
          />
          
          <div className="flex space-x-3">
            <Button variant="secondary" className="flex-1" onClick={() => navigate('/play')}>
              Back
            </Button>
            <Button variant="primary" className="flex-1" onClick={startGame}>
              Start Game
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
          <h2 className="text-lg font-bold text-white">Local Game</h2>
          <Badge variant="success">Playing</Badge>
        </div>
        
        <div className="flex items-center space-x-3">
          <span className="text-sm text-gray-400">Turn {turnNumber}</span>
          <Button variant="ghost" size="sm" onClick={toggleChat}>
            💬
          </Button>
          <Button variant="ghost" size="sm" onClick={toggleHand}>
            🃏
          </Button>
          <Button variant="danger" size="sm" onClick={() => navigate('/play')}>
            End Game
          </Button>
        </div>
      </div>

      {/* Main Area */}
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
            onCardClick={(card) => selectCards([parseInt(card.id)], false)}
            onCardDoubleClick={(card) => {
              const c = cards.get(parseInt(card.id));
              if (c) updateCard(c.id, { faceUp: !c.faceUp });
            }}
            onCardContextMenu={handleCardContextMenu}
            onCardDragStart={(card, e) => cardDrag.startDrag([parseInt(card.id)], e)}
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

        {/* Sidebar */}
        <div className="w-64 bg-octgn-primary border-l border-octgn-accent/30 flex flex-col">
          {/* Player Info */}
          <div className="p-4 border-b border-octgn-accent/30">
            <h3 className="font-bold text-white mb-3">Players</h3>
            <PlayerList
              players={Array.from(players.values())}
              activePlayerId={playerId}
              localPlayerId={playerId}
            />
          </div>

          {/* Turn Info */}
          <div className="p-4 border-b border-octgn-accent/30">
            <TurnIndicator
              turnNumber={turnNumber}
              phase={phase.toString()}
              phases={phases}
              activePlayerId={activePlayerId}
              onNextTurn={nextTurn}
              onSetPhase={setPhase}
            />
          </div>

          {/* Quick Actions */}
          <div className="p-4 space-y-2">
            <h3 className="font-bold text-white mb-3">Quick Actions</h3>
            <Button variant="secondary" size="sm" className="w-full">
              🃏 Draw Card
            </Button>
            <Button variant="secondary" size="sm" className="w-full">
              🔀 Shuffle Deck
            </Button>
            <Button variant="secondary" size="sm" className="w-full">
              🗑️ Discard Hand
            </Button>
          </div>
        </div>
      </div>

      {/* Hand */}
      {showHand && (
        <div className="h-48 bg-octgn-primary border-t border-octgn-accent/30 p-4">
          <div className="flex items-center space-x-2 h-full overflow-x-auto">
            {handCards.map((card) => (
              <div
                key={card.id}
                onClick={() => selectCards([card.id], false)}
                onDoubleClick={() => {
                  // Play card to table
                  updateCard(card.id, { groupId: 0, x: 400 + Math.random() * 200, y: 300 + Math.random() * 100 });
                }}
                className={`flex-shrink-0 w-[80px] h-[112px] rounded-lg cursor-pointer transition-transform hover:scale-105 ${
                  card.faceUp
                    ? 'bg-gradient-to-br from-white to-gray-200'
                    : 'bg-gradient-to-br from-octgn-highlight to-purple-900'
                } ${selectedCards.includes(card.id) ? 'ring-2 ring-octgn-highlight' : ''}`}
              >
                <div className="h-full flex items-center justify-center p-2">
                  {card.faceUp ? (
                    <span className="text-xs text-gray-800 text-center font-medium">{card.name}</span>
                  ) : (
                    <span className="text-2xl">🂠</span>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Card Zoom */}
      {zoomedCard && (
        <CardZoom card={zoomedCard} onClose={() => setZoomedCard(null)} />
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
