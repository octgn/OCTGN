import React, { useCallback } from 'react';
import { clsx } from 'clsx';
import CardComponent from './CardComponent';
import { useDragDrop, cardToDragInfo } from './DragDropContext';
import type { Card, Group } from '../../shared/types';

export interface PileThumbnailProps {
  group: Group;
  isOwn: boolean;
  onPileClick: (group: Group) => void;
  onGroupContextMenu?: (e: React.MouseEvent, group: Group) => void;
  onCardMoveToGroup?: (cardId: string, groupId: string) => void;
}

const PileThumbnail: React.FC<PileThumbnailProps> = ({
  group,
  isOwn,
  onPileClick,
  onGroupContextMenu,
  onCardMoveToGroup,
}) => {
  const { dragState, startDrag, startTouchDrag, updateDropTarget, endDrag, isDragging } = useDragDrop();

  const isDropTarget = isDragging && dragState.dropTargetZone === group.id;
  const hasCards = group.cards.length > 0;
  const topCard = hasCards ? group.cards[0] : null;

  // Drag the top card out of the pile to the table
  const handleTopCardDragStart = useCallback(
    (card: Card, e: React.DragEvent) => {
      if (!isOwn) return;
      e.stopPropagation();
      startDrag(card.id, `pile:${group.id}`, e, cardToDragInfo(card));
    },
    [isOwn, startDrag, group.id],
  );

  const handleTopCardDragEnd = useCallback(() => {
    endDrag();
  }, [endDrag]);

  const handleTopCardTouchDragStart = useCallback(
    (card: Card, x: number, y: number) => {
      if (!isOwn) return;
      startTouchDrag(card.id, `pile:${group.id}`, x, y, cardToDragInfo(card));
    },
    [isOwn, startTouchDrag, group.id],
  );

  const handleDragOver = useCallback(
    (e: React.DragEvent) => {
      if (!isOwn) return;
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';
      updateDropTarget(group.id);
    },
    [isOwn, updateDropTarget, group.id],
  );

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      if (!isOwn || !onCardMoveToGroup) return;
      e.preventDefault();
      const cardId = e.dataTransfer.getData('application/octgn-card');
      if (!cardId) return;
      onCardMoveToGroup(cardId, group.id);
      endDrag();
    },
    [isOwn, onCardMoveToGroup, endDrag, group.id],
  );

  return (
    <div
      data-testid={`pile-${group.id}`}
      role="button"
      tabIndex={0}
      className={clsx(
        'group/pile relative flex flex-col items-center rounded-xl p-2 min-w-[72px] sm:min-w-[80px] shrink-0 cursor-pointer select-none',
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
      data-drop-zone={group.id}
      onClick={() => onPileClick(group)}
      onContextMenu={onGroupContextMenu ? (e) => { e.preventDefault(); onGroupContextMenu(e, group); } : undefined}
      onDragOver={isOwn ? handleDragOver : undefined}
      onDrop={isOwn ? handleDrop : undefined}
    >
      {/* Group name */}
      <span className="text-[8px] sm:text-[9px] font-semibold text-octgn-text-dim uppercase tracking-wider mb-1.5 truncate w-full text-center transition-colors group-hover/pile:text-octgn-text-muted">
        {group.name}
      </span>

      {/* Card thumbnail with stacking depth */}
      {hasCards ? (
        <div className="relative mb-1">
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
          <div className="relative w-[52px] sm:w-[56px] h-[72px] sm:h-[78px] rounded-lg border border-octgn-border/15 overflow-hidden transition-transform duration-200 group-hover/pile:translate-y-[-2px]">
            <CardComponent
              card={topCard!}
              interactive={isOwn}
              onDragStart={isOwn ? handleTopCardDragStart : undefined}
              onDragEnd={isOwn ? handleTopCardDragEnd : undefined}
              onTouchDragStart={isOwn ? handleTopCardTouchDragStart : undefined}
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
    </div>
  );
};

export default PileThumbnail;
