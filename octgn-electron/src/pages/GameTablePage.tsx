import { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
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
  Layout,
  ContextMenu,
} from '../components';
import { Card as CardType, Player } from '../types/game';
import { soundManager } from '../utils';

export default function GameTablePage() {
  const { gameId } = useParams();
  const navigate = useNavigate();

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
      // Would send to server here
    },
  });

  // Context menu
  const contextMenu = useContextMenu();

  // Game client
  const gameClient = useGameClient({
    onConnected: () => {
      setConnected(true);
      setConnecting(false);
    },
    onDisconnected: () => {
      setConnected(false);
      setConnecting(false);
    },
    onError: (err) => {
      console.error('Connection error:', err);
      setConnecting(false);
    },
    onChat: (data) => {
      const player = players.find((p) => p.id === data.player);
      addChatMessage({
        id: Date.now().toString(),
        playerId: data.player,
        playerName: player?.name || 'Unknown',
        message: data.text,
        timestamp: Date.now(),
      });
      soundManager.play('chat');
    },
    onPlayerJoined: (data) => {
      addPlayer({
        id: data.id,
        name: data.nick,
        userId: data.userId,
        publicKey: BigInt(data.pkey),
        tableSide: data.tableSide,
        spectator: data.spectator,
        ready: false,
        disconnected: false,
        invertedTable: false,
        hand: { id: -1, name: 'Hand', type: 'hand', visibility: 'owner', visibleTo: [], cards: [], controllerId: data.id },
        deck: { id: -2, name: 'Deck', type: 'deck', visibility: 'nobody', visibleTo: [], cards: [], controllerId: data.id },
        discard: { id: -3, name: 'Discard', type: 'discard', visibility: 'all', visibleTo: [], cards: [], controllerId: data.id },
        counters: [],
      });
    },
    onPlayerLeft: (data) => {
      removePlayer(data.player);
    },
    onCardTurned: (data) => {
      updateCard(data.card, { faceUp: data.up });
      soundManager.play('cardflip');
    },
    onCardsMoved: (data) => {
      // Update card positions
    },
    onTurnChanged: (data) => {
      nextTurn();
    },
  });

  // Keyboard shortcuts
  useKeyboardShortcuts([
    { key: 'Escape', action: () => clearSelection() },
    { key: 'Delete', action: () => deleteSelectedCards() },
    { key: 'h', action: () => toggleHand() },
    { key: 'c', action: () => toggleChat() },
    { key: 'f', action: () => flipSelectedCards(true) },
    { key: 'r', action: () => rotateSelectedCards(90) },
    { key: '=', ctrl: true, action: () => setZoom(zoom * 1.1) },
    { key: '-', ctrl: true, action: () => setZoom(zoom / 1.1) },
    { key: '0', ctrl: true, action: () => setZoom(1) },
    { key: 's', ctrl: true, action: () => saveGame() },
    { key: 'o', ctrl: true, action: () => loadGame() },
  ]);

  // Initialize sound
  useEffect(() => {
    soundManager.initialize();
  }, []);

  // Demo cards for testing
  useEffect(() => {
    if (cards.size === 0) {
      const demoCards: CardType[] = [
        {
          id: 1,
          modelId: 'card1',
          groupId: 0,
          x: 200,
          y: 200,
          width: 200,
          height: 280,
          faceUp: true,
          rotation: 0,
          ownerId: playerId || '1',
          name: 'Lightning Bolt',
          properties: { type: 'Instant', cost: 'R' },
          markers: [],
          anchored: false,
          targeted: false,
        },
        {
          id: 2,
          modelId: 'card2',
          groupId: 0,
          x: 450,
          y: 200,
          width: 200,
          height: 280,
          faceUp: true,
          rotation: 0,
          ownerId: playerId || '1',
          name: 'Giant Growth',
          properties: { type: 'Instant', cost: 'G' },
          markers: [],
          anchored: false,
          targeted: false,
        },
        {
          id: 3,
          modelId: 'card3',
          groupId: 0,
          x: 700,
          y: 200,
          width: 200,
          height: 280,
          faceUp: false,
          rotation: 0,
          ownerId: playerId || '1',
          name: 'Mystery Card',
          properties: {},
          markers: [],
          anchored: false,
          targeted: false,
        },
        {
          id: 4,
          modelId: 'card4',
          groupId: 0,
          x: 300,
          y: 500,
          width: 200,
          height: 280,
          faceUp: true,
          rotation: 90,
          ownerId: playerId || '1',
          name: 'Serra Angel',
          properties: { type: 'Creature', cost: '3WW' },
          markers: [{ id: 'm1', name: '+1/+1', count: 2 }],
          anchored: false,
          targeted: false,
        },
      ];

      demoCards.forEach((card) => {
        useGameStore.getState().addCard(card);
      });
    }
  }, []);

  // Handlers
  const handleCardClick = useCallback(
    (card: CardType, event: React.MouseEvent) => {
      cardSelection.handleCardClick(card.id, event);
    },
    [cardSelection]
  );

  const handleCardDoubleClick = useCallback((card: CardType) => {
    updateCard(card.id, { faceUp: !card.faceUp });
    soundManager.play('cardflip');
  }, [updateCard]);

  const handleCardContextMenu = useCallback(
    (e: React.MouseEvent, card: CardType) => {
      e.preventDefault();
      const selected = selectedCards.includes(card.id) ? selectedCards : [card.id];
      selectCards(selected);

      const menuItems = getCardContextMenuItems(
        selected.map((id) => cards.get(id)!).filter(Boolean),
        {
          onFlipFaceUp: () => flipSelectedCards(true),
          onFlipFaceDown: () => flipSelectedCards(false),
          onRotate: (deg) => rotateSelectedCards(deg),
          onDelete: () => deleteSelectedCards(),
          onTarget: () => targetSelectedCards(),
          onHighlight: (color) => highlightSelectedCards(color),
        }
      );

      contextMenu.open(e.clientX, e.clientY, menuItems, { cards: selected.map((id) => cards.get(id)!) });
    },
    [selectedCards, cards, selectCards, contextMenu]
  );

  const flipSelectedCards = useCallback(
    (faceUp: boolean) => {
      selectedCards.forEach((id) => {
        updateCard(id, { faceUp });
      });
      soundManager.play('cardflip');
    },
    [selectedCards, updateCard]
  );

  const rotateSelectedCards = useCallback(
    (degrees: number) => {
      selectedCards.forEach((id) => {
        const card = cards.get(id);
        if (card) {
          const newRotation = (card.rotation + degrees) % 360;
          updateCard(id, { rotation: newRotation });
        }
      });
    },
    [selectedCards, cards, updateCard]
  );

  const deleteSelectedCards = useCallback(() => {
    selectedCards.forEach((id) => {
      useGameStore.getState().removeCard(id);
    });
    clearSelection();
  }, [selectedCards, clearSelection]);

  const targetSelectedCards = useCallback(() => {
    selectedCards.forEach((id) => {
      const card = cards.get(id);
      if (card) {
        updateCard(id, { targeted: !card.targeted });
      }
    });
  }, [selectedCards, cards, updateCard]);

  const highlightSelectedCards = useCallback(
    (color: string) => {
      selectedCards.forEach((id) => {
        updateCard(id, { highlighted: color || undefined });
      });
    },
    [selectedCards, updateCard]
  );

  const handleLeave = useCallback(() => {
    gameClient.leave();
    resetGame();
    navigate('/');
  }, [gameClient, resetGame, navigate]);

  const [chatInput, setChatInput] = useState('');

  const handleSendChat = useCallback(() => {
    if (chatInput.trim()) {
      gameClient.sendChat(chatInput);
      addChatMessage({
        id: Date.now().toString(),
        playerId: playerId || 'local',
        playerName: playerName,
        message: chatInput,
        timestamp: Date.now(),
      });
      setChatInput('');
    }
  }, [chatInput, gameClient, playerId, playerName, addChatMessage]);

  const isMyTurn = activePlayerId === playerId;
  const handCards = Array.from(cards.values()).filter((c) => c.groupId === -1);

  return (
    <div className="h-screen flex flex-col bg-octgn-dark">
      {/* Top toolbar */}
      <div className="bg-octgn-primary border-b border-octgn-accent px-4 py-2 flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <h1 className="text-lg font-bold text-white">
            {gameId || 'Local Game'}
          </h1>
          <span
            className={`text-sm ${connected ? 'text-green-500' : 'text-gray-500'}`}
          >
            {connected ? '● Connected' : connecting ? '○ Connecting...' : '○ Offline'}
          </span>
        </div>

        <div className="flex items-center space-x-2">
          <span className="text-sm text-gray-400">Zoom: {Math.round(zoom * 100)}%</span>
          <Button size="sm" variant="secondary" onClick={() => setZoom(1)}>
            Reset
          </Button>
          <Button size="sm" variant="secondary" onClick={toggleHand}>
            👋 Hand
          </Button>
          <Button size="sm" variant="secondary" onClick={toggleChat}>
            💬 Chat
          </Button>
          <Button size="sm" variant="secondary" onClick={() => saveGame()}>
            💾 Save
          </Button>
          <Button size="sm" variant="secondary" onClick={() => loadGame()}>
            📂 Load
          </Button>
          <Button size="sm" variant="danger" onClick={handleLeave}>
            Leave
          </Button>
        </div>
      </div>

      {/* Main game area */}
      <div className="flex-1 flex overflow-hidden">
        {/* Left sidebar - Players & Turn */}
        <div className="w-64 bg-octgn-primary border-r border-octgn-accent flex flex-col">
          <div className="p-3">
            <TurnIndicator
              turnNumber={turnNumber}
              activePlayerName={
                players.find((p) => p.id === activePlayerId)?.name || null
              }
              currentPhase={phase}
              isMyTurn={isMyTurn}
              phases={phases}
              onNextTurn={() => nextTurn()}
              onSetPhase={(p) => setPhase(p)}
            />
          </div>

          <div className="flex-1 overflow-y-auto p-3">
            <PlayerList
              players={players}
              currentPlayerId={playerId}
              activePlayerId={activePlayerId}
            />
          </div>

          <div className="p-3 border-t border-octgn-accent">
            <CounterPanel
              counters={Array.from(counters.values())}
              playerId={playerId || ''}
              onUpdateCounter={(name, value) => {
                useGameStore.getState().setCounter(name, value);
              }}
            />
          </div>
        </div>

        {/* Center - Game table */}
        <div className="flex-1 flex flex-col">
          {/* Opponent's area */}
          <div className="h-32 bg-octgn-accent/20 border-b border-octgn-accent flex items-center justify-center text-gray-500">
            Opponent's Area
          </div>

          {/* Table - Game Canvas */}
          <div className="flex-1 relative">
            <GameCanvas className="absolute inset-0" />

            {/* Zoom controls overlay */}
            <div className="absolute bottom-4 right-4 flex space-x-2">
              <Button
                size="sm"
                variant="secondary"
                onClick={() => setZoom(zoom / 1.2)}
              >
                -
              </Button>
              <Button size="sm" variant="secondary" onClick={() => setZoom(1)}>
                100%
              </Button>
              <Button
                size="sm"
                variant="secondary"
                onClick={() => setZoom(zoom * 1.2)}
              >
                +
              </Button>
            </div>
          </div>

          {/* Player's hand */}
          {showHand && (
            <div className="border-t border-octgn-accent">
              <PlayerHand
                cards={handCards}
                selectedCardIds={selectedCards}
                onCardClick={(card) => {
                  if (!selectedCards.includes(card.id)) {
                    selectCards([card.id]);
                  }
                }}
                onCardDoubleClick={handleCardDoubleClick}
                onCardContextMenu={(e, card) => handleCardContextMenu(e, card)}
              />
            </div>
          )}
        </div>

        {/* Right sidebar - Chat */}
        {showChat && (
          <div className="w-72 bg-octgn-primary border-l border-octgn-accent flex flex-col">
            <div className="p-3 border-b border-octgn-accent">
              <h3 className="font-bold text-white">Chat</h3>
            </div>

            <div className="flex-1 overflow-y-auto p-3 space-y-2">
              {chatMessages.map((msg) => (
                <div key={msg.id} className="text-sm">
                  <span className="font-medium text-octgn-highlight">
                    {msg.playerName}:
                  </span>{' '}
                  <span className="text-gray-300">{msg.message}</span>
                </div>
              ))}
              {chatMessages.length === 0 && (
                <div className="text-center text-gray-500 text-sm">
                  No messages yet
                </div>
              )}
            </div>

            <div className="p-3 border-t border-octgn-accent">
              <form
                onSubmit={(e) => {
                  e.preventDefault();
                  handleSendChat();
                }}
              >
                <div className="flex space-x-2">
                  <input
                    type="text"
                    value={chatInput}
                    onChange={(e) => setChatInput(e.target.value)}
                    placeholder="Type a message..."
                    className="input flex-1 text-sm"
                  />
                  <Button type="submit" size="sm">
                    Send
                  </Button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>

      {/* Card Zoom Overlay */}
      {zoomedCard && (
        <CardZoom
          card={zoomedCard}
          onClose={() => setZoomedCard(null)}
          showActions
          onPlayCard={(card) => {
            setZoomedCard(null);
          }}
        />
      )}

      {/* Context Menu */}
      <ContextMenu
        state={contextMenu}
        menuRef={contextMenu.menuRef}
        onClose={contextMenu.close}
      />
    </div>
  );
}
