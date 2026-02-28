import React, { useMemo } from 'react';
import PlayerRowHeader from './PlayerRowHeader';
import GroupStrip from './GroupStrip';
import HandZone from './HandZone';
import type { Card, Group, Player } from '../../shared/types';

export interface PlayerRowProps {
  player: Player;
  isLocal: boolean;
  isExpanded: boolean;
  isSpectator: boolean;
  visibleGroups: Group[];
  onToggleExpand: () => void;
  onPileClick: (group: Group) => void;
  selectedCardId: string | null;
  onCardClick: (card: Card) => void;
  onCardContextMenu: (e: React.MouseEvent, card: Card) => void;
  onCardMoveToGroup: (cardId: string, groupId: string) => void;
}

const PlayerRow: React.FC<PlayerRowProps> = ({
  player,
  isLocal,
  isExpanded,
  isSpectator,
  visibleGroups,
  onToggleExpand,
  onPileClick,
  selectedCardId,
  onCardClick,
  onCardContextMenu,
  onCardMoveToGroup,
}) => {
  const isGlobal = player.id === 0;
  const isOwnRow = isLocal && !isSpectator;

  const handGroup = useMemo(
    () => (isOwnRow ? visibleGroups.find((g) => g.name.toLowerCase() === 'hand') : null),
    [isOwnRow, visibleGroups],
  );

  const sideGroups = useMemo(
    () =>
      isOwnRow
        ? visibleGroups.filter((g) => g.name.toLowerCase() !== 'hand')
        : visibleGroups,
    [isOwnRow, visibleGroups],
  );

  const handCards = handGroup?.cards ?? [];

  return (
    <div className="border-b border-white/[0.04] last:border-b-0">
      <PlayerRowHeader
        playerId={isGlobal ? 'global' : player.id}
        playerName={isGlobal ? 'Shared' : player.name}
        playerColor={isGlobal ? '#6b7280' : player.color}
        isLocal={isLocal}
        isSpectator={isSpectator}
        isExpanded={isExpanded}
        counters={player.counters}
        visibleGroups={visibleGroups}
        onToggle={onToggleExpand}
      />

      {isExpanded && (
        <div className="animate-expand-down overflow-hidden">
          {/* Expanded counter detail row */}
          {!isGlobal && player.counters.length > 0 && (
            <div className="flex items-center gap-3 px-4 sm:px-5 py-1.5 border-t border-white/[0.03] overflow-x-auto scrollbar-thin">
              {player.counters.map((c) => (
                <div key={c.id} className="flex items-center gap-1.5 text-xs whitespace-nowrap">
                  <span className="text-octgn-text-dim text-[10px] uppercase tracking-wider">{c.name}</span>
                  <span className="font-mono font-bold text-octgn-text bg-octgn-surface/80 px-1.5 py-0.5 rounded border border-octgn-border/25 text-[11px] tabular-nums">
                    {c.value}
                  </span>
                </div>
              ))}
            </div>
          )}

          {/* Group strip */}
          <GroupStrip
            groups={sideGroups}
            isOwn={isOwnRow}
            onPileClick={onPileClick}
            onCardMoveToGroup={isOwnRow ? onCardMoveToGroup : undefined}
          />

          {/* Hand zone — own player only */}
          {isOwnRow && handGroup && (
            <HandZone
              cards={handCards}
              handGroupId={handGroup.id}
              selectedCardId={selectedCardId}
              onCardClick={onCardClick}
              onCardContextMenu={onCardContextMenu}
              onCardMoveToGroup={onCardMoveToGroup}
            />
          )}
        </div>
      )}
    </div>
  );
};

export default PlayerRow;
