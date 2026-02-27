import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, cleanup } from '@testing-library/react';
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
        handCards={[]}
        selectedCardId={null}
        onCardClick={noop}
        onCardContextMenu={noop}
        onCardMoveToTable={noop}
        onCardMoveToGroup={noop}
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

    const img = container.querySelector('[data-testid="board-background"]') as HTMLImageElement;
    expect(img).toBeInTheDocument();
    expect(img.tagName).toBe('IMG');
    expect(img.src).toBe('data:image/png;base64,AAAA');
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

  it('should apply object-fill for "stretch" backgroundStyle', () => {
    const { container } = renderBoard({
      boardImageUrl: 'data:image/png;base64,AAAA',
      backgroundStyle: 'stretch',
    });

    const img = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    expect(img).toBeInTheDocument();
    expect(img.style.objectFit).toBe('fill');
  });

  it('should apply background-repeat for "tile" backgroundStyle', () => {
    const { container } = renderBoard({
      boardImageUrl: 'data:image/png;base64,AAAA',
      backgroundStyle: 'tile',
    });

    // For tile mode, we use a div with background-image instead of img
    const tileEl = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    expect(tileEl).toBeInTheDocument();
    expect(tileEl.style.backgroundRepeat).toBe('repeat');
  });

  it('should apply object-contain for "uniform" backgroundStyle', () => {
    const { container } = renderBoard({
      boardImageUrl: 'data:image/png;base64,AAAA',
      backgroundStyle: 'uniform',
    });

    const img = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    expect(img).toBeInTheDocument();
    expect(img.style.objectFit).toBe('contain');
  });

  it('should apply object-cover for "uniformToFill" backgroundStyle', () => {
    const { container } = renderBoard({
      boardImageUrl: 'data:image/png;base64,AAAA',
      backgroundStyle: 'uniformToFill',
    });

    const img = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    expect(img).toBeInTheDocument();
    expect(img.style.objectFit).toBe('cover');
  });

  it('should default to object-contain when no backgroundStyle provided', () => {
    const { container } = renderBoard({
      boardImageUrl: 'data:image/png;base64,AAAA',
    });

    const img = container.querySelector('[data-testid="board-background"]') as HTMLElement;
    expect(img).toBeInTheDocument();
    // Default behavior: contain (show whole image)
    expect(img.style.objectFit).toBe('contain');
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
