import { useState, useRef, useEffect } from 'react';
import { Card as CardType } from '../types/game';
import Card from './Card';

interface PlayerHandProps {
  cards: CardType[];
  selectedCardIds: number[];
  onCardClick?: (card: CardType) => void;
  onCardDoubleClick?: (card: CardType) => void;
  onCardContextMenu?: (e: React.MouseEvent, card: CardType) => void;
  maxVisible?: number;
  expanded?: boolean;
  onToggleExpand?: () => void;
}

export default function PlayerHand({
  cards,
  selectedCardIds,
  onCardClick,
  onCardDoubleClick,
  onCardContextMenu,
  maxVisible = 10,
  expanded = false,
  onToggleExpand,
}: PlayerHandProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const [containerWidth, setContainerWidth] = useState(0);

  // Calculate card positions for fan layout
  useEffect(() => {
    const updateWidth = () => {
      if (containerRef.current) {
        setContainerWidth(containerRef.current.offsetWidth);
      }
    };
    updateWidth();
    window.addEventListener('resize', updateWidth);
    return () => window.removeEventListener('resize', updateWidth);
  }, []);

  const visibleCards = cards.slice(0, maxVisible);
  const overflow = cards.length - maxVisible;

  // Calculate overlap based on container width
  const cardWidth = 140;
  const cardHeight = 196;
  const maxOverlap = 40;
  const minOverlap = 10;
  
  const calculateOverlap = () => {
    if (visibleCards.length <= 1) return 0;
    const totalCardsWidth = visibleCards.length * cardWidth;
    const availableWidth = containerWidth - 40;
    const neededOverlap = (totalCardsWidth - availableWidth) / (visibleCards.length - 1);
    return Math.max(minOverlap, Math.min(maxOverlap, neededOverlap));
  };

  const overlap = expanded ? maxOverlap : calculateOverlap();
  const totalWidth = expanded 
    ? visibleCards.length * (cardWidth - overlap) + overlap
    : Math.min(visibleCards.length * (cardWidth - overlap) + overlap, containerWidth);

  const getCardPosition = (index: number) => {
    const centerX = containerWidth / 2;
    const startOffset = centerX - totalWidth / 2;
    
    const x = startOffset + index * (cardWidth - overlap);
    
    // Fan effect - cards curve up slightly
    const centerIndex = (visibleCards.length - 1) / 2;
    const distanceFromCenter = index - centerIndex;
    const rotation = distanceFromCenter * 2;
    const yOffset = Math.abs(distanceFromCenter) * -3;
    
    return { x, rotation, yOffset };
  };

  return (
    <div className="player-hand relative">
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-1 bg-octgn-primary/80 rounded-t-lg">
        <h3 className="text-sm font-medium text-white">
          Hand ({cards.length})
        </h3>
        <div className="flex items-center space-x-2">
          {overflow > 0 && (
            <span className="text-xs text-gray-400">+{overflow} more</span>
          )}
          <button
            onClick={onToggleExpand}
            className="text-gray-400 hover:text-white text-sm"
          >
            {expanded ? '▼' : '▲'}
          </button>
        </div>
      </div>

      {/* Cards container */}
      <div
        ref={containerRef}
        className={`
          relative bg-octgn-accent/30 rounded-lg overflow-visible
          ${expanded ? 'h-56' : 'h-52'}
        `}
      >
        {visibleCards.map((card, index) => {
          const { x, rotation, yOffset } = getCardPosition(index);
          const isSelected = selectedCardIds.includes(card.id);

          return (
            <div
              key={card.id}
              className="absolute transition-all duration-200 ease-out"
              style={{
                left: x,
                bottom: expanded ? 10 : 5,
                transform: `rotate(${rotation}deg) translateY(${yOffset}px)`,
                zIndex: isSelected ? 100 : index + 1,
              }}
            >
              <Card
                card={card}
                selected={isSelected}
                onClick={() => onCardClick?.(card)}
                onDoubleClick={() => onCardDoubleClick?.(card)}
                onContextMenu={(e) => onCardContextMenu?.(e, card)}
                width={cardWidth}
                height={cardHeight}
                showFace={true}
              />
            </div>
          );
        })}

        {cards.length === 0 && (
          <div className="flex items-center justify-center h-full text-gray-400">
            No cards in hand
          </div>
        )}
      </div>
    </div>
  );
}
