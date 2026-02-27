import React, { useCallback, useRef, useMemo, useEffect, useState } from 'react';
import { clsx } from 'clsx';
import CardComponent from './CardComponent';
import { useDragDrop } from './DragDropContext';
import { useTableTransform } from '../hooks/useTableTransform';
import { calculateTableScale } from '../utils/table-scaling';
import type { Card, Group, Player } from '../../shared/types';

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
  boardX?: number;
  boardY?: number;
  boardWidth?: number;
  boardHeight?: number;
  backgroundStyle?: 'stretch' | 'tile' | 'uniform' | 'uniformToFill';
  tableBackgroundUrl?: string;
  tableWidth?: number;
  tableHeight?: number;
  onCardClick: (card: Card) => void;
  onCardContextMenu: (e: React.MouseEvent, card: Card) => void;
  onCardMoveToTable: (cardId: string, x: number, y: number) => void;
  onCardMoveToGroup: (cardId: string, groupId: string) => void;
  isSpectator?: boolean;
  allPlayers?: Player[];
}

const GameBoard: React.FC<GameBoardProps> = ({
  tableCards,
  handCards,
  groups = [],
  selectedCardId,
  boardImageUrl,
  boardX = 0,
  boardY = 0,
  boardWidth,
  boardHeight,
  backgroundStyle,
  tableBackgroundUrl,
  tableWidth,
  tableHeight,
  onCardClick,
  onCardContextMenu,
  onCardMoveToTable,
  onCardMoveToGroup,
  isSpectator = false,
  allPlayers,
}) => {
  const tableRef = useRef<HTMLDivElement>(null);
  const { dragState, startDrag, updateDropTarget, updateMousePosition, endDrag, isDragging } =
    useDragDrop();

  // ─── Container size tracking for base scale ─────────────────────────
  const [containerSize, setContainerSize] = useState({ width: 0, height: 0 });

  useEffect(() => {
    const el = tableRef.current;
    if (!el) return;
    const ro = new ResizeObserver((entries) => {
      for (const entry of entries) {
        const { width, height } = entry.contentRect;
        setContainerSize({ width, height });
      }
    });
    ro.observe(el);
    return () => ro.disconnect();
  }, []);

  // ─── Base scale: map mm-space to screen-space ──────────────────────
  const baseScale = useMemo(
    () =>
      calculateTableScale(
        tableWidth ?? 0,
        tableHeight ?? 0,
        containerSize.width,
        containerSize.height,
      ),
    [tableWidth, tableHeight, containerSize.width, containerSize.height],
  );

  const hasTableDimensions = !!(tableWidth && tableWidth > 0 && tableHeight && tableHeight > 0);

  // ─── Zoom / Pan transform ────────────────────────────────────────────
  const {
    zoom,
    panX,
    panY,
    isPanning,
    handleWheel,
    handlePanStart,
    handlePanMove,
    handlePanEnd,
    resetView,
    setContainerRect,
    screenToTable,
  } = useTableTransform();

  // Track space key for space+click panning
  const spaceDownRef = useRef(false);

  // Keep container rect in sync for cursor-centered zoom
  useEffect(() => {
    const updateRect = () => {
      if (tableRef.current) {
        const r = tableRef.current.getBoundingClientRect();
        setContainerRect({ left: r.left, top: r.top, width: r.width, height: r.height });
      }
    };
    updateRect();
    window.addEventListener('resize', updateRect);
    return () => window.removeEventListener('resize', updateRect);
  }, [setContainerRect]);

  // Attach wheel handler as a native event (need { passive: false } to preventDefault)
  useEffect(() => {
    const el = tableRef.current;
    if (!el) return;
    const handler = (e: WheelEvent) => handleWheel(e);
    el.addEventListener('wheel', handler, { passive: false });
    return () => el.removeEventListener('wheel', handler);
  }, [handleWheel]);

  // Track space key for space+left-click pan
  useEffect(() => {
    const down = (e: KeyboardEvent) => {
      if (e.code === 'Space') spaceDownRef.current = true;
    };
    const up = (e: KeyboardEvent) => {
      if (e.code === 'Space') {
        spaceDownRef.current = false;
        handlePanEnd();
      }
    };
    window.addEventListener('keydown', down);
    window.addEventListener('keyup', up);
    return () => {
      window.removeEventListener('keydown', down);
      window.removeEventListener('keyup', up);
    };
  }, [handlePanEnd]);

  // ─── Pan via mouse (middle-click or space+left-click) ────────────────
  const handleTableMouseDown = useCallback(
    (e: React.MouseEvent) => {
      // Middle-click always starts pan
      if (e.button === 1) {
        e.preventDefault();
        handlePanStart(e.clientX, e.clientY);
        return;
      }
      // Space + left-click starts pan
      if (e.button === 0 && spaceDownRef.current) {
        e.preventDefault();
        handlePanStart(e.clientX, e.clientY);
      }
    },
    [handlePanStart]
  );

  const handleTableMouseMove = useCallback(
    (e: React.MouseEvent) => {
      if (isPanning) {
        handlePanMove(e.clientX, e.clientY);
      }
    },
    [isPanning, handlePanMove]
  );

  const handleTableMouseUp = useCallback(
    (e: React.MouseEvent) => {
      if (e.button === 1 || (e.button === 0 && isPanning)) {
        handlePanEnd();
      }
    },
    [isPanning, handlePanEnd]
  );

  // Also end pan if mouse leaves table zone while panning
  const handleTableMouseLeave = useCallback(() => {
    if (isPanning) handlePanEnd();
  }, [isPanning, handlePanEnd]);

  // ─── Table zone drag handlers ──────────────────────────────────────
  const handleTableDragOver = useCallback(
    (e: React.DragEvent) => {
      if (isSpectator) return;
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';
      updateDropTarget(ZONE_TABLE);
      updateMousePosition(e.clientX, e.clientY);
    },
    [updateDropTarget, updateMousePosition, isSpectator]
  );

  const handleTableDrop = useCallback(
    (e: React.DragEvent) => {
      if (isSpectator) return;
      e.preventDefault();
      const cardId = e.dataTransfer.getData('application/octgn-card');
      if (!cardId || !tableRef.current) return;

      // Convert screen drop position to zoom/pan content space
      const contentPos = screenToTable(e.clientX, e.clientY);

      // Then invert the base scale to get mm-space coordinates
      let x = contentPos.x;
      let y = contentPos.y;
      if (hasTableDimensions && baseScale.scale !== 0) {
        x = (x - baseScale.offsetX) / baseScale.scale;
        y = (y - baseScale.offsetY) / baseScale.scale;
        // Invert the origin shift: protocol uses center-based coords
        x -= (tableWidth ?? 0) / 2;
        y -= (tableHeight ?? 0) / 2;
      }

      onCardMoveToTable(cardId, x, y);
      endDrag();
    },
    [onCardMoveToTable, endDrag, isSpectator, screenToTable, hasTableDimensions, baseScale, tableWidth, tableHeight]
  );

  const handleTableDragLeave = useCallback(
    (e: React.DragEvent) => {
      if (!tableRef.current?.contains(e.relatedTarget as Node)) {
        updateDropTarget(null);
      }
    },
    [updateDropTarget]
  );

  // ─── Hand zone drag handlers ───────────────────────────────────────
  const handleHandDragOver = useCallback(
    (e: React.DragEvent) => {
      if (isSpectator) return;
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';
      updateDropTarget(ZONE_HAND);
      updateMousePosition(e.clientX, e.clientY);
    },
    [updateDropTarget, updateMousePosition, isSpectator]
  );

  const handleHandDrop = useCallback(
    (e: React.DragEvent) => {
      if (isSpectator) return;
      e.preventDefault();
      const cardId = e.dataTransfer.getData('application/octgn-card');
      if (!cardId) return;
      const handGroup = groups.find(
        (g) => g.name.toLowerCase() === 'hand'
      );
      if (handGroup) {
        onCardMoveToGroup(cardId, handGroup.id);
      }
      endDrag();
    },
    [groups, onCardMoveToGroup, endDrag, isSpectator]
  );

  // ─── Group zone drag handlers ──────────────────────────────────────
  const handleGroupDragOver = useCallback(
    (groupId: string) => (e: React.DragEvent) => {
      if (isSpectator) return;
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';
      updateDropTarget(groupId);
    },
    [updateDropTarget, isSpectator]
  );

  const handleGroupDrop = useCallback(
    (groupId: string) => (e: React.DragEvent) => {
      if (isSpectator) return;
      e.preventDefault();
      const cardId = e.dataTransfer.getData('application/octgn-card');
      if (!cardId) return;
      onCardMoveToGroup(cardId, groupId);
      endDrag();
    },
    [onCardMoveToGroup, endDrag, isSpectator]
  );

  // ─── Card event wrappers ───────────────────────────────────────────
  const handleCardDragStart = useCallback(
    (zone: string) => (card: Card, e: React.DragEvent) => {
      if (isSpectator) return;
      startDrag(card.id, zone, e);
    },
    [startDrag, isSpectator]
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

  // For spectator view: collect all groups across all players
  const spectatorGroups = useMemo(() => {
    if (!isSpectator || !allPlayers) return [];
    const groups: { player: Player; group: Group }[] = [];
    for (const player of allPlayers) {
      for (const group of player.groups) {
        if (group.name.toLowerCase() !== 'hand') {
          groups.push({ player, group });
        }
      }
    }
    return groups;
  }, [isSpectator, allPlayers]);

  // Zoom percentage for display
  const zoomPercent = Math.round(zoom * 100);

  return (
    <div className="flex-1 flex flex-col min-h-0 relative">
      {/* ── Table / Play Area ───────────────────────────────────────── */}
      <div
        ref={tableRef}
        className={clsx(
          'flex-1 relative overflow-hidden transition-colors duration-200',
          !isSpectator && isDragging &&
            dragState.dropTargetZone === ZONE_TABLE &&
            'bg-octgn-primary/5',
          !isSpectator && isDragging &&
            dragState.dropTargetZone !== ZONE_TABLE &&
            'bg-transparent',
          isPanning && 'cursor-grabbing'
        )}
        onDragOver={handleTableDragOver}
        onDrop={handleTableDrop}
        onDragLeave={handleTableDragLeave}
        onMouseDown={handleTableMouseDown}
        onMouseMove={handleTableMouseMove}
        onMouseUp={handleTableMouseUp}
        onMouseLeave={handleTableMouseLeave}
      >
        {/* ── Zoom/Pan transform container ────────────────────────── */}
        <div
          data-testid="transform-container"
          className="absolute inset-0"
          style={{
            transform: `translate(${panX}px, ${panY}px) scale(${zoom})`,
            transformOrigin: '0 0',
            willChange: 'transform',
          }}
        >
        {/* ── Base scale container: mm-space to screen-space ───────── */}
        <div
          data-testid="base-scale-container"
          style={hasTableDimensions ? {
            position: 'absolute',
            left: 0,
            top: 0,
            width: `${tableWidth}px`,
            height: `${tableHeight}px`,
            transform: `translate(${baseScale.offsetX}px, ${baseScale.offsetY}px) scale(${baseScale.scale})`,
            transformOrigin: '0 0',
          } : {
            position: 'absolute',
            inset: 0,
          }}
        >
          {/* Origin-shift wrapper: OCTGN protocol uses (0,0) as table center;
              translate by half the table dimensions so center-based coords
              map to the correct pixel positions. */}
          <div
            data-testid="origin-shift"
            style={hasTableDimensions ? {
              position: 'absolute',
              inset: 0,
              transform: `translate(${(tableWidth ?? 0) / 2}px, ${(tableHeight ?? 0) / 2}px)`,
            } : {
              position: 'absolute',
              inset: 0,
            }}
          >
          {/* Table background image (e.g. wood texture) — fills the entire table */}
          {tableBackgroundUrl && (
            <div
              data-testid="table-background"
              className="pointer-events-none select-none"
              style={{
                position: 'absolute',
                left: hasTableDimensions ? `${-(tableWidth ?? 0) / 2}px` : 0,
                top: hasTableDimensions ? `${-(tableHeight ?? 0) / 2}px` : 0,
                width: hasTableDimensions ? `${tableWidth}px` : '100%',
                height: hasTableDimensions ? `${tableHeight}px` : '100%',
                backgroundImage: `url(${tableBackgroundUrl})`,
                backgroundRepeat: backgroundStyle === 'tile' ? 'repeat' : 'no-repeat',
                backgroundSize: backgroundStyle === 'stretch' ? '100% 100%'
                  : backgroundStyle === 'tile' ? 'auto'
                  : backgroundStyle === 'uniformToFill' ? 'cover'
                  : 'contain',
                backgroundPosition: 'center',
                zIndex: 0,
              }}
            />
          )}

          {/* Board image (e.g. chess board) — positioned at specific coordinates on the table */}
          {boardImageUrl && (
            <div
              data-testid="board-background"
              className="pointer-events-none select-none"
              style={{
                position: 'absolute',
                left: `${boardX}px`,
                top: `${boardY}px`,
                width: boardWidth ? `${boardWidth}px` : '100%',
                height: boardHeight ? `${boardHeight}px` : '100%',
                backgroundImage: `url(${boardImageUrl})`,
                backgroundRepeat: backgroundStyle === 'tile' ? 'repeat' : 'no-repeat',
                backgroundSize: backgroundStyle === 'stretch' ? '100% 100%'
                  : backgroundStyle === 'tile' ? 'auto'
                  : backgroundStyle === 'uniformToFill' ? 'cover'
                  : 'contain',
                backgroundPosition: 'center',
                zIndex: 1,
              }}
            />
          )}

          {/* Subtle grid pattern */}
          <div
            data-testid="table-grid"
            className="absolute inset-0 pointer-events-none opacity-[0.03]"
            style={{
              backgroundImage:
                'linear-gradient(rgba(59,130,246,0.3) 1px, transparent 1px), linear-gradient(90deg, rgba(59,130,246,0.3) 1px, transparent 1px)',
              backgroundSize: '40px 40px',
              zIndex: 2,
            }}
          />

          {/* Table cards with smooth transitions */}
          <div data-testid="table-cards" className="absolute inset-0 p-4" style={{ zIndex: 3 }}>
            {tableCards.map((card) => (
              <div
                key={card.id}
                className={clsx(
                  'absolute transition-all duration-500 ease-out',
                  !isSpectator && isDragging &&
                    dragState.draggingCardId === card.id &&
                    'opacity-30 scale-95'
                )}
                style={{ left: card.position.x, top: card.position.y }}
              >
                <CardComponent
                  card={card}
                  selected={card.id === selectedCardId}
                  interactive={!isSpectator}
                  onClick={onCardClick}
                  onContextMenu={(c, e) => onCardContextMenu(e, c)}
                  onDragStart={isSpectator ? undefined : handleCardDragStart(ZONE_TABLE)}
                  onDragEnd={isSpectator ? undefined : handleCardDragEnd}
                />
              </div>
            ))}

            {tableCards.length === 0 && !isDragging && (
              <div className="flex items-center justify-center h-full">
                <p className="text-octgn-text-dim/40 font-display text-sm tracking-widest uppercase">
                  {isSpectator ? 'Waiting for gameplay...' : 'Game Table'}
                </p>
              </div>
            )}

            {!isSpectator && tableCards.length === 0 && isDragging && (
              <div className="flex items-center justify-center h-full">
                <p className="text-octgn-primary/40 font-display text-sm tracking-widest uppercase animate-pulse">
                  Drop card here
                </p>
              </div>
            )}
          </div>
          </div>{/* close origin-shift */}
        </div>{/* close base-scale-container */}
        </div>{/* close transform-container */}

        {/* Drop zone highlight ring (outside transform so it stays fixed) */}
        {!isSpectator && isDragging && dragState.dropTargetZone === ZONE_TABLE && (
          <div className="absolute inset-2 rounded-xl border-2 border-dashed border-octgn-primary/30 pointer-events-none transition-opacity duration-200 animate-pulse z-20">
            <div className="absolute inset-0 rounded-xl bg-octgn-primary/5" />
          </div>
        )}

        {/* Side groups (deck/discard/etc) stacked on the right — outside transform */}
        {!isSpectator && sideGroups.length > 0 && (
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

        {/* Spectator view: all players' groups on the right — outside transform */}
        {isSpectator && spectatorGroups.length > 0 && (
          <div className="absolute top-3 right-3 flex flex-col gap-2 z-10 max-h-[calc(100%-24px)] overflow-y-auto">
            {spectatorGroups.map(({ player, group }) => (
              <div
                key={`${player.id}-${group.id}`}
                className="w-[80px] rounded-lg border border-octgn-border/30 bg-octgn-surface/40 backdrop-blur-sm p-1.5"
              >
                <div
                  className="text-[7px] font-bold uppercase tracking-wider mb-0.5 truncate text-center"
                  style={{ color: player.color || '#9ca3af' }}
                >
                  {player.name}
                </div>
                <div className="text-[8px] font-semibold text-octgn-text-dim uppercase tracking-wider mb-1 truncate text-center">
                  {group.name}
                </div>
                <div className="text-[10px] text-octgn-text-muted text-center font-mono">
                  {group.cards.length}
                </div>
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

        {/* ── Zoom indicator + reset button (bottom-left corner) ───── */}
        <div className="absolute bottom-3 left-3 z-20 flex items-center gap-1.5">
          <div
            className={clsx(
              'px-2 py-1 rounded-md text-[10px] font-mono font-semibold tracking-wide',
              'bg-octgn-surface/80 backdrop-blur-sm border border-octgn-border/30',
              'text-octgn-text-muted select-none',
              zoomPercent !== 100 && 'text-octgn-primary'
            )}
          >
            {zoomPercent}%
          </div>
          {(zoomPercent !== 100 || panX !== 0 || panY !== 0) && (
            <button
              onClick={resetView}
              className={clsx(
                'px-2 py-1 rounded-md text-[10px] font-semibold tracking-wide',
                'bg-octgn-surface/80 backdrop-blur-sm border border-octgn-border/30',
                'text-octgn-text-dim hover:text-octgn-text hover:border-octgn-primary/40',
                'transition-colors duration-150'
              )}
              title="Reset zoom and pan (Ctrl+0)"
            >
              Reset
            </button>
          )}
        </div>
      </div>

      {/* ── Hand Zone (non-spectator only) ──────────────────────────── */}
      {!isSpectator && (
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
          {isDragging && dragState.dropTargetZone === ZONE_HAND && (
            <div className="absolute inset-1 rounded-lg border-2 border-dashed border-octgn-primary/30 pointer-events-none" />
          )}

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
      )}

      {/* ── Drag Ghost Overlay ──────────────────────────────────────── */}
      {!isSpectator && isDragging && draggingCard && (
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
