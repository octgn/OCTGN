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

import { DragDropProvider, useDragDrop } from '@renderer/components/DragDropContext';
import CardDragAdorner from '@renderer/components/CardDragAdorner';
import TouchDragLayer from '@renderer/components/TouchDragLayer';

// ---------------------------------------------------------------------------
// Helper to trigger drag state from inside the provider
// ---------------------------------------------------------------------------

/** Renders the adorner inside a DragDropProvider with a button that triggers drag */
function renderWithDragControl(props: { cardInfo?: DragCardInfo; grabOffset?: { x: number; y: number } } = {}) {
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
          ctx.startTouchDrag('card-1', 'table', 400, 300, cardInfo, grabOffset);
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
    const match = adorner.style.transform.match(/translate\((-?[\d.]+)px,\s*(-?[\d.]+)px\)/);
    expect(match).toBeTruthy();
    const tx = parseFloat(match![1]);
    const ty = parseFloat(match![2]);
    // Adorner top-left = cursor - grabOffset * adornerScale
    // adornerScale = 0.85
    // x: 400 - 30*0.85 = 374.5, y: 300 - 20*0.85 = 283
    expect(tx).toBeCloseTo(374.5, 0);
    expect(ty).toBeCloseTo(283, 0);
  });

  it('positions adorner at cursor top-left when grab offset is (0,0)', () => {
    // Grabbing from the exact top-left corner
    renderWithDragControl({ grabOffset: { x: 0, y: 0 } });

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    const adorner = screen.getByTestId('card-drag-adorner');
    const match = adorner.style.transform.match(/translate\((-?[\d.]+)px,\s*(-?[\d.]+)px\)/);
    expect(match).toBeTruthy();
    const tx = parseFloat(match![1]);
    const ty = parseFloat(match![2]);
    // With (0,0) offset, adorner top-left is exactly at cursor position
    expect(tx).toBeCloseTo(400, 0);
    expect(ty).toBeCloseTo(300, 0);
  });

  it('positions adorner correctly when grabbed from bottom-right corner', () => {
    // Grab from bottom-right of a 100x140 card
    renderWithDragControl({ grabOffset: { x: 100, y: 140 } });

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    const adorner = screen.getByTestId('card-drag-adorner');
    const match = adorner.style.transform.match(/translate\((-?[\d.]+)px,\s*(-?[\d.]+)px\)/);
    expect(match).toBeTruthy();
    const tx = parseFloat(match![1]);
    const ty = parseFloat(match![2]);
    // x: 400 - 100*0.85 = 315, y: 300 - 140*0.85 = 181
    expect(tx).toBeCloseTo(315, 0);
    expect(ty).toBeCloseTo(181, 0);
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
    const match = adorner.style.transform.match(/translate\((-?[\d.]+)px,\s*(-?[\d.]+)px\)/);
    expect(match).toBeTruthy();
    const tx = parseFloat(match![1]);
    const ty = parseFloat(match![2]);
    // x: 600 - 30*0.85 = 574.5, y: 450 - 20*0.85 = 433
    expect(tx).toBeCloseTo(574.5, 0);
    expect(ty).toBeCloseTo(433, 0);
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
    expect(onTableDrop).toHaveBeenCalledWith('card-1', 470, 360);
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
    expect(onTableDrop).toHaveBeenCalledWith('card-1', 500, 400);
  });
});
