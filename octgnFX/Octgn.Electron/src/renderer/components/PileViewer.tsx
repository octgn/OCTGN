import React, { useEffect, useCallback, useState, useRef } from 'react';
import { clsx } from 'clsx';
import CardComponent from './CardComponent';
import { useDragDrop, cardToDragInfo } from './DragDropContext';
import { readablePlayerColor } from '../utils/player-colors';
import type { Card, Group } from '../../shared/types';

export interface PileViewerProps {
  group: Group;
  playerName: string;
  playerColor: string;
  isOwn: boolean;
  onClose: () => void;
  onCardClick?: (card: Card) => void;
  onCardContextMenu?: (e: React.MouseEvent, card: Card) => void;
}

const PileViewer: React.FC<PileViewerProps> = ({
  group,
  playerName,
  playerColor,
  isOwn,
  onClose,
  onCardClick,
  onCardContextMenu,
}) => {
  const { startDrag, startTouchDrag, endDrag, isDragging } = useDragDrop();
  const readableColor = readablePlayerColor(playerColor);
  const [draggingFromPile, setDraggingFromPile] = useState(false);
  const wasTouchDragging = useRef(false);

  // Close the viewer when a touch drag ends (TouchDragLayer calls endDrag globally)
  useEffect(() => {
    if (wasTouchDragging.current && !isDragging) {
      wasTouchDragging.current = false;
      onClose();
    }
  }, [isDragging, onClose]);

  // When drag starts, hide the overlay so the table can receive the drop.
  // We don't unmount (onClose) because that would remove the drag source and cancel the drag.
  const handleCardDragStart = useCallback(
    (card: Card, e: React.DragEvent) => {
      startDrag(card.id, `pile:${group.id}`, e, cardToDragInfo(card));
      setDraggingFromPile(true);
    },
    [startDrag, group.id, isOwn],
  );

  const handleCardDragEnd = useCallback(() => {
    endDrag();
    // Close the viewer after the drag completes (drop or cancel)
    onClose();
  }, [endDrag, onClose]);

  const handleCardTouchDragStart = useCallback(
    (card: Card, x: number, y: number, grabOffset: { x: number; y: number }) => {
      startTouchDrag(card.id, `pile:${group.id}`, x, y, cardToDragInfo(card), grabOffset);
      setDraggingFromPile(true);
      wasTouchDragging.current = true;
    },
    [startTouchDrag, group.id],
  );

  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    },
    [onClose],
  );

  useEffect(() => {
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [handleKeyDown]);

  const handleOverlayClick = useCallback(
    (e: React.MouseEvent) => {
      if (e.target === e.currentTarget) onClose();
    },
    [onClose],
  );

  return (
    <div
      data-testid="pile-viewer-overlay"
      className={clsx(
        'fixed inset-0 z-50 flex items-end sm:items-center justify-center animate-fade-in',
        draggingFromPile
          ? 'pointer-events-none opacity-0'
          : 'bg-black/65 backdrop-blur-sm',
      )}
      onClick={handleOverlayClick}
    >
      <div
        className={clsx(
          'relative w-full sm:w-[90vw] sm:max-w-3xl max-h-[80vh] sm:max-h-[70vh]',
          'rounded-t-2xl sm:rounded-2xl',
          'bg-octgn-surface/[0.97] backdrop-blur-xl',
          'border border-white/[0.08]',
          'shadow-[0_-8px_40px_rgba(0,0,0,0.4),0_0_0_1px_rgba(255,255,255,0.03)]',
          'flex flex-col',
          'animate-slide-up',
        )}
      >
        {/* Top edge highlight */}
        <div className="absolute inset-x-0 top-0 h-px bg-gradient-to-r from-transparent via-white/[0.1] to-transparent rounded-t-2xl" />

        {/* ── Header ──────────────────────────────────────────────── */}
        <div className="flex items-center gap-3 px-4 sm:px-5 py-3 border-b border-white/[0.06] shrink-0">
          {/* Player color indicator with glow */}
          <div className="relative">
            <div
              className="w-3 h-3 rounded-full shrink-0"
              style={{
                backgroundColor: playerColor,
                boxShadow: `0 0 8px ${playerColor}60`,
              }}
            />
          </div>

          <div className="flex items-center gap-2 min-w-0 flex-1">
            <span
              className="font-semibold text-sm truncate"
              style={{ color: readableColor }}
            >
              {playerName}
            </span>
            <svg className="w-2 h-2 text-octgn-text-dim/30 shrink-0" viewBox="0 0 8 8">
              <path d="M2 0l4 4-4 4" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
            </svg>
            <span className="font-display text-sm font-semibold text-octgn-text tracking-wide truncate">
              {group.name}
            </span>
            <span className="text-[10px] text-octgn-text-muted font-mono font-bold bg-white/[0.04] px-2 py-0.5 rounded-md border border-white/[0.06] shrink-0 tabular-nums">
              {group.cards.length}
            </span>
          </div>

          <button
            onClick={onClose}
            data-testid="pile-viewer-close"
            className={clsx(
              'p-1.5 rounded-lg shrink-0 transition-all duration-200',
              'text-octgn-text-dim hover:text-octgn-text',
              'hover:bg-white/[0.06] active:bg-white/[0.08] active:scale-95',
            )}
            aria-label="Close pile viewer"
          >
            <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
              <path d="M4 4l8 8M12 4l-8 8" />
            </svg>
          </button>
        </div>

        {/* ── Card Grid ───────────────────────────────────────────── */}
        <div className="flex-1 overflow-y-auto overscroll-contain p-3 sm:p-4">
          {group.cards.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-16 text-octgn-text-dim">
              <svg className="w-12 h-12 mb-4 opacity-20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="0.8">
                <rect x="4" y="3" width="16" height="18" rx="2" />
                <path d="M8 8h8M8 12h5" />
              </svg>
              <p className="text-sm font-display tracking-widest uppercase opacity-40">No cards in this pile</p>
            </div>
          ) : (
            <div className="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-5 lg:grid-cols-6 gap-2.5 sm:gap-3">
              {group.cards.map((card, i) => (
                <div
                  key={card.id}
                  className="group flex flex-col items-center gap-1 animate-fade-in"
                  style={{ animationDelay: `${Math.min(i * 20, 300)}ms`, animationFillMode: 'both' }}
                >
                  <div className={clsx(
                    'w-full aspect-[5/7] rounded-lg overflow-hidden transition-all duration-200',
                    'border border-white/[0.06]',
                    'hover:border-octgn-primary/30 hover:shadow-[0_4px_16px_rgba(59,130,246,0.12)]',
                    'group-hover:translate-y-[-2px]',
                  )}>
                    <CardComponent
                      card={card}
                      interactive={isOwn}
                      onClick={onCardClick}
                      onDragStart={isOwn ? handleCardDragStart : undefined}
                      onDragEnd={isOwn ? handleCardDragEnd : undefined}
                      onTouchDragStart={isOwn ? handleCardTouchDragStart : undefined}
                      onContextMenu={
                        onCardContextMenu
                          ? (c, e) => onCardContextMenu(e, c)
                          : undefined
                      }
                      style={{ width: '100%', height: '100%' }}
                    />
                  </div>
                  <span className="text-[9px] sm:text-[10px] text-octgn-text-dim text-center leading-tight line-clamp-2 w-full px-0.5 transition-colors group-hover:text-octgn-text-muted">
                    {card.faceUp ? (card.name || 'Unknown') : 'Face-down'}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default PileViewer;
