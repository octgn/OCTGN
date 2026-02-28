import React, { useCallback } from 'react';
import { clsx } from 'clsx';
import CardComponent from './CardComponent';
import { useDragDrop } from './DragDropContext';
import type { Card } from '../../shared/types';

const HAND_CARD_WIDTH = 80;
const HAND_CARD_HEIGHT = 112;
const ZONE_HAND = 'hand';

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

export interface HandZoneProps {
  cards: Card[];
  handGroupId: string;
  selectedCardId: string | null;
  onCardClick: (card: Card) => void;
  onCardContextMenu: (e: React.MouseEvent, card: Card) => void;
  onCardMoveToGroup: (cardId: string, groupId: string) => void;
}

const HandZone: React.FC<HandZoneProps> = ({
  cards,
  handGroupId,
  selectedCardId,
  onCardClick,
  onCardContextMenu,
  onCardMoveToGroup,
}) => {
  const { dragState, startDrag, updateDropTarget, updateMousePosition, endDrag, isDragging } =
    useDragDrop();

  const handleHandDragOver = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';
      updateDropTarget(ZONE_HAND);
      updateMousePosition(e.clientX, e.clientY);
    },
    [updateDropTarget, updateMousePosition],
  );

  const handleHandDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      const cardId = e.dataTransfer.getData('application/octgn-card');
      if (!cardId) return;
      onCardMoveToGroup(cardId, handGroupId);
      endDrag();
    },
    [handGroupId, onCardMoveToGroup, endDrag],
  );

  const handleCardDragStart = useCallback(
    (card: Card, e: React.DragEvent) => {
      startDrag(card.id, ZONE_HAND, e);
    },
    [startDrag],
  );

  const handleCardDragEnd = useCallback(() => {
    endDrag();
  }, [endDrag]);

  return (
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
        {cards.length === 0 && (
          <p className="text-xs text-octgn-text-dim/40 py-4 font-display tracking-wider">
            {isDragging ? 'Drop to add to hand' : 'Your hand is empty'}
          </p>
        )}

        <div
          className="relative flex items-end justify-center"
          style={{ height: HAND_CARD_HEIGHT + 16 }}
        >
          {cards.map((card, index) => {
            const { x, y, rotate } = handCardTransform(index, cards.length);
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
                  onDragStart={handleCardDragStart}
                  onDragEnd={handleCardDragEnd}
                  style={{ width: HAND_CARD_WIDTH, height: HAND_CARD_HEIGHT }}
                />
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};

export { HAND_CARD_WIDTH, HAND_CARD_HEIGHT };
export default HandZone;
