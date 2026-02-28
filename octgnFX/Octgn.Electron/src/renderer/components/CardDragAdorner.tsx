import React from 'react';
import { useDragDrop } from './DragDropContext';

/**
 * Floating card preview that follows the cursor/finger during drag operations.
 *
 * Renders a semi-transparent card image with a dramatic glow, slight rotation,
 * and drop shadow to give visceral feedback that a card is being moved.
 * Uses `pointer-events: none` so it never intercepts drop targets.
 */
const CardDragAdorner: React.FC = () => {
  const { dragState, isDragging } = useDragDrop();

  if (!isDragging || !dragState.cardInfo) return null;

  const { imageUrl, name, width, height, faceUp, cardBackUrl } = dragState.cardInfo;
  const { x, y } = dragState.mousePosition;

  // Scale the adorner to a comfortable drag size (slightly smaller than original)
  const adornerScale = 0.85;
  const adornerWidth = width * adornerScale;
  const adornerHeight = height * adornerScale;

  const showFront = faceUp && imageUrl;
  const showBack = !faceUp && cardBackUrl;
  const showPlaceholder = !showFront && !showBack;

  return (
    <div
      data-testid="card-drag-adorner"
      style={{
        position: 'fixed',
        left: 0,
        top: 0,
        // Translate so the card is centered under cursor, offset slightly down-right
        // for a natural "picked up" feel
        transform: `translate(${x - adornerWidth / 2}px, ${y - adornerHeight / 2}px) rotate(3deg)`,
        width: `${adornerWidth}px`,
        height: `${adornerHeight}px`,
        pointerEvents: 'none',
        zIndex: 99999,
        willChange: 'transform',
      }}
    >
      {/* Outer glow ring — creates a dramatic luminous halo */}
      <div
        className="absolute -inset-3 rounded-xl opacity-60"
        style={{
          background: 'radial-gradient(ellipse at center, rgba(59,130,246,0.35) 0%, rgba(139,92,246,0.15) 50%, transparent 70%)',
          filter: 'blur(8px)',
          animation: 'adorner-pulse 1.5s ease-in-out infinite alternate',
        }}
      />

      {/* Card container with glass border and elevation shadow */}
      <div
        className="relative w-full h-full overflow-hidden"
        style={{
          borderRadius: '4px',
          boxShadow: [
            '0 20px 40px rgba(0,0,0,0.55)',         // deep elevation
            '0 8px 16px rgba(0,0,0,0.35)',           // mid shadow
            '0 0 0 1px rgba(255,255,255,0.12)',      // subtle edge highlight
            '0 0 24px rgba(59,130,246,0.25)',         // blue ambient glow
            '0 0 48px rgba(139,92,246,0.12)',         // purple far glow
          ].join(', '),
        }}
      >
        {/* Card content */}
        {showFront && (
          <img
            src={imageUrl}
            alt={name}
            className="w-full h-full object-fill select-none"
            draggable={false}
          />
        )}

        {showBack && (
          <img
            src={cardBackUrl}
            alt="Face-down card"
            className="w-full h-full object-fill select-none"
            draggable={false}
          />
        )}

        {showPlaceholder && (
          <div className="w-full h-full bg-gradient-to-br from-gray-900/90 to-gray-700/60 flex items-center justify-center">
            <span className="text-[9px] font-semibold text-white/70 text-center leading-tight px-2 line-clamp-3">
              {name}
            </span>
          </div>
        )}

        {/* Shine sweep — an animated light streak across the card surface */}
        <div
          className="absolute inset-0 pointer-events-none"
          style={{
            background: 'linear-gradient(105deg, transparent 40%, rgba(255,255,255,0.08) 45%, rgba(255,255,255,0.15) 50%, rgba(255,255,255,0.08) 55%, transparent 60%)',
            animation: 'adorner-shine 2s ease-in-out infinite',
          }}
        />

        {/* Top-edge specular highlight */}
        <div
          className="absolute top-0 left-0 right-0 h-[2px] pointer-events-none"
          style={{
            background: 'linear-gradient(90deg, transparent, rgba(255,255,255,0.25) 30%, rgba(255,255,255,0.4) 50%, rgba(255,255,255,0.25) 70%, transparent)',
          }}
        />
      </div>

      {/* Inline keyframes (injected once) */}
      <style>{`
        @keyframes adorner-pulse {
          0% { opacity: 0.4; transform: scale(0.95); }
          100% { opacity: 0.7; transform: scale(1.05); }
        }
        @keyframes adorner-shine {
          0% { transform: translateX(-100%); }
          100% { transform: translateX(100%); }
        }
      `}</style>
    </div>
  );
};

export default CardDragAdorner;
