import React, { useCallback, useRef, useMemo, useEffect, useState } from 'react';
import { clsx } from 'clsx';
import CardComponent from './CardComponent';
import { useDragDrop } from './DragDropContext';
import { useTableTransform } from '../hooks/useTableTransform';
import { calculateTableScale } from '../utils/table-scaling';
import type { Card } from '../../shared/types';
import { isInInvertedZone } from '../../shared/table-utils';

// ─── Zone identifiers ────────────────────────────────────────────────
const ZONE_TABLE = 'table';

// ─── Props ───────────────────────────────────────────────────────────
interface GameBoardProps {
  tableCards: Card[];
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
  useTwoSidedTable?: boolean;
  isSpectator?: boolean;
}

const GameBoard: React.FC<GameBoardProps> = ({
  tableCards,
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
  useTwoSidedTable = false,
  isSpectator = false,
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
      if (e.button === 1) {
        e.preventDefault();
        handlePanStart(e.clientX, e.clientY);
        return;
      }
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

      const contentPos = screenToTable(e.clientX, e.clientY);

      let x = contentPos.x;
      let y = contentPos.y;
      if (hasTableDimensions && baseScale.scale !== 0) {
        x = (x - baseScale.offsetX) / baseScale.scale;
        y = (y - baseScale.offsetY) / baseScale.scale;
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

  // ─── Card event wrappers ───────────────────────────────────────────
  const handleCardDragStart = useCallback(
    (card: Card, e: React.DragEvent) => {
      if (isSpectator) return;
      startDrag(card.id, ZONE_TABLE, e);
    },
    [startDrag, isSpectator]
  );

  const handleCardDragEnd = useCallback(() => {
    endDrag();
  }, [endDrag]);

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
        {/* Table background image */}
        {tableBackgroundUrl && (
          <div
            data-testid="table-background"
            className="absolute inset-0 pointer-events-none select-none"
            style={{
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

          {/* Board image */}
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

          {/* Two-sided table middle line */}
          {useTwoSidedTable && (
            <div
              data-testid="two-sided-middle-line"
              className="absolute pointer-events-none"
              style={{
                left: hasTableDimensions ? `${-(tableWidth ?? 0) / 2}px` : 0,
                top: 0,
                width: hasTableDimensions ? `${tableWidth}px` : '100%',
                height: 0,
                borderTop: '2px dashed rgba(255, 255, 255, 0.3)',
                zIndex: 4,
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

          {/* Table cards */}
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
                  invertedZone={useTwoSidedTable && isInInvertedZone(card.position.y, card.size.height)}
                  onClick={onCardClick}
                  onContextMenu={(c, e) => onCardContextMenu(e, c)}
                  onDragStart={isSpectator ? undefined : handleCardDragStart}
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

        {/* Drop zone highlight ring */}
        {!isSpectator && isDragging && dragState.dropTargetZone === ZONE_TABLE && (
          <div className="absolute inset-2 rounded-xl border-2 border-dashed border-octgn-primary/30 pointer-events-none transition-opacity duration-200 animate-pulse z-20">
            <div className="absolute inset-0 rounded-xl bg-octgn-primary/5" />
          </div>
        )}

        {/* ── Zoom indicator + reset button ───── */}
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
    </div>
  );
};

export default GameBoard;
