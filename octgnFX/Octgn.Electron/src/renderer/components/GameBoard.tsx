import React, { useCallback, useRef, useMemo } from 'react';
import { clsx } from 'clsx';
import CardComponent from './CardComponent';
import { useDragDrop } from './DragDropContext';
import type { Card, Group } from '../../shared/types';

// ─── Zone identifiers ────────────────────────────────────────────────
const ZONE_TABLE = 'table';
const ZONE_HAND = 'hand';

// ─── Hand arc layout helpers ─────────────────────────────────────────
const HAND_CARD_WIDTH = 80;
const HAND_CARD_HEIGHT = 112;

/** Returns transform values for a card in a fan/arc hand layout */
function handCardTransform(
  index: number,
  total: number
): { x: number; y: number; rotate: number } {
  if (total <= 1) return { x: 0, y: 0, rotate: 0 };

  const maxSpread = Math.min(total * 55, 600);
  const maxArc = Math.min(total * 2.2, 18);
  const maxLift = Math.min(total * 1.2, 12);

  // Normalized position: -1 to 1
  const t = total === 1 ? 0 : (index / (total - 1)) * 2 - 1;

  const x = t * (maxSpread / 2);
  const rotate = t * maxArc;
  // Parabolic arc: cards in center are higher
  const y = (1 - t * t) * -maxLift;

  return { x, y, rotate };
}

// ─── Props ───────────────────────────────────────────────────────────
interface GameBoardProps {
  tableCards: Card[];
  handCards: Card[];
  groups?: Group[];
  selectedCardId: string | null;
  boardImageUrl?: string;
  onCardClick: (card: Card) => void;
  onCardContextMenu: (e: React.MouseEvent, card: Card) => void;
  onCardMoveToTable: (cardId: string, x: number, y: number) => void;
  onCardMoveToGroup: (cardId: string, groupId: string) => void;
}

const GameBoard: React.FC<GameBoardProps> = ({
  tableCards,
  handCards,
  groups = [],
  selectedCardId,
  boardImageUrl,
  onCardClick,
  onCardContextMenu,
  onCardMoveToTable,
  onCardMoveToGroup,
}) => {
  const tableRef = useRef<HTMLDivElement>(null);
  const { dragState, startDrag, updateDropTarget, updateMousePosition, endDrag, isDragging } =
    useDragDrop();

  // ─── Table zone drag handlers ──────────────────────────────────────
  const handleTableDragOver = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';
      updateDropTarget(ZONE_TABLE);
      updateMousePosition(e.clientX, e.clientY);
    },
    [updateDropTarget, updateMousePosition]
  );

  const handleTableDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      const cardId = e.dataTransfer.getData('application/octgn-card');
      if (!cardId || !tableRef.current) return;

      const rect = tableRef.current.getBoundingClientRect();
      const x = e.clientX - rect.left;
      const y = e.clientY - rect.top;

      onCardMoveToTable(cardId, x, y);
      endDrag();
    },
    [onCardMoveToTable, endDrag]
  );

  const handleTableDragLeave = useCallback(
    (e: React.DragEvent) => {
      // Only clear if truly leaving the table zone
      if (!tableRef.current?.contains(e.relatedTarget as Node)) {
        updateDropTarget(null);
      }
    },
    [updateDropTarget]
  );

  // ─── Hand zone drag handlers ───────────────────────────────────────
  const handleHandDragOver = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';
      updateDropTarget(ZONE_HAND);
      updateMousePosition(e.clientX, e.clientY);
    },
    [updateDropTarget, updateMousePosition]
  );

  const handleHandDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      const cardId = e.dataTransfer.getData('application/octgn-card');
      if (!cardId) return;
      // Moving to hand = moving to the hand group
      const handGroup = groups.find(
        (g) => g.name.toLowerCase() === 'hand'
      );
      if (handGroup) {
        onCardMoveToGroup(cardId, handGroup.id);
      }
      endDrag();
    },
    [groups, onCardMoveToGroup, endDrag]
  );

  // ─── Group zone drag handlers ──────────────────────────────────────
  const handleGroupDragOver = useCallback(
    (groupId: string) => (e: React.DragEvent) => {
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';
      updateDropTarget(groupId);
    },
    [updateDropTarget]
  );

  const handleGroupDrop = useCallback(
    (groupId: string) => (e: React.DragEvent) => {
      e.preventDefault();
      const cardId = e.dataTransfer.getData('application/octgn-card');
      if (!cardId) return;
      onCardMoveToGroup(cardId, groupId);
      endDrag();
    },
    [onCardMoveToGroup, endDrag]
  );

  // ─── Card event wrappers ───────────────────────────────────────────
  const handleCardDragStart = useCallback(
    (zone: string) => (card: Card, e: React.DragEvent) => {
      startDrag(card.id, zone, e);
    },
    [startDrag]
  );

  const handleCardDragEnd = useCallback(() => {
    endDrag();
  }, [endDrag]);

  // ─── Dragging card ghost ───────────────────────────────────────────
  const draggingCard = useMemo(() => {
    if (!dragState.draggingCardId) return null;
    return (
      [...tableCards, ...handCards].find(
        (c) => c.id === dragState.draggingCardId
      ) ?? null
    );
  }, [dragState.draggingCardId, tableCards, handCards]);

  // Non-hand groups (deck, discard, etc.)
  const sideGroups = useMemo(
    () => groups.filter((g) => g.name.toLowerCase() !== 'hand'),
    [groups]
  );

  return (
    <div className="flex-1 flex flex-col min-h-0 relative">
      {/* ── Table / Play Area ───────────────────────────────────────── */}
      <div
        ref={tableRef}
        className={clsx(
          'flex-1 relative overflow-hidden transition-colors duration-200',
          isDragging &&
            dragState.dropTargetZone === ZONE_TABLE &&
            'bg-octgn-primary/5',
          isDragging &&
            dragState.dropTargetZone !== ZONE_TABLE &&
            'bg-transparent'
        )}
        onDragOver={handleTableDragOver}
        onDrop={handleTableDrop}
        onDragLeave={handleTableDragLeave}
      >
        {/* Board background image */}
        {boardImageUrl && (
          <img
            src={boardImageUrl}
            alt=""
            className="absolute inset-0 w-full h-full object-contain opacity-20 pointer-events-none select-none"
          />
        )}

        {/* Subtle grid pattern */}
        <div
          className="absolute inset-0 pointer-events-none opacity-[0.03]"
          style={{
            backgroundImage:
              'linear-gradient(rgba(59,130,246,0.3) 1px, transparent 1px), linear-gradient(90deg, rgba(59,130,246,0.3) 1px, transparent 1px)',
            backgroundSize: '40px 40px',
          }}
        />

        {/* Drop zone highlight ring */}
        {isDragging && dragState.dropTargetZone === ZONE_TABLE && (
          <div className="absolute inset-2 rounded-xl border-2 border-dashed border-octgn-primary/30 pointer-events-none transition-opacity duration-200 animate-pulse">
            <div className="absolute inset-0 rounded-xl bg-octgn-primary/5" />
          </div>
        )}

        {/* Table cards */}
        <div className="absolute inset-0 p-4">
          {tableCards.map((card) => (
            <div
              key={card.id}
              className={clsx(
                'absolute transition-all duration-200',
                isDragging &&
                  dragState.draggingCardId === card.id &&
                  'opacity-30 scale-95'
              )}
              style={{ left: card.position.x, top: card.position.y }}
            >
              <CardComponent
                card={card}
                selected={card.id === selectedCardId}
                onClick={onCardClick}
                onContextMenu={(c, e) => onCardContextMenu(e, c)}
                onDragStart={handleCardDragStart(ZONE_TABLE)}
                onDragEnd={handleCardDragEnd}
              />
            </div>
          ))}

          {tableCards.length === 0 && !isDragging && (
            <div className="flex items-center justify-center h-full">
              <p className="text-octgn-text-dim/40 font-display text-sm tracking-widest uppercase">
                Game Table
              </p>
            </div>
          )}

          {tableCards.length === 0 && isDragging && (
            <div className="flex items-center justify-center h-full">
              <p className="text-octgn-primary/40 font-display text-sm tracking-widest uppercase animate-pulse">
                Drop card here
              </p>
            </div>
          )}
        </div>

        {/* Side groups (deck/discard/etc) stacked on the right */}
        {sideGroups.length > 0 && (
          <div className="absolute top-3 right-3 flex flex-col gap-2 z-10">
            {sideGroups.map((group) => (
              <div
                key={group.id}
                className={clsx(
                  'w-[72px] rounded-lg border transition-all duration-200 p-1.5',
                  isDragging && dragState.dropTargetZone === group.id
                    ? 'border-octgn-primary/60 bg-octgn-primary/10 shadow-[0_0_12px_rgba(59,130,246,0.3)]'
                    : 'border-octgn-border/30 bg-octgn-surface/40 backdrop-blur-sm'
                )}
                onDragOver={handleGroupDragOver(group.id)}
                onDrop={handleGroupDrop(group.id)}
              >
                <div className="text-[8px] font-semibold text-octgn-text-dim uppercase tracking-wider mb-1 truncate text-center">
                  {group.name}
                </div>
                <div className="text-[10px] text-octgn-text-muted text-center font-mono">
                  {group.cards.length}
                </div>
                {/* Top card preview */}
                {group.cards.length > 0 && (
                  <div className="mt-1 mx-auto w-[56px] h-[78px] rounded border border-octgn-border/20 overflow-hidden">
                    <CardComponent
                      card={group.cards[0]}
                      interactive={false}
                      style={{ width: 56, height: 78 }}
                    />
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      {/* ── Hand Zone ───────────────────────────────────────────────── */}
      <div
        className={clsx(
          'relative border-t transition-all duration-200',
          isDragging && dragState.dropTargetZone === ZONE_HAND
            ? 'border-octgn-primary/50 bg-octgn-primary/5'
            : 'border-octgn-border/30 bg-octgn-surface/40',
          'backdrop-blur-sm'
        )}
        onDragOver={handleHandDragOver}
        onDrop={handleHandDrop}
        style={{ minHeight: HAND_CARD_HEIGHT + 32 }}
      >
        {/* Drop highlight for hand */}
        {isDragging && dragState.dropTargetZone === ZONE_HAND && (
          <div className="absolute inset-1 rounded-lg border-2 border-dashed border-octgn-primary/30 pointer-events-none" />
        )}

        {/* Fan/arc hand layout */}
        <div className="flex items-end justify-center h-full py-2 px-4 overflow-visible">
          {handCards.length === 0 && (
            <p className="text-xs text-octgn-text-dim py-4">
              {isDragging ? 'Drop to add to hand' : 'Your hand is empty'}
            </p>
          )}

          <div className="relative flex items-end justify-center" style={{ height: HAND_CARD_HEIGHT + 16 }}>
            {handCards.map((card, index) => {
              const { x, y, rotate } = handCardTransform(
                index,
                handCards.length
              );
              const isDraggingThis =
                isDragging && dragState.draggingCardId === card.id;

              return (
                <div
                  key={card.id}
                  className={clsx(
                    'absolute transition-all duration-300 ease-out',
                    isDraggingThis && 'opacity-30 scale-90'
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

      {/* ── Drag Ghost Overlay ──────────────────────────────────────── */}
      {isDragging && draggingCard && (
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
      )}
    </div>
  );
};

export default GameBoard;
