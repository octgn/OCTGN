import React, { useCallback } from 'react';
import { clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';
import type { Card as CardType } from '../../shared/types';

interface CardProps {
  card: CardType;
  cardBackUrl?: string;
  selected?: boolean;
  interactive?: boolean;
  className?: string;
  style?: React.CSSProperties;
  onClick?: (card: CardType) => void;
  onDoubleClick?: (card: CardType) => void;
  onContextMenu?: (card: CardType, e: React.MouseEvent) => void;
}

const CardComponent: React.FC<CardProps> = ({
  card,
  cardBackUrl = '/assets/card-back.png',
  selected = false,
  interactive = true,
  className,
  style,
  onClick,
  onDoubleClick,
  onContextMenu,
}) => {
  const imageUrl = card.faceUp ? card.imageUrl : cardBackUrl;
  const rotationDeg = card.rotation || 0;

  const handleClick = useCallback(() => onClick?.(card), [onClick, card]);
  const handleDoubleClick = useCallback(() => onDoubleClick?.(card), [onDoubleClick, card]);
  const handleContext = useCallback(
    (e: React.MouseEvent) => {
      e.preventDefault();
      onContextMenu?.(card, e);
    },
    [onContextMenu, card]
  );

  return (
    <div
      className={twMerge(
        clsx(
          'relative rounded-lg overflow-hidden',
          interactive && 'cursor-pointer card-glow',
          selected && 'card-glow-selected ring-2 ring-octgn-gold/60',
          card.highlighted && 'ring-2',
        ),
        className
      )}
      style={{
        width: card.size.width,
        height: card.size.height,
        transform: rotationDeg ? `rotate(${rotationDeg}deg)` : undefined,
        ...style,
      }}
      onClick={interactive ? handleClick : undefined}
      onDoubleClick={interactive ? handleDoubleClick : undefined}
      onContextMenu={interactive ? handleContext : undefined}
      role={interactive ? 'button' : undefined}
      tabIndex={interactive ? 0 : undefined}
    >
      {/* Card image */}
      <img
        src={imageUrl}
        alt={card.faceUp ? card.name : 'Face-down card'}
        className="w-full h-full object-cover select-none pointer-events-none"
        draggable={false}
      />

      {/* Highlight overlay */}
      {card.highlighted && (
        <div
          className="absolute inset-0 pointer-events-none mix-blend-overlay opacity-30"
          style={{ backgroundColor: card.highlighted }}
        />
      )}

      {/* Selection shine */}
      {selected && (
        <div className="absolute inset-0 pointer-events-none bg-gradient-to-tr from-transparent via-octgn-gold/10 to-transparent" />
      )}

      {/* Target indicator */}
      {card.targetedBy && (
        <div className="absolute top-1 right-1 w-4 h-4 rounded-full bg-octgn-danger animate-pulse shadow-[0_0_8px_rgba(239,68,68,0.6)]" />
      )}

      {/* Markers overlay */}
      {card.markers.length > 0 && (
        <div className="absolute bottom-0 left-0 right-0 flex flex-wrap gap-0.5 p-1 bg-black/60 backdrop-blur-sm">
          {card.markers.map((marker) => (
            <div
              key={marker.id}
              className="flex items-center gap-0.5 px-1 py-0.5 rounded bg-octgn-surface/80 border border-octgn-border/40"
              title={`${marker.name}: ${marker.count}`}
            >
              {marker.iconUrl ? (
                <img src={marker.iconUrl} alt="" className="w-3 h-3" />
              ) : (
                <span className="w-3 h-3 rounded-full bg-octgn-accent/60 inline-block" />
              )}
              {marker.count > 1 && (
                <span className="text-[9px] font-bold text-octgn-text leading-none">
                  {marker.count}
                </span>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Peeking indicator */}
      {!card.faceUp && card.peekingPlayers.length > 0 && (
        <div className="absolute top-1 left-1">
          <svg className="w-3.5 h-3.5 text-octgn-warning drop-shadow" viewBox="0 0 16 16" fill="currentColor">
            <path d="M8 3C4.5 3 1.7 5.1.3 8c1.4 2.9 4.2 5 7.7 5s6.3-2.1 7.7-5c-1.4-2.9-4.2-5-7.7-5zm0 8.5a3.5 3.5 0 110-7 3.5 3.5 0 010 7zM8 6a2 2 0 100 4 2 2 0 000-4z" />
          </svg>
        </div>
      )}
    </div>
  );
};

export default CardComponent;
