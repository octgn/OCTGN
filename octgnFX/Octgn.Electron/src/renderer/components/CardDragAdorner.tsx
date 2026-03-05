import React from 'react';
import { useDragDrop } from './DragDropContext';
import type { DragCardInfo, DraggingCardData } from './DragDropContext';

/**
 * Floating card preview that follows the cursor/finger during drag operations.
 *
 * For single-card drags: renders a card image with glow and rotation.
 * For multi-card drags with draggingCards: renders all cards at their relative positions.
 * Uses `pointer-events: none` so it never intercepts drop targets.
 */
const CardDragAdorner: React.FC = () => {
  const { dragState, isDragging } = useDragDrop();

  if (!isDragging || !dragState.cardInfo) return null;

  const { x, y } = dragState.mousePosition;
  const { grabOffset } = dragState;
  const hasMultiCards = dragState.draggingCards.length > 1;

  // Scale the adorner to a comfortable drag size (slightly smaller than original)
  const adornerScale = 0.85;

  if (hasMultiCards) {
    return <MultiCardAdorner
      cards={dragState.draggingCards}
      cursorX={x}
      cursorY={y}
      grabOffset={grabOffset}
      adornerScale={adornerScale}
    />;
  }

  return <SingleCardAdorner
    cardInfo={dragState.cardInfo}
    cursorX={x}
    cursorY={y}
    grabOffset={grabOffset}
    adornerScale={adornerScale}
  />;
};

/** Single card adorner — original behavior */
const SingleCardAdorner: React.FC<{
  cardInfo: DragCardInfo;
  cursorX: number;
  cursorY: number;
  grabOffset: { x: number; y: number };
  adornerScale: number;
}> = ({ cardInfo, cursorX, cursorY, grabOffset, adornerScale }) => {
  const { imageUrl, name, width, height, faceUp, cardBackUrl } = cardInfo;
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
        transform: `translate(${cursorX - grabOffset.x * adornerScale}px, ${cursorY - grabOffset.y * adornerScale}px) rotate(3deg)`,
        width: `${adornerWidth}px`,
        height: `${adornerHeight}px`,
        pointerEvents: 'none',
        zIndex: 99999,
        willChange: 'transform',
      }}
    >
      {/* Outer glow ring */}
      <div
        className="absolute -inset-3 rounded-xl opacity-60"
        style={{
          background: 'radial-gradient(ellipse at center, rgba(59,130,246,0.35) 0%, rgba(139,92,246,0.15) 50%, transparent 70%)',
          filter: 'blur(8px)',
          animation: 'adorner-pulse 1.5s ease-in-out infinite alternate',
        }}
      />

      {/* Card container */}
      <div
        className="relative w-full h-full overflow-hidden"
        style={{
          borderRadius: '4px',
          boxShadow: [
            '0 20px 40px rgba(0,0,0,0.55)',
            '0 8px 16px rgba(0,0,0,0.35)',
            '0 0 0 1px rgba(255,255,255,0.12)',
            '0 0 24px rgba(59,130,246,0.25)',
            '0 0 48px rgba(139,92,246,0.12)',
          ].join(', '),
        }}
      >
        {showFront && (
          <img src={imageUrl} alt={name} className="w-full h-full object-fill select-none" draggable={false} />
        )}
        {showBack && (
          <img src={cardBackUrl} alt="Face-down card" className="w-full h-full object-fill select-none" draggable={false} />
        )}
        {showPlaceholder && (
          <div className="w-full h-full bg-gradient-to-br from-gray-900/90 to-gray-700/60 flex items-center justify-center">
            <span className="text-[9px] font-semibold text-white/70 text-center leading-tight px-2 line-clamp-3">
              {faceUp ? name : 'OCTGN'}
            </span>
          </div>
        )}

        {/* Shine sweep */}
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

/** Multi-card adorner — renders all cards at their relative table positions */
const MultiCardAdorner: React.FC<{
  cards: DraggingCardData[];
  cursorX: number;
  cursorY: number;
  grabOffset: { x: number; y: number };
  adornerScale: number;
}> = ({ cards, cursorX, cursorY, grabOffset, adornerScale }) => {
  // The primary card (relativeX=0, relativeY=0) anchors to cursor - grabOffset
  const baseX = cursorX - grabOffset.x * adornerScale;
  const baseY = cursorY - grabOffset.y * adornerScale;

  return (
    <div
      data-testid="card-drag-adorner"
      style={{
        position: 'fixed',
        left: 0,
        top: 0,
        pointerEvents: 'none',
        zIndex: 99999,
        willChange: 'transform',
      }}
    >
      {/* Ambient glow for the group */}
      <div
        className="absolute opacity-50"
        style={{
          left: `${baseX - 20}px`,
          top: `${baseY - 20}px`,
          width: '200px',
          height: '200px',
          background: 'radial-gradient(ellipse at center, rgba(251,191,36,0.3) 0%, rgba(59,130,246,0.15) 40%, transparent 70%)',
          filter: 'blur(16px)',
          animation: 'adorner-pulse 1.5s ease-in-out infinite alternate',
        }}
      />

      {cards.map((card) => {
        const cardX = baseX + card.relativeX * adornerScale;
        const cardY = baseY + card.relativeY * adornerScale;
        const { info } = card;
        const w = info.width * adornerScale;
        const h = info.height * adornerScale;

        const showFront = info.faceUp && info.imageUrl;
        const showBack = !info.faceUp && info.cardBackUrl;
        const showPlaceholder = !showFront && !showBack;

        return (
          <div
            key={card.id}
            data-testid={`adorner-card-${card.id}`}
            style={{
              position: 'absolute',
              left: `${cardX}px`,
              top: `${cardY}px`,
              width: `${w}px`,
              height: `${h}px`,
              transform: 'rotate(2deg)',
            }}
          >
            {/* Per-card glow */}
            <div
              className="absolute -inset-2 rounded-lg opacity-40"
              style={{
                background: 'radial-gradient(ellipse at center, rgba(251,191,36,0.25) 0%, transparent 70%)',
                filter: 'blur(6px)',
              }}
            />

            <div
              className="relative w-full h-full overflow-hidden"
              style={{
                borderRadius: '4px',
                boxShadow: [
                  '0 12px 28px rgba(0,0,0,0.5)',
                  '0 4px 10px rgba(0,0,0,0.3)',
                  '0 0 0 1px rgba(255,255,255,0.1)',
                  '0 0 16px rgba(251,191,36,0.2)',
                ].join(', '),
              }}
            >
              {showFront && (
                <img src={info.imageUrl} alt={info.name} className="w-full h-full object-fill select-none" draggable={false} />
              )}
              {showBack && (
                <img src={info.cardBackUrl} alt="Face-down card" className="w-full h-full object-fill select-none" draggable={false} />
              )}
              {showPlaceholder && (
                <div className="w-full h-full bg-gradient-to-br from-gray-900/90 to-gray-700/60 flex items-center justify-center">
                  <span className="text-[9px] font-semibold text-white/70 text-center leading-tight px-2 line-clamp-3">
                    {info.faceUp ? info.name : 'OCTGN'}
                  </span>
                </div>
              )}

              {/* Top-edge specular highlight */}
              <div
                className="absolute top-0 left-0 right-0 h-[2px] pointer-events-none"
                style={{
                  background: 'linear-gradient(90deg, transparent, rgba(255,255,255,0.2) 30%, rgba(255,255,255,0.35) 50%, rgba(255,255,255,0.2) 70%, transparent)',
                }}
              />
            </div>
          </div>
        );
      })}

      <style>{`
        @keyframes adorner-pulse {
          0% { opacity: 0.4; transform: scale(0.95); }
          100% { opacity: 0.7; transform: scale(1.05); }
        }
      `}</style>
    </div>
  );
};

export default CardDragAdorner;
