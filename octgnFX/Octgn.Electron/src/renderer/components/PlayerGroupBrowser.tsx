import React, { useState, useCallback, useMemo, useRef } from 'react';
import { clsx } from 'clsx';
import CardComponent from './CardComponent';
import PileViewer from './PileViewer';
import GroupStrip from './GroupStrip';
import HandZone from './HandZone';
import { useDragDrop } from './DragDropContext';
import { readablePlayerColor } from '../utils/player-colors';
import type { Card, Group, Player } from '../../shared/types';
import { GroupVisibility } from '../../shared/types';

// ─── Visibility helpers ─────────────────────────────────────────────

/**
 * Check if the local player can see cards in this group face-up.
 * If not, cards should be shown face-down (card backs).
 */
function canSeeGroupCards(
  group: Group,
  groupOwnerId: number,
  localPlayerId: number,
  isSpectator: boolean,
): boolean {
  if (isSpectator) return true;
  if (groupOwnerId === localPlayerId) return true;
  switch (group.visibility) {
    case GroupVisibility.Everybody:
      return true;
    case GroupVisibility.Owner:
    case GroupVisibility.Undefined:
    case GroupVisibility.Nobody:
      return false;
    default:
      return true;
  }
}

/**
 * Apply visibility to a group's cards: if the viewer can't see them,
 * return copies with faceUp forced to false.
 */
function applyCardVisibility(
  group: Group,
  groupOwnerId: number,
  localPlayerId: number,
  isSpectator: boolean,
): Group {
  if (canSeeGroupCards(group, groupOwnerId, localPlayerId, isSpectator)) {
    return group;
  }
  return {
    ...group,
    cards: group.cards.map((c) => ({ ...c, faceUp: false })),
  };
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
  onReorderCard?: (cardId: string, newIndex: number) => void;
}

const HAND_CARD_WIDTH = 80;
const HAND_CARD_HEIGHT = 112;

const PlayerGroupBrowser: React.FC<PlayerGroupBrowserProps> = ({
  players,
  localPlayerId,
  globalGroups,
  isSpectator,
  selectedCardId,
  onCardClick,
  onCardContextMenu,
  onCardMoveToGroup,
  onReorderCard,
}) => {
  const { dragState, isDragging, updateMousePosition } = useDragDrop();

  // Active players (non-spectator, non-global)
  const activePlayers = useMemo(
    () => players.filter((p) => !p.isSpectator && p.id !== 0),
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
  const isOwnTab = !isSpectator && (isGlobalTab || selectedTab === localPlayerId);
  const selectedPlayerId = selectedPlayer?.id ?? 0;

  // Apply card visibility to all groups
  const displayGroups = useMemo(() => {
    const raw = isGlobalTab ? (globalGroups ?? []) : (selectedPlayer?.groups ?? []);
    const ownerId = isGlobalTab ? 0 : selectedPlayerId;
    return raw.map((g) => applyCardVisibility(g, ownerId, localPlayerId, isSpectator));
  }, [isGlobalTab, globalGroups, selectedPlayer, selectedPlayerId, localPlayerId, isSpectator]);

  // Always separate hand from other groups for ALL players
  const handGroup = useMemo(
    () => displayGroups.find((g) => g.name.toLowerCase() === 'hand') ?? null,
    [displayGroups],
  );

  const sideGroups = useMemo(
    () => displayGroups.filter((g) => g.name.toLowerCase() !== 'hand'),
    [displayGroups],
  );

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

  // ─── Resize drag state ───────────────────────────────────────────
  const containerRef = useRef<HTMLDivElement>(null);
  const dragRef = useRef<{ startY: number; startH: number } | null>(null);

  const handleResizePointerDown = useCallback((e: React.PointerEvent) => {
    e.preventDefault();
    const el = containerRef.current;
    if (!el) return;
    dragRef.current = { startY: e.clientY, startH: el.offsetHeight };
    (e.target as HTMLElement).setPointerCapture(e.pointerId);
  }, []);

  const handleResizePointerMove = useCallback((e: React.PointerEvent) => {
    const d = dragRef.current;
    const el = containerRef.current;
    if (!d || !el) return;
    const newH = Math.max(100, d.startH - (e.clientY - d.startY));
    el.style.height = `${newH}px`;
  }, []);

  const handleResizePointerUp = useCallback(() => {
    dragRef.current = null;
  }, []);

  // ─── Counters for selected player ─────────────────────────────────
  const counters = selectedPlayer?.counters ?? [];

  // Track mouse position during drag so the ghost adorner follows the cursor
  // even when dragging over the bottom panel (not just the table).
  const handlePanelDragOver = useCallback(
    (e: React.DragEvent) => {
      if (isDragging) {
        updateMousePosition(e.clientX, e.clientY);
      }
    },
    [isDragging, updateMousePosition],
  );

  return (
    <>
      <div
        ref={containerRef}
        data-testid="player-group-browser"
        className={clsx(
          'border-t border-octgn-border/30 flex flex-col shrink-0',
          'bg-gradient-to-t from-octgn-bg/80 via-octgn-surface/60 to-octgn-surface/40',
          'backdrop-blur-md',
        )}
        style={{ height: 200 }}
        onDragOver={handlePanelDragOver}
      >
        {/* ── Resize handle (top edge) ──────────────────────────── */}
        <div
          className="h-1 shrink-0 cursor-ns-resize hover:bg-octgn-primary/30 active:bg-octgn-primary/50 transition-colors"
          onPointerDown={handleResizePointerDown}
          onPointerMove={handleResizePointerMove}
          onPointerUp={handleResizePointerUp}
          onPointerCancel={handleResizePointerUp}
        />

        {/* ── Tab bar ─────────────────────────────────────────────── */}
        <div className="flex items-center gap-0.5 px-2 sm:px-3 py-1 overflow-x-auto scrollbar-thin border-b border-octgn-border/20 shrink-0">
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
          <div className="flex items-center gap-3 px-3 sm:px-4 py-1 border-b border-octgn-border/10 overflow-x-auto shrink-0">
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

        {/* ── Scrollable content area ──────────────────────────────── */}
        <div className="flex-1 min-h-0 overflow-y-auto scrollbar-thin">
          {/* ── Group strip (all groups except hand) ───────────────── */}
          {sideGroups.length > 0 && (
            <GroupStrip
              groups={sideGroups}
              isOwn={isOwnTab}
              onPileClick={handlePileClick}
              onCardMoveToGroup={isOwnTab ? onCardMoveToGroup : undefined}
            />
          )}

          {/* ── Hand zone (always shown as fan for all players) ───── */}
          {handGroup && (
            <HandZone
              cards={handGroup.cards}
              handGroupId={handGroup.id}
              selectedCardId={selectedCardId}
              interactive={isOwnTab}
              onCardClick={onCardClick}
              onCardContextMenu={onCardContextMenu}
              onCardMoveToGroup={onCardMoveToGroup}
              onReorderCard={isOwnTab ? onReorderCard : undefined}
            />
          )}
        </div>
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

      {/* Drag Ghost Overlay — show card adorner following mouse during any drag */}
      {isDragging && dragState.draggingCardId && (() => {
        const allCards = [
          ...players.flatMap((p) => p.groups.flatMap((g) => g.cards)),
          ...(globalGroups ?? []).flatMap((g) => g.cards),
        ];
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
