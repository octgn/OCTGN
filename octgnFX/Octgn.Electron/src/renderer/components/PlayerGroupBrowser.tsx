import React, { useState, useCallback, useMemo } from 'react';
import { clsx } from 'clsx';
import CardComponent from './CardComponent';
import PileViewer from './PileViewer';
import GroupStrip from './GroupStrip';
import HandZone from './HandZone';
import { useDragDrop } from './DragDropContext';
import { readablePlayerColor } from '../utils/player-colors';
import type { Card, Group, Player } from '../../shared/types';
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
}) => {
  const { dragState, isDragging } = useDragDrop();

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
        <GroupStrip
          groups={sideGroups}
          isOwn={isOwnTab}
          onPileClick={handlePileClick}
          onCardMoveToGroup={isOwnTab ? onCardMoveToGroup : undefined}
        />

        {/* ── Hand zone (own player only, non-spectator) ──────────── */}
        {isOwnTab && handGroup && (
          <HandZone
            cards={handGroup.cards}
            handGroupId={handGroup.id}
            selectedCardId={selectedCardId}
            onCardClick={onCardClick}
            onCardContextMenu={onCardContextMenu}
            onCardMoveToGroup={onCardMoveToGroup}
          />
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
