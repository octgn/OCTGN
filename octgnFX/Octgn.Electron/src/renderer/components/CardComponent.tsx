import React, { useCallback, useMemo, useRef, useState } from 'react';
import { clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';
import type { Card as CardType } from '../../shared/types';

/** Color gradient for placeholder cards when no image is available */
const TYPE_GRADIENTS: Record<string, string> = {
  creature: 'from-emerald-900/80 to-emerald-700/40',
  spell: 'from-blue-900/80 to-blue-600/40',
  artifact: 'from-amber-900/80 to-amber-600/40',
  enchantment: 'from-purple-900/80 to-purple-600/40',
  land: 'from-stone-900/80 to-stone-600/40',
  default: 'from-gray-900/80 to-gray-600/40',
};

function getGradientForCard(card: CardType): string {
  const cardType = (card.properties?.Type ?? card.properties?.type ?? '').toLowerCase();
  for (const [key, gradient] of Object.entries(TYPE_GRADIENTS)) {
    if (cardType.includes(key)) return gradient;
  }
  // Hash-based fallback for visual variety
  const hash = card.definitionId
    ? card.definitionId.charCodeAt(0) % Object.keys(TYPE_GRADIENTS).length
    : 0;
  const keys = Object.keys(TYPE_GRADIENTS);
  return TYPE_GRADIENTS[keys[hash]] ?? TYPE_GRADIENTS.default;
}

// Standard poker card ratio: 2.5 : 3.5
const CARD_ASPECT = 2.5 / 3.5;
const DEFAULT_WIDTH = 100;
const DEFAULT_HEIGHT = Math.round(DEFAULT_WIDTH / CARD_ASPECT);

/**
 * Compute the shortest-path rotation in degrees.
 * Given a previous angle (fromDeg) and target angle (toDeg), both in [0,360),
 * returns the target angle adjusted so CSS transitions take the shortest arc.
 *
 * Examples:
 *   shortestRotationDeg(0, 270) => -90  (go -90 instead of +270)
 *   shortestRotationDeg(270, 0) => 360  (go +90 instead of -270)
 */
export function shortestRotationDeg(fromDeg: number, toDeg: number): number {
  let diff = toDeg - fromDeg;
  // Normalize diff to (-360, 360)
  // Then pick the shortest direction: if |diff| > 180, go the other way
  while (diff > 180) diff -= 360;
  while (diff < -180) diff += 360;
  return fromDeg + diff;
}

export interface CardComponentProps {
  card: CardType;
  cardBackUrl?: string;
  selected?: boolean;
  interactive?: boolean;
  className?: string;
  style?: React.CSSProperties;
  onClick?: (card: CardType) => void;
  onDoubleClick?: (card: CardType) => void;
  onContextMenu?: (card: CardType, e: React.MouseEvent) => void;
  onDragStart?: (card: CardType, e: React.DragEvent) => void;
  onDragEnd?: (card: CardType, e: React.DragEvent) => void;
}

const CardComponent: React.FC<CardComponentProps> = ({
  card,
  cardBackUrl,
  selected = false,
  interactive = true,
  className,
  style,
  onClick,
  onDoubleClick,
  onContextMenu,
  onDragStart,
  onDragEnd,
}) => {
  const [isHovered, setIsHovered] = useState(false);
  const [imgError, setImgError] = useState(false);

  const width = style?.width ?? card.size?.width ?? DEFAULT_WIDTH;
  const height = style?.height ?? card.size?.height ?? DEFAULT_HEIGHT;

  // Track previous rotation to compute shortest path
  const prevRotationRef = useRef(0);
  const targetDeg = (card.rotation ?? 0) * 90; // 0,1,2,3 -> 0,90,180,270
  const rotationDeg = shortestRotationDeg(prevRotationRef.current, targetDeg);
  // Update ref for next render
  prevRotationRef.current = rotationDeg;

  const isFlippedUp = card.faceUp;
  const isPeeking =
    !card.faceUp && card.peekingPlayers && card.peekingPlayers.length > 0;
  const isTargeted = !!card.targetedBy;
  const isHighlighted = !!card.highlighted;
  const hasMarkers = card.markers && card.markers.length > 0;
  const frontImageUrl = card.imageUrl;
  const backImageUrl = cardBackUrl;
  const showFrontPlaceholder = !frontImageUrl || imgError;
  const showBackPlaceholder = !backImageUrl;
  const gradient = useMemo(() => getGradientForCard(card), [card]);

  const handleClick = useCallback(() => onClick?.(card), [onClick, card]);
  const handleDoubleClick = useCallback(
    () => onDoubleClick?.(card),
    [onDoubleClick, card]
  );
  const handleContext = useCallback(
    (e: React.MouseEvent) => {
      e.preventDefault();
      onContextMenu?.(card, e);
    },
    [onContextMenu, card]
  );
  const handleDragStart = useCallback(
    (e: React.DragEvent) => {
      e.dataTransfer.effectAllowed = 'move';
      e.dataTransfer.setData('text/plain', card.id);
      onDragStart?.(card, e);
    },
    [onDragStart, card]
  );
  const handleDragEnd = useCallback(
    (e: React.DragEvent) => {
      onDragEnd?.(card, e);
    },
    [onDragEnd, card]
  );

  // Build the combined transform for the inner 3D container:
  // - Z-axis rotation (card orientation: 0/90/180/270)
  // - Y-axis rotation (flip: 0 or 180)
  // - Hover scale
  const innerTransformParts: string[] = [];
  if (rotationDeg) innerTransformParts.push(`rotate(${rotationDeg}deg)`);
  if (!isFlippedUp) innerTransformParts.push('rotateY(180deg)');
  if (isHovered && interactive) innerTransformParts.push('scale(1.06)');

  // Shared CSS classes for both card faces (front and back)
  // WPF client renders cards as pure images with no border/shadow/rounding in normal state.
  // Only selection, targeting, and highlight states add visual decoration.
  const faceClasses = clsx(
    'absolute inset-0 overflow-hidden',
    interactive && 'transition-shadow duration-300',
    selected &&
      'ring-2 ring-octgn-gold/70 shadow-[0_0_10px_rgba(251,191,36,0.5),0_0_30px_rgba(251,191,36,0.3)]',
    isTargeted &&
      'ring-2 ring-octgn-danger/80 shadow-[0_0_12px_rgba(239,68,68,0.6),0_0_30px_rgba(239,68,68,0.3)] animate-glow-pulse',
    isHighlighted && 'ring-2'
  );

  const highlightStyle = isHighlighted
    ? { outlineColor: card.highlighted, borderColor: card.highlighted }
    : undefined;

  return (
    <div
      className={twMerge(
        clsx('octgn-card-wrapper', 'relative select-none'),
        className
      )}
      style={{
        width: typeof width === 'number' ? `${width}px` : width,
        height: typeof height === 'number' ? `${height}px` : height,
        perspective: '600px',
      }}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      {/* 3D Flip + Rotation container */}
      <div
        className={clsx(
          'octgn-card-inner',
          'absolute inset-0',
          interactive && 'cursor-pointer'
        )}
        style={{
          transformStyle: 'preserve-3d',
          transform: innerTransformParts.length > 0
            ? innerTransformParts.join(' ')
            : undefined,
          transition:
            'transform 0.4s cubic-bezier(0.23, 1, 0.32, 1), box-shadow 0.35s cubic-bezier(0.23, 1, 0.32, 1)',
        }}
        onClick={interactive ? handleClick : undefined}
        onDoubleClick={interactive ? handleDoubleClick : undefined}
        onContextMenu={interactive ? handleContext : undefined}
        draggable={interactive}
        onDragStart={interactive ? handleDragStart : undefined}
        onDragEnd={interactive ? handleDragEnd : undefined}
        role={interactive ? 'button' : undefined}
        tabIndex={interactive ? 0 : undefined}
      >
        {/* ── FRONT FACE (visible when face-up) ───────────────────────── */}
        <div
          className={clsx('octgn-card-front', faceClasses)}
          style={{
            backfaceVisibility: 'hidden',
            ...highlightStyle,
          }}
        >
          {/* Card image or placeholder gradient */}
          {showFrontPlaceholder ? (
            <div
              className={clsx(
                'w-full h-full bg-gradient-to-br',
                gradient
              )}
            >
              {/* Face-up placeholder with name */}
              <div className="absolute inset-0 flex flex-col items-center justify-center p-2">
                <div className="w-8 h-8 mb-2 rounded-full bg-white/10 flex items-center justify-center">
                  <svg
                    className="w-4 h-4 text-white/40"
                    viewBox="0 0 16 16"
                    fill="currentColor"
                  >
                    <path d="M8 1l2.5 5 5.5.8-4 3.9.9 5.3L8 13.5 3.1 16l.9-5.3-4-3.9L5.5 6z" />
                  </svg>
                </div>
                <span className="text-[9px] font-semibold text-white/70 text-center leading-tight line-clamp-3">
                  {card.name}
                </span>
              </div>
            </div>
          ) : (
            <img
              src={frontImageUrl}
              alt={card.name}
              className="w-full h-full object-fill select-none pointer-events-none"
              draggable={false}
              onError={() => setImgError(true)}
            />
          )}

          {/* Highlight color overlay */}
          {isHighlighted && (
            <div
              className="absolute inset-0 pointer-events-none mix-blend-overlay opacity-25"
              style={{ backgroundColor: card.highlighted }}
            />
          )}

          {/* Selection shine sweep */}
          {selected && (
            <div className="absolute inset-0 pointer-events-none bg-gradient-to-tr from-transparent via-octgn-gold/10 to-transparent" />
          )}

          {/* Peeking overlay (shown on front when someone peeks at a face-down card) */}
          {isPeeking && (
            <div className="absolute inset-0 pointer-events-none">
              <div className="absolute inset-0 bg-octgn-primary/10 backdrop-blur-[1px]" />
              <div className="absolute top-1 left-1">
                <svg
                  className="w-3.5 h-3.5 text-octgn-warning drop-shadow-[0_0_4px_rgba(251,191,36,0.6)]"
                  viewBox="0 0 16 16"
                  fill="currentColor"
                >
                  <path d="M8 3C4.5 3 1.7 5.1.3 8c1.4 2.9 4.2 5 7.7 5s6.3-2.1 7.7-5c-1.4-2.9-4.2-5-7.7-5zm0 8.5a3.5 3.5 0 110-7 3.5 3.5 0 010 7zM8 6a2 2 0 100 4 2 2 0 000-4z" />
                </svg>
              </div>
            </div>
          )}

          {/* Targeting glow ring */}
          {isTargeted && (
            <div className="absolute inset-0 pointer-events-none">
              <div className="absolute -inset-0.5 rounded-lg border-2 border-octgn-danger/60 animate-pulse" />
              <div className="absolute top-1 right-1 w-3 h-3 rounded-full bg-octgn-danger shadow-[0_0_8px_rgba(239,68,68,0.7)]">
                <div className="w-full h-full rounded-full bg-octgn-danger animate-ping opacity-40" />
              </div>
            </div>
          )}

          {/* Markers */}
          {hasMarkers && (
            <div className="absolute bottom-0 left-0 right-0 flex flex-wrap gap-0.5 p-1 bg-black/60 backdrop-blur-sm">
              {card.markers.map((marker) => (
                <div
                  key={marker.id}
                  className="flex items-center gap-0.5 px-1 py-0.5 rounded bg-octgn-surface/80 border border-octgn-border/40"
                  title={`${marker.name}: ${marker.count}`}
                >
                  {marker.iconUrl ? (
                    <img
                      src={marker.iconUrl}
                      alt=""
                      className="w-3 h-3"
                    />
                  ) : (
                    <span className="w-2.5 h-2.5 rounded-full bg-octgn-accent/70 inline-block shadow-[0_0_4px_rgba(139,92,246,0.5)]" />
                  )}
                  {marker.count > 1 && (
                    <span className="text-[8px] font-bold text-octgn-text leading-none">
                      {marker.count}
                    </span>
                  )}
                </div>
              ))}
            </div>
          )}

          {/* Card name tooltip on face-up cards with real images */}
          {!showFrontPlaceholder && isHovered && (
            <div className="absolute bottom-0 left-0 right-0 px-1.5 py-1 bg-gradient-to-t from-black/80 to-transparent">
              <span className="text-[9px] font-medium text-octgn-text/90 leading-tight line-clamp-1">
                {card.name}
              </span>
            </div>
          )}
        </div>

        {/* ── BACK FACE (visible when face-down) ──────────────────────── */}
        <div
          className={clsx('octgn-card-back', faceClasses)}
          style={{
            backfaceVisibility: 'hidden',
            transform: 'rotateY(180deg)',
            ...highlightStyle,
          }}
        >
          {showBackPlaceholder ? (
            <div className="w-full h-full bg-gradient-to-br from-indigo-950 to-slate-900">
              <div className="absolute inset-0 flex items-center justify-center">
                <div className="absolute inset-2 rounded border border-octgn-border/20 bg-gradient-to-br from-octgn-primary/10 to-octgn-accent/10" />
                <div className="absolute inset-4 rounded border border-octgn-border/10">
                  <div
                    className="w-full h-full opacity-[0.06]"
                    style={{
                      backgroundImage:
                        'repeating-linear-gradient(45deg, transparent, transparent 8px, currentColor 8px, currentColor 9px)',
                    }}
                  />
                </div>
                <span className="relative font-display text-[10px] text-octgn-text-dim/40 tracking-widest uppercase">
                  OCTGN
                </span>
              </div>
            </div>
          ) : (
            <img
              src={backImageUrl}
              alt="Face-down card"
              className="w-full h-full object-fill select-none pointer-events-none"
              draggable={false}
            />
          )}
        </div>
      </div>
    </div>
  );
};

export default CardComponent;
