import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useGameStore } from '../stores/gameStore';
import { useGameClient } from '../hooks/useGameClient';
import {
  Layout,
  PlayerHand,
  PlayerList,
  TurnIndicator,
  CounterPanel,
  CardZoom,
  Modal,
  Button,
} from '../components';
import { Card as CardType, Player, Counter, Group } from '../types/game';
import { soundManager } from '../utils';

export default function PlayPage() {
  const { gameId } = useParams();
  const navigate = useNavigate();
  
  // Local game state
  const [players, setPlayers] = useState<Player[]>([]);
  const [groups, setGroups] = useState<Group[]>([]);
  const [handCards, setHandCards] = useState<CardType[]>([]);
  const [tableCards, setTableCards] = useState<CardType[]>([]);
  const [counters, setCounters] = useState<Counter[]>([]);
  const [activePlayerId, setActivePlayerId] = useState<string | null>(null);
  const [turnNumber, setTurnNumber] = useState(0);
  const [currentPhase, setCurrentPhase] = useState(0);
  
  // UI state
  const [selectedCards, setSelectedCards] = useState<number[]>([]);
  const [zoomedCard, setZoomedCard] = useState<CardType | null>(null);
  const [showHand, setShowHand] = useState(true);
  const [showChat, setShowChat] = useState(true);
  const [chatMessages, setChatMessages] = useState<ChatMessage[]>([]);
  const [chatInput, setChatInput] = useState('');
  
  // Connection state
  const [isConnecting, setIsConnecting] = useState(false);
  const [connectionError, setError] = useState<string | null>(null);

  const {
    playerId,
    playerName,
    connected,
    connect: storeConnect,
    disconnect,
  } = useGameStore();

  // Game client hook
  const {
    connected: clientConnected,
    connecting,
    error: clientError,
    connect,
    disconnect: clientDisconnect,
    sendChat,
    moveCards,
    turnCard,
    rotateCard,
    ready,
    leave,
  } = useGameClient({
    onConnected: () => {
      setIsConnecting(false);
      setError(null);
    },
    onDisconnected: () => {
      setIsConnecting(false);
    },
    onError: (err) => {
      setError(err.message || 'Connection error');
      setIsConnecting(false);
    },
    onChat: (data) => {
      const player = players.find(p => p.id === data.player);
      setChatMessages(prev => [...prev, {
        id: Date.now().toString(),
        playerId: data.player,
        playerName: player?.name || 'Unknown',
        message: data.text,
        timestamp: Date.now(),
      }]);
      soundManager.play('chat');
    },
    onPlayerJoined: (data) => {
      const newPlayer: Player = {
        id: data.id,
        name: data.nick,
        userId: data.userId,
        publicKey: data.pkey,
        tableSide: data.tableSide,
        spectator: data.spectator,
        ready: false,
        hand: { id: -1, name: 'Hand', type: 'hand', visibility: 'owner', visibleTo: [], cards: [], controllerId: data.id },
        deck: { id: -2, name: 'Deck', type: 'deck', visibility: 'nobody', visibleTo: [], cards: [], controllerId: data.id },
        discard: { id: -3, name: 'Discard', type: 'discard', visibility: 'all', visibleTo: [], cards: [], controllerId: data.id },
        counters: [],
        disconnected: false,
        invertedTable: false,
      };
      setPlayers(prev => [...prev, newPlayer]);
    },
    onPlayerLeft: (data) => {
      setPlayers(prev => prev.filter(p => p.id !== data.player));
    },
    onCardCreated: (data) => {
      // Add new card to appropriate group
    },
    onCardsMoved: (data) => {
      // Update card positions
    },
    onCardTurned: (data) => {
      soundManager.play('cardflip');
    },
    onTurnChanged: (data) => {
      setActivePlayerId(data.activePlayer);
      setTurnNumber(prev => prev + 1);
      soundManager.play('turn');
    },
  });

  // Initialize sound manager
  useEffect(() => {
    soundManager.initialize();
  }, []);

  // Handle card selection
  const handleCardClick = useCallback((card: CardType) => {
    setSelectedCards(prev => {
      if (prev.includes(card.id)) {
        return prev.filter(id => id !== card.id);
      }
      return [...prev, card.id];
    });
  }, []);

  const handleCardDoubleClick = useCallback((card: CardType) => {
    // Toggle face up/down
    turnCard(card.id, !card.faceUp);
  }, [turnCard]);

  const handleCardContextMenu = useCallback((e: React.MouseEvent, card: CardType) => {
    e.preventDefault();
    setZoomedCard(card);
  }, []);

  // Handle chat
  const handleSendChat = useCallback(() => {
    if (chatInput.trim()) {
      sendChat(chatInput);
      setChatInput('');
    }
  }, [chatInput, sendChat]);

  // Handle leave
  const handleLeave = useCallback(() => {
    leave();
    navigate('/');
  }, [leave, navigate]);

  return (
    <div className="h-screen flex flex-col bg-octgn-dark">
      {/* Top toolbar */}
      <div className="bg-octgn-primary border-b border-octgn-accent px-4 py-2 flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <h1 className="text-lg font-bold text-white">
            {gameId || 'Local Game'}
          </h1>
          <span className={`text-sm ${connected ? 'text-green-500' : 'text-gray-500'}`}>
            {connected ? '● Connected' : connecting ? '○ Connecting...' : '○ Disconnected'}
          </span>
        </div>

        <div className="flex items-center space-x-2">
          <button
            onClick={() => setShowHand(!showHand)}
            className={`btn ${showHand ? 'btn-primary' : 'btn-secondary'} text-sm`}
          >
            👋 Hand
          </button>
          <button
            onClick={() => setShowChat(!showChat)}
            className={`btn ${showChat ? 'btn-primary' : 'btn-secondary'} text-sm`}
          >
            💬 Chat
          </button>
          <button
            onClick={handleLeave}
            className="btn btn-danger text-sm"
          >
            Leave
          </button>
        </div>
      </div>

      {/* Main game area */}
      <div className="flex-1 flex overflow-hidden">
        {/* Left sidebar - Players & Turn */}
        <div className="w-64 bg-octgn-primary border-r border-octgn-accent flex flex-col">
          <div className="p-3">
            <TurnIndicator
              turnNumber={turnNumber}
              activePlayerName={players.find(p => p.id === activePlayerId)?.name || null}
              currentPhase={currentPhase}
              isMyTurn={activePlayerId === playerId}
              phases={[
                { name: 'Draw' },
                { name: 'Main' },
                { name: 'Combat' },
                { name: 'End' },
              ]}
              onNextTurn={() => {
                // Send next turn
              }}
              onSetPhase={(phase) => setCurrentPhase(phase)}
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
              counters={counters}
              playerId={playerId || ''}
              onUpdateCounter={(name, value) => {
                // Send counter update
              }}
            />
          </div>
        </div>

        {/* Center - Game table */}
        <div className="flex-1 flex flex-col">
          {/* Opponent's area */}
          <div className="h-48 bg-octgn-accent/20 border-b border-octgn-accent">
            {/* Opponent's cards/areas would go here */}
            <div className="h-full flex items-center justify-center text-gray-500">
              Opponent's Area
            </div>
          </div>

          {/* Table */}
          <div className="flex-1 bg-octgn-primary/50 relative overflow-hidden">
            {/* Game board / cards would be rendered here */}
            <div className="absolute inset-0 flex items-center justify-center text-gray-500">
              {tableCards.length === 0 ? (
                <div className="text-center">
                  <p className="text-lg">No cards on the table</p>
                  <p className="text-sm mt-2">Load a deck to start playing</p>
                </div>
              ) : (
                // Render table cards
                <div>Table cards would render here</div>
              )}
            </div>
          </div>

          {/* Player's hand */}
          {showHand && (
            <div className="border-t border-octgn-accent">
              <PlayerHand
                cards={handCards}
                selectedCardIds={selectedCards}
                onCardClick={handleCardClick}
                onCardDoubleClick={handleCardDoubleClick}
                onCardContextMenu={handleCardContextMenu}
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
            // Play card to table
            setZoomedCard(null);
          }}
        />
      )}

      {/* Connection Error Modal */}
      {!connected && connectionError && (
        <Modal
          isOpen={true}
          onClose={() => navigate('/')}
          title="Connection Error"
          footer={
            <>
              <Button variant="ghost" onClick={() => navigate('/')}>
                Go Home
              </Button>
              <Button variant="primary" onClick={() => setIsConnecting(true)}>
                Retry
              </Button>
            </>
          }
        >
          <p className="text-gray-300">{connectionError}</p>
        </Modal>
      )}
    </div>
  );
}

interface ChatMessage {
  id: string;
  playerId: string;
  playerName: string;
  message: string;
  timestamp: number;
}
