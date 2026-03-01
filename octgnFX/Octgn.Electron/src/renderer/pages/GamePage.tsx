import React, { useState, useCallback, useRef, useEffect, useMemo } from 'react';
import { clsx } from 'clsx';
import Button from '../components/Button';
import PreGameLobby from '../components/PreGameLobby';
import { DragDropProvider } from '../components/DragDropContext';
import TouchDragLayer from '../components/TouchDragLayer';
import GameBoard from '../components/GameBoard';
import type { ScreenToTableCoordsFn } from '../components/GameBoard';
import PlayerGroupBrowser from '../components/PlayerGroupBrowser';
import { parseO8dXml } from '../components/DeckLoader';
import { useGameStore } from '../stores/game-store';
import { useAppStore } from '../stores/app-store';
import { useToastStore } from '../stores/toast-store';
import type { Card, ChatMessage, Player } from '../../shared/types';
import { readablePlayerColor } from '../utils/player-colors';

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
  const reorderHandCard = useGameStore((s) => s.reorderHandCard);
  const nextTurn = useGameStore((s) => s.nextTurn);
  const subscribe = useGameStore((s) => s.subscribe);
  const leaveGame = useGameStore((s) => s.leaveGame);
  const loadDeck = useGameStore((s) => s.loadDeck);
  const getDeckPaths = useGameStore((s) => s.getDeckPaths);
  const navigate = useAppStore((s) => s.navigate);
  const addToast = useToastStore((s) => s.addToast);

  const [chatOpen, setChatOpen] = useState(true);
  const [deckLoading, setDeckLoading] = useState<'user' | 'prebuilt' | false>(false);
  const [prebuiltDecksPath, setPrebuiltDecksPath] = useState<string | null>(null);
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

  // Fetch deck paths when game definition is available
  useEffect(() => {
    if (gameState?.gameDefinition?.id && !isSpectator) {
      getDeckPaths(gameState.gameDefinition.id).then(({ prebuiltDecksPath: prebuilt }) => {
        setPrebuiltDecksPath(prebuilt);
      });
    }
  }, [gameState?.gameDefinition?.id, isSpectator, getDeckPaths]);

  const handleLoadDeck = useCallback(async (defaultPath?: string | null) => {
    try {
      const result = await window.octgn.openFileDialog(
        [
          { name: 'OCTGN Deck Files', extensions: ['o8d'] },
          { name: 'All Files', extensions: ['*'] },
        ],
        defaultPath || undefined,
      );

      if (!result) {
        setDeckLoading(false);
        return;
      }

      const deck = parseO8dXml(result.content);
      const totalCards = deck.sections.reduce(
        (sum, section) => sum + section.cards.reduce((a, c) => a + c.quantity, 0),
        0,
      );

      if (totalCards === 0) {
        addToast('Deck file is empty - no cards found', 'warning');
        setDeckLoading(false);
        return;
      }

      await loadDeck(deck);
      const fileName = result.filePath.split(/[/\\]/).pop() || 'deck';
      addToast(
        `Loaded "${fileName}" - ${totalCards} card${totalCards !== 1 ? 's' : ''}`,
        'success',
      );
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to load deck file';
      addToast(message, 'error');
    } finally {
      setDeckLoading(false);
    }
  }, [loadDeck, addToast]);

  const handleLoadUserDeck = useCallback(async () => {
    setDeckLoading('user');
    const { userDecksPath } = await getDeckPaths(gameState?.gameDefinition?.id);
    handleLoadDeck(userDecksPath);
  }, [getDeckPaths, gameState?.gameDefinition?.id, handleLoadDeck]);

  const handleLoadPrebuiltDeck = useCallback(async () => {
    setDeckLoading('prebuilt');
    handleLoadDeck(prebuiltDecksPath);
  }, [prebuiltDecksPath, handleLoadDeck]);

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

  /** Drag-drop: move card to table at (x, y) position (table-space coords) */
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
        [true]
      );
    },
    [moveCards, isSpectator]
  );

  /** Drag-drop: reorder card within the hand (local only) */
  const handleReorderCard = useCallback(
    (cardId: string, newIndex: number) => {
      if (isSpectator) return;
      // Find which hand group this card belongs to
      const player = gameState?.players.find((p) => p.id === gameState.localPlayerId);
      const handGroup = player?.groups.find((g) => g.name.toLowerCase() === 'hand');
      if (handGroup) {
        reorderHandCard(handGroup.id, cardId, newIndex);
      }
    },
    [isSpectator, reorderHandCard, gameState],
  );

  /** Touch drag: screen→table coordinate conversion ref, populated by GameBoard */
  const screenToTableCoordsRef = useRef<ScreenToTableCoordsFn | null>(null);

  /** Touch drag: drop card on table using screen coordinates */
  const handleTouchTableDrop = useCallback(
    (cardId: string, screenX: number, screenY: number) => {
      if (isSpectator) return;
      const convert = screenToTableCoordsRef.current;
      if (!convert) return;
      const { x, y } = convert(screenX, screenY);
      handleCardMoveToTable(cardId, x, y);
    },
    [isSpectator, handleCardMoveToTable]
  );

  /** Touch drag: drop card on a group */
  const handleTouchGroupDrop = useCallback(
    (cardId: string, groupId: string, sourceZone: string | null) => {
      if (isSpectator) return;
      // Same-zone hand drops are handled by HandZone's touch reorder effect
      if (sourceZone === groupId) return;
      handleCardMoveToGroup(cardId, groupId);
    },
    [isSpectator, handleCardMoveToGroup]
  );

  const handleLeave = useCallback(async () => {
    await leaveGame();
    navigate('lobby');
  }, [leaveGame, navigate]);

  const tableCards: Card[] = gameState?.table.cards ?? [];

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
    <TouchDragLayer onTableDrop={handleTouchTableDrop} onGroupDrop={handleTouchGroupDrop}>
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
            {gameState.activePlayer != null && gameState.activePlayer !== 0 && (() => {
              const activePlayer = gameState.players.find((p: Player) => p.id === gameState.activePlayer);
              return (
                <>
                  <div className="w-px h-4 bg-octgn-border/50" />
                  <span className="text-xs text-octgn-text-muted">
                    Active:{' '}
                    <span
                      className="font-medium"
                      style={{ color: readablePlayerColor(activePlayer?.color || '#f9fafb') }}
                    >
                      {activePlayer?.name ?? '?'}
                    </span>
                  </span>
                </>
              );
            })()}
            <div className="flex-1" />

            {!isSpectator && (
              <>
                {/* Deck actions cluster */}
                <div className="flex items-center gap-0.5 bg-white/[0.03] rounded-lg border border-octgn-border/20 p-0.5">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={handleLoadUserDeck}
                    loading={deckLoading === 'user'}
                    disabled={!!deckLoading && deckLoading !== 'user'}
                    icon={
                      <svg className="w-3.5 h-3.5" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round">
                        <rect x="3" y="4" width="10" height="10" rx="1.5" />
                        <path d="M5 4V3a1.5 1.5 0 011.5-1.5h5A1.5 1.5 0 0113 3v7" />
                        <path d="M6 8.5h4M6 11h2.5" />
                      </svg>
                    }
                  >
                    Load Deck
                  </Button>
                  {prebuiltDecksPath && (
                    <>
                      <div className="w-px h-4 bg-octgn-border/30" />
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={handleLoadPrebuiltDeck}
                        loading={deckLoading === 'prebuilt'}
                        disabled={!!deckLoading && deckLoading !== 'prebuilt'}
                        icon={
                          <svg className="w-3.5 h-3.5" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round">
                            <path d="M2.5 5L8 2l5.5 3L8 8 2.5 5z" />
                            <path d="M2.5 5v6L8 14l5.5-3V5" />
                            <path d="M8 8v6" />
                          </svg>
                        }
                      >
                        Pre-Built Deck
                      </Button>
                    </>
                  )}
                </div>

                <div className="w-px h-4 bg-octgn-border/30" />

                <Button variant="ghost" size="sm" onClick={() => nextTurn()}>
                  Next Turn
                </Button>
              </>
            )}
            <Button variant="ghost" size="sm" onClick={() => setChatOpen((v) => !v)}>
              Chat
            </Button>
            <Button variant="ghost" size="sm" onClick={handleLeave}>
              Leave
            </Button>
          </div>

          {/* Game Board — table area only */}
          <GameBoard
            tableCards={tableCards}
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
            useTwoSidedTable={gameState.useTwoSidedTable}
            isSpectator={isSpectator}
            screenToTableCoordsRef={screenToTableCoordsRef}
          />

          {/* Player Group Browser — bottom panel with tabs, groups, hand */}
          <PlayerGroupBrowser
            players={gameState.players}
            localPlayerId={gameState.localPlayerId}
            globalGroups={gameState.players.find((p) => p.id === 0)?.groups}
            isSpectator={isSpectator}
            selectedCardId={selectedCardId}
            onCardClick={handleCardClick}
            onCardContextMenu={handleCardContextMenu}
            onCardMoveToGroup={handleCardMoveToGroup}
            onReorderCard={handleReorderCard}
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
                const isTurnMsg = msg.isSystem && /^Turn \d+ — /.test(msg.message);

                if (isTurnMsg) {
                  const msgColor = msg.color || '#9ca3af';
                  return (
                    <div
                      key={msg.id}
                      className="leading-relaxed italic border-l-4 pl-2 py-0.5 my-1 rounded-r bg-octgn-surface/30"
                      style={{ borderColor: `${msgColor}99` }}
                    >
                      <span style={{ color: readablePlayerColor(msgColor) }}>
                        {msg.message}
                      </span>
                    </div>
                  );
                }

                return (
                  <div
                    key={msg.id}
                    className={clsx(
                      'leading-relaxed',
                      msg.isSystem ? 'italic' : 'text-octgn-text'
                    )}
                    style={msg.isSystem && msg.color
                      ? { color: readablePlayerColor(msg.color) }
                      : msg.isSystem ? { color: 'var(--color-octgn-text-dim)' } : undefined}
                  >
                    {!msg.isSystem && (
                      <span
                        className="font-semibold mr-1"
                        style={{ color: readablePlayerColor(senderColor) }}
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
    </TouchDragLayer>
    </DragDropProvider>
  );
};

export default GamePage;
