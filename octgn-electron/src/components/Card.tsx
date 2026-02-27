import { useState, useCallback } from 'react';
import { Card as CardType } from '../types/game';

interface CardProps {
  card: CardType;
  selected?: boolean;
  onClick?: () => void;
  onDoubleClick?: () => void;
  onContextMenu?: (e: React.MouseEvent) => void;
  width?: number;
  height?: number;
  showFace?: boolean;
  className?: string;
}

export default function Card({
  card,
  selected = false,
  onClick,
  onDoubleClick,
  onContextMenu,
  width = 200,
  height = 280,
  showFace = true,
  className = '',
}: CardProps) {
  const [imageLoaded, setImageLoaded] = useState(false);
  const [imageError, setImageError] = useState(false);

  const faceUp = showFace && card.faceUp;
  const rotation = card.rotation || 0;

  // Determine card dimensions based on rotation
  const isRotated = rotation === 90 || rotation === 270;
  const displayWidth = isRotated ? height : width;
  const displayHeight = isRotated ? width : height;

  const handleImageLoad = useCallback(() => {
    setImageLoaded(true);
  }, []);

  const handleImageError = useCallback(() => {
    setImageError(true);
  }, []);

  const style: React.CSSProperties = {
    width: displayWidth,
    height: displayHeight,
    transform: `rotate(${rotation}deg)`,
    transition: 'transform 0.2s ease',
  };

  return (
    <div
      className={`
        card relative cursor-pointer select-none
        ${selected ? 'card-selected' : ''}
        ${className}
      `}
      style={style}
      onClick={onClick}
      onDoubleClick={onDoubleClick}
      onContextMenu={onContextMenu}
    >
      {/* Card container */}
      <div
        className="w-full h-full rounded-lg overflow-hidden shadow-lg bg-white relative"
        style={{ borderRadius: 10 }}
      >
        {faceUp ? (
          <>
            {/* Card front */}
            {card.imageUrl && !imageError ? (
              <>
                <img
                  src={card.imageUrl}
                  alt={card.name}
                  className="w-full h-full object-cover"
                  onLoad={handleImageLoad}
                  onError={handleImageError}
                  style={{ display: imageLoaded ? 'block' : 'none' }}
                />
                {!imageLoaded && (
                  <div className="w-full h-full flex items-center justify-center bg-gray-200">
                    <span className="text-gray-400">Loading...</span>
                  </div>
                )}
              </>
            ) : (
              // Placeholder
              <div className="w-full h-full flex flex-col items-center justify-center bg-gray-100 p-2">
                <span className="text-gray-800 text-center font-medium text-sm">
                  {card.name}
                </span>
                {card.properties && Object.keys(card.properties).length > 0 && (
                  <div className="mt-2 text-xs text-gray-500 text-center">
                    {Object.entries(card.properties).slice(0, 3).map(([key, value]) => (
                      <div key={key}>{value}</div>
                    ))}
                  </div>
                )}
              </div>
            )}

            {/* Markers */}
            {card.markers && card.markers.length > 0 && (
              <div className="absolute bottom-2 left-2 flex gap-1">
                {card.markers.map((marker) => (
                  <div
                    key={marker.id}
                    className="w-6 h-6 rounded-full bg-red-500 text-white text-xs flex items-center justify-center font-bold shadow-md"
                    title={marker.name}
                  >
                    {marker.count}
                  </div>
                ))}
              </div>
            )}

            {/* Highlight */}
            {card.highlighted && (
              <div
                className="absolute inset-0 pointer-events-none"
                style={{
                  border: `3px solid ${card.highlighted}`,
                  borderRadius: 10,
                }}
              />
            )}

            {/* Target indicator */}
            {card.targeted && (
              <div className="absolute top-2 right-2 w-4 h-4 rounded-full bg-green-500 animate-pulse" />
            )}
          </>
        ) : (
          // Card back
          <div className="w-full h-full bg-blue-700 flex items-center justify-center">
            <div
              className="w-[calc(100%-16px)] h-[calc(100%-16px)] rounded-lg border-2 border-blue-400"
              style={{
                backgroundImage: 'repeating-linear-gradient(45deg, transparent, transparent 10px, rgba(59, 130, 246, 0.3) 10px, rgba(59, 130, 246, 0.3) 20px)',
              }}
            />
          </div>
        )}

        {/* Anchored indicator */}
        {card.anchored && (
          <div className="absolute top-2 left-2 w-4 h-4 rounded-full bg-yellow-500 flex items-center justify-center">
            📌
          </div>
        )}
      </div>

      {/* Selection glow */}
      {selected && (
        <div
          className="absolute inset-0 rounded-lg pointer-events-none"
          style={{
            boxShadow: '0 0 20px 5px rgba(233, 69, 96, 0.6)',
            borderRadius: 10,
          }}
        />
      )}
    </div>
  );
}
