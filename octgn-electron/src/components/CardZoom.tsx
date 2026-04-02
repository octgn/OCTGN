import { useEffect, useState } from 'react';
import { Card as CardType } from '../types/game';

interface CardZoomProps {
  card: CardType | null;
  position?: { x: number; y: number };
  onClose?: () => void;
  showActions?: boolean;
  onPlayCard?: (card: CardType) => void;
  onAddMarker?: (card: CardType) => void;
  onSelectAll?: (card: CardType) => void;
}

export default function CardZoom({
  card,
  position,
  onClose,
  showActions = true,
  onPlayCard,
  onAddMarker,
  onSelectAll,
}: CardZoomProps) {
  const [imageLoaded, setImageLoaded] = useState(false);
  const [imageError, setImageError] = useState(false);

  useEffect(() => {
    setImageLoaded(false);
    setImageError(false);
  }, [card?.id]);

  if (!card) return null;

  const zoomWidth = 300;
  const zoomHeight = 420;

  // Calculate position to stay on screen
  const calculatePosition = () => {
    if (!position) {
      return {
        right: 20,
        top: 80,
      };
    }

    const { x, y } = position;
    const windowWidth = window.innerWidth;
    const windowHeight = window.innerHeight;

    let left = x + 20;
    let top = y - zoomHeight / 2;

    // Keep on screen horizontally
    if (left + zoomWidth > windowWidth - 20) {
      left = x - zoomWidth - 20;
    }

    // Keep on screen vertically
    if (top < 20) {
      top = 20;
    } else if (top + zoomHeight > windowHeight - 20) {
      top = windowHeight - zoomHeight - 20;
    }

    return { left, top };
  };

  const pos = calculatePosition();

  return (
    <div
      className="fixed z-50 pointer-events-auto"
      style={pos}
    >
      {/* Card image container */}
      <div
        className="bg-octgn-primary rounded-lg shadow-2xl overflow-hidden border border-octgn-accent"
        style={{ width: zoomWidth }}
      >
        {/* Card image */}
        <div
          className="relative bg-white"
          style={{ width: zoomWidth, height: zoomHeight }}
        >
          {card.faceUp ? (
            <>
              {card.imageUrl && !imageError ? (
                <>
                  <img
                    src={card.imageUrl}
                    alt={card.name}
                    className="w-full h-full object-contain"
                    onLoad={() => setImageLoaded(true)}
                    onError={() => setImageError(true)}
                    style={{ display: imageLoaded ? 'block' : 'none' }}
                  />
                  {!imageLoaded && (
                    <div className="absolute inset-0 flex items-center justify-center bg-gray-200">
                      <span className="text-gray-400">Loading...</span>
                    </div>
                  )}
                </>
              ) : (
                // Placeholder when no image
                <div className="w-full h-full flex flex-col items-center justify-center bg-gray-100 p-4">
                  <span className="text-xl font-bold text-gray-800 text-center mb-2">
                    {card.name}
                  </span>
                  {card.properties && (
                    <div className="text-sm text-gray-600 space-y-1 text-center">
                      {Object.entries(card.properties).map(([key, value]) => (
                        <div key={key}>
                          <span className="font-medium">{key}:</span> {String(value)}
                        </div>
                      ))}
                    </div>
                  )}
                  <div className="absolute bottom-2 right-2 text-xs text-gray-400">
                    #{card.id}
                  </div>
                </div>
              )}
            </>
          ) : (
            // Card back
            <div className="w-full h-full bg-blue-700 flex items-center justify-center">
              <div
                className="w-[calc(100%-16px)] h-[calc(100%-16px)] rounded-lg border-2 border-blue-400"
                style={{
                  backgroundImage:
                    'repeating-linear-gradient(45deg, transparent, transparent 10px, rgba(59, 130, 246, 0.3) 10px, rgba(59, 130, 246, 0.3) 20px)',
                }}
              />
            </div>
          )}

          {/* Card state indicators */}
          {card.targeted && (
            <div className="absolute top-2 right-2 w-6 h-6 rounded-full bg-green-500 animate-pulse flex items-center justify-center">
              🎯
            </div>
          )}
          {card.anchored && (
            <div className="absolute top-2 left-2 w-6 h-6 rounded-full bg-yellow-500 flex items-center justify-center">
              📌
            </div>
          )}
          {card.highlighted && (
            <div
              className="absolute inset-0 pointer-events-none"
              style={{ border: `4px solid ${card.highlighted}` }}
            />
          )}
        </div>

        {/* Card info */}
        <div className="p-3">
          <h4 className="font-bold text-white mb-1">{card.name}</h4>
          {card.properties && Object.keys(card.properties).length > 0 && (
            <div className="text-xs text-gray-400 space-y-0.5 mb-2">
              {Object.entries(card.properties).slice(0, 4).map(([key, value]) => (
                <div key={key} className="flex justify-between">
                  <span>{key}</span>
                  <span>{String(value)}</span>
                </div>
              ))}
            </div>
          )}

          {/* Markers */}
          {card.markers && card.markers.length > 0 && (
            <div className="flex flex-wrap gap-1 mb-2">
              {card.markers.map((marker) => (
                <div
                  key={marker.id}
                  className="bg-octgn-accent px-2 py-0.5 rounded text-xs text-white"
                  title={marker.name}
                >
                  {marker.name}: {marker.count}
                </div>
              ))}
            </div>
          )}

          {/* Actions */}
          {showActions && card.faceUp && (
            <div className="flex flex-wrap gap-1 pt-2 border-t border-octgn-accent">
              <button
                onClick={() => onPlayCard?.(card)}
                className="text-xs bg-octgn-highlight hover:bg-red-600 px-2 py-1 rounded text-white"
              >
                Play
              </button>
              <button
                onClick={() => onAddMarker?.(card)}
                className="text-xs bg-octgn-accent hover:bg-blue-600 px-2 py-1 rounded text-white"
              >
                +Marker
              </button>
              <button
                onClick={() => onSelectAll?.(card)}
                className="text-xs bg-octgn-accent hover:bg-blue-600 px-2 py-1 rounded text-white"
              >
                Select All
              </button>
            </div>
          )}
        </div>

        {/* Close button */}
        {onClose && (
          <button
            onClick={onClose}
            className="absolute top-2 right-2 w-6 h-6 rounded-full bg-black/50 hover:bg-black/70 text-white text-sm flex items-center justify-center"
          >
            ✕
          </button>
        )}
      </div>
    </div>
  );
}
