import { describe, it, expect, vi, afterEach, beforeEach } from 'vitest';
import { render, screen, cleanup, act } from '@testing-library/react';
import React from 'react';

// Mock window.octgn
(window as any).octgn = {
  login: vi.fn(), logout: vi.fn(), getSession: vi.fn(), getGames: vi.fn(),
  hostGame: vi.fn(), joinGame: vi.fn(), leaveGame: vi.fn(),
  minimize: vi.fn(), maximize: vi.fn(), quit: vi.fn(), getVersion: vi.fn(),
  onGameStateUpdate: vi.fn(() => vi.fn()), gameAction: vi.fn(),
  gameChat: vi.fn(), loadDeck: vi.fn(), openFileDialog: vi.fn(),
};

import { DragDropProvider, useDragDrop, type DraggingCardData } from '@renderer/components/DragDropContext';
import CardDragAdorner from '@renderer/components/CardDragAdorner';
import TouchDragLayer from '@renderer/components/TouchDragLayer';

// ---------------------------------------------------------------------------
// Helper to trigger drag state from inside the provider
// ---------------------------------------------------------------------------

/** Renders the adorner inside a DragDropProvider with a button that triggers drag */
function renderWithDragControl(props: {
  cardInfo?: DragCardInfo;
  grabOffset?: { x: number; y: number };
  draggingCards?: DraggingCardData[];
  allCardIds?: string[];
} = {}) {
  const cardInfo: DragCardInfo = props.cardInfo ?? {
    imageUrl: 'octgn-asset://card/test.png',
    name: 'Dragon Knight',
    width: 100,
    height: 140,
    faceUp: true,
  };
  const grabOffset = props.grabOffset ?? { x: 50, y: 70 }; // default: center of 100x140

  let dragActions: ReturnType<typeof useDragDrop> | null = null;

  function DragController() {
    const ctx = useDragDrop();
    dragActions = ctx;
    return (
      <button
        data-testid="start-drag"
        onClick={() => {
          // Simulate starting a drag with card info and grab offset
          ctx.startTouchDrag('card-1', 'table', 400, 300, cardInfo, grabOffset, props.allCardIds, props.draggingCards);
        }}
      />
    );
  }

  const result = render(
    <DragDropProvider>
      <DragController />
      <CardDragAdorner />
    </DragDropProvider>,
  );

  return { ...result, getDragActions: () => dragActions! };
}

// Import the type — this will fail until we define it, which is expected in RED phase
type DragCardInfo = {
  imageUrl: string;
  name: string;
  width: number;
  height: number;
  faceUp: boolean;
  cardBackUrl?: string;
};

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('DragDropContext — recentlyDroppedCardId', () => {
  afterEach(cleanup);

  it('sets recentlyDroppedCardId when drag ends', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return <div data-testid="dropped">{ctx.recentlyDroppedCardId ?? 'none'}</div>;
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    // Start drag, then end it
    act(() => {
      dragActions!.startTouchDrag('card-42', 'table', 100, 100);
    });
    act(() => {
      dragActions!.endDrag();
    });

    expect(screen.getByTestId('dropped').textContent).toBe('card-42');
  });

  it('clears recentlyDroppedCardId when clearRecentlyDropped is called', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return <div data-testid="dropped">{ctx.recentlyDroppedCardId ?? 'none'}</div>;
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    act(() => {
      dragActions!.startTouchDrag('card-42', 'table', 100, 100);
    });
    act(() => {
      dragActions!.endDrag();
    });

    expect(screen.getByTestId('dropped').textContent).toBe('card-42');

    // Consumer (e.g. GameBoard) calls clearRecentlyDropped after position is painted
    act(() => {
      dragActions!.clearRecentlyDropped();
    });

    expect(screen.getByTestId('dropped').textContent).toBe('none');
  });
});

describe('DragDropContext — grabOffset', () => {
  afterEach(cleanup);

  it('stores grabOffset when startTouchDrag is called with offset', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return (
        <div data-testid="offset">
          {ctx.dragState.grabOffset?.x},{ctx.dragState.grabOffset?.y}
        </div>
      );
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    act(() => {
      dragActions!.startTouchDrag('card-1', 'table', 400, 300, undefined, { x: 25, y: 35 });
    });

    expect(screen.getByTestId('offset').textContent).toBe('25,35');
  });

  it('defaults grabOffset to {0,0} when not provided', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return (
        <div data-testid="offset">
          {ctx.dragState.grabOffset.x},{ctx.dragState.grabOffset.y}
        </div>
      );
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    act(() => {
      dragActions!.startTouchDrag('card-1', 'table', 400, 300);
    });

    expect(screen.getByTestId('offset').textContent).toBe('0,0');
  });

  it('resets grabOffset when drag ends', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return (
        <div data-testid="offset">
          {ctx.dragState.grabOffset.x},{ctx.dragState.grabOffset.y}
        </div>
      );
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    act(() => {
      dragActions!.startTouchDrag('card-1', 'table', 400, 300, undefined, { x: 25, y: 35 });
    });
    act(() => {
      dragActions!.endDrag();
    });

    expect(screen.getByTestId('offset').textContent).toBe('0,0');
  });
});

describe('CardDragAdorner', () => {
  afterEach(cleanup);

  // ─── Visibility ─────────────────────────────────────────────────────

  it('does not render when no drag is active', () => {
    render(
      <DragDropProvider>
        <CardDragAdorner />
      </DragDropProvider>,
    );
    expect(screen.queryByTestId('card-drag-adorner')).toBeNull();
  });

  it('renders when a drag is active', () => {
    const { getDragActions } = renderWithDragControl();

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    expect(screen.getByTestId('card-drag-adorner')).toBeTruthy();
  });

  it('disappears when drag ends', () => {
    const { getDragActions } = renderWithDragControl();

    act(() => {
      screen.getByTestId('start-drag').click();
    });
    expect(screen.getByTestId('card-drag-adorner')).toBeTruthy();

    act(() => {
      getDragActions().endDrag();
    });
    expect(screen.queryByTestId('card-drag-adorner')).toBeNull();
  });

  // ─── Positioning (grab offset) ─────────────────────────────────────

  it('positions adorner using grab offset, not centered on cursor', () => {
    // Grab offset (30, 20) means user grabbed 30px from left, 20px from top of the card
    renderWithDragControl({ grabOffset: { x: 30, y: 20 } });

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    const adorner = screen.getByTestId('card-drag-adorner');
    // Adorner top-left = cursor - grabOffset * adornerScale
    // adornerScale = 0.85
    // x: 400 - 30*0.85 = 374.5, y: 300 - 20*0.85 = 283
    expect(parseFloat(adorner.style.left)).toBeCloseTo(374.5, 0);
    expect(parseFloat(adorner.style.top)).toBeCloseTo(283, 0);
  });

  it('positions adorner at cursor top-left when grab offset is (0,0)', () => {
    // Grabbing from the exact top-left corner
    renderWithDragControl({ grabOffset: { x: 0, y: 0 } });

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    const adorner = screen.getByTestId('card-drag-adorner');
    // With (0,0) offset, adorner top-left is exactly at cursor position
    expect(parseFloat(adorner.style.left)).toBeCloseTo(400, 0);
    expect(parseFloat(adorner.style.top)).toBeCloseTo(300, 0);
  });

  it('positions adorner correctly when grabbed from bottom-right corner', () => {
    // Grab from bottom-right of a 100x140 card
    renderWithDragControl({ grabOffset: { x: 100, y: 140 } });

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    const adorner = screen.getByTestId('card-drag-adorner');
    // x: 400 - 100*0.85 = 315, y: 300 - 140*0.85 = 181
    expect(parseFloat(adorner.style.left)).toBeCloseTo(315, 0);
    expect(parseFloat(adorner.style.top)).toBeCloseTo(181, 0);
  });

  it('maintains grab offset when mouse moves', () => {
    const { getDragActions } = renderWithDragControl({ grabOffset: { x: 30, y: 20 } });

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    act(() => {
      getDragActions().updateMousePosition(600, 450);
    });

    const adorner = screen.getByTestId('card-drag-adorner');
    // x: 600 - 30*0.85 = 574.5, y: 450 - 20*0.85 = 433
    expect(parseFloat(adorner.style.left)).toBeCloseTo(574.5, 0);
    expect(parseFloat(adorner.style.top)).toBeCloseTo(433, 0);
  });

  // ─── Card Image ─────────────────────────────────────────────────────

  it('renders the card image when face up and imageUrl provided', () => {
    renderWithDragControl({
      cardInfo: {
        imageUrl: 'octgn-asset://card/dragon.png',
        name: 'Dragon',
        width: 100,
        height: 140,
        faceUp: true,
      },
    });

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    const img = screen.getByAltText('Dragon');
    expect(img).toBeTruthy();
    expect((img as HTMLImageElement).src).toContain('dragon.png');
  });

  it('renders card name placeholder when no image url', () => {
    renderWithDragControl({
      cardInfo: {
        imageUrl: '',
        name: 'Mystery Card',
        width: 100,
        height: 140,
        faceUp: true,
      },
    });

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    expect(screen.getByText('Mystery Card')).toBeTruthy();
  });

  it('renders card back when face down', () => {
    renderWithDragControl({
      cardInfo: {
        imageUrl: 'octgn-asset://card/dragon.png',
        name: 'Dragon',
        width: 100,
        height: 140,
        faceUp: false,
        cardBackUrl: 'octgn-asset://card/back.png',
      },
    });

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    // Should show back image, not front
    const img = screen.getByAltText('Face-down card');
    expect(img).toBeTruthy();
    expect((img as HTMLImageElement).src).toContain('back.png');
  });

  // ─── Style / Structure ──────────────────────────────────────────────

  it('has pointer-events none to avoid interfering with drop targets', () => {
    renderWithDragControl();

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    const adorner = screen.getByTestId('card-drag-adorner');
    expect(adorner.style.pointerEvents).toBe('none');
  });

  it('uses fixed positioning', () => {
    renderWithDragControl();

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    const adorner = screen.getByTestId('card-drag-adorner');
    expect(adorner.style.position).toBe('fixed');
  });

  // ─── Face-down card identity leak prevention ──────────────────────

  it('does NOT reveal card name in placeholder when face-down without cardBackUrl', () => {
    renderWithDragControl({
      cardInfo: {
        imageUrl: 'octgn-asset://card/secret-dragon.png',
        name: 'Secret Dragon',
        width: 100,
        height: 140,
        faceUp: false,
        // No cardBackUrl — falls through to placeholder
      },
    });

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    // The card name should NOT appear anywhere in the adorner
    expect(screen.queryByText('Secret Dragon')).toBeNull();
  });

  it('does NOT reveal card name in placeholder when face-down with empty cardBackUrl', () => {
    renderWithDragControl({
      cardInfo: {
        imageUrl: 'octgn-asset://card/hidden-spell.png',
        name: 'Hidden Spell',
        width: 100,
        height: 140,
        faceUp: false,
        cardBackUrl: '',
      },
    });

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    // The card name should NOT appear anywhere in the adorner
    expect(screen.queryByText('Hidden Spell')).toBeNull();
  });

  it('does NOT show front card image when face-down without cardBackUrl', () => {
    renderWithDragControl({
      cardInfo: {
        imageUrl: 'octgn-asset://card/secret-dragon.png',
        name: 'Secret Dragon',
        width: 100,
        height: 140,
        faceUp: false,
      },
    });

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    // Should NOT render an img with the front image
    expect(screen.queryByAltText('Secret Dragon')).toBeNull();
    // Should show a generic face-down indicator instead
    const adorner = screen.getByTestId('card-drag-adorner');
    expect(adorner.textContent).not.toContain('Secret Dragon');
  });

  // ─── Multi-card adorner: renders all cards at relative positions ────

  it('renders multiple cards when draggingCards has entries', () => {
    const draggingCards: DraggingCardData[] = [
      { id: 'card-1', relativeX: 0, relativeY: 0, info: { imageUrl: 'a.png', name: 'Card A', width: 100, height: 140, faceUp: true } },
      { id: 'card-2', relativeX: 80, relativeY: 30, info: { imageUrl: 'b.png', name: 'Card B', width: 100, height: 140, faceUp: true } },
      { id: 'card-3', relativeX: -40, relativeY: 60, info: { imageUrl: 'c.png', name: 'Card C', width: 100, height: 140, faceUp: true } },
    ];

    renderWithDragControl({
      draggingCards,
      allCardIds: ['card-1', 'card-2', 'card-3'],
    });

    act(() => { screen.getByTestId('start-drag').click(); });

    // Each card should have its own adorner element
    expect(screen.getByTestId('adorner-card-card-1')).toBeTruthy();
    expect(screen.getByTestId('adorner-card-card-2')).toBeTruthy();
    expect(screen.getByTestId('adorner-card-card-3')).toBeTruthy();
  });

  it('does not show count badge for multi-card drag with draggingCards', () => {
    const draggingCards: DraggingCardData[] = [
      { id: 'card-1', relativeX: 0, relativeY: 0, info: { imageUrl: 'a.png', name: 'A', width: 100, height: 140, faceUp: true } },
      { id: 'card-2', relativeX: 50, relativeY: 0, info: { imageUrl: 'b.png', name: 'B', width: 100, height: 140, faceUp: true } },
    ];

    renderWithDragControl({ draggingCards, allCardIds: ['card-1', 'card-2'] });

    act(() => { screen.getByTestId('start-drag').click(); });

    // No count badge — cards are shown individually
    expect(screen.queryByTestId('multi-card-badge')).toBeNull();
  });

  it('positions multi-card adorner cards using relative offsets', () => {
    const draggingCards: DraggingCardData[] = [
      { id: 'card-1', relativeX: 0, relativeY: 0, info: { imageUrl: 'a.png', name: 'A', width: 100, height: 140, faceUp: true } },
      { id: 'card-2', relativeX: 120, relativeY: -50, info: { imageUrl: 'b.png', name: 'B', width: 100, height: 140, faceUp: true } },
    ];

    renderWithDragControl({ draggingCards, allCardIds: ['card-1', 'card-2'], grabOffset: { x: 50, y: 70 } });

    act(() => { screen.getByTestId('start-drag').click(); });

    const card2 = screen.getByTestId('adorner-card-card-2');
    // Card 2 should be offset from card 1 by (120, -50) scaled by adornerScale
    const style = card2.style;
    expect(style.left).toBeTruthy();
    expect(style.top).toBeTruthy();
  });

  it('maintains card aspect ratio in adorner dimensions', () => {
    renderWithDragControl({
      cardInfo: {
        imageUrl: 'octgn-asset://card/test.png',
        name: 'Test',
        width: 100,
        height: 140,
        faceUp: true,
      },
    });

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    const adorner = screen.getByTestId('card-drag-adorner');
    const width = parseInt(adorner.style.width);
    const height = parseInt(adorner.style.height);
    // Adorner should maintain roughly the same aspect ratio as the card
    expect(width).toBeGreaterThan(0);
    expect(height).toBeGreaterThan(0);
    const ratio = width / height;
    const expectedRatio = 100 / 140;
    expect(Math.abs(ratio - expectedRatio)).toBeLessThan(0.1);
  });
});

// ---------------------------------------------------------------------------
// Multi-card drag with relative positions
// ---------------------------------------------------------------------------

describe('DragDropContext — draggingCards with relative positions', () => {
  afterEach(cleanup);

  it('stores draggingCards array when startTouchDrag is called with card positions', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return (
        <div data-testid="cards-count">{ctx.dragState.draggingCards.length}</div>
      );
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    const cards: DraggingCardData[] = [
      { id: 'card-1', relativeX: 0, relativeY: 0, info: { imageUrl: 'a.png', name: 'A', width: 100, height: 140, faceUp: true } },
      { id: 'card-2', relativeX: 50, relativeY: 30, info: { imageUrl: 'b.png', name: 'B', width: 100, height: 140, faceUp: true } },
    ];

    act(() => {
      dragActions!.startTouchDrag('card-1', 'table', 400, 300, cards[0].info, { x: 25, y: 35 }, ['card-1', 'card-2'], cards);
    });

    expect(screen.getByTestId('cards-count').textContent).toBe('2');
  });

  it('stores relative positions for each dragging card', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      const cards = ctx.dragState.draggingCards;
      return (
        <div data-testid="positions">
          {cards.map((c) => `${c.id}:${c.relativeX},${c.relativeY}`).join('|')}
        </div>
      );
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    const cards: DraggingCardData[] = [
      { id: 'card-1', relativeX: 0, relativeY: 0, info: { imageUrl: 'a.png', name: 'A', width: 100, height: 140, faceUp: true } },
      { id: 'card-2', relativeX: 80, relativeY: -20, info: { imageUrl: 'b.png', name: 'B', width: 100, height: 140, faceUp: true } },
      { id: 'card-3', relativeX: -30, relativeY: 50, info: { imageUrl: 'c.png', name: 'C', width: 100, height: 140, faceUp: true } },
    ];

    act(() => {
      dragActions!.startTouchDrag('card-1', 'table', 400, 300, cards[0].info, { x: 0, y: 0 }, ['card-1', 'card-2', 'card-3'], cards);
    });

    expect(screen.getByTestId('positions').textContent).toBe('card-1:0,0|card-2:80,-20|card-3:-30,50');
  });

  it('defaults to empty draggingCards when not provided', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return (
        <div data-testid="cards-count">{ctx.dragState.draggingCards.length}</div>
      );
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    act(() => {
      dragActions!.startTouchDrag('card-1', 'table', 400, 300);
    });

    expect(screen.getByTestId('cards-count').textContent).toBe('0');
  });

  it('clears draggingCards when drag ends', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return (
        <div data-testid="cards-count">{ctx.dragState.draggingCards.length}</div>
      );
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    const cards: DraggingCardData[] = [
      { id: 'card-1', relativeX: 0, relativeY: 0, info: { imageUrl: 'a.png', name: 'A', width: 100, height: 140, faceUp: true } },
    ];

    act(() => {
      dragActions!.startTouchDrag('card-1', 'table', 400, 300, undefined, undefined, ['card-1'], cards);
    });

    expect(screen.getByTestId('cards-count').textContent).toBe('1');

    act(() => {
      dragActions!.endDrag();
    });

    expect(screen.getByTestId('cards-count').textContent).toBe('0');
  });
});

// ---------------------------------------------------------------------------
// DragDropContext — isOverInvertedZone
// ---------------------------------------------------------------------------

describe('DragDropContext — isOverInvertedZone', () => {
  afterEach(cleanup);

  it('defaults isOverInvertedZone to false', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return <div data-testid="inverted">{String(ctx.dragState.isOverInvertedZone)}</div>;
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    expect(screen.getByTestId('inverted').textContent).toBe('false');
  });

  it('updates isOverInvertedZone via updateInvertedZone', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return <div data-testid="inverted">{String(ctx.dragState.isOverInvertedZone)}</div>;
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    act(() => {
      dragActions!.startTouchDrag('card-1', 'table', 400, 300);
    });

    act(() => {
      dragActions!.updateInvertedZone(true);
    });

    expect(screen.getByTestId('inverted').textContent).toBe('true');
  });

  it('resets isOverInvertedZone when drag ends', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return <div data-testid="inverted">{String(ctx.dragState.isOverInvertedZone)}</div>;
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    act(() => {
      dragActions!.startTouchDrag('card-1', 'table', 400, 300);
    });
    act(() => {
      dragActions!.updateInvertedZone(true);
    });
    expect(screen.getByTestId('inverted').textContent).toBe('true');

    act(() => {
      dragActions!.endDrag();
    });
    expect(screen.getByTestId('inverted').textContent).toBe('false');
  });

  it('keeps isOverInvertedZone as false when updateInvertedZone(false) called while already false', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return <div data-testid="inverted">{String(ctx.dragState.isOverInvertedZone)}</div>;
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    act(() => {
      dragActions!.startTouchDrag('card-1', 'table', 400, 300);
    });

    act(() => {
      dragActions!.updateInvertedZone(false);
    });

    expect(screen.getByTestId('inverted').textContent).toBe('false');
  });
});

// ---------------------------------------------------------------------------
// DragDropContext — tableMidlineScreenY
// ---------------------------------------------------------------------------

describe('DragDropContext — tableMidlineScreenY', () => {
  afterEach(cleanup);

  it('defaults tableMidlineScreenY to null', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return <div data-testid="midline">{String(ctx.dragState.tableMidlineScreenY)}</div>;
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    expect(screen.getByTestId('midline').textContent).toBe('null');
  });

  it('updates tableMidlineScreenY via setTableMidlineScreenY', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return <div data-testid="midline">{String(ctx.dragState.tableMidlineScreenY)}</div>;
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    act(() => {
      dragActions!.startTouchDrag('card-1', 'table', 400, 300);
    });

    act(() => {
      dragActions!.setTableMidlineScreenY(450);
    });

    expect(screen.getByTestId('midline').textContent).toBe('450');
  });

  it('resets tableMidlineScreenY when drag ends', () => {
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function Inspector() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return <div data-testid="midline">{String(ctx.dragState.tableMidlineScreenY)}</div>;
    }

    render(
      <DragDropProvider>
        <Inspector />
      </DragDropProvider>,
    );

    act(() => {
      dragActions!.startTouchDrag('card-1', 'table', 400, 300);
    });
    act(() => {
      dragActions!.setTableMidlineScreenY(450);
    });
    expect(screen.getByTestId('midline').textContent).toBe('450');

    act(() => {
      dragActions!.endDrag();
    });
    expect(screen.getByTestId('midline').textContent).toBe('null');
  });
});

// ---------------------------------------------------------------------------
// CardDragAdorner — inverted zone flipping
// ---------------------------------------------------------------------------

describe('CardDragAdorner — inverted zone flipping', () => {
  afterEach(cleanup);

  it('applies 180deg rotation to single card adorner when isOverInvertedZone is true', () => {
    const { getDragActions } = renderWithDragControl();

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    act(() => {
      getDragActions().updateInvertedZone(true);
    });

    const adorner = screen.getByTestId('card-drag-adorner');
    // Should contain rotate(183deg) — 180 + original 3deg tilt
    expect(adorner.style.transform).toContain('rotate(183deg)');
  });

  it('uses normal 3deg rotation when isOverInvertedZone is false', () => {
    const { getDragActions } = renderWithDragControl();

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    const adorner = screen.getByTestId('card-drag-adorner');
    expect(adorner.style.transform).toContain('rotate(3deg)');
    expect(adorner.style.transform).not.toContain('rotate(183deg)');
  });

  it('flips only multi-card adorner cards whose center is above tableMidlineScreenY', () => {
    // Card 1: relativeY=0, Card 2: relativeY=200 (well below card 1)
    // With cursor at (400,300), grabOffset (50,70):
    //   baseY = 300 - 70*0.85 = 240.5
    //   card1 Y = 240.5, center = 240.5 + 140*0.85/2 = 300
    //   card2 Y = 240.5 + 200*0.85 = 410.5, center = 410.5 + 59.5 = 470
    // midlineScreenY = 350 → card1 center(300) < 350 = inverted, card2 center(470) > 350 = normal
    const draggingCards: DraggingCardData[] = [
      { id: 'card-1', relativeX: 0, relativeY: 0, info: { imageUrl: 'a.png', name: 'A', width: 100, height: 140, faceUp: true } },
      { id: 'card-2', relativeX: 80, relativeY: 200, info: { imageUrl: 'b.png', name: 'B', width: 100, height: 140, faceUp: true } },
    ];

    const { getDragActions } = renderWithDragControl({
      draggingCards,
      allCardIds: ['card-1', 'card-2'],
    });

    act(() => { screen.getByTestId('start-drag').click(); });
    act(() => { getDragActions().setTableMidlineScreenY(350); });

    const card1 = screen.getByTestId('adorner-card-card-1');
    const card2 = screen.getByTestId('adorner-card-card-2');
    // Card 1 (center above midline) should be flipped
    expect(card1.style.transform).toContain('rotate(182deg)');
    // Card 2 (center below midline) should NOT be flipped
    expect(card2.style.transform).toContain('rotate(2deg)');
    expect(card2.style.transform).not.toContain('rotate(182deg)');
  });

  it('flips all multi-card adorner cards when all are above midline', () => {
    // Both cards close together, both above midline at 500
    const draggingCards: DraggingCardData[] = [
      { id: 'card-1', relativeX: 0, relativeY: 0, info: { imageUrl: 'a.png', name: 'A', width: 100, height: 140, faceUp: true } },
      { id: 'card-2', relativeX: 80, relativeY: 30, info: { imageUrl: 'b.png', name: 'B', width: 100, height: 140, faceUp: true } },
    ];

    const { getDragActions } = renderWithDragControl({
      draggingCards,
      allCardIds: ['card-1', 'card-2'],
    });

    act(() => { screen.getByTestId('start-drag').click(); });
    act(() => { getDragActions().setTableMidlineScreenY(500); });

    expect(screen.getByTestId('adorner-card-card-1').style.transform).toContain('rotate(182deg)');
    expect(screen.getByTestId('adorner-card-card-2').style.transform).toContain('rotate(182deg)');
  });

  it('flips no multi-card adorner cards when all are below midline', () => {
    const draggingCards: DraggingCardData[] = [
      { id: 'card-1', relativeX: 0, relativeY: 0, info: { imageUrl: 'a.png', name: 'A', width: 100, height: 140, faceUp: true } },
      { id: 'card-2', relativeX: 80, relativeY: 30, info: { imageUrl: 'b.png', name: 'B', width: 100, height: 140, faceUp: true } },
    ];

    const { getDragActions } = renderWithDragControl({
      draggingCards,
      allCardIds: ['card-1', 'card-2'],
    });

    act(() => { screen.getByTestId('start-drag').click(); });
    // Midline well above all cards
    act(() => { getDragActions().setTableMidlineScreenY(100); });

    expect(screen.getByTestId('adorner-card-card-1').style.transform).toContain('rotate(2deg)');
    expect(screen.getByTestId('adorner-card-card-2').style.transform).toContain('rotate(2deg)');
  });

  it('does not flip multi-card adorner cards when tableMidlineScreenY is null', () => {
    const draggingCards: DraggingCardData[] = [
      { id: 'card-1', relativeX: 0, relativeY: 0, info: { imageUrl: 'a.png', name: 'A', width: 100, height: 140, faceUp: true } },
      { id: 'card-2', relativeX: 80, relativeY: 30, info: { imageUrl: 'b.png', name: 'B', width: 100, height: 140, faceUp: true } },
    ];

    renderWithDragControl({
      draggingCards,
      allCardIds: ['card-1', 'card-2'],
    });

    act(() => { screen.getByTestId('start-drag').click(); });
    // No setTableMidlineScreenY call — stays null

    expect(screen.getByTestId('adorner-card-card-1').style.transform).toContain('rotate(2deg)');
    expect(screen.getByTestId('adorner-card-card-2').style.transform).toContain('rotate(2deg)');
  });

  it('reverts to normal rotation when isOverInvertedZone changes back to false', () => {
    const { getDragActions } = renderWithDragControl();

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    act(() => {
      getDragActions().updateInvertedZone(true);
    });

    expect(screen.getByTestId('card-drag-adorner').style.transform).toContain('rotate(183deg)');

    act(() => {
      getDragActions().updateInvertedZone(false);
    });

    expect(screen.getByTestId('card-drag-adorner').style.transform).toContain('rotate(3deg)');
    expect(screen.getByTestId('card-drag-adorner').style.transform).not.toContain('rotate(183deg)');
  });

  it('adorner has a CSS transition for smooth flipping', () => {
    const { getDragActions } = renderWithDragControl();

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    const adorner = screen.getByTestId('card-drag-adorner');
    // Should have a transition on transform for smooth flip animation
    expect(adorner.style.transition).toContain('transform');
  });
});

// Type for tests — matches what will be added to DragDropContext
type DraggingCardData = {
  id: string;
  relativeX: number;
  relativeY: number;
  info: DragCardInfo;
};

// ---------------------------------------------------------------------------
// Touch drop position offset tests
// ---------------------------------------------------------------------------

describe('TouchDragLayer — drop position accounts for grab offset', () => {
  afterEach(cleanup);

  it('subtracts grabOffset from touch drop coordinates for table drops', () => {
    const onTableDrop = vi.fn();
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function DragController() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return null;
    }

    render(
      <DragDropProvider>
        <TouchDragLayer onTableDrop={onTableDrop}>
          <DragController />
          <div data-drop-zone="table" data-testid="table" style={{ width: 800, height: 600 }} />
        </TouchDragLayer>
      </DragDropProvider>,
    );

    // Start touch drag with grab offset (30, 40) — user grabbed 30px from left, 40px from top
    act(() => {
      dragActions!.startTouchDrag('card-1', 'hand', 500, 400, undefined, { x: 30, y: 40 });
    });

    // Simulate touch moving to position and hovering over table
    act(() => {
      dragActions!.updateMousePosition(500, 400);
      dragActions!.updateDropTarget('table');
    });

    // Simulate touchend by dispatching the event
    act(() => {
      document.dispatchEvent(new TouchEvent('touchend', { cancelable: true }));
    });

    // The drop coordinates should be offset: (500-30, 400-40) = (470, 360)
    expect(onTableDrop).toHaveBeenCalledWith('card-1', 470, 360, ['card-1'], undefined);
  });

  it('passes unmodified coordinates when grabOffset is (0,0)', () => {
    const onTableDrop = vi.fn();
    let dragActions: ReturnType<typeof useDragDrop> | null = null;

    function DragController() {
      const ctx = useDragDrop();
      dragActions = ctx;
      return null;
    }

    render(
      <DragDropProvider>
        <TouchDragLayer onTableDrop={onTableDrop}>
          <DragController />
          <div data-drop-zone="table" style={{ width: 800, height: 600 }} />
        </TouchDragLayer>
      </DragDropProvider>,
    );

    // Start drag with default (0,0) offset
    act(() => {
      dragActions!.startTouchDrag('card-1', 'hand', 500, 400);
    });

    act(() => {
      dragActions!.updateMousePosition(500, 400);
      dragActions!.updateDropTarget('table');
    });

    act(() => {
      document.dispatchEvent(new TouchEvent('touchend', { cancelable: true }));
    });

    // With (0,0) offset, coordinates should pass through unchanged
    expect(onTableDrop).toHaveBeenCalledWith('card-1', 500, 400, ['card-1'], undefined);
  });
});
