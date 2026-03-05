import React, { useCallback, useRef, useMemo, useEffect, useState } from 'react';
import { clsx } from 'clsx';
import CardComponent from './CardComponent';
import { useDragDrop, cardToDragInfo, type DraggingCardData } from './DragDropContext';
import { useTableTransform } from '../hooks/useTableTransform';
import { calculateTableScale } from '../utils/table-scaling';
import type { Card } from '../../shared/types';
import { isInInvertedZone } from '../../shared/table-utils';
import { computeMarqueeRect, findCardsInMarquee } from '../utils/marquee-selection';

// ─── Zone identifiers ────────────────────────────────────────────────
const ZONE_TABLE = 'table';

// ─── Props ───────────────────────────────────────────────────────────
/** Function that converts screen (clientX, clientY) to table mm-space coordinates */
export type ScreenToTableCoordsFn = (screenX: number, screenY: number) => { x: number; y: number };

interface GameBoardProps {
  tableCards: Card[];
  selectedCardIds: Set<string>;
  boardImageUrl?: string;
  boardX?: number;
  boardY?: number;
  boardWidth?: number;
  boardHeight?: number;
  backgroundStyle?: 'stretch' | 'tile' | 'uniform' | 'uniformToFill';
  tableBackgroundUrl?: string;
  tableWidth?: number;
  tableHeight?: number;
  onCardClick: (card: Card, e?: React.MouseEvent) => void;
  onCardContextMenu: (e: React.MouseEvent, card: Card) => void;
  onTableContextMenu?: (e: React.MouseEvent) => void;
  onCardMoveToTable: (cardIds: string[], x: number, y: number, relativePositions?: { relativeX: number; relativeY: number }[]) => void;
  onSelectionChange?: (cardIds: Set<string>) => void;
  useTwoSidedTable?: boolean;
  isSpectator?: boolean;
  /** Ref that receives a function to convert screen coords to table coords */
  screenToTableCoordsRef?: React.MutableRefObject<ScreenToTableCoordsFn | null>;
}

const GameBoard: React.FC<GameBoardProps> = ({
  tableCards,
  selectedCardIds,
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
  onTableContextMenu,
  onCardMoveToTable,
  onSelectionChange,
  useTwoSidedTable = false,
  isSpectator = false,
  screenToTableCoordsRef,
}) => {
  const tableRef = useRef<HTMLDivElement>(null);
  const { dragState, startDrag, startTouchDrag, updateDropTarget, updateMousePosition, endDrag, isDragging, recentlyDroppedCardId, recentlyDroppedCardIds, clearRecentlyDropped } =
    useDragDrop();

  // When a dropped card's position changes in the DOM, wait for the paint
  // then re-enable transitions so future server-driven moves animate normally.
  const prevDroppedPosRef = useRef<{ id: string; x: number; y: number } | null>(null);
  useEffect(() => {
    if (!recentlyDroppedCardId) {
      prevDroppedPosRef.current = null;
      return;
    }
    const card = tableCards.find((c) => c.id === recentlyDroppedCardId);
    if (!card) {
      // Card moved off the table (e.g. to a group) — clear immediately
      clearRecentlyDropped();
      return;
    }
    const prev = prevDroppedPosRef.current;
    if (prev && prev.id === card.id && (prev.x !== card.position.x || prev.y !== card.position.y)) {
      // Position changed — the new position is being rendered without transition.
      // Wait two rAFs (one for React commit, one for paint) then re-enable transitions.
      requestAnimationFrame(() => {
        requestAnimationFrame(() => {
          clearRecentlyDropped();
        });
      });
    }
    prevDroppedPosRef.current = { id: card.id, x: card.position.x, y: card.position.y };
  }, [recentlyDroppedCardId, tableCards, clearRecentlyDropped]);

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

  // ─── Expose screen→table coordinate conversion for touch drag ────────
  // This is populated after screenToTable and baseScale are available.
  // We use a second useEffect below (after screenToTable is defined).

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

  // Expose screen→table coordinate conversion for touch drag layer
  useEffect(() => {
    if (!screenToTableCoordsRef) return;
    screenToTableCoordsRef.current = (screenX: number, screenY: number) => {
      const contentPos = screenToTable(screenX, screenY);
      let x = contentPos.x;
      let y = contentPos.y;
      if (hasTableDimensions && baseScale.scale !== 0) {
        x = (x - baseScale.offsetX) / baseScale.scale;
        y = (y - baseScale.offsetY) / baseScale.scale;
        x -= (tableWidth ?? 0) / 2;
        y -= (tableHeight ?? 0) / 2;
      }
      return { x, y };
    };
  }, [screenToTableCoordsRef, screenToTable, hasTableDimensions, baseScale, tableWidth, tableHeight]);

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

  // ─── Marquee (box) selection ──────────────────────────────────────────
  const [marquee, setMarquee] = useState<{
    startX: number; startY: number; // table-space start
    endX: number; endY: number;     // table-space current
    screenStartX: number; screenStartY: number; // screen start (for threshold)
    active: boolean; // true once threshold exceeded
  } | null>(null);

  // Minimum drag distance (in screen pixels) before marquee starts
  const MARQUEE_THRESHOLD = 5;

  /** Convert screen coords to table-space coords for marquee */
  const screenToTableSpace = useCallback((screenX: number, screenY: number) => {
    const contentPos = screenToTable(screenX, screenY);
    let x = contentPos.x;
    let y = contentPos.y;
    if (hasTableDimensions && baseScale.scale !== 0) {
      x = (x - baseScale.offsetX) / baseScale.scale;
      y = (y - baseScale.offsetY) / baseScale.scale;
      x -= (tableWidth ?? 0) / 2;
      y -= (tableHeight ?? 0) / 2;
    }
    return { x, y };
  }, [screenToTable, hasTableDimensions, baseScale, tableWidth, tableHeight]);

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
        return;
      }
      // Left-click on empty table area: start marquee selection
      if (e.button === 0 && !isSpectator) {
        const target = e.target as HTMLElement;
        // Only start marquee if clicking on the table (not on a card)
        if (target.closest('[data-card-id]') || target.closest('.octgn-card-wrapper')) return;
        const pos = screenToTableSpace(e.clientX, e.clientY);
        setMarquee({
          startX: pos.x, startY: pos.y,
          endX: pos.x, endY: pos.y,
          screenStartX: e.clientX, screenStartY: e.clientY,
          active: false,
        });
      }
    },
    [handlePanStart, isSpectator, screenToTableSpace]
  );

  const handleTableMouseMove = useCallback(
    (e: React.MouseEvent) => {
      if (isPanning) {
        handlePanMove(e.clientX, e.clientY);
        return;
      }
      if (marquee) {
        const dx = e.clientX - marquee.screenStartX;
        const dy = e.clientY - marquee.screenStartY;
        const dist = Math.sqrt(dx * dx + dy * dy);
        const pos = screenToTableSpace(e.clientX, e.clientY);
        setMarquee((prev) => prev ? {
          ...prev,
          endX: pos.x,
          endY: pos.y,
          active: prev.active || dist > MARQUEE_THRESHOLD,
        } : null);
      }
    },
    [isPanning, handlePanMove, marquee, screenToTableSpace]
  );

  const handleTableMouseUp = useCallback(
    (e: React.MouseEvent) => {
      if (e.button === 1 || (e.button === 0 && isPanning)) {
        handlePanEnd();
        return;
      }
      if (marquee) {
        if (marquee.active) {
          // Finalize marquee selection
          const rect = computeMarqueeRect(marquee.startX, marquee.startY, marquee.endX, marquee.endY);
          const ids = findCardsInMarquee(tableCards, rect);
          onSelectionChange?.(new Set(ids));
        } else {
          // Click on empty area without dragging: clear selection
          const target = e.target as HTMLElement;
          if (!target.closest('[data-card-id]') && !target.closest('.octgn-card-wrapper')) {
            onSelectionChange?.(new Set());
          }
        }
        setMarquee(null);
      }
    },
    [isPanning, handlePanEnd, marquee, tableCards, onSelectionChange]
  );

  const handleTableMouseLeave = useCallback(() => {
    if (isPanning) handlePanEnd();
    if (marquee) setMarquee(null);
  }, [isPanning, handlePanEnd, marquee]);

  // ─── Touch marquee selection ────────────────────────────────────────
  // Use native event listeners (not React synthetic) so we can use { passive: false }
  // to allow preventDefault() for blocking scroll during marquee drag.
  const touchMarqueeRef = useRef<{ id: number; startX: number; startY: number } | null>(null);
  const marqueeRef = useRef(marquee);
  marqueeRef.current = marquee;
  const screenToTableSpaceRef = useRef(screenToTableSpace);
  screenToTableSpaceRef.current = screenToTableSpace;
  const tableCardsRef = useRef(tableCards);
  tableCardsRef.current = tableCards;
  const onSelectionChangeRef = useRef(onSelectionChange);
  onSelectionChangeRef.current = onSelectionChange;

  const handleTableTouchStart = useCallback(
    (e: React.TouchEvent) => {
      if (isSpectator || e.touches.length !== 1) return;
      const target = e.target as HTMLElement;
      if (target.closest('[data-card-id]') || target.closest('.octgn-card-wrapper')) return;
      const touch = e.touches[0];
      const pos = screenToTableSpaceRef.current(touch.clientX, touch.clientY);
      touchMarqueeRef.current = { id: touch.identifier, startX: touch.clientX, startY: touch.clientY };
      setMarquee({
        startX: pos.x, startY: pos.y,
        endX: pos.x, endY: pos.y,
        screenStartX: touch.clientX, screenStartY: touch.clientY,
        active: false,
      });
    },
    [isSpectator]
  );

  // Attach native touchmove/touchend on the table element for marquee (passive: false)
  useEffect(() => {
    const el = tableRef.current;
    if (!el) return;

    const handleTouchMove = (e: TouchEvent) => {
      const m = marqueeRef.current;
      if (!m || !touchMarqueeRef.current) return;
      const touch = Array.from(e.touches).find((t) => t.identifier === touchMarqueeRef.current!.id);
      if (!touch) return;
      const dx = touch.clientX - m.screenStartX;
      const dy = touch.clientY - m.screenStartY;
      const dist = Math.sqrt(dx * dx + dy * dy);
      if (dist > MARQUEE_THRESHOLD) {
        e.preventDefault(); // Block scroll during active marquee
      }
      const pos = screenToTableSpaceRef.current(touch.clientX, touch.clientY);
      setMarquee((prev) => prev ? {
        ...prev,
        endX: pos.x,
        endY: pos.y,
        active: prev.active || dist > MARQUEE_THRESHOLD,
      } : null);
    };

    const handleTouchEnd = (e: TouchEvent) => {
      const m = marqueeRef.current;
      if (!m || !touchMarqueeRef.current) return;
      const stillDown = Array.from(e.touches).some((t) => t.identifier === touchMarqueeRef.current!.id);
      if (stillDown) return;

      if (m.active) {
        const rect = computeMarqueeRect(m.startX, m.startY, m.endX, m.endY);
        const ids = findCardsInMarquee(tableCardsRef.current, rect);
        onSelectionChangeRef.current?.(new Set(ids));
      } else {
        onSelectionChangeRef.current?.(new Set());
      }
      setMarquee(null);
      touchMarqueeRef.current = null;
    };

    el.addEventListener('touchmove', handleTouchMove, { passive: false });
    el.addEventListener('touchend', handleTouchEnd, { passive: false });
    el.addEventListener('touchcancel', handleTouchEnd, { passive: false });
    return () => {
      el.removeEventListener('touchmove', handleTouchMove);
      el.removeEventListener('touchend', handleTouchEnd);
      el.removeEventListener('touchcancel', handleTouchEnd);
    };
  }, []); // stable — uses refs for all mutable state

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

      // Offset by grabOffset so the card drops where the adorner was,
      // not with top-left at cursor position
      const { grabOffset } = dragState;
      const contentPos = screenToTable(e.clientX - grabOffset.x, e.clientY - grabOffset.y);

      let x = contentPos.x;
      let y = contentPos.y;
      if (hasTableDimensions && baseScale.scale !== 0) {
        x = (x - baseScale.offsetX) / baseScale.scale;
        y = (y - baseScale.offsetY) / baseScale.scale;
        x -= (tableWidth ?? 0) / 2;
        y -= (tableHeight ?? 0) / 2;
      }

      // Move all dragged cards (multi-select support)
      const allIds = dragState.draggingCardIds.length > 0 ? dragState.draggingCardIds : [cardId];
      // Pass relative positions so cards maintain their layout
      const relativePositions = dragState.draggingCards.length > 0
        ? dragState.draggingCards.map((c) => ({ relativeX: c.relativeX, relativeY: c.relativeY }))
        : undefined;
      onCardMoveToTable(allIds, x, y, relativePositions);
      endDrag();
    },
    [onCardMoveToTable, endDrag, isSpectator, screenToTable, hasTableDimensions, baseScale, tableWidth, tableHeight, dragState]
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
  /** Build DraggingCardData[] for multi-card drags with relative positions */
  const buildDraggingCards = useCallback(
    (primaryCard: Card, allIds: string[]): DraggingCardData[] => {
      if (allIds.length <= 1) return [];
      const primaryPos = primaryCard.position;
      return allIds.map((id) => {
        const c = tableCards.find((tc) => tc.id === id);
        if (!c) return { id, relativeX: 0, relativeY: 0, info: cardToDragInfo(primaryCard) };
        return {
          id,
          relativeX: c.position.x - primaryPos.x,
          relativeY: c.position.y - primaryPos.y,
          info: cardToDragInfo(c),
        };
      });
    },
    [tableCards]
  );

  const handleCardDragStart = useCallback(
    (card: Card, e: React.DragEvent) => {
      if (isSpectator) return;
      // If the dragged card is part of a multi-selection, drag all selected cards
      const allIds = selectedCardIds.has(card.id) && selectedCardIds.size > 1
        ? Array.from(selectedCardIds)
        : [card.id];
      const draggingCards = buildDraggingCards(card, allIds);
      startDrag(card.id, ZONE_TABLE, e, cardToDragInfo(card), allIds, draggingCards);
    },
    [startDrag, isSpectator, selectedCardIds, buildDraggingCards]
  );

  const handleCardDragEnd = useCallback(() => {
    endDrag();
  }, [endDrag]);

  const handleCardTouchDragStart = useCallback(
    (card: Card, x: number, y: number, grabOffset: { x: number; y: number }) => {
      if (isSpectator) return;
      const allIds = selectedCardIds.has(card.id) && selectedCardIds.size > 1
        ? Array.from(selectedCardIds)
        : [card.id];
      const draggingCards = buildDraggingCards(card, allIds);
      startTouchDrag(card.id, ZONE_TABLE, x, y, cardToDragInfo(card), grabOffset, allIds, draggingCards);
    },
    [startTouchDrag, isSpectator, selectedCardIds, buildDraggingCards]
  );

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
          isPanning && 'cursor-grabbing',
          marquee?.active && 'cursor-crosshair'
        )}
        data-drop-zone={ZONE_TABLE}
        onDragOver={handleTableDragOver}
        onDrop={handleTableDrop}
        onDragLeave={handleTableDragLeave}
        onMouseDown={handleTableMouseDown}
        onMouseMove={handleTableMouseMove}
        onMouseUp={handleTableMouseUp}
        onMouseLeave={handleTableMouseLeave}
        onTouchStart={handleTableTouchStart}
        onContextMenu={(e) => {
          // Only trigger table context menu if the click target is the table itself, not a card
          if (onTableContextMenu && (e.target as HTMLElement).closest('[data-card-id]') === null) {
            e.preventDefault();
            onTableContextMenu(e);
          }
        }}
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
                data-card-id={card.id}
                className={clsx(
                  'absolute',
                  (recentlyDroppedCardId === card.id || recentlyDroppedCardIds.includes(card.id))
                    ? '' // Skip transition — adorner already provided visual continuity
                    : 'transition-all duration-500 ease-out',
                  !isSpectator && isDragging &&
                    dragState.draggingCardIds.includes(card.id) &&
                    'opacity-30 scale-95'
                )}
                style={{ left: card.position.x, top: card.position.y }}
              >
                <CardComponent
                  card={card}
                  selected={selectedCardIds.has(card.id)}
                  interactive={!isSpectator}
                  invertedZone={useTwoSidedTable && isInInvertedZone(card.position.y, card.size.height)}
                  onClick={onCardClick}
                  onContextMenu={(c, e) => onCardContextMenu(e, c)}
                  onDragStart={isSpectator ? undefined : handleCardDragStart}
                  onDragEnd={isSpectator ? undefined : handleCardDragEnd}
                  onTouchDragStart={isSpectator ? undefined : handleCardTouchDragStart}
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

          {/* Marquee selection rectangle */}
          {marquee?.active && (() => {
            const rect = computeMarqueeRect(marquee.startX, marquee.startY, marquee.endX, marquee.endY);
            return (
              <div
                data-testid="marquee-selection"
                className="pointer-events-none"
                style={{
                  position: 'absolute',
                  left: `${rect.x}px`,
                  top: `${rect.y}px`,
                  width: `${rect.width}px`,
                  height: `${rect.height}px`,
                  borderRadius: '2px',
                  backgroundColor: 'rgba(59, 130, 246, 0.06)',
                  zIndex: 50,
                }}
              >
                {/* Animated marching-ants border */}
                <div
                  className="absolute inset-0 rounded-sm"
                  style={{
                    border: '1.5px dashed rgba(59, 130, 246, 0.6)',
                    animation: 'marquee-march 0.4s linear infinite',
                  }}
                />
                {/* Inner glow border */}
                <div
                  className="absolute inset-0 rounded-sm"
                  style={{
                    boxShadow: '0 0 8px rgba(59, 130, 246, 0.15), inset 0 0 8px rgba(59, 130, 246, 0.05)',
                  }}
                />
                {/* Corner accents */}
                <div className="absolute -top-px -left-px w-2.5 h-2.5 border-t-2 border-l-2 border-blue-400/90 rounded-tl-sm" />
                <div className="absolute -top-px -right-px w-2.5 h-2.5 border-t-2 border-r-2 border-blue-400/90 rounded-tr-sm" />
                <div className="absolute -bottom-px -left-px w-2.5 h-2.5 border-b-2 border-l-2 border-blue-400/90 rounded-bl-sm" />
                <div className="absolute -bottom-px -right-px w-2.5 h-2.5 border-b-2 border-r-2 border-blue-400/90 rounded-br-sm" />

                <style>{`
                  @keyframes marquee-march {
                    0% { stroke-dashoffset: 0; border-color: rgba(59, 130, 246, 0.6); }
                    50% { border-color: rgba(59, 130, 246, 0.8); }
                    100% { stroke-dashoffset: 12; border-color: rgba(59, 130, 246, 0.6); }
                  }
                `}</style>
              </div>
            );
          })()}

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
