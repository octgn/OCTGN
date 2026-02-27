import { useEffect, useRef, useState, useCallback } from 'react';
import { useParams } from 'react-router-dom';
import { useGameStore } from '../stores/gameStore';
import { Card as CardType } from '../types/game';

interface TableCard extends CardType {
  screenX: number;
  screenY: number;
  screenRotation: number;
}

export default function GameTablePage() {
  const { gameId } = useParams();
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [tableCards, setTableCards] = useState<TableCard[]>([]);
  const [isDragging, setIsDragging] = useState(false);
  const [dragStart, setDragStart] = useState({ x: 0, y: 0 });
  const [draggedCards, setDraggedCards] = useState<number[]>([]);
  const [panOffset, setPanOffset] = useState({ x: 0, y: 0 });
  const [zoom, setZoom] = useState(1);
  const [contextMenu, setContextMenu] = useState<{
    x: number;
    y: number;
    cards: number[];
  } | null>(null);

  const {
    cards,
    groups,
    selectedCards,
    selectCards,
    clearSelection,
    moveCards,
    turnCard,
    rotateCard,
    zoomCard,
    showChat,
    chatMessages,
    sendChat,
    chatInput,
    setChatInput,
  } = useGameStore();

  // Card dimensions
  const CARD_WIDTH = 200;
  const CARD_HEIGHT = 280;
  const CARD_RADIUS = 10;

  // Render table
  const render = useCallback(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const { width, height } = canvas.getBoundingClientRect();
    canvas.width = width * window.devicePixelRatio;
    canvas.height = height * window.devicePixelRatio;
    ctx.scale(window.devicePixelRatio, window.devicePixelRatio);

    // Clear canvas
    ctx.fillStyle = '#1a1a2e';
    ctx.fillRect(0, 0, width, height);

    // Draw table background
    ctx.fillStyle = '#16213e';
    ctx.fillRect(20, 20, width - 40, height - 40);

    // Draw grid
    ctx.strokeStyle = '#0f3460';
    ctx.lineWidth = 1;
    const gridSize = 50 * zoom;
    for (let x = (panOffset.x % gridSize); x < width; x += gridSize) {
      ctx.beginPath();
      ctx.moveTo(x, 0);
      ctx.lineTo(x, height);
      ctx.stroke();
    }
    for (let y = (panOffset.y % gridSize); y < height; y += gridSize) {
      ctx.beginPath();
      ctx.moveTo(0, y);
      ctx.lineTo(width, y);
      ctx.stroke();
    }

    // Draw cards
    tableCards.forEach((card) => {
      const isSelected = selectedCards.includes(card.id);
      const isHovered = false; // TODO

      drawCard(ctx, card, isSelected, isHovered);
    });

    // Draw selection box if dragging
    if (isDragging && draggedCards.length === 0) {
      ctx.strokeStyle = '#e94560';
      ctx.lineWidth = 2;
      ctx.setLineDash([5, 5]);
      const x = Math.min(dragStart.x, panOffset.x);
      const y = Math.min(dragStart.y, panOffset.y);
      const w = Math.abs(panOffset.x - dragStart.x);
      const h = Math.abs(panOffset.y - dragStart.y);
      ctx.strokeRect(x, y, w, h);
      ctx.setLineDash([]);
    }
  }, [tableCards, selectedCards, isDragging, dragStart, panOffset, zoom]);

  // Draw a single card
  const drawCard = (
    ctx: CanvasRenderingContext2D,
    card: TableCard,
    isSelected: boolean,
    isHovered: boolean
  ) => {
    const x = card.screenX + panOffset.x;
    const y = card.screenY + panOffset.y;
    const w = CARD_WIDTH * zoom;
    const h = CARD_HEIGHT * zoom;

    ctx.save();
    ctx.translate(x + w / 2, y + h / 2);
    ctx.rotate((card.screenRotation * Math.PI) / 180);

    // Card shadow
    ctx.shadowColor = 'rgba(0, 0, 0, 0.3)';
    ctx.shadowBlur = 10;
    ctx.shadowOffsetX = 3;
    ctx.shadowOffsetY = 3;

    // Card background
    ctx.beginPath();
    ctx.roundRect(-w / 2, -h / 2, w, h, CARD_RADIUS * zoom);
    ctx.fillStyle = card.faceUp ? '#ffffff' : '#2563eb';
    ctx.fill();

    // Selection highlight
    if (isSelected) {
      ctx.strokeStyle = '#e94560';
      ctx.lineWidth = 3;
      ctx.stroke();
    } else if (isHovered) {
      ctx.strokeStyle = '#10b981';
      ctx.lineWidth = 2;
      ctx.stroke();
    }

    ctx.shadowColor = 'transparent';

    // Card content
    if (card.faceUp) {
      // Draw card image or placeholder
      if (card.imageUrl) {
        // TODO: Load and draw image
        drawCardPlaceholder(ctx, card, w, h);
      } else {
        drawCardPlaceholder(ctx, card, w, h);
      }
    } else {
      // Card back pattern
      ctx.fillStyle = '#1d4ed8';
      ctx.beginPath();
      ctx.roundRect(-w / 2 + 5, -h / 2 + 5, w - 10, h - 10, CARD_RADIUS * zoom - 2);
      ctx.fill();

      // Back pattern
      ctx.strokeStyle = '#3b82f6';
      ctx.lineWidth = 2;
      for (let i = -w / 2 + 15; i < w / 2 - 10; i += 15) {
        ctx.beginPath();
        ctx.moveTo(i, -h / 2 + 15);
        ctx.lineTo(i, h / 2 - 15);
        ctx.stroke();
      }
    }

    // Markers
    if (card.markers && card.markers.length > 0) {
      const markerSize = 20 * zoom;
      let mx = -w / 2 + 5;
      const my = h / 2 - markerSize - 5;
      card.markers.forEach((marker) => {
        ctx.fillStyle = '#ef4444';
        ctx.beginPath();
        ctx.arc(mx + markerSize / 2, my + markerSize / 2, markerSize / 2, 0, Math.PI * 2);
        ctx.fill();
        ctx.fillStyle = '#ffffff';
        ctx.font = `${12 * zoom}px sans-serif`;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(String(marker.count), mx + markerSize / 2, my + markerSize / 2);
        mx += markerSize + 2;
      });
    }

    ctx.restore();
  };

  // Draw card placeholder
  const drawCardPlaceholder = (
    ctx: CanvasRenderingContext2D,
    card: TableCard,
    w: number,
    h: number
  ) => {
    ctx.fillStyle = '#f3f4f6';
    ctx.beginPath();
    ctx.roundRect(-w / 2 + 5, -h / 2 + 5, w - 10, h - 10, CARD_RADIUS * zoom - 2);
    ctx.fill();

    // Card name
    ctx.fillStyle = '#1f2937';
    ctx.font = `bold ${14 * zoom}px sans-serif`;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'top';
    const name = card.name || `Card ${card.id}`;
    ctx.fillText(name.substring(0, 20), 0, -h / 2 + 15);

    // Card ID (debug)
    ctx.fillStyle = '#9ca3af';
    ctx.font = `${10 * zoom}px monospace`;
    ctx.fillText(`#${card.id}`, 0, h / 2 - 20);
  };

  // Handle window resize
  useEffect(() => {
    const handleResize = () => render();
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, [render]);

  // Render on state change
  useEffect(() => {
    render();
  }, [render]);

  // Mouse handlers
  const handleMouseDown = (e: React.MouseEvent) => {
    const rect = canvasRef.current?.getBoundingClientRect();
    if (!rect) return;

    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    // Check if clicking on a card
    const clickedCard = getCardAtPosition(x, y);

    if (clickedCard) {
      if (e.ctrlKey || e.metaKey) {
        // Add to selection
        if (selectedCards.includes(clickedCard.id)) {
          selectCards(selectedCards.filter((id) => id !== clickedCard.id));
        } else {
          selectCards([...selectedCards, clickedCard.id]);
        }
      } else if (!selectedCards.includes(clickedCard.id)) {
        selectCards([clickedCard.id]);
      }
      setIsDragging(true);
      setDraggedCards(selectedCards.length > 0 ? selectedCards : [clickedCard.id]);
      setDragStart({ x, y });
    } else {
      // Start selection box or pan
      clearSelection();
      setIsDragging(true);
      setDraggedCards([]);
      setDragStart({ x, y });
    }

    setContextMenu(null);
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!isDragging) return;

    const rect = canvasRef.current?.getBoundingClientRect();
    if (!rect) return;

    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    if (draggedCards.length > 0) {
      // Move cards
      const dx = x - dragStart.x;
      const dy = y - dragStart.y;

      setTableCards((cards) =>
        cards.map((card) => {
          if (draggedCards.includes(card.id)) {
            return {
              ...card,
              screenX: card.screenX + dx,
              screenY: card.screenY + dy,
            };
          }
          return card;
        })
      );

      setDragStart({ x, y });
    } else {
      // Update selection box endpoint
      setPanOffset({ x, y });
    }
  };

  const handleMouseUp = (e: React.MouseEvent) => {
    if (isDragging && draggedCards.length > 0) {
      // Send move to server
      // moveCards(draggedCards, ...);
    }
    setIsDragging(false);
    setDraggedCards([]);
  };

  const handleContextMenu = (e: React.MouseEvent) => {
    e.preventDefault();
    const rect = canvasRef.current?.getBoundingClientRect();
    if (!rect) return;

    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    const clickedCard = getCardAtPosition(x, y);
    if (clickedCard) {
      if (!selectedCards.includes(clickedCard.id)) {
        selectCards([clickedCard.id]);
      }
      setContextMenu({ x: e.clientX, y: e.clientY, cards: selectedCards.length > 0 ? selectedCards : [clickedCard.id] });
    }
  };

  const getCardAtPosition = (x: number, y: number): TableCard | null => {
    // Check cards in reverse order (top cards first)
    for (let i = tableCards.length - 1; i >= 0; i--) {
      const card = tableCards[i];
      const cx = card.screenX + panOffset.x;
      const cy = card.screenY + panOffset.y;
      const w = CARD_WIDTH * zoom;
      const h = CARD_HEIGHT * zoom;

      if (x >= cx && x <= cx + w && y >= cy && y <= cy + h) {
        return card;
      }
    }
    return null;
  };

  // Zoom handler
  const handleWheel = (e: React.WheelEvent) => {
    if (e.ctrlKey || e.metaKey) {
      e.preventDefault();
      const delta = e.deltaY > 0 ? 0.9 : 1.1;
      setZoom((z) => Math.max(0.25, Math.min(3, z * delta)));
    }
  };

  // Add demo cards
  useEffect(() => {
    const demoCards: TableCard[] = [
      { id: 1, modelId: 'card1', groupId: 0, x: 100, y: 100, screenX: 100, screenY: 100, screenRotation: 0, width: CARD_WIDTH, height: CARD_HEIGHT, faceUp: true, rotation: 0, ownerId: '1', name: 'Lightning Bolt', properties: {}, markers: [], anchored: false, targeted: false },
      { id: 2, modelId: 'card2', groupId: 0, x: 350, y: 100, screenX: 350, screenY: 100, screenRotation: 0, width: CARD_WIDTH, height: CARD_HEIGHT, faceUp: true, rotation: 0, ownerId: '1', name: 'Giant Growth', properties: {}, markers: [], anchored: false, targeted: false },
      { id: 3, modelId: 'card3', groupId: 0, x: 600, y: 100, screenX: 600, screenY: 100, screenRotation: 0, width: CARD_WIDTH, height: CARD_HEIGHT, faceUp: false, rotation: 0, ownerId: '1', name: 'Mystery Card', properties: {}, markers: [], anchored: false, targeted: false },
      { id: 4, modelId: 'card4', groupId: 0, x: 200, y: 400, screenX: 200, screenY: 400, screenRotation: 90, width: CARD_WIDTH, height: CARD_HEIGHT, faceUp: true, rotation: 90, ownerId: '1', name: 'Serra Angel', properties: {}, markers: [{ id: 'm1', name: '+1/+1', count: 3 }], anchored: false, targeted: false },
    ];
    setTableCards(demoCards);
  }, []);

  return (
    <div className="h-full flex flex-col">
      {/* Toolbar */}
      <div className="bg-octgn-primary p-2 flex items-center space-x-2 border-b border-octgn-accent">
        <button className="btn btn-secondary text-sm">🎴 Load Deck</button>
        <button className="btn btn-secondary text-sm">🔀 Shuffle</button>
        <button className="btn btn-secondary text-sm">↩️ Reset</button>
        <div className="flex-1" />
        <span className="text-sm text-gray-400">Zoom: {Math.round(zoom * 100)}%</span>
        <button onClick={() => setZoom(1)} className="btn btn-secondary text-sm">100%</button>
        <button onClick={() => setZoom((z) => Math.max(0.25, z - 0.25))} className="btn btn-secondary text-sm">-</button>
        <button onClick={() => setZoom((z) => Math.min(3, z + 0.25))} className="btn btn-secondary text-sm">+</button>
      </div>

      {/* Main game area */}
      <div className="flex-1 flex">
        {/* Canvas */}
        <div className="flex-1 relative">
          <canvas
            ref={canvasRef}
            className="w-full h-full cursor-crosshair"
            onMouseDown={handleMouseDown}
            onMouseMove={handleMouseMove}
            onMouseUp={handleMouseUp}
            onContextMenu={handleContextMenu}
            onWheel={handleWheel}
          />

          {/* Context Menu */}
          {contextMenu && (
            <div
              className="absolute bg-octgn-primary border border-octgn-accent rounded-lg shadow-lg py-2 min-w-[150px]"
              style={{ left: contextMenu.x, top: contextMenu.y }}
            >
              <button
                className="w-full px-4 py-2 text-left hover:bg-octgn-accent/50"
                onClick={() => {
                  contextMenu.cards.forEach((id) => turnCard(id, true));
                  setContextMenu(null);
                }}
              >
                👁️ Flip Face Up
              </button>
              <button
                className="w-full px-4 py-2 text-left hover:bg-octgn-accent/50"
                onClick={() => {
                  contextMenu.cards.forEach((id) => turnCard(id, false));
                  setContextMenu(null);
                }}
              >
                🔽 Flip Face Down
              </button>
              <hr className="border-octgn-accent my-1" />
              <button
                className="w-full px-4 py-2 text-left hover:bg-octgn-accent/50"
                onClick={() => {
                  contextMenu.cards.forEach((id) => rotateCard(id, 90));
                  setContextMenu(null);
                }}
              >
                🔄 Rotate 90°
              </button>
              <button
                className="w-full px-4 py-2 text-left hover:bg-octgn-accent/50"
                onClick={() => {
                  contextMenu.cards.forEach((id) => rotateCard(id, 180));
                  setContextMenu(null);
                }}
              >
                🔄 Rotate 180°
              </button>
            </div>
          )}
        </div>

        {/* Chat Panel */}
        {showChat && (
          <div className="w-72 bg-octgn-primary border-l border-octgn-accent flex flex-col">
            <div className="p-2 border-b border-octgn-accent">
              <h3 className="font-bold text-white">Chat</h3>
            </div>
            <div className="flex-1 overflow-y-auto p-2 space-y-1">
              {chatMessages.map((msg) => (
                <div key={msg.id} className={`text-sm ${msg.isSystem ? 'text-gray-400 italic' : ''}`}>
                  <span className="font-bold text-octgn-highlight">{msg.playerName}: </span>
                  {msg.message}
                </div>
              ))}
            </div>
            <div className="p-2 border-t border-octgn-accent">
              <form
                onSubmit={(e) => {
                  e.preventDefault();
                  if (chatInput.trim()) {
                    sendChat(chatInput);
                    setChatInput('');
                  }
                }}
              >
                <input
                  type="text"
                  value={chatInput}
                  onChange={(e) => setChatInput(e.target.value)}
                  placeholder="Type a message..."
                  className="input w-full text-sm"
                />
              </form>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
