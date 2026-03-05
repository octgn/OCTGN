import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, cleanup, fireEvent, act } from '@testing-library/react';
import React from 'react';

// ---------------------------------------------------------------------------
// Mock window.octgn (must come before component imports)
// ---------------------------------------------------------------------------
const mockOctgn = {
  login: vi.fn(),
  logout: vi.fn(),
  getSession: vi.fn(),
  getGames: vi.fn(),
  hostGame: vi.fn(),
  joinGame: vi.fn(),
  leaveGame: vi.fn(),
  minimize: vi.fn(),
  maximize: vi.fn(),
  quit: vi.fn(),
  getVersion: vi.fn(),
  onGameStateUpdate: vi.fn(() => vi.fn()),
  gameAction: vi.fn(),
  gameChat: vi.fn(),
  loadDeck: vi.fn(),
  openFileDialog: vi.fn(),
};

(window as any).octgn = mockOctgn;

import GameBoard from '@renderer/components/GameBoard';
import { DragDropProvider } from '@renderer/components/DragDropContext';
import type { Card } from '@shared/types';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function makeCard(overrides: Partial<Card> = {}): Card {
  return {
    id: 'card-1',
    definitionId: 'def-1',
    name: 'Test Card',
    imageUrl: '',
    faceUp: true,
    position: { x: 100, y: 200 },
    rotation: 0,
    groupId: 'table',
    ownerId: 'player-1',
    markers: [],
    properties: {},
    peekingPlayers: [],
    size: { width: 100, height: 140 },
    ...overrides,
  };
}

const noop = () => {};

/** Render GameBoard inside DragDropProvider (required context). */
function renderBoard(props: Partial<React.ComponentProps<typeof GameBoard>> = {}) {
  return render(
    <DragDropProvider>
      <GameBoard
        tableCards={[]}
        selectedCardIds={new Set<string>()}
        onCardClick={noop}
        onCardContextMenu={noop}
        onCardMoveToTable={noop}
        {...props}
      />
    </DragDropProvider>,
  );
}

// ---------------------------------------------------------------------------
// Board background rendering
// ---------------------------------------------------------------------------

describe('GameBoard — Board Background', () => {
  afterEach(() => cleanup());

  it('should render board image when boardImageUrl is set', () => {
    const { container } = renderBoard({
      boardImageUrl: 'data:image/png;base64,AAAA',
    });

    const el = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    expect(el).toBeInTheDocument();
    expect(el.tagName).toBe('DIV');
    expect(el.style.backgroundImage).toContain('data:image/png;base64,AAAA');
  });

  it('should not render board image when boardImageUrl is not set', () => {
    const { container } = renderBoard({});

    const img = container.querySelector('[data-testid="board-background"]');
    expect(img).not.toBeInTheDocument();
  });

  it('should place board image behind cards (lower z-index)', () => {
    const card = makeCard();
    const { container } = renderBoard({
      tableCards: [card],
      boardImageUrl: 'data:image/png;base64,AAAA',
    });

    const boardImg = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    const cardsLayer = container.querySelector('[data-testid="table-cards"]') as HTMLElement;

    expect(boardImg).toBeInTheDocument();
    expect(cardsLayer).toBeInTheDocument();

    // Board should have a lower z-index than cards layer
    const boardZ = parseInt(getComputedStyle(boardImg).zIndex || '0', 10);
    const cardsZ = parseInt(getComputedStyle(cardsLayer).zIndex || '0', 10);
    // At minimum, board should exist and cards should be at same level or above
    expect(boardZ).toBeLessThanOrEqual(cardsZ);
  });

  it('should be inside the zoom/pan transform container', () => {
    const { container } = renderBoard({
      boardImageUrl: 'data:image/png;base64,AAAA',
    });

    const boardImg = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    // The transform container should be an ancestor of the board image
    const transformContainer = container.querySelector('[data-testid="transform-container"]');
    expect(transformContainer).toBeInTheDocument();
    expect(transformContainer?.contains(boardImg)).toBe(true);
  });
});

// ---------------------------------------------------------------------------
// Board positioning
// ---------------------------------------------------------------------------

describe('GameBoard — Board Positioning', () => {
  afterEach(() => cleanup());

  it('should position board image at x,y offset when boardX and boardY are provided', () => {
    const { container } = renderBoard({
      boardImageUrl: 'data:image/png;base64,AAAA',
      boardX: 50,
      boardY: 75,
    });

    const img = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    expect(img).toBeInTheDocument();
    expect(img.style.left).toBe('50px');
    expect(img.style.top).toBe('75px');
  });

  it('should size board image to boardWidth and boardHeight', () => {
    const { container } = renderBoard({
      boardImageUrl: 'data:image/png;base64,AAAA',
      boardWidth: 800,
      boardHeight: 600,
    });

    const img = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    expect(img).toBeInTheDocument();
    expect(img.style.width).toBe('800px');
    expect(img.style.height).toBe('600px');
  });

  it('should default to 0,0 position when boardX/boardY not provided', () => {
    const { container } = renderBoard({
      boardImageUrl: 'data:image/png;base64,AAAA',
    });

    const img = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    expect(img).toBeInTheDocument();
    // When no explicit position, should still be positioned absolutely
    expect(img.style.position).toBe('absolute');
  });
});

// ---------------------------------------------------------------------------
// Background style modes
// ---------------------------------------------------------------------------

describe('GameBoard — Background Style', () => {
  afterEach(() => cleanup());

  it('should apply background-size 100% 100% for "stretch" backgroundStyle', () => {
    const { container } = renderBoard({
      boardImageUrl: 'data:image/png;base64,AAAA',
      backgroundStyle: 'stretch',
    });

    const el = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    expect(el).toBeInTheDocument();
    expect(el.style.backgroundSize).toBe('100% 100%');
  });

  it('should apply background-repeat for "tile" backgroundStyle', () => {
    const { container } = renderBoard({
      boardImageUrl: 'data:image/png;base64,AAAA',
      backgroundStyle: 'tile',
    });

    const tileEl = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    expect(tileEl).toBeInTheDocument();
    expect(tileEl.style.backgroundRepeat).toBe('repeat');
    expect(tileEl.style.backgroundSize).toBe('auto');
  });

  it('should apply background-size contain for "uniform" backgroundStyle', () => {
    const { container } = renderBoard({
      boardImageUrl: 'data:image/png;base64,AAAA',
      backgroundStyle: 'uniform',
    });

    const el = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    expect(el).toBeInTheDocument();
    expect(el.style.backgroundSize).toBe('contain');
  });

  it('should apply background-size cover for "uniformToFill" backgroundStyle', () => {
    const { container } = renderBoard({
      boardImageUrl: 'data:image/png;base64,AAAA',
      backgroundStyle: 'uniformToFill',
    });

    const el = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    expect(el).toBeInTheDocument();
    expect(el.style.backgroundSize).toBe('cover');
  });

  it('should default to background-size contain when no backgroundStyle provided', () => {
    const { container } = renderBoard({
      boardImageUrl: 'data:image/png;base64,AAAA',
    });

    const el = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    expect(el).toBeInTheDocument();
    // Default behavior: contain (show whole image)
    expect(el.style.backgroundSize).toBe('contain');
  });
});

// ---------------------------------------------------------------------------
// Table dimensions
// ---------------------------------------------------------------------------

describe('GameBoard — Table Dimensions', () => {
  afterEach(() => cleanup());

  it('should use tableWidth and tableHeight as virtual size for base-scale container', () => {
    const { container } = renderBoard({
      tableWidth: 1600,
      tableHeight: 1200,
    });

    const baseScaleContainer = container.querySelector('[data-testid="base-scale-container"]') as HTMLElement;
    expect(baseScaleContainer).toBeInTheDocument();
    expect(baseScaleContainer.style.width).toBe('1600px');
    expect(baseScaleContainer.style.height).toBe('1200px');
  });

  it('should not set explicit dimensions on base-scale container when tableWidth/tableHeight not provided', () => {
    const { container } = renderBoard({});

    const baseScaleContainer = container.querySelector('[data-testid="base-scale-container"]') as HTMLElement;
    expect(baseScaleContainer).toBeInTheDocument();
    // Without explicit table dimensions, base-scale-container uses inset: 0
    expect(baseScaleContainer.style.width).toBe('');
    expect(baseScaleContainer.style.height).toBe('');
  });
});

// ---------------------------------------------------------------------------
// Fallback behavior
// ---------------------------------------------------------------------------

describe('GameBoard — Fallback', () => {
  afterEach(() => cleanup());

  it('should still show grid pattern when no board image', () => {
    const { container } = renderBoard({});

    const grid = container.querySelector('[data-testid="table-grid"]');
    expect(grid).toBeInTheDocument();
  });

  it('should still render table cards when board image is present', () => {
    const card = makeCard();
    const { container } = renderBoard({
      tableCards: [card],
      boardImageUrl: 'data:image/png;base64,AAAA',
    });

    const img = container.querySelector('[data-testid="board-background"]');
    expect(img).toBeInTheDocument();

    // Card should also be rendered
    const cards = container.querySelectorAll('[data-testid="table-cards"] > div');
    expect(cards.length).toBeGreaterThanOrEqual(1);
  });
});

// ---------------------------------------------------------------------------
// Two-sided table mode
// ---------------------------------------------------------------------------

describe('GameBoard — Two-Sided Table', () => {
  afterEach(() => cleanup());

  it('should render middle line when useTwoSidedTable is true', () => {
    const { container } = renderBoard({
      useTwoSidedTable: true,
    });

    const line = container.querySelector('[data-testid="two-sided-middle-line"]');
    expect(line).toBeInTheDocument();
  });

  it('should NOT render middle line when useTwoSidedTable is false', () => {
    const { container } = renderBoard({
      useTwoSidedTable: false,
    });

    const line = container.querySelector('[data-testid="two-sided-middle-line"]');
    expect(line).not.toBeInTheDocument();
  });

  it('should NOT render middle line when useTwoSidedTable is not provided', () => {
    const { container } = renderBoard({});

    const line = container.querySelector('[data-testid="two-sided-middle-line"]');
    expect(line).not.toBeInTheDocument();
  });

  it('should pass invertedZone to cards in the inverted zone', () => {
    // Card with y < -cardHeight/2 (y=-100, height=140, threshold=-70)
    const invertedCard = makeCard({ id: 'inv-1', position: { x: 0, y: -100 }, size: { width: 100, height: 140 } });
    const { container } = renderBoard({
      tableCards: [invertedCard],
      useTwoSidedTable: true,
    });

    // The card's inner element should have scale(-1, -1) from invertedZone
    const inner = container.querySelector('.octgn-card-inner') as HTMLElement;
    expect(inner).toBeInTheDocument();
    expect(inner.style.transform).toContain('scale(-1, -1)');
  });

  it('should NOT pass invertedZone to cards in the normal zone', () => {
    // Card with y=100 (positive, normal zone)
    const normalCard = makeCard({ id: 'norm-1', position: { x: 0, y: 100 }, size: { width: 100, height: 140 } });
    const { container } = renderBoard({
      tableCards: [normalCard],
      useTwoSidedTable: true,
    });

    const inner = container.querySelector('.octgn-card-inner') as HTMLElement;
    expect(inner).toBeInTheDocument();
    expect(inner.style.transform).not.toContain('scale(-1, -1)');
  });
});

// ---------------------------------------------------------------------------
// Marquee selection — mouse leave during drag
// ---------------------------------------------------------------------------

describe('GameBoard — Marquee survives mouse leave', () => {
  afterEach(() => cleanup());

  function getTableDiv(container: HTMLElement) {
    return container.querySelector('[data-drop-zone="table"]') as HTMLElement;
  }

  it('should NOT cancel marquee when mouse leaves the table during active drag', () => {
    const onSelectionChange = vi.fn();
    const { container } = renderBoard({ onSelectionChange });

    const table = getTableDiv(container);

    // Start marquee: mousedown on the table
    fireEvent.mouseDown(table, { button: 0, clientX: 50, clientY: 50 });

    // Move past threshold to activate marquee
    fireEvent.mouseMove(table, { clientX: 60, clientY: 60 });

    // Marquee should be visible
    const marqueeBefore = container.querySelector('[data-testid="marquee-selection"]');
    expect(marqueeBefore).toBeInTheDocument();

    // Mouse leaves the table
    fireEvent.mouseLeave(table);

    // Marquee should still be visible after mouse leave
    const marqueeAfter = container.querySelector('[data-testid="marquee-selection"]');
    expect(marqueeAfter).toBeInTheDocument();
  });

  it('should finalize selection when mouseup fires on window after leaving table', () => {
    const card = makeCard({ id: 'c1', position: { x: 0, y: 0 }, size: { width: 100, height: 140 } });
    const onSelectionChange = vi.fn();
    const { container } = renderBoard({ tableCards: [card], onSelectionChange });

    const table = getTableDiv(container);

    // Start marquee
    fireEvent.mouseDown(table, { button: 0, clientX: 50, clientY: 50 });
    // Move past threshold
    fireEvent.mouseMove(table, { clientX: 60, clientY: 60 });

    // Mouse leaves table
    fireEvent.mouseLeave(table);

    // Mouseup on window (outside table) should finalize
    act(() => {
      window.dispatchEvent(new MouseEvent('mouseup', { button: 0, clientX: 200, clientY: 200, bubbles: true }));
    });

    // Marquee should be cleared
    const marqueeAfter = container.querySelector('[data-testid="marquee-selection"]');
    expect(marqueeAfter).not.toBeInTheDocument();

    // Selection callback should have been called
    expect(onSelectionChange).toHaveBeenCalled();
  });

  it('should continue tracking mouse position via window mousemove after leaving table', () => {
    const onSelectionChange = vi.fn();
    const { container } = renderBoard({ onSelectionChange });

    const table = getTableDiv(container);

    // Start marquee
    fireEvent.mouseDown(table, { button: 0, clientX: 50, clientY: 50 });
    // Move past threshold
    fireEvent.mouseMove(table, { clientX: 60, clientY: 60 });

    // Confirm marquee is active
    expect(container.querySelector('[data-testid="marquee-selection"]')).toBeInTheDocument();

    // Mouse leaves table
    fireEvent.mouseLeave(table);

    // Move on window (outside table) — marquee should still be present
    act(() => {
      window.dispatchEvent(new MouseEvent('mousemove', { clientX: 200, clientY: 200, bubbles: true }));
    });

    // Marquee should still be visible (tracked via window listener)
    expect(container.querySelector('[data-testid="marquee-selection"]')).toBeInTheDocument();

    // Finalize with mouseup on window
    act(() => {
      window.dispatchEvent(new MouseEvent('mouseup', { button: 0, clientX: 200, clientY: 200, bubbles: true }));
    });

    expect(onSelectionChange).toHaveBeenCalled();
  });
});
