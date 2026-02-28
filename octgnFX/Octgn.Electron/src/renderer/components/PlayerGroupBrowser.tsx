import React, { useState, useCallback, useMemo } from 'react';
import { clsx } from 'clsx';
import CardComponent from './CardComponent';
import PileViewer from './PileViewer';
import PlayerRow from './PlayerRow';
import { useDragDrop } from './DragDropContext';
import type { Card, Group, Player } from '../../shared/types';
import { GroupVisibility } from '../../shared/types';

// ─── Visibility filtering ──────────────────────────────────────────
function filterVisibleGroups(
  groups: Group[],
  playerId: number,
  localPlayerId: number,
  isSpectator: boolean,
): Group[] {
  if (isSpectator) return groups;
  return groups.filter((g) => {
    switch (g.visibility) {
      case GroupVisibility.Everybody:
        return true;
      case GroupVisibility.Owner:
      case GroupVisibility.Undefined:
        return playerId === localPlayerId;
      case GroupVisibility.Nobody:
        return false;
      default:
        return true;
    }
  });
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

  // Active (non-spectator, non-global) players
  const activePlayers = useMemo(
    () => players.filter((p) => !p.isSpectator && p.id !== 0),
    [players],
  );

  // Default expanded: local player (or first active for spectator)
  const defaultExpanded = useMemo(() => {
    if (isSpectator && activePlayers.length > 0) {
      return new Set<number | 'global'>([activePlayers[0].id]);
    }
    return new Set<number | 'global'>([localPlayerId]);
  }, []);

  const [expandedPlayers, setExpandedPlayers] = useState<Set<number | 'global'>>(defaultExpanded);

  const toggleExpand = useCallback((id: number | 'global') => {
    setExpandedPlayers((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }, []);

  // Pile viewer state
  const [viewingPile, setViewingPile] = useState<{
    group: Group;
    playerName: string;
    playerColor: string;
    isOwn: boolean;
  } | null>(null);

  const handlePileClick = useCallback(
    (group: Group, player: Player, isOwn: boolean) => {
      const isGlobal = player.id === 0;
      setViewingPile({
        group,
        playerName: isGlobal ? 'Shared' : player.name,
        playerColor: isGlobal ? '#6b7280' : player.color,
        isOwn,
      });
    },
    [],
  );

  // Synthetic global player
  const globalPlayer = useMemo((): Player | null => {
    if (!globalGroups || globalGroups.length === 0) return null;
    return {
      id: 0,
      name: 'Shared',
      color: '#6b7280',
      isHost: false,
      isSpectator: false,
      groups: globalGroups,
      counters: [],
      globalVariables: {},
    };
  }, [globalGroups]);

  return (
    <>
      <div
        data-testid="player-group-browser"
        className={clsx(
          'border-t border-octgn-border/30 flex flex-col shrink-0',
          'bg-gradient-to-t from-octgn-bg/90 via-octgn-surface/70 to-octgn-surface/50',
          'backdrop-blur-md',
          'max-h-[50vh] overflow-y-auto scrollbar-thin',
        )}
      >
        {/* Top edge accent line */}
        <div className="h-px bg-gradient-to-r from-transparent via-white/[0.06] to-transparent" />

        {/* Global player row */}
        {globalPlayer && (
          <PlayerRow
            player={globalPlayer}
            isLocal={false}
            isExpanded={expandedPlayers.has('global')}
            isSpectator={isSpectator}
            visibleGroups={filterVisibleGroups(globalPlayer.groups, 0, localPlayerId, isSpectator)}
            onToggleExpand={() => toggleExpand('global')}
            onPileClick={(group) => handlePileClick(group, globalPlayer, false)}
            selectedCardId={selectedCardId}
            onCardClick={onCardClick}
            onCardContextMenu={onCardContextMenu}
            onCardMoveToGroup={onCardMoveToGroup}
          />
        )}

        {/* Player rows */}
        {activePlayers.map((player) => {
          const isLocal = player.id === localPlayerId;
          const isOwnRow = isLocal && !isSpectator;
          const visibleGroups = filterVisibleGroups(
            player.groups,
            player.id,
            localPlayerId,
            isSpectator,
          );

          return (
            <PlayerRow
              key={player.id}
              player={player}
              isLocal={isLocal}
              isExpanded={expandedPlayers.has(player.id)}
              isSpectator={isSpectator}
              visibleGroups={visibleGroups}
              onToggleExpand={() => toggleExpand(player.id)}
              onPileClick={(group) => handlePileClick(group, player, isOwnRow)}
              selectedCardId={selectedCardId}
              onCardClick={onCardClick}
              onCardContextMenu={onCardContextMenu}
              onCardMoveToGroup={onCardMoveToGroup}
            />
          );
        })}
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
      {isDragging && dragState.draggingCardId && (() => {
        const allCards = players.flatMap((p) => p.groups.flatMap((g) => g.cards));
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
