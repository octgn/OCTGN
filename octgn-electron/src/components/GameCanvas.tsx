import { useEffect, useRef, useCallback, useState } from 'react';
import { useGameStore } from '../stores/gameStore';
import { TableRenderer, RenderCard } from '../utils/TableRenderer';

export default function GameCanvas({ className = '' }: { className?: string }) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const rendererRef = useRef<TableRenderer | null>(null);
  const animationRef = useRef<number>(0);

  const [isPanning, setIsPanning] = useState(false);
  const [lastMousePos, setLastMousePos] = useState({ x: 0, y: 0 });
  const [hoveredCard, setHoveredCard] = useState<number | null>(null);

  const {
    cards,
    panOffset,
    zoom,
    selectedCards,
    setPanOffset,
    setZoom,
    updateCard,
  } = useGameStore();

  // Initialize renderer
  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    rendererRef.current = new TableRenderer(ctx);
  }, []);

  // Handle resize
  useEffect(() => {
    const handleResize = () => {
      const canvas = canvasRef.current;
      if (!canvas) return;

      const container = canvas.parentElement;
      if (!container) return;

      canvas.width = container.clientWidth;
      canvas.height = container.clientHeight;
    };

    handleResize();
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  // Render loop
  useEffect(() => {
    const canvas = canvasRef.current;
    const renderer = rendererRef.current;
    if (!canvas || !renderer) return;

    const render = () => {
      const ctx = canvas.getContext('2d');
      if (!ctx) return;

      // Render background
      renderer.renderBackground(panOffset, zoom);

      // Convert cards to render cards
      const renderCards: RenderCard[] = Array.from(cards.values()).map((card) => ({
        ...card,
        screenX: card.x * zoom,
        screenY: card.y * zoom,
        screenRotation: card.rotation,
        zIndex: 0,
      }));

      // Render cards
      renderCards.forEach((card) => {
        renderer.renderCard(
          card,
          panOffset,
          zoom,
          selectedCards.includes(card.id),
          hoveredCard === card.id
        );
      });

      animationRef.current = requestAnimationFrame(render);
    };

    render();

    return () => {
      cancelAnimationFrame(animationRef.current);
    };
  }, [cards, panOffset, zoom, selectedCards, hoveredCard]);

  // Mouse handlers
  const handleMouseDown = useCallback((e: React.MouseEvent) => {
    if (e.button === 1 || (e.button === 0 && e.altKey)) {
      // Middle click or alt+left click for panning
      setIsPanning(true);
      setLastMousePos({ x: e.clientX, y: e.clientY });
    }
  }, []);

  const handleMouseMove = useCallback(
    (e: React.MouseEvent) => {
      if (isPanning) {
        const dx = e.clientX - lastMousePos.x;
        const dy = e.clientY - lastMousePos.y;
        setPanOffset({ x: panOffset.x + dx, y: panOffset.y + dy });
        setLastMousePos({ x: e.clientX, y: e.clientY });
      } else {
        // Hit test for hover
        const canvas = canvasRef.current;
        if (!canvas) return;

        const rect = canvas.getBoundingClientRect();
        const mouseX = e.clientX - rect.left - panOffset.x;
        const mouseY = e.clientY - rect.top - panOffset.y;

        let found = false;
        for (const card of cards.values()) {
          const cardX = card.x * zoom;
          const cardY = card.y * zoom;
          const cardW = 200 * zoom;
          const cardH = 280 * zoom;

          if (
            mouseX >= cardX &&
            mouseX <= cardX + cardW &&
            mouseY >= cardY &&
            mouseY <= cardY + cardH
          ) {
            setHoveredCard(card.id);
            found = true;
            break;
          }
        }

        if (!found) {
          setHoveredCard(null);
        }
      }
    },
    [isPanning, lastMousePos, panOffset, setPanOffset, cards, zoom]
  );

  const handleMouseUp = useCallback(() => {
    setIsPanning(false);
  }, []);

  const handleWheel = useCallback(
    (e: React.WheelEvent) => {
      e.preventDefault();
      const delta = e.deltaY > 0 ? 0.9 : 1.1;
      const newZoom = Math.max(0.1, Math.min(3, zoom * delta));
      setZoom(newZoom);
    },
    [zoom, setZoom]
  );

  // Keyboard handlers
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === '+' && e.ctrlKey) {
        e.preventDefault();
        setZoom(Math.min(3, zoom * 1.1));
      } else if (e.key === '-' && e.ctrlKey) {
        e.preventDefault();
        setZoom(Math.max(0.1, zoom / 1.1));
      } else if (e.key === '0' && e.ctrlKey) {
        e.preventDefault();
        setZoom(1);
        setPanOffset({ x: 0, y: 0 });
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [zoom, setZoom, setPanOffset]);

  return (
    <canvas
      ref={canvasRef}
      className={`game-table cursor-grab active:cursor-grabbing ${className}`}
      onMouseDown={handleMouseDown}
      onMouseMove={handleMouseMove}
      onMouseUp={handleMouseUp}
      onMouseLeave={handleMouseUp}
      onWheel={handleWheel}
      onContextMenu={(e) => e.preventDefault()}
    />
  );
}

// Hook for external access
export function useGameCanvas() {
  const { panOffset, zoom, setPanOffset, setZoom } = useGameStore();

  const resetView = useCallback(() => {
    setPanOffset({ x: 0, y: 0 });
    setZoom(1);
  }, [setPanOffset, setZoom]);

  const zoomIn = useCallback(() => {
    setZoom(Math.min(3, zoom * 1.2));
  }, [zoom, setZoom]);

  const zoomOut = useCallback(() => {
    setZoom(Math.max(0.1, zoom / 1.2));
  }, [zoom, setZoom]);

  return {
    panOffset,
    zoom,
    setPanOffset,
    setZoom,
    resetView,
    zoomIn,
    zoomOut,
  };
}
