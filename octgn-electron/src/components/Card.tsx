import { useState } from 'react';
import { Card as CardType } from '../types/game';

interface CardProps {
  card: CardType;
  size?: 'sm' | 'md' | 'lg';
  selected?: boolean;
  onClick?: () => void;
  onDoubleClick?: () => void;
  onContextMenu?: (e: React.MouseEvent) => void;
  style?: React.CSSProperties;
  className?: string;
}

export default function Card({
  card,
  size = 'md',
  selected = false,
  onClick,
  onDoubleClick,
  onContextMenu,
  style,
  className = '',
}: CardProps) {
  const [imageError, setImageError] = useState(false);
  const [isHovered, setIsHovered] = useState(false);

  const sizeClasses = {
    sm: 'w-[100px] h-[140px]',
    md: 'w-[200px] h-[280px]',
    lg: 'w-[300px] h-[420px]',
  };

  const imageUrl = card.faceUp && !imageError && card.imageUrl 
    ? card.imageUrl 
    : null;

  return (
    <div
      className={`
        game-card ${sizeClasses[size]}
        ${selected ? 'game-card-selected' : ''}
        ${isHovered ? 'hover-lift' : ''}
        ${className}
      `}
      style={{
        transform: `rotate(${card.rotation}deg)`,
        ...style,
      }}
      onClick={onClick}
      onDoubleClick={onDoubleClick}
      onContextMenu={onContextMenu}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      {/* Card Content */}
      {card.faceUp && imageUrl ? (
        <img
          src={imageUrl}
          alt={card.name}
          className="w-full h-full object-cover"
          onError={() => setImageError(true)}
          draggable={false}
        />
      ) : card.faceUp ? (
        // Face-up placeholder with card info
        <div className="w-full h-full flex flex-col items-center justify-center p-4 bg-gradient-to-br from-octgn-primary to-octgn-dark">
          <span className="text-4xl mb-2">🃏</span>
          <p className="text-white font-bold text-center text-sm">{card.name}</p>
          {card.properties?.type && (
            <p className="text-gray-400 text-xs mt-1">{card.properties.type}</p>
          )}
          {card.properties?.cost && (
            <p className="text-octgn-highlight text-xs mt-1 font-mono">
              {card.properties.cost}
            </p>
          )}
        </div>
      ) : (
        // Face-down card back
        <div className="w-full h-full game-card-face-down">
          <div className="absolute inset-4 border border-octgn-accent/30 rounded-lg" />
        </div>
      )}

      {/* Selection glow */}
      {selected && (
        <div className="absolute inset-0 ring-2 ring-octgn-highlight rounded-xl pointer-events-none" />
      )}

      {/* Hover overlay */}
      {isHovered && !selected && (
        <div className="absolute inset-0 bg-octgn-highlight/10 rounded-xl pointer-events-none" />
      )}

      {/* Target indicator */}
      {card.targeted && (
        <div className="absolute top-2 right-2 w-5 h-5 bg-red-500 rounded-full flex items-center justify-center shadow-lg animate-pulse">
          <span className="text-xs">🎯</span>
        </div>
      )}

      {/* Highlight color */}
      {card.highlighted && (
        <div
          className="absolute inset-0 rounded-xl pointer-events-none"
          style={{
            boxShadow: `inset 0 0 20px ${card.highlighted}40, 0 0 10px ${card.highlighted}30`,
          }}
        />
      )}

      {/* Markers */}
      {card.markers && card.markers.length > 0 && (
        <div className="absolute bottom-2 left-2 flex flex-wrap gap-1">
          {card.markers.map((marker) => (
            <div
              key={marker.id}
              className="card-marker"
              title={`${marker.name}: ${marker.count}`}
            >
              {marker.count > 0 ? marker.count : marker.name[0]}
            </div>
          ))}
        </div>
      )}

      {/* Anchored indicator */}
      {card.anchored && (
        <div className="absolute top-2 left-2 text-2xl drop-shadow-lg">⚓</div>
      )}
    </div>
  );
}

// Minimal card for lists
export function CardThumbnail({ card, onClick }: { card: CardType; onClick?: () => void }) {
  return (
    <div
      className="w-12 h-16 rounded-md overflow-hidden cursor-pointer hover:ring-2 hover:ring-octgn-highlight transition-all"
      onClick={onClick}
    >
      {card.faceUp ? (
        <div className="w-full h-full bg-octgn-primary flex items-center justify-center">
          <span className="text-xl">🃏</span>
        </div>
      ) : (
        <div className="w-full h-full game-card-face-down" />
      )}
    </div>
  );
}

// Card in pile (stacked view)
export function CardStack({ count, faceDown = true }: { count: number; faceDown?: boolean }) {
  const displayCount = Math.min(count, 5);
  
  return (
    <div className="relative w-[200px] h-[280px]">
      {Array.from({ length: displayCount }).map((_, i) => (
        <div
          key={i}
          className={`
            absolute game-card
            ${faceDown ? 'game-card-face-down' : ''}
          `}
          style={{
            top: i * 2,
            left: i * 0.5,
            zIndex: i,
          }}
        />
      ))}
    </div>
  );
}
