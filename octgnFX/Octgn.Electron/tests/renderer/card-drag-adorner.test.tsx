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

// ---------------------------------------------------------------------------
// Helper to trigger drag state from inside the provider
// ---------------------------------------------------------------------------

/** Renders the adorner inside a DragDropProvider with a button that triggers drag */
function renderWithDragControl(props: { cardInfo?: DragCardInfo } = {}) {
  const cardInfo: DragCardInfo = props.cardInfo ?? {
    imageUrl: 'octgn-asset://card/test.png',
    name: 'Dragon Knight',
    width: 100,
    height: 140,
    faceUp: true,
  };

  let dragActions: ReturnType<typeof useDragDrop> | null = null;

  function DragController() {
    const ctx = useDragDrop();
    dragActions = ctx;
    return (
      <button
        data-testid="start-drag"
        onClick={() => {
          // Simulate starting a drag with card info
          ctx.startTouchDrag('card-1', 'table', 400, 300, cardInfo);
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

  // ─── Positioning ────────────────────────────────────────────────────

  it('positions at the mouse/touch coordinates (centered under cursor)', () => {
    renderWithDragControl();

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    const adorner = screen.getByTestId('card-drag-adorner');
    // Adorner is centered under the cursor with a scale factor applied,
    // so the translate values are offset by half the adorner dimensions
    expect(adorner.style.transform).toContain('translate(');
    expect(adorner.style.transform).toContain('px');
    // Verify it's in the ballpark of (400, 300) — accounting for centering offset
    const match = adorner.style.transform.match(/translate\(([.\d]+)px,\s*([.\d]+)px\)/);
    expect(match).toBeTruthy();
    const tx = parseFloat(match![1]);
    const ty = parseFloat(match![2]);
    // x offset: 400 - (100*0.85/2) = 357.5, y offset: 300 - (140*0.85/2) = 240.5
    expect(tx).toBeCloseTo(357.5, 0);
    expect(ty).toBeCloseTo(240.5, 0);
  });

  it('updates position when mouse moves', () => {
    const { getDragActions } = renderWithDragControl();

    act(() => {
      screen.getByTestId('start-drag').click();
    });

    act(() => {
      getDragActions().updateMousePosition(600, 450);
    });

    const adorner = screen.getByTestId('card-drag-adorner');
    const match = adorner.style.transform.match(/translate\(([.\d]+)px,\s*([.\d]+)px\)/);
    expect(match).toBeTruthy();
    const tx = parseFloat(match![1]);
    const ty = parseFloat(match![2]);
    // x offset: 600 - 42.5 = 557.5, y offset: 450 - 59.5 = 390.5
    expect(tx).toBeCloseTo(557.5, 0);
    expect(ty).toBeCloseTo(390.5, 0);
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
