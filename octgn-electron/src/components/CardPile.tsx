import { useMemo } from 'react';
import { Group, Card as CardType } from '../types/game';
import Card from './Card';

interface CardPileProps {
  group: Group;
  cards: CardType[];
  selectedCardIds: number[];
  onCardClick?: (card: CardType) => void;
  onCardDoubleClick?: (card: CardType) => void;
  onCardContextMenu?: (e: React.MouseEvent, card: CardType) => void;
  maxVisible?: number;
  stacked?: boolean;
  fanOut?: boolean;
  className?: string;
}

export default function CardPile({
  group,
  cards,
  selectedCardIds,
  onCardClick,
  onCardDoubleClick,
  onCardContextMenu,
  maxVisible = 20,
  stacked = false,
  fanOut = false,
  className = '',
}: CardPileProps) {
  const visibleCards = useMemo(() => {
    return cards.slice(-maxVisible);
  }, [cards, maxVisible]);

  const getCardStyle = (index: number, total: number): React.CSSProperties => {
    if (fanOut) {
      // Fan out cards horizontally
      const angle = (index - (total - 1) / 2) * 3;
      const offsetX = index * 30;
      const offsetY = Math.abs(index - (total - 1) / 2) * 5;
      return {
        position: 'absolute',
        left: offsetX,
        top: offsetY,
        transform: `rotate(${angle}deg)`,
        zIndex: index,
      };
    } else if (stacked) {
      // Stack cards with slight offset
      const offsetX = index * 0.5;
      const offsetY = index * 0.5;
      return {
        position: 'absolute',
        left: offsetX,
        top: offsetY,
        zIndex: index,
      };
    }
    return {};
  };

  const containerStyle: React.CSSProperties = {
    position: 'relative',
    width: fanOut ? Math.max(200 + (visibleCards.length - 1) * 30, 200) : 200,
    height: 280,
    minHeight: 280,
  };

  return (
    <div className={`card-pile ${className}`}>
      {/* Group header */}
      <div className="flex items-center justify-between mb-2 px-2">
        <h3 className="text-sm font-medium text-white">{group.name}</h3>
        <span className="text-xs text-gray-400">{cards.length} cards</span>
      </div>

      {/* Cards container */}
      <div style={containerStyle} className="overflow-visible">
        {visibleCards.map((card, index) => (
          <div key={card.id} style={getCardStyle(index, visibleCards.length)}>
            <Card
              card={card}
              selected={selectedCardIds.includes(card.id)}
              onClick={() => onCardClick?.(card)}
              onDoubleClick={() => onCardDoubleClick?.(card)}
              onContextMenu={(e) => onCardContextMenu?.(e, card)}
              showFace={group.visibility === 'all' || group.visibility === 'owner'}
            />
          </div>
        ))}

        {cards.length === 0 && (
          <div className="w-full h-full rounded-lg border-2 border-dashed border-gray-600 flex items-center justify-center">
            <span className="text-gray-500 text-sm">Empty</span>
          </div>
        )}
      </div>

      {/* Overflow indicator */}
      {cards.length > maxVisible && (
        <div className="text-xs text-gray-400 text-center mt-1">
          +{cards.length - maxVisible} more
        </div>
      )}
    </div>
  );
}
