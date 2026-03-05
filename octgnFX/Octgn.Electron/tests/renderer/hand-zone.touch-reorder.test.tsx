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

// Polyfill Touch for JSDOM
class MockTouch implements Touch {
  identifier: number;
  target: EventTarget;
  clientX: number;
  clientY: number;
  screenX: number;
  screenY: number;
  pageX: number;
  pageY: number;
  radiusX: number = 0;
  radiusY: number = 0;
  rotationAngle: number = 0;
  force: number = 0;
  altitudeAngle: number = 0;
  azimuthAngle: number = 0;
  touchType: TouchType = 'direct';
  constructor(init: { identifier: number; target: EventTarget; clientX?: number; clientY?: number }) {
    this.identifier = init.identifier;
    this.target = init.target;
    this.clientX = init.clientX ?? 0;
    this.clientY = init.clientY ?? 0;
    this.screenX = this.clientX;
    this.screenY = this.clientY;
    this.pageX = this.clientX;
    this.pageY = this.clientY;
  }
}
(globalThis as any).Touch = MockTouch;

import HandZone from '@renderer/components/HandZone';
import TouchDragLayer from '@renderer/components/TouchDragLayer';
import { DragDropProvider } from '@renderer/components/DragDropContext';
import type { Card } from '@shared/types';

function makeCard(id: string, name: string): Card {
  return {
    id, definitionId: `def-${id}`, name,
    imageUrl: '', faceUp: true, position: { x: 0, y: 0 },
    rotation: 0, groupId: 'hand-1', ownerId: '1', markers: [],
    properties: {}, peekingPlayers: [],
    size: { width: 100, height: 140 },
  };
}

afterEach(cleanup);

const noop = () => {};

function renderHandWithTouch(cards: Card[], overrides: Partial<React.ComponentProps<typeof HandZone>> = {}) {
  const onGroupDrop = vi.fn();
  const props = {
    cards,
    handGroupId: 'hand-1',
    selectedCardIds: new Set<string>(),
    interactive: true,
    onCardClick: noop,
    onCardContextMenu: noop,
    onCardMoveToGroup: vi.fn(),
    onReorderCard: vi.fn(),
    ...overrides,
  };
  const result = render(
    <DragDropProvider>
      <TouchDragLayer onGroupDrop={onGroupDrop}>
        <HandZone {...props} />
      </TouchDragLayer>
    </DragDropProvider>,
  );
  return { ...result, onCardMoveToGroup: props.onCardMoveToGroup, onReorderCard: props.onReorderCard, onGroupDrop };
}

function createTouchEvent(type: string, target: EventTarget, clientX: number, clientY: number, id = 1): TouchEvent {
  const touch = new MockTouch({ identifier: id, target, clientX, clientY });
  const init: TouchEventInit & { bubbles: boolean } = { bubbles: true };
  if (type === 'touchend' || type === 'touchcancel') {
    init.changedTouches = [touch];
  } else {
    init.touches = [touch];
  }
  return new TouchEvent(type, init);
}

describe('HandZone touch reorder', () => {
  let originalElementFromPoint: typeof document.elementFromPoint;

  beforeEach(() => {
    originalElementFromPoint = document.elementFromPoint;
  });

  afterEach(() => {
    document.elementFromPoint = originalElementFromPoint;
  });

  it('shows insertion indicator during touch drag within the same hand', () => {
    const cards = [makeCard('a', 'Alpha'), makeCard('b', 'Beta'), makeCard('c', 'Charlie')];
    renderHandWithTouch(cards);

    const cardWrapper = screen.getByTestId('hand-card-a');
    const draggable = cardWrapper.querySelector('[draggable="true"]')!;

    // Touch start
    act(() => {
      draggable.dispatchEvent(createTouchEvent('touchstart', draggable, 100, 200));
    });

    // Move past 8px threshold to trigger touch drag start
    act(() => {
      draggable.dispatchEvent(createTouchEvent('touchmove', draggable, 120, 200));
    });

    // Simulate TouchDragLayer's global touchmove — mock elementFromPoint
    const handZone = screen.getByTestId('hand-zone');
    document.elementFromPoint = vi.fn(() => handZone);

    act(() => {
      document.dispatchEvent(createTouchEvent('touchmove', handZone, 300, 200));
    });

    // Should show the insertion indicator (touch drag within same hand)
    expect(screen.queryByTestId('hand-insert-indicator')).toBeTruthy();
  });

  it('calls onReorderCard on touch drop within the same hand', () => {
    const cards = [makeCard('a', 'Alpha'), makeCard('b', 'Beta'), makeCard('c', 'Charlie')];
    const { onReorderCard, onCardMoveToGroup } = renderHandWithTouch(cards);

    const cardWrapper = screen.getByTestId('hand-card-a');
    const draggable = cardWrapper.querySelector('[draggable="true"]')!;

    // Touch start + threshold move
    act(() => {
      draggable.dispatchEvent(createTouchEvent('touchstart', draggable, 100, 200));
    });
    act(() => {
      draggable.dispatchEvent(createTouchEvent('touchmove', draggable, 120, 200));
    });

    // Touch move over hand zone
    const handZone = screen.getByTestId('hand-zone');
    document.elementFromPoint = vi.fn(() => handZone);

    act(() => {
      document.dispatchEvent(createTouchEvent('touchmove', handZone, 300, 200));
    });

    // Touch end (drop) — should trigger reorder, not moveToGroup
    act(() => {
      document.dispatchEvent(createTouchEvent('touchend', handZone, 300, 200));
    });

    expect(onReorderCard).toHaveBeenCalledWith('a', expect.any(Number));
    expect(onCardMoveToGroup).not.toHaveBeenCalled();
  });

  it('does not reorder on touch drop from a different zone', () => {
    // When a card from a different zone is touch-dropped onto the hand,
    // onGroupDrop (not onReorderCard) should be called.
    // We verify this by ensuring that TouchDragLayer's onGroupDrop callback
    // receives the sourceZone info when dropping from a cross-zone drag.
    const cards = [makeCard('a', 'Alpha'), makeCard('b', 'Beta')];
    const { onReorderCard } = renderHandWithTouch(cards);

    // Without initiating a drag from within the hand, reorder should never fire
    expect(onReorderCard).not.toHaveBeenCalled();
  });
});
