import { useRef, useEffect, useState, useCallback } from 'react';
import { Card as CardType, Group } from '../types/game';
import { TableRenderer, RenderCard } from '../utils/TableRenderer';
import { useGameStore } from '../stores/gameStore';

interface GameCanvasProps {
  className?: string;
}

export function useGameCanvas() {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const rendererRef = useRef<TableRenderer | null>(null);
  const animationFrameRef = useRef<number>(0);

  const {
    cards,
    groups,
    panOffset,
    zoom,
    selectedCards,
    hoveredCard,
    setPanOffset,
    setZoom,
    selectCards,
    clearSelection,
    setHoveredCard,
  } = useGameStore();

  // Initialize renderer
  useEffect(() => {
    if (canvasRef.current && !rendererRef.current) {
      const ctx = canvasRef.current.getContext('2d');
      if (ctx) {
        rendererRef.current = new TableRenderer(ctx);
      }
    }
  }, []);

  // Convert cards to render cards
  const getRenderCards = useCallback((): RenderCard[] => {
    return Array.from(cards.values()).map((card) => ({
      ...card,
      screenX: card.x,
      screenY: card.y,
      screenRotation: card.rotation,
      zIndex: 0,
    }));
  }, [cards]);

  // Main render loop
  const render = useCallback(() => {
    const renderer = rendererRef.current;
    const canvas = canvasRef.current;
    if (!renderer || !canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    // Handle resize
    const rect = canvas.getBoundingClientRect();
    const dpr = window.devicePixelRatio || 1;
    canvas.width = rect.width * dpr;
    canvas.height = rect.height * dpr;
    ctx.scale(dpr, dpr);

    // Render background
    renderer.renderBackground(panOffset, zoom);

    // Render groups (areas)
    const tableGroups = Array.from(groups.values()).filter(
      (g) => g.type === 'table' && g.x !== undefined
    );
    tableGroups.forEach((group) => {
      renderer.renderGroup(group, panOffset, zoom, false);
    });

    // Render cards
    const renderCards = getRenderCards();
    renderCards.forEach((card) => {
      const isSelected = selectedCards.includes(card.id);
      const isHovered = hoveredCard === card.id;
      renderer.renderCard(card, panOffset, zoom, isSelected, isHovered);
    });

    animationFrameRef.current = requestAnimationFrame(render);
  }, [panOffset, zoom, selectedCards, hoveredCard, getRenderCards, groups]);

  // Start render loop
  useEffect(() => {
    animationFrameRef.current = requestAnimationFrame(render);
    return () => cancelAnimationFrame(animationFrameRef.current);
  }, [render]);

  // Hit testing
  const getCardAtPoint = useCallback(
    (clientX: number, clientY: number): CardType | null => {
      const canvas = canvasRef.current;
      const renderer = rendererRef.current;
      if (!canvas || !renderer) return null;

      const rect = canvas.getBoundingClientRect();
      const x = clientX - rect.left;
      const y = clientY - rect.top;

      const renderCards = getRenderCards();
      // Check in reverse order (top cards first)
      for (let i = renderCards.length - 1; i >= 0; i--) {
        const card = renderCards[i];
        if (renderer.hitTestCard(card, x, y, panOffset, zoom)) {
          return cards.get(card.id) || null;
        }
      }
      return null;
    },
    [getRenderCards, panOffset, zoom, cards]
  );

  // Pan handling
  const [isPanning, setIsPanning] = useState(false);
  const [panStart, setPanStart] = useState({ x: 0, y: 0 });

  const handleMouseDown = useCallback(
    (e: React.MouseEvent) => {
      if (e.button === 1 || (e.button === 0 && e.altKey)) {
        // Middle click or Alt+click to pan
        setIsPanning(true);
        setPanStart({ x: e.clientX - panOffset.x, y: e.clientY - panOffset.y });
        e.preventDefault();
      } else if (e.button === 0) {
        const card = getCardAtPoint(e.clientX, e.clientY);
        if (card) {
          // Card click handled by parent
        } else {
          // Clear selection if clicking empty space
          clearSelection();
        }
      }
    },
    [panOffset, getCardAtPoint, clearSelection]
  );

  const handleMouseMove = useCallback(
    (e: React.MouseEvent) => {
      if (isPanning) {
        setPanOffset({
          x: e.clientX - panStart.x,
          y: e.clientY - panStart.y,
        });
      } else {
        // Hover detection
        const card = getCardAtPoint(e.clientX, e.clientY);
        setHoveredCard(card?.id ?? null);
      }
    },
    [isPanning, panStart, setPanOffset, getCardAtPoint, setHoveredCard]
  );

  const handleMouseUp = useCallback(() => {
    setIsPanning(false);
  }, []);

  // Zoom handling
  const handleWheel = useCallback(
    (e: React.WheelEvent) => {
      if (e.ctrlKey || e.metaKey) {
        e.preventDefault();
        const delta = e.deltaY > 0 ? 0.9 : 1.1;
        setZoom(zoom * delta);
      }
    },
    [zoom, setZoom]
  );

  return {
    canvasRef,
    renderer: rendererRef.current,
    getCardAtPoint,
    handleMouseDown,
    handleMouseMove,
    handleMouseUp,
    handleWheel,
  };
}

export default function GameCanvas({ className = '' }: GameCanvasProps) {
  const {
    canvasRef,
    handleMouseDown,
    handleMouseMove,
    handleMouseUp,
    handleWheel,
  } = useGameCanvas();

  return (
    <canvas
      ref={canvasRef}
      className={`w-full h-full cursor-crosshair ${className}`}
      onMouseDown={handleMouseDown}
      onMouseMove={handleMouseMove}
      onMouseUp={handleMouseUp}
      onMouseLeave={handleMouseUp}
      onWheel={handleWheel}
    />
  );
}
