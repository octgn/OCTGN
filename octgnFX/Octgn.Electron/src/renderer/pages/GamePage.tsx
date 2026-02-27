import React, { useState, useCallback, useRef, useEffect, useMemo } from 'react';
import { clsx } from 'clsx';
import GlassPanel from '../components/GlassPanel';
import Button from '../components/Button';
import PreGameLobby from '../components/PreGameLobby';
import { DragDropProvider } from '../components/DragDropContext';
import GameBoard from '../components/GameBoard';
import { useGameStore } from '../stores/game-store';
import { useAppStore } from '../stores/app-store';
import type { Card, ChatMessage, Player, Counter, Group } from '../../shared/types';

interface ContextMenu {
  x: number;
  y: number;
  cardId: string;
}

const GamePage: React.FC = () => {
  const gameState = useGameStore((s) => s.gameState);
  const sendChat = useGameStore((s) => s.sendChat);
  const flipCard = useGameStore((s) => s.flipCard);
  const rotateCard = useGameStore((s) => s.rotateCard);
  const peekCard = useGameStore((s) => s.peekCard);
  const moveCardsAt = useGameStore((s) => s.moveCardsAt);
  const moveCards = useGameStore((s) => s.moveCards);
  const nextTurn = useGameStore((s) => s.nextTurn);
  const subscribe = useGameStore((s) => s.subscribe);
  const leaveGame = useGameStore((s) => s.leaveGame);
  const navigate = useAppStore((s) => s.navigate);

  const [chatOpen, setChatOpen] = useState(true);
  const [chatInput, setChatInput] = useState('');
  const [selectedCardId, setSelectedCardId] = useState<string | null>(null);
  const [contextMenu, setContextMenu] = useState<ContextMenu | null>(null);
  const chatEndRef = useRef<HTMLDivElement>(null);

  const isSpectator = gameState?.isSpectator ?? false;

  // Subscribe to game state updates from main process
  useEffect(() => {
    const unsubscribe = subscribe();
    return unsubscribe;
  }, [subscribe]);

  useEffect(() => {
    chatEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [gameState?.chatMessages?.length]);

  // Close context menu on click anywhere
  useEffect(() => {
    const handler = () => setContextMenu(null);
    window.addEventListener('click', handler);
    return () => window.removeEventListener('click', handler);
  }, []);

  const handleSendChat = useCallback(() => {
    if (!chatInput.trim()) return;
    sendChat(chatInput.trim());
    setChatInput('');
  }, [chatInput, sendChat]);

  const handleCardClick = useCallback(
    (card: Card) => {
      setSelectedCardId(card.id === selectedCardId ? null : card.id);
    },
    [selectedCardId]
  );

  const handleCardContextMenu = useCallback((e: React.MouseEvent, card: Card) => {
    if (isSpectator) return; // No context menu for spectators
    e.preventDefault();
    setContextMenu({ x: e.clientX, y: e.clientY, cardId: card.id });
    setSelectedCardId(card.id);
  }, [isSpectator]);

  const allCards = useMemo(() => {
    if (!gameState) return [];
    const cards = [...(gameState.table.cards ?? [])];
    for (const player of gameState.players) {
      for (const group of player.groups) {
        cards.push(...group.cards);
      }
    }
    return cards;
  }, [gameState]);

  const handleFlipCard = useCallback(() => {
    if (!contextMenu) return;
    const card = allCards.find(c => c.id === contextMenu.cardId);
    if (card) flipCard(Number(card.id), !card.faceUp);
    setContextMenu(null);
  }, [contextMenu, allCards, flipCard]);

  const handleRotateCard = useCallback((degrees: number) => {
    if (!contextMenu) return;
    rotateCard(Number(contextMenu.cardId), degrees);
    setContextMenu(null);
  }, [contextMenu, rotateCard]);

  const handlePeekCard = useCallback(() => {
    if (!contextMenu) return;
    peekCard(Number(contextMenu.cardId));
    setContextMenu(null);
  }, [contextMenu, peekCard]);

  /** Drag-drop: move card to table at (x, y) position */
  const handleCardMoveToTable = useCallback(
    (cardId: string, x: number, y: number) => {
      if (isSpectator) return;
      moveCardsAt(
        [Number(cardId)],
        [x],
        [y],
        [0],
        [true]
      );
    },
    [moveCardsAt, isSpectator]
  );

  /** Drag-drop: move card to a named group */
  const handleCardMoveToGroup = useCallback(
    (cardId: string, groupId: string) => {
      if (isSpectator) return;
      moveCards(
        [Number(cardId)],
        Number(groupId),
        [0],
        [false]
      );
    },
    [moveCards, isSpectator]
  );

  const handleLeave = useCallback(async () => {
    await leaveGame();
    navigate('lobby');
  }, [leaveGame, navigate]);

  // Collect all players (spectators see everyone)
  const allPlayers = gameState?.players ?? [];
  const localPlayer = gameState?.players.find(
    (p: Player) => p.id === gameState?.localPlayerId
  );
  const activePlayers = allPlayers.filter((p: Player) => !p.isSpectator);
  const otherPlayers = isSpectator
    ? activePlayers
    : allPlayers.filter((p: Player) => p.id !== gameState?.localPlayerId);

  // Spectators see ALL hands; regular players only see their own
  const handCards: Card[] = isSpectator
    ? [] // spectators don't have a personal hand zone
    : localPlayer?.groups.find((g) => g.name.toLowerCase() === 'hand')?.cards ?? [];
  const tableCards: Card[] = gameState?.table.cards ?? [];
  const playerGroups = isSpectator ? [] : localPlayer?.groups ?? [];

  // Connection status
  const connectionStatus = gameState?.connectionStatus ?? 'connected';

  if (!gameState) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-center animate-in">
          <div className="w-8 h-8 border-2 border-octgn-primary/30 border-t-octgn-primary rounded-full animate-spin mx-auto mb-4" />
          <p className="font-display text-lg text-octgn-text-muted">Connecting to game...</p>
          <Button variant="ghost" size="sm" className="mt-4" onClick={() => navigate('lobby')}>
            Return to Lobby
          </Button>
        </div>
      </div>
    );
  }

  // Show pre-game lobby when game hasn't started yet
  if (!gameState.isStarted) {
    return <PreGameLobby gameState={gameState} />;
  }

  return (
    <DragDropProvider>
      <div className="flex h-full bg-octgn-bg">
        {/* Main game area */}
        <div className="flex-1 flex flex-col min-w-0">
          {/* Top bar: game info + turn info */}
          <div className="flex items-center gap-3 px-4 py-2 border-b border-octgn-border/30 bg-octgn-surface/40 backdrop-blur-sm">
            {/* Connection status indicator */}
            <div className="flex items-center gap-1.5">
              <span className={clsx(
                'w-2 h-2 rounded-full',
                connectionStatus === 'connected' && 'bg-octgn-success',
                connectionStatus === 'disconnected' && 'bg-octgn-danger',
                connectionStatus === 'reconnecting' && 'bg-octgn-warning animate-pulse',
              )} />
              {connectionStatus !== 'connected' && (
                <span className="text-[10px] text-octgn-warning">
                  {connectionStatus === 'reconnecting' ? 'Reconnecting...' : 'Disconnected'}
                </span>
              )}
            </div>

            {/* Spectator badge */}
            {isSpectator && (
              <span className="px-2 py-0.5 rounded-full bg-octgn-accent/20 border border-octgn-accent/40 text-[10px] font-bold text-octgn-accent tracking-widest uppercase animate-pulse">
                Spectating
              </span>
            )}

            <span className="font-display text-xs font-semibold tracking-wider text-octgn-primary">
              {gameState.gameName}
            </span>
            <div className="w-px h-4 bg-octgn-border/50" />
            <span className="text-xs text-octgn-text-muted">
              Turn {gameState.turnNumber}
            </span>
            {gameState.activePlayer != null && gameState.activePlayer !== 0 && (
              <>
                <div className="w-px h-4 bg-octgn-border/50" />
                <span className="text-xs text-octgn-text-muted">
                  Active:{' '}
                  <span className="text-octgn-text font-medium">
                    {gameState.players.find((p: Player) => p.id === gameState.activePlayer)?.name ?? '?'}
                  </span>
                </span>
              </>
            )}
            <div className="flex-1" />

            {/* Player counters (spectator sees none, non-spectator sees own) */}
            {!isSpectator && localPlayer?.counters.map((c: Counter) => (
              <div key={c.id} className="flex items-center gap-1.5 text-xs">
                <span className="text-octgn-text-dim">{c.name}</span>
                <span className="font-mono font-bold text-octgn-text bg-octgn-surface px-1.5 py-0.5 rounded border border-octgn-border/30">
                  {c.value}
                </span>
              </div>
            ))}

            {!isSpectator && (
              <Button variant="ghost" size="sm" onClick={() => nextTurn()}>
                Next Turn
              </Button>
            )}
            <Button variant="ghost" size="sm" onClick={() => setChatOpen((v) => !v)}>
              Chat
            </Button>
            <Button variant="ghost" size="sm" onClick={handleLeave}>
              Leave
            </Button>
          </div>

          {/* Player panels — spectators see all players, non-spectators see opponents */}
          {otherPlayers.length > 0 && (
            <div className="flex items-start gap-3 px-4 py-2 border-b border-octgn-border/20 bg-octgn-surface/20 overflow-x-auto">
              {otherPlayers.map((player: Player) => (
                <PlayerPanel
                  key={player.id}
                  player={player}
                  isActive={player.id === gameState.activePlayer}
                  isSpectatorView={isSpectator}
                />
              ))}
            </div>
          )}

          {/* Game Board with table, hand, and group zones */}
          <GameBoard
            tableCards={tableCards}
            handCards={handCards}
            groups={playerGroups}
            selectedCardId={selectedCardId}
            boardImageUrl={gameState.table.board?.imageUrl}
            boardX={gameState.table.board?.x}
            boardY={gameState.table.board?.y}
            boardWidth={gameState.table.board?.width}
            boardHeight={gameState.table.board?.height}
            backgroundStyle={gameState.table.backgroundStyle}
            tableBackgroundUrl={gameState.table.backgroundUrl}
            tableWidth={gameState.table.width}
            tableHeight={gameState.table.height}
            onCardClick={handleCardClick}
            onCardContextMenu={handleCardContextMenu}
            onCardMoveToTable={handleCardMoveToTable}
            onCardMoveToGroup={handleCardMoveToGroup}
            useTwoSidedTable={gameState.useTwoSidedTable}
            isSpectator={isSpectator}
            allPlayers={isSpectator ? activePlayers : undefined}
          />
        </div>

        {/* Context menu (non-spectator only) */}
        {!isSpectator && contextMenu && (
          <div
            className="fixed z-50 py-1 min-w-[160px] rounded-lg border border-octgn-border/50 bg-octgn-surface/95 backdrop-blur-md shadow-xl shadow-black/40"
            style={{ left: contextMenu.x, top: contextMenu.y }}
            onClick={(e) => e.stopPropagation()}
          >
            <button onClick={handleFlipCard} className="w-full text-left px-3 py-1.5 text-xs text-octgn-text hover:bg-octgn-primary/20 transition-colors">
              Flip Card
            </button>
            <button onClick={() => handleRotateCard(1)} className="w-full text-left px-3 py-1.5 text-xs text-octgn-text hover:bg-octgn-primary/20 transition-colors">
              Rotate 90°
            </button>
            <button onClick={() => handleRotateCard(2)} className="w-full text-left px-3 py-1.5 text-xs text-octgn-text hover:bg-octgn-primary/20 transition-colors">
              Rotate 180°
            </button>
            <button onClick={() => handleRotateCard(0)} className="w-full text-left px-3 py-1.5 text-xs text-octgn-text hover:bg-octgn-primary/20 transition-colors">
              Reset Rotation
            </button>
            <div className="my-1 border-t border-octgn-border/30" />
            <button onClick={handlePeekCard} className="w-full text-left px-3 py-1.5 text-xs text-octgn-text hover:bg-octgn-primary/20 transition-colors">
              Peek
            </button>
          </div>
        )}

        {/* Chat panel */}
        {chatOpen && (
          <div className="w-72 shrink-0 flex flex-col border-l border-octgn-border/30 bg-octgn-surface/50">
            <div className="px-3 py-2 border-b border-octgn-border/30 flex items-center justify-between">
              <span className="text-xs font-semibold text-octgn-text-muted tracking-wide uppercase">
                Chat
              </span>
              <button
                onClick={() => setChatOpen(false)}
                className="text-octgn-text-dim hover:text-octgn-text transition-colors p-0.5"
              >
                <svg className="w-3.5 h-3.5" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
                  <path d="M4 4l8 8M12 4l-8 8" />
                </svg>
              </button>
            </div>

            <div className="flex-1 overflow-y-auto p-3 space-y-1.5 text-xs">
              {gameState.chatMessages.map((msg: ChatMessage) => {
                const senderColor = msg.color
                  || gameState.players.find((p: Player) => p.id === msg.playerId)?.color
                  || '#9ca3af';
                return (
                  <div
                    key={msg.id}
                    className={clsx(
                      'leading-relaxed',
                      msg.isSystem ? 'text-octgn-text-dim italic' : 'text-octgn-text'
                    )}
                  >
                    {!msg.isSystem && (
                      <span
                        className="font-semibold mr-1"
                        style={{ color: senderColor }}
                      >
                        {msg.playerName}:
                      </span>
                    )}
                    {msg.message}
                  </div>
                );
              })}
              <div ref={chatEndRef} />
            </div>

            <div className="p-2 border-t border-octgn-border/30">
              <div className="flex gap-1.5">
                <input
                  type="text"
                  value={chatInput}
                  onChange={(e) => setChatInput(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleSendChat()}
                  placeholder={isSpectator ? 'Chat as spectator...' : 'Send a message...'}
                  className="flex-1 h-7 px-2 text-xs rounded bg-octgn-surface border border-octgn-border/50 text-octgn-text placeholder-octgn-text-dim outline-none focus:border-octgn-primary/50 transition-colors"
                />
                <Button variant="primary" size="sm" onClick={handleSendChat} className="h-7 px-2">
                  <svg className="w-3 h-3" viewBox="0 0 16 16" fill="currentColor">
                    <path d="M1.5 1.5l13 6.5-13 6.5V9l8-1-8-1V1.5z" />
                  </svg>
                </Button>
              </div>
            </div>
          </div>
        )}
      </div>
    </DragDropProvider>
  );
};

// ─── Player Panel Component ──────────────────────────────────────────────
interface PlayerPanelProps {
  player: Player;
  isActive: boolean;
  isSpectatorView: boolean;
}

const PlayerPanel: React.FC<PlayerPanelProps> = ({ player, isActive, isSpectatorView }) => {
  const handGroup = player.groups.find((g) => g.name.toLowerCase() === 'hand');
  const otherGroups = player.groups.filter((g) => g.name.toLowerCase() !== 'hand');

  return (
    <GlassPanel
      variant="light"
      padding="sm"
      glow={isActive ? 'blue' : 'none'}
      className={clsx(
        'text-xs min-w-[180px] transition-all duration-300',
        isActive && 'ring-1 ring-octgn-primary/50 shadow-[0_0_12px_rgba(59,130,246,0.3)]'
      )}
    >
      <div className="flex items-center gap-2 mb-1">
        <div
          className={clsx(
            'w-2.5 h-2.5 rounded-full transition-shadow duration-300',
            isActive && 'shadow-[0_0_8px_rgba(59,130,246,0.7)]'
          )}
          style={{ backgroundColor: player.color || '#6b7280' }}
        />
        <span className="font-medium text-octgn-text">{player.name}</span>
        {isActive && (
          <span className="text-[9px] text-octgn-primary font-bold tracking-wider">ACTIVE</span>
        )}
        {player.isSpectator && (
          <span className="text-[9px] text-octgn-accent">(Spectator)</span>
        )}
      </div>

      {/* Counters */}
      {player.counters.length > 0 && (
        <div className="flex flex-wrap gap-2 mt-1">
          {player.counters.map((c: Counter) => (
            <div key={c.id} className="flex items-center gap-1">
              <span className="text-octgn-text-dim text-[10px]">{c.name}</span>
              <span className="font-mono font-bold text-octgn-text text-[11px] bg-octgn-surface/60 px-1 rounded border border-octgn-border/20">
                {c.value}
              </span>
            </div>
          ))}
        </div>
      )}

      {/* Group card counts (spectator view) */}
      {isSpectatorView && (
        <div className="flex flex-wrap gap-1.5 mt-1.5">
          {handGroup && (
            <span className="text-[10px] text-octgn-text-dim bg-octgn-surface/40 px-1.5 py-0.5 rounded">
              Hand: {handGroup.cards.length}
            </span>
          )}
          {otherGroups.map((g: Group) => (
            <span key={g.id} className="text-[10px] text-octgn-text-dim bg-octgn-surface/40 px-1.5 py-0.5 rounded">
              {g.name}: {g.cards.length}
            </span>
          ))}
        </div>
      )}
    </GlassPanel>
  );
};

export default GamePage;
