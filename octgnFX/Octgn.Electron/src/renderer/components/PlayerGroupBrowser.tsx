import React, { useState, useCallback, useMemo } from 'react';
import { clsx } from 'clsx';
import CardComponent from './CardComponent';
import PileViewer from './PileViewer';
import { useDragDrop } from './DragDropContext';
import { readablePlayerColor } from '../utils/player-colors';
import type { Card, Group, Player } from '../../shared/types';

// ─── Hand arc layout helpers (moved from GameBoard) ─────────────────
const HAND_CARD_WIDTH = 80;
const HAND_CARD_HEIGHT = 112;

function handCardTransform(
  index: number,
  total: number,
): { x: number; y: number; rotate: number } {
  if (total <= 1) return { x: 0, y: 0, rotate: 0 };

  const maxSpread = Math.min(total * 55, 600);
  const maxArc = Math.min(total * 2.2, 18);
  const maxLift = Math.min(total * 1.2, 12);

  const t = total === 1 ? 0 : (index / (total - 1)) * 2 - 1;
  const x = t * (maxSpread / 2);
  const rotate = t * maxArc;
  const y = (1 - t * t) * -maxLift;

  return { x, y, rotate };
}

// ─── Props ──────────────────────────────────────────────────────────
export interface PlayerGroupBrowserProps {
  players: Player[];
  localPlayerId: number;
  globalGroups?: Group[];
  isSpectator: boolean;
  selectedCardId: string | null;
  onCardClick: (card: Card) => void;
  onCardContextMenu: (e: React.MouseEvent, card: Card) => void;
  onCardMoveToGroup: (cardId: string, groupId: string) => void;
}

const ZONE_HAND = 'hand';

const PlayerGroupBrowser: React.FC<PlayerGroupBrowserProps> = ({
  players,
  localPlayerId,
  globalGroups,
  isSpectator,
  selectedCardId,
  onCardClick,
  onCardContextMenu,
  onCardMoveToGroup,
}) => {
  const { dragState, startDrag, updateDropTarget, updateMousePosition, endDrag, isDragging } =
    useDragDrop();

  // Active players (non-spectator)
  const activePlayers = useMemo(
    () => players.filter((p) => !p.isSpectator),
    [players],
  );

  // Tab state: 'global' | player ID
  const [selectedTab, setSelectedTab] = useState<string | number>(
    isSpectator && activePlayers.length > 0
      ? activePlayers[0].id
      : localPlayerId,
  );

  // Pile viewer state
  const [viewingPile, setViewingPile] = useState<{
    group: Group;
    playerName: string;
    playerColor: string;
    isOwn: boolean;
  } | null>(null);

  // Resolve selected player/groups
  const isGlobalTab = selectedTab === 'global';
  const selectedPlayer = isGlobalTab
    ? null
    : activePlayers.find((p) => p.id === selectedTab);
  const isOwnTab = !isGlobalTab && selectedTab === localPlayerId && !isSpectator;

  const displayGroups = useMemo(() => {
    if (isGlobalTab) return globalGroups ?? [];
    return selectedPlayer?.groups ?? [];
  }, [isGlobalTab, globalGroups, selectedPlayer]);

  const handGroup = useMemo(
    () => (isOwnTab ? displayGroups.find((g) => g.name.toLowerCase() === 'hand') : null),
    [isOwnTab, displayGroups],
  );

  const sideGroups = useMemo(
    () =>
      isOwnTab
        ? displayGroups.filter((g) => g.name.toLowerCase() !== 'hand')
        : displayGroups,
    [isOwnTab, displayGroups],
  );

  const handCards = handGroup?.cards ?? [];

  // ─── Drag handlers for groups ──────────────────────────────────────
  const handleGroupDragOver = useCallback(
    (groupId: string) => (e: React.DragEvent) => {
      if (!isOwnTab) return;
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';
      updateDropTarget(groupId);
    },
    [isOwnTab, updateDropTarget],
  );

  const handleGroupDrop = useCallback(
    (groupId: string) => (e: React.DragEvent) => {
      if (!isOwnTab) return;
      e.preventDefault();
      const cardId = e.dataTransfer.getData('application/octgn-card');
      if (!cardId) return;
      onCardMoveToGroup(cardId, groupId);
      endDrag();
    },
    [isOwnTab, onCardMoveToGroup, endDrag],
  );

  // ─── Hand zone drag handlers ──────────────────────────────────────
  const handleHandDragOver = useCallback(
    (e: React.DragEvent) => {
      if (!isOwnTab) return;
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';
      updateDropTarget(ZONE_HAND);
      updateMousePosition(e.clientX, e.clientY);
    },
    [isOwnTab, updateDropTarget, updateMousePosition],
  );

  const handleHandDrop = useCallback(
    (e: React.DragEvent) => {
      if (!isOwnTab) return;
      e.preventDefault();
      const cardId = e.dataTransfer.getData('application/octgn-card');
      if (!cardId || !handGroup) return;
      onCardMoveToGroup(cardId, handGroup.id);
      endDrag();
    },
    [isOwnTab, handGroup, onCardMoveToGroup, endDrag],
  );

  // ─── Card drag start ──────────────────────────────────────────────
  const handleCardDragStart = useCallback(
    (zone: string) => (card: Card, e: React.DragEvent) => {
      if (!isOwnTab) return;
      startDrag(card.id, zone, e);
    },
    [isOwnTab, startDrag],
  );

  const handleCardDragEnd = useCallback(() => {
    endDrag();
  }, [endDrag]);

  // ─── Pile click handler ────────────────────────────────────────────
  const handlePileClick = useCallback(
    (group: Group) => {
      const pName = isGlobalTab ? 'Global' : selectedPlayer?.name ?? '';
      const pColor = isGlobalTab ? '#6b7280' : selectedPlayer?.color ?? '#6b7280';
      setViewingPile({
        group,
        playerName: pName,
        playerColor: pColor,
        isOwn: isOwnTab,
      });
    },
    [isGlobalTab, selectedPlayer, isOwnTab],
  );

  // ─── Counters for selected player ─────────────────────────────────
  const counters = selectedPlayer?.counters ?? [];

  return (
    <>
      <div
        data-testid="player-group-browser"
        className={clsx(
          'border-t border-octgn-border/30 flex flex-col shrink-0',
          'bg-gradient-to-t from-octgn-bg/80 via-octgn-surface/60 to-octgn-surface/40',
          'backdrop-blur-md',
        )}
      >
        {/* ── Tab bar ─────────────────────────────────────────────── */}
        <div className="flex items-center gap-0.5 px-2 sm:px-3 py-1 overflow-x-auto scrollbar-thin border-b border-octgn-border/20">
          {/* Global tab */}
          {globalGroups && globalGroups.length > 0 && (
            <button
              data-testid="tab-global"
              onClick={() => setSelectedTab('global')}
              className={clsx(
                'group relative flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold whitespace-nowrap',
                'transition-all duration-250 ease-out',
                isGlobalTab
                  ? 'bg-white/[0.08] text-octgn-text shadow-[0_0_10px_rgba(107,114,128,0.15),inset_0_1px_0_rgba(255,255,255,0.06)]'
                  : 'text-octgn-text-dim hover:text-octgn-text-muted hover:bg-white/[0.03]',
              )}
            >
              {isGlobalTab && (
                <div className="absolute inset-x-0 -bottom-[1px] h-[2px] bg-gradient-to-r from-transparent via-octgn-text-dim/50 to-transparent" />
              )}
              <svg
                className={clsx('w-3 h-3 transition-opacity duration-200', isGlobalTab ? 'opacity-70' : 'opacity-40 group-hover:opacity-50')}
                viewBox="0 0 16 16"
              >
                <circle cx="8" cy="8" r="5.5" stroke="currentColor" strokeWidth="1.2" fill="none" />
                <path d="M2.5 8h11M8 2.5c-1.8 1.8-1.8 3.5-1.8 5.5s0 3.7 1.8 5.5M8 2.5c1.8 1.8 1.8 3.5 1.8 5.5s0 3.7-1.8 5.5" stroke="currentColor" strokeWidth="0.8" fill="none" />
              </svg>
              <span>Shared</span>
            </button>
          )}

          {/* Player tabs */}
          {activePlayers.map((player) => {
            const isSelected = !isGlobalTab && selectedTab === player.id;
            const isLocal = player.id === localPlayerId;
            const pColor = player.color || '#6b7280';
            const readableColor = readablePlayerColor(pColor);

            return (
              <button
                key={player.id}
                data-testid={`tab-player-${player.id}`}
                onClick={() => setSelectedTab(player.id)}
                className={clsx(
                  'group relative flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold whitespace-nowrap',
                  'transition-all duration-250 ease-out',
                  isSelected
                    ? 'bg-white/[0.07] shadow-[inset_0_1px_0_rgba(255,255,255,0.05)]'
                    : 'text-octgn-text-dim hover:text-octgn-text-muted hover:bg-white/[0.03]',
                )}
              >
                {/* Active indicator bar */}
                {isSelected && (
                  <div
                    className="absolute inset-x-2 -bottom-[1px] h-[2px] rounded-full"
                    style={{
                      background: `linear-gradient(90deg, transparent, ${pColor}cc, transparent)`,
                      boxShadow: `0 0 6px ${pColor}40`,
                    }}
                  />
                )}

                {/* Player color dot */}
                <div
                  className={clsx(
                    'w-2 h-2 rounded-full shrink-0 transition-all duration-250',
                    isSelected && 'scale-110',
                  )}
                  style={{
                    backgroundColor: pColor,
                    boxShadow: isSelected ? `0 0 6px ${pColor}80` : undefined,
                  }}
                />
                <span style={isSelected ? { color: readableColor } : undefined}>
                  {player.name}
                  {isLocal && !isSpectator && (
                    <span className="ml-1 text-[9px] opacity-40 font-normal">(you)</span>
                  )}
                </span>
              </button>
            );
          })}
        </div>

        {/* ── Counters row ────────────────────────────────────────── */}
        {!isGlobalTab && counters.length > 0 && (
          <div className="flex items-center gap-3 px-3 sm:px-4 py-1 border-b border-octgn-border/10 overflow-x-auto">
            {counters.map((c) => (
              <div key={c.id} className="flex items-center gap-1.5 text-xs whitespace-nowrap">
                <span className="text-octgn-text-dim text-[10px] uppercase tracking-wider">{c.name}</span>
                <span className="font-mono font-bold text-octgn-text bg-octgn-surface/80 px-1.5 py-0.5 rounded border border-octgn-border/25 text-[11px]">
                  {c.value}
                </span>
              </div>
            ))}
          </div>
        )}

        {/* ── Group strip ─────────────────────────────────────────── */}
        <div className="flex items-start gap-1.5 sm:gap-2 px-2 sm:px-3 py-2 overflow-x-auto scrollbar-thin">
          {sideGroups.length === 0 && !handGroup && (
            <div className="flex items-center justify-center w-full py-3">
              <p className="text-xs text-octgn-text-dim/40 font-display tracking-widest uppercase">
                No groups
              </p>
            </div>
          )}

          {sideGroups.map((group) => {
            const isDropTarget = isDragging && dragState.dropTargetZone === group.id;
            const hasCards = group.cards.length > 0;

            return (
              <button
                key={group.id}
                data-testid={`pile-${group.id}`}
                className={clsx(
                  'group/pile relative flex flex-col items-center rounded-xl p-2 min-w-[72px] sm:min-w-[80px] shrink-0',
                  'border transition-all duration-200 ease-out',
                  isDropTarget
                    ? 'border-octgn-primary/60 bg-octgn-primary/10 shadow-[0_0_16px_rgba(59,130,246,0.25)] scale-[1.04]'
                    : [
                        'border-white/[0.06] bg-white/[0.02]',
                        'hover:bg-white/[0.05] hover:border-white/[0.12]',
                        'hover:shadow-[0_2px_12px_rgba(0,0,0,0.3)]',
                        'active:scale-[0.97]',
                      ],
                )}
                onClick={() => handlePileClick(group)}
                onDragOver={isOwnTab ? handleGroupDragOver(group.id) : undefined}
                onDrop={isOwnTab ? handleGroupDrop(group.id) : undefined}
              >
                {/* Group name */}
                <span className="text-[8px] sm:text-[9px] font-semibold text-octgn-text-dim uppercase tracking-wider mb-1.5 truncate w-full text-center transition-colors group-hover/pile:text-octgn-text-muted">
                  {group.name}
                </span>

                {/* Card thumbnail with stacking depth */}
                {hasCards ? (
                  <div className="relative mb-1">
                    {/* Back cards for stacking depth */}
                    {group.cards.length > 2 && (
                      <div
                        className="absolute w-[52px] sm:w-[56px] h-[72px] sm:h-[78px] rounded-lg border border-white/[0.04] bg-octgn-surface/30"
                        style={{ top: 4, left: 4 }}
                      />
                    )}
                    {group.cards.length > 1 && (
                      <div
                        className="absolute w-[52px] sm:w-[56px] h-[72px] sm:h-[78px] rounded-lg border border-white/[0.06] bg-octgn-surface/50"
                        style={{ top: 2, left: 2 }}
                      />
                    )}
                    {/* Top card */}
                    <div className="relative w-[52px] sm:w-[56px] h-[72px] sm:h-[78px] rounded-lg border border-octgn-border/15 overflow-hidden transition-transform duration-200 group-hover/pile:translate-y-[-2px]">
                      <CardComponent
                        card={group.cards[0]}
                        interactive={false}
                        style={{ width: '100%', height: '100%' }}
                      />
                    </div>
                  </div>
                ) : (
                  <div className="w-[52px] sm:w-[56px] h-[72px] sm:h-[78px] rounded-lg border border-dashed border-white/[0.08] flex items-center justify-center mb-1 bg-white/[0.01]">
                    <svg className="w-4 h-4 text-octgn-text-dim/20" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1">
                      <rect x="3" y="2" width="10" height="12" rx="1.5" />
                    </svg>
                  </div>
                )}

                {/* Card count badge */}
                <span className={clsx(
                  'text-[10px] font-mono font-bold transition-colors',
                  hasCards ? 'text-octgn-text-muted' : 'text-octgn-text-dim/40',
                )}>
                  {group.cards.length}
                </span>
              </button>
            );
          })}
        </div>

        {/* ── Hand zone (own player only, non-spectator) ──────────── */}
        {isOwnTab && (
          <div
            data-testid="hand-zone"
            className={clsx(
              'relative border-t transition-all duration-200',
              isDragging && dragState.dropTargetZone === ZONE_HAND
                ? 'border-octgn-primary/50 bg-octgn-primary/[0.04]'
                : 'border-white/[0.05] bg-gradient-to-b from-transparent to-octgn-bg/40',
            )}
            onDragOver={handleHandDragOver}
            onDrop={handleHandDrop}
            style={{ minHeight: HAND_CARD_HEIGHT + 32 }}
          >
            {isDragging && dragState.dropTargetZone === ZONE_HAND && (
              <div className="absolute inset-1 rounded-xl border-2 border-dashed border-octgn-primary/25 pointer-events-none">
                <div className="absolute inset-0 rounded-xl bg-octgn-primary/[0.03]" />
              </div>
            )}

            <div className="flex items-end justify-center h-full py-2 px-4 overflow-visible">
              {handCards.length === 0 && (
                <p className="text-xs text-octgn-text-dim/40 py-4 font-display tracking-wider">
                  {isDragging ? 'Drop to add to hand' : 'Your hand is empty'}
                </p>
              )}

              <div
                className="relative flex items-end justify-center"
                style={{ height: HAND_CARD_HEIGHT + 16 }}
              >
                {handCards.map((card, index) => {
                  const { x, y, rotate } = handCardTransform(index, handCards.length);
                  const isDraggingThis = isDragging && dragState.draggingCardId === card.id;

                  return (
                    <div
                      key={card.id}
                      className={clsx(
                        'absolute transition-all duration-300 ease-out',
                        isDraggingThis && 'opacity-30 scale-90',
                      )}
                      style={{
                        transform: `translateX(${x}px) translateY(${y}px) rotate(${rotate}deg)`,
                        zIndex: index,
                        transformOrigin: 'bottom center',
                      }}
                      onContextMenu={(e) => {
                        e.preventDefault();
                        onCardContextMenu(e, card);
                      }}
                    >
                      <CardComponent
                        card={card}
                        selected={card.id === selectedCardId}
                        onClick={onCardClick}
                        onDragStart={handleCardDragStart(ZONE_HAND)}
                        onDragEnd={handleCardDragEnd}
                        style={{ width: HAND_CARD_WIDTH, height: HAND_CARD_HEIGHT }}
                      />
                    </div>
                  );
                })}
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Pile Viewer overlay */}
      {viewingPile && (
        <PileViewer
          group={viewingPile.group}
          playerName={viewingPile.playerName}
          playerColor={viewingPile.playerColor}
          isOwn={viewingPile.isOwn}
          onClose={() => setViewingPile(null)}
          onCardClick={onCardClick}
          onCardContextMenu={onCardContextMenu}
        />
      )}

      {/* Drag Ghost Overlay */}
      {isOwnTab && isDragging && dragState.draggingCardId && (() => {
        const allCards = players
          .flatMap((p) => p.groups.flatMap((g) => g.cards));
        const draggingCard = allCards.find((c) => c.id === dragState.draggingCardId);
        if (!draggingCard) return null;
        return (
          <div
            className="fixed pointer-events-none z-[9999]"
            style={{
              left: dragState.mousePosition.x - HAND_CARD_WIDTH / 2,
              top: dragState.mousePosition.y - HAND_CARD_HEIGHT / 2,
              opacity: 0.85,
              transform: 'rotate(-4deg) scale(1.05)',
              filter: 'drop-shadow(0 8px 24px rgba(0,0,0,0.5))',
            }}
          >
            <CardComponent
              card={draggingCard}
              interactive={false}
              style={{ width: HAND_CARD_WIDTH, height: HAND_CARD_HEIGHT }}
            />
          </div>
        );
      })()}
    </>
  );
};

export default PlayerGroupBrowser;
