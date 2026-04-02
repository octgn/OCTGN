import React, { useCallback, useEffect, useRef, useState } from 'react';
import { clsx } from 'clsx';
import CardComponent from './CardComponent';
import { useDragDrop, cardToDragInfo } from './DragDropContext';
import type { Card } from '../../shared/types';

const HAND_CARD_WIDTH = 80;
const HAND_CARD_HEIGHT = 112;

/** Gap (px) that opens between cards at the insertion point */
const REORDER_GAP = 36;

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

/** Given mouse clientX and card element refs, compute which index slot the card should be inserted at */
function computeInsertIndex(
  clientX: number,
  cardRefs: Map<string, HTMLDivElement>,
  cards: Card[],
  draggingCardId: string | null,
): number {
  if (cards.length === 0) return 0;

  const positions: { index: number; centerX: number }[] = [];
  for (let i = 0; i < cards.length; i++) {
    if (cards[i].id === draggingCardId) continue;
    const el = cardRefs.get(cards[i].id);
    if (!el) continue;
    const rect = el.getBoundingClientRect();
    positions.push({ index: i, centerX: rect.left + rect.width / 2 });
  }

  if (positions.length === 0) return 0;

  for (const pos of positions) {
    if (clientX < pos.centerX) return pos.index;
  }
  return cards.length;
}

/**
 * Compute per-card X offset so cards spread apart at the insertion point.
 * Cards before the gap shift left, cards after shift right.
 *
 * Both cardIndex and insertIndex are in original-array index space,
 * so we compare them directly (no visible-slot mapping needed).
 */
function spreadOffset(
  cardIndex: number,
  insertIndex: number | null,
  draggingCardId: string | null,
  cards: Card[],
): number {
  if (insertIndex === null || draggingCardId === null) return 0;
  if (cards[cardIndex]?.id === draggingCardId) return 0;

  if (cardIndex >= insertIndex) {
    return REORDER_GAP / 2;
  } else {
    return -REORDER_GAP / 2;
  }
}

export interface HandZoneProps {
  cards: Card[];
  handGroupId: string;
  selectedCardIds: Set<string>;
  /** Whether the user can interact with these cards (drag, click, context menu) */
  interactive: boolean;
  onCardClick: (card: Card) => void;
  onCardContextMenu: (e: React.MouseEvent, card: Card) => void;
  onCardMoveToGroup: (cardId: string, groupId: string) => void;
  /** Called when a card is reordered within the hand (local-only) */
  onReorderCard?: (cardId: string, newIndex: number) => void;
}

const HandZone: React.FC<HandZoneProps> = ({
  cards,
  handGroupId,
  selectedCardIds,
  interactive,
  onCardClick,
  onCardContextMenu,
  onCardMoveToGroup,
  onReorderCard,
}) => {
  const { dragState, startDrag, startTouchDrag, updateDropTarget, updateMousePosition, endDrag, isDragging } =
    useDragDrop();

  const cardRefs = useRef<Map<string, HTMLDivElement>>(new Map());
  const [insertIndex, setInsertIndex] = useState<number | null>(null);

  // Track the local drag info in a ref so it's immediately available
  // after handleCardDragStart — avoids the stale closure issue where
  // React hasn't re-rendered yet when the first dragOver fires.
  const localDragRef = useRef<{ cardId: string; sourceZone: string } | null>(null);

  const isSameZoneDrag = isDragging && dragState.sourceZone === handGroupId;

  const handleHandDragOver = useCallback(
    (e: React.DragEvent) => {
      if (!interactive) return;
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';
      updateDropTarget(handGroupId);
      updateMousePosition(e.clientX, e.clientY);

      // Read from localDragRef — this is set synchronously in handleCardDragStart
      // so it's immediately available, unlike dragState which requires a React re-render.
      const localDrag = localDragRef.current;
      if (localDrag && localDrag.sourceZone === handGroupId) {
        const idx = computeInsertIndex(e.clientX, cardRefs.current, cards, localDrag.cardId);
        setInsertIndex(idx);
      }
    },
    [interactive, handGroupId, updateDropTarget, updateMousePosition, cards],
  );

  // No dragLeave handler needed — insertIndex is cleared on drop/dragEnd,
  // and the indicator visibility is already gated by isDropTarget which
  // auto-hides when the cursor moves to a different drop zone.

  const handleHandDrop = useCallback(
    (e: React.DragEvent) => {
      if (!interactive) return;
      e.preventDefault();
      const cardId = e.dataTransfer.getData('application/octgn-card');
      const sourceZone = e.dataTransfer.getData('application/octgn-zone');
      if (!cardId) return;

      if (sourceZone === handGroupId && onReorderCard) {
        const idx = computeInsertIndex(e.clientX, cardRefs.current, cards, cardId);
        const oldIndex = cards.findIndex((c) => c.id === cardId);
        const adjustedIdx = oldIndex < idx ? idx - 1 : idx;
        onReorderCard(cardId, adjustedIdx);
      } else {
        // Support multi-card drop into hand
        const cardsJson = e.dataTransfer.getData('application/octgn-cards');
        const cardIds: string[] = cardsJson ? JSON.parse(cardsJson) : [];
        if (cardIds.length === 0) cardIds.push(cardId);
        for (const id of cardIds) {
          onCardMoveToGroup(id, handGroupId);
        }
      }
      setInsertIndex(null);
      endDrag();
    },
    [interactive, handGroupId, onCardMoveToGroup, onReorderCard, endDrag, cards],
  );

  const handleCardDragStart = useCallback(
    (card: Card, e: React.DragEvent) => {
      if (!interactive) return;
      // Set ref synchronously BEFORE startDrag so handleHandDragOver
      // can read it immediately — no waiting for React re-render.
      localDragRef.current = { cardId: card.id, sourceZone: handGroupId };
      startDrag(card.id, handGroupId, e, cardToDragInfo(card));
    },
    [interactive, startDrag, handGroupId],
  );

  const handleCardDragEnd = useCallback(() => {
    localDragRef.current = null;
    setInsertIndex(null);
    endDrag();
  }, [endDrag]);

  const handleCardTouchDragStart = useCallback(
    (card: Card, x: number, y: number, grabOffset: { x: number; y: number }) => {
      if (!interactive) return;
      // Set ref synchronously so useEffect can detect same-zone touch drag
      localDragRef.current = { cardId: card.id, sourceZone: handGroupId };
      startTouchDrag(card.id, handGroupId, x, y, cardToDragInfo(card), grabOffset);
    },
    [interactive, startTouchDrag, handGroupId],
  );

  const setCardRef = useCallback((id: string, el: HTMLDivElement | null) => {
    if (el) {
      cardRefs.current.set(id, el);
    } else {
      cardRefs.current.delete(id);
    }
  }, []);

  // ── Touch drag: compute insertIndex reactively ──────────────────────
  // HTML5 drag fires dragOver on HandZone directly, but touch drag events
  // are handled globally by TouchDragLayer. This effect watches dragState
  // to compute insertIndex when a touch drag hovers over this hand zone.
  //
  // pendingTouchReorder stores the reorder info that persists through the
  // drag-end state transition (touchInsertRef would be cleared too early).
  const pendingTouchReorder = useRef<{ cardId: string; insertIndex: number } | null>(null);

  useEffect(() => {
    if (!interactive || !isDragging || !dragState.isTouchDrag) {
      return;
    }
    if (dragState.dropTargetZone !== handGroupId || dragState.sourceZone !== handGroupId) {
      if (dragState.isTouchDrag) setInsertIndex(null);
      pendingTouchReorder.current = null;
      return;
    }

    const idx = computeInsertIndex(
      dragState.mousePosition.x, cardRefs.current, cards, dragState.draggingCardId,
    );
    setInsertIndex(idx);
    pendingTouchReorder.current = {
      cardId: dragState.draggingCardId!,
      insertIndex: idx,
    };
  }, [interactive, isDragging, dragState.isTouchDrag, dragState.dropTargetZone, dragState.sourceZone, dragState.mousePosition.x, dragState.draggingCardId, handGroupId, cards]);

  // ── Touch drag: handle same-zone drop (reorder) ────────────────────
  // When a touch drag ends while targeting this hand zone from the same zone,
  // we need to perform a reorder. We detect this by watching isDragging
  // transition from true→false while we have a pending touch reorder.
  const prevIsDraggingRef = useRef(false);
  useEffect(() => {
    const wasDragging = prevIsDraggingRef.current;
    prevIsDraggingRef.current = isDragging;

    if (wasDragging && !isDragging && pendingTouchReorder.current) {
      const { cardId, insertIndex: idx } = pendingTouchReorder.current;
      const oldIndex = cards.findIndex((c) => c.id === cardId);
      const adjustedIdx = oldIndex < idx ? idx - 1 : idx;
      if (onReorderCard) {
        onReorderCard(cardId, adjustedIdx);
      }
      pendingTouchReorder.current = null;
      localDragRef.current = null;
      setInsertIndex(null);
    }
  }, [isDragging, cards, onReorderCard]);

  const isDropTarget = interactive && isDragging && dragState.dropTargetZone === handGroupId;
  const showInsertIndicator = interactive && isSameZoneDrag && insertIndex !== null && isDropTarget;

  return (
    <div
      data-testid="hand-zone"
      data-drop-zone={handGroupId}
      className={clsx(
        'relative border-t transition-all duration-300',
        isDropTarget
          ? 'border-octgn-gold/20 bg-octgn-gold/[0.02]'
          : 'border-white/[0.05] bg-gradient-to-b from-transparent to-octgn-bg/40',
      )}
      onDragOver={interactive ? handleHandDragOver : undefined}
      onDrop={interactive ? handleHandDrop : undefined}
      style={{ minHeight: HAND_CARD_HEIGHT + 32 }}
    >
      {/* Cross-zone drop target outline */}
      {isDropTarget && !isSameZoneDrag && (
        <div className="absolute inset-1 rounded-xl border-2 border-dashed border-octgn-primary/25 pointer-events-none">
          <div className="absolute inset-0 rounded-xl bg-octgn-primary/[0.03]" />
        </div>
      )}

      <div className="flex items-end justify-center h-full py-2 px-4 overflow-visible">
        {cards.length === 0 && (
          <p className="text-xs text-octgn-text-dim/40 py-4 font-display tracking-wider">
            {interactive && isDragging ? 'Drop to add to hand' : interactive ? 'Your hand is empty' : 'No cards in hand'}
          </p>
        )}

        <div
          className="relative flex items-end justify-center"
          style={{ height: HAND_CARD_HEIGHT + 16 }}
        >
          {/* ── Insertion indicator ── */}
          {showInsertIndicator && (() => {
            const ind = computeIndicatorTransform(insertIndex, dragState.draggingCardId, cards);
            return (
            <div
              data-testid="hand-insert-indicator"
              className="absolute pointer-events-none z-50"
              style={{
                left: '50%',
                bottom: 8,
                height: HAND_CARD_HEIGHT - 16,
                transform: `translateX(${ind.x}px) translateY(${ind.y}px) translateX(-50%) rotate(${ind.rotate}deg)`,
                transformOrigin: 'bottom center',
                transition: 'transform 0.18s cubic-bezier(0.23, 1, 0.32, 1)',
              }}
            >
              {/* Wide diffuse glow — breathes in and out */}
              <div
                className="absolute inset-y-2 left-1/2 -translate-x-1/2"
                style={{
                  width: 32,
                  background: 'radial-gradient(ellipse 100% 90% at center, rgba(59,130,246,0.25) 0%, rgba(251,191,36,0.12) 40%, transparent 70%)',
                  filter: 'blur(6px)',
                  animation: 'slot-glow 1.8s ease-in-out infinite',
                }}
              />

              {/* Core line — bright white-gold with stacked glow shadows */}
              <div
                className="relative h-full mx-auto rounded-full"
                style={{
                  width: 2.5,
                  background: 'linear-gradient(to bottom, transparent 0%, rgba(251,191,36,0.6) 8%, rgba(255,255,255,0.95) 20%, rgba(251,191,36,1) 50%, rgba(255,255,255,0.95) 80%, rgba(251,191,36,0.6) 92%, transparent 100%)',
                  boxShadow: '0 0 4px 1px rgba(251,191,36,0.8), 0 0 10px 2px rgba(251,191,36,0.4), 0 0 20px 4px rgba(59,130,246,0.2)',
                  animation: 'slot-pulse 1.8s ease-in-out infinite',
                }}
              />

              {/* Traveling spark — moves up the line continuously */}
              <div
                className="absolute left-1/2 -translate-x-1/2 overflow-hidden rounded-full"
                style={{
                  width: 6,
                  top: '5%',
                  bottom: '5%',
                  maskImage: 'linear-gradient(to bottom, transparent 0%, white 10%, white 90%, transparent 100%)',
                }}
              >
                <div
                  className="absolute w-full"
                  style={{
                    height: '200%',
                    background: 'linear-gradient(to top, transparent 0%, transparent 42%, rgba(255,255,255,0.9) 48%, rgba(251,191,36,0.7) 52%, transparent 58%, transparent 100%)',
                    animation: 'slot-spark 1.4s linear infinite',
                  }}
                />
              </div>

              {/* Top cap — bright bloom dot */}
              <div
                className="absolute -top-1 left-1/2 -translate-x-1/2"
                style={{
                  width: 6,
                  height: 6,
                  borderRadius: '50%',
                  background: 'radial-gradient(circle, rgba(255,255,255,0.95) 0%, rgba(251,191,36,0.8) 40%, rgba(59,130,246,0.3) 70%, transparent 100%)',
                  boxShadow: '0 0 6px 2px rgba(251,191,36,0.6), 0 0 12px 3px rgba(59,130,246,0.2)',
                  animation: 'slot-pulse 1.8s ease-in-out infinite',
                }}
              />

              {/* Bottom cap — bright bloom dot */}
              <div
                className="absolute -bottom-1 left-1/2 -translate-x-1/2"
                style={{
                  width: 6,
                  height: 6,
                  borderRadius: '50%',
                  background: 'radial-gradient(circle, rgba(255,255,255,0.95) 0%, rgba(251,191,36,0.8) 40%, rgba(59,130,246,0.3) 70%, transparent 100%)',
                  boxShadow: '0 0 6px 2px rgba(251,191,36,0.6), 0 0 12px 3px rgba(59,130,246,0.2)',
                  animation: 'slot-pulse 1.8s ease-in-out infinite',
                }}
              />
            </div>
            );
          })()}

          {cards.map((card, index) => {
            const { x, y, rotate } = handCardTransform(index, cards.length);
            const isDraggingThis = interactive && isDragging && dragState.draggingCardId === card.id;

            // During same-zone reorder, spread cards apart at the insertion point
            const reorderOffset = (isSameZoneDrag && isDropTarget)
              ? spreadOffset(index, insertIndex, dragState.draggingCardId, cards)
              : 0;

            return (
              <div
                key={card.id}
                data-testid={`hand-card-${card.id}`}
                ref={(el) => setCardRef(card.id, el)}
                className={clsx(
                  'absolute',
                  isSameZoneDrag
                    ? 'transition-all duration-200 ease-out'
                    : 'transition-all duration-300 ease-out',
                )}
                style={{
                  transform: `translateX(${x + reorderOffset}px) translateY(${y}px) rotate(${rotate}deg)`,
                  zIndex: isDraggingThis ? 0 : index + 1,
                  transformOrigin: 'bottom center',
                  ...(isDraggingThis ? {
                    opacity: 0.3,
                    filter: 'saturate(0.3) brightness(0.7)',
                    transform: `translateX(${x}px) translateY(${y + 6}px) rotate(${rotate}deg) scale(0.92)`,
                  } : {}),
                }}
                onContextMenu={interactive ? (e) => {
                  e.preventDefault();
                  onCardContextMenu(e, card);
                } : undefined}
              >
                <CardComponent
                  card={card}
                  selected={interactive && selectedCardIds.has(card.id)}
                  onClick={interactive ? onCardClick : undefined}
                  onDragStart={interactive ? handleCardDragStart : undefined}
                  onDragEnd={interactive ? handleCardDragEnd : undefined}
                  onTouchDragStart={interactive ? handleCardTouchDragStart : undefined}
                  interactive={interactive}
                  style={{ width: HAND_CARD_WIDTH, height: HAND_CARD_HEIGHT }}
                />
              </div>
            );
          })}
        </div>
      </div>

      {/* Keyframes for the insertion indicator */}
      {showInsertIndicator && (
        <style>{`
          @keyframes slot-pulse {
            0%, 100% { opacity: 0.75; }
            50%      { opacity: 1; }
          }
          @keyframes slot-glow {
            0%, 100% { opacity: 0.5; transform: translateX(-50%) scaleX(0.8); }
            50%      { opacity: 1;   transform: translateX(-50%) scaleX(1.3); }
          }
          @keyframes slot-spark {
            0%   { transform: translateY(0); }
            100% { transform: translateY(-50%); }
          }
        `}</style>
      )}
    </div>
  );
};

/**
 * Compute indicator position using the SAME transform math as the card render loop.
 * Uses handCardTransform(origIndex, cards.length) + spreadOffset so it matches
 * exactly where cards actually appear on screen during a reorder drag.
 */
function computeIndicatorTransform(
  insertIndex: number,
  draggingCardId: string | null,
  cards: Card[],
): { x: number; y: number; rotate: number } {
  // Build actual screen positions of visible (non-dragged) cards
  const visible: { x: number; y: number; rotate: number; origIndex: number }[] = [];
  for (let i = 0; i < cards.length; i++) {
    if (cards[i].id === draggingCardId) continue;
    const base = handCardTransform(i, cards.length);
    const offset = spreadOffset(i, insertIndex, draggingCardId, cards);
    visible.push({
      x: base.x + offset,
      y: base.y,
      rotate: base.rotate,
      origIndex: i,
    });
  }

  if (visible.length === 0) return { x: 0, y: 0, rotate: 0 };

  // Map insertIndex (original array index) to visible slot
  let visibleInsert = 0;
  for (const v of visible) {
    if (v.origIndex < insertIndex) visibleInsert++;
  }

  if (visibleInsert === 0) {
    const first = visible[0];
    return {
      x: first.x - HAND_CARD_WIDTH / 2 - REORDER_GAP / 2,
      y: first.y,
      rotate: first.rotate,
    };
  }
  if (visibleInsert >= visible.length) {
    const last = visible[visible.length - 1];
    return {
      x: last.x + HAND_CARD_WIDTH / 2 + REORDER_GAP / 2,
      y: last.y,
      rotate: last.rotate,
    };
  }

  // Midpoint between the two flanking visible cards
  const before = visible[visibleInsert - 1];
  const after = visible[visibleInsert];
  return {
    x: (before.x + after.x) / 2,
    y: (before.y + after.y) / 2,
    rotate: (before.rotate + after.rotate) / 2,
  };
}

export { HAND_CARD_WIDTH, HAND_CARD_HEIGHT };
export default HandZone;
