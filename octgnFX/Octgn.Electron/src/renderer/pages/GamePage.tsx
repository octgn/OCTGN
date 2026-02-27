import React, { useState, useCallback, useRef, useEffect } from 'react';
import { clsx } from 'clsx';
import GlassPanel from '../components/GlassPanel';
import Button from '../components/Button';
import CardComponent from '../components/Card';
import { useGameStore } from '../stores/game-store';
import { useAppStore } from '../stores/app-store';
import type { Card, ChatMessage, Player, Counter } from '../../shared/types';

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
  const nextTurn = useGameStore((s) => s.nextTurn);
  const subscribe = useGameStore((s) => s.subscribe);
  const navigate = useAppStore((s) => s.navigate);

  const [chatOpen, setChatOpen] = useState(true);
  const [chatInput, setChatInput] = useState('');
  const [selectedCardId, setSelectedCardId] = useState<string | null>(null);
  const [contextMenu, setContextMenu] = useState<ContextMenu | null>(null);
  const chatEndRef = useRef<HTMLDivElement>(null);

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

  const handleCardContextMenu = useCallback((e: React.MouseEvent, card: Card) => {
    e.preventDefault();
    setContextMenu({ x: e.clientX, y: e.clientY, cardId: card.id });
    setSelectedCardId(card.id);
  }, []);

  const handleFlipCard = useCallback(() => {
    if (!contextMenu) return;
    const card = [...(gameState?.table.cards ?? []), ...handCards].find(c => c.id === contextMenu.cardId);
    if (card) flipCard(Number(card.id), !card.faceUp);
    setContextMenu(null);
  }, [contextMenu, gameState, flipCard]);

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

  const localPlayer = gameState?.players.find(
    (p: Player) => p.id === gameState.localPlayerId
  );
  const otherPlayers = gameState?.players.filter(
    (p: Player) => p.id !== gameState?.localPlayerId
  ) ?? [];

  const handCards: Card[] =
    localPlayer?.groups.find((g) => g.name.toLowerCase() === 'hand')?.cards ?? [];
  const tableCards: Card[] = gameState?.table.cards ?? [];

  if (!gameState) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-center animate-in">
          <p className="font-display text-lg text-octgn-text-muted">Waiting for game state...</p>
          <Button variant="ghost" size="sm" className="mt-4" onClick={() => navigate('lobby')}>
            Return to Lobby
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex h-full bg-octgn-bg">
      {/* Main game area */}
      <div className="flex-1 flex flex-col min-w-0">
        {/* Top bar: game info + turn info */}
        <div className="flex items-center gap-3 px-4 py-2 border-b border-octgn-border/30 bg-octgn-surface/40 backdrop-blur-sm">
          <span className="font-display text-xs font-semibold tracking-wider text-octgn-primary">
            {gameState.gameName}
          </span>
          <div className="w-px h-4 bg-octgn-border/50" />
          <span className="text-xs text-octgn-text-muted">
            Turn {gameState.turnNumber}
          </span>
          {gameState.activePlayer != null && (
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

          {/* Player counters */}
          {localPlayer?.counters.map((c: Counter) => (
            <div key={c.id} className="flex items-center gap-1.5 text-xs">
              <span className="text-octgn-text-dim">{c.name}</span>
              <span className="font-mono font-bold text-octgn-text bg-octgn-surface px-1.5 py-0.5 rounded border border-octgn-border/30">
                {c.value}
              </span>
            </div>
          ))}

          <Button variant="ghost" size="sm" onClick={() => nextTurn()}>
            Next Turn
          </Button>
          <Button variant="ghost" size="sm" onClick={() => setChatOpen((v) => !v)}>
            Chat
          </Button>
          <Button variant="ghost" size="sm" onClick={() => navigate('lobby')}>
            Leave
          </Button>
        </div>

        {/* Other players area */}
        {otherPlayers.length > 0 && (
          <div className="flex items-start gap-3 px-4 py-2 border-b border-octgn-border/20 bg-octgn-surface/20">
            {otherPlayers.map((player: Player) => (
              <GlassPanel key={player.id} variant="light" padding="sm" glow="none" className="text-xs">
                <div className="flex items-center gap-2">
                  <div
                    className="w-2 h-2 rounded-full"
                    style={{ backgroundColor: player.color || '#6b7280' }}
                  />
                  <span className="font-medium text-octgn-text">{player.name}</span>
                </div>
                <div className="flex gap-2 mt-1">
                  {player.counters.map((c: Counter) => (
                    <span key={c.id} className="text-octgn-text-dim">
                      {c.name}: <span className="text-octgn-text font-mono">{c.value}</span>
                    </span>
                  ))}
                </div>
              </GlassPanel>
            ))}
          </div>
        )}

        {/* Table / play area */}
        <div className="flex-1 relative overflow-hidden">
          {/* Board background */}
          {gameState.table.board && (
            <img
              src={gameState.table.board.imageUrl}
              alt=""
              className="absolute inset-0 w-full h-full object-contain opacity-20 pointer-events-none select-none"
            />
          )}

          {/* Grid pattern overlay */}
          <div
            className="absolute inset-0 pointer-events-none opacity-[0.03]"
            style={{
              backgroundImage:
                'linear-gradient(rgba(59,130,246,0.3) 1px, transparent 1px), linear-gradient(90deg, rgba(59,130,246,0.3) 1px, transparent 1px)',
              backgroundSize: '40px 40px',
            }}
          />

          {/* Table cards */}
          <div className="absolute inset-0 p-4">
            {tableCards.map((card: Card) => (
              <div
                key={card.id}
                className="absolute"
                style={{ left: card.position.x, top: card.position.y }}
                onContextMenu={(e) => handleCardContextMenu(e, card)}
              >
                <CardComponent
                  card={card}
                  selected={card.id === selectedCardId}
                  onClick={(c) => setSelectedCardId(c.id === selectedCardId ? null : c.id)}
                />
              </div>
            ))}

            {tableCards.length === 0 && (
              <div className="flex items-center justify-center h-full">
                <p className="text-octgn-text-dim/40 font-display text-sm tracking-widest uppercase">
                  Game Table
                </p>
              </div>
            )}
          </div>
        </div>

        {/* Player hand */}
        <div className="border-t border-octgn-border/30 bg-octgn-surface/40 backdrop-blur-sm">
          <div className="flex items-center gap-1 px-4 py-2 overflow-x-auto">
            {handCards.length === 0 && (
              <p className="text-xs text-octgn-text-dim py-4 mx-auto">Your hand is empty</p>
            )}
            {handCards.map((card: Card) => (
              <div key={card.id} onContextMenu={(e) => handleCardContextMenu(e, card)}>
                <CardComponent
                  card={card}
                  selected={card.id === selectedCardId}
                  onClick={(c) => setSelectedCardId(c.id === selectedCardId ? null : c.id)}
                  className="shrink-0"
                  style={{ width: 80, height: 112 }}
                />
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Context menu */}
      {contextMenu && (
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
            {gameState.chatMessages.map((msg: ChatMessage) => (
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
                    style={{ color: msg.color || '#9ca3af' }}
                  >
                    {msg.playerName}:
                  </span>
                )}
                {msg.message}
              </div>
            ))}
            <div ref={chatEndRef} />
          </div>

          <div className="p-2 border-t border-octgn-border/30">
            <div className="flex gap-1.5">
              <input
                type="text"
                value={chatInput}
                onChange={(e) => setChatInput(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleSendChat()}
                placeholder="Send a message..."
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
  );
};

export default GamePage;
