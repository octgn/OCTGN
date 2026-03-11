import { useState, useCallback, useRef, useEffect } from 'react';
import { Card as CardType } from '../types/game';

export interface DragState {
  isDragging: boolean;
  cardIds: number[];
  startX: number;
  startY: number;
  offsetX: number;
  offsetY: number;
  startXs: number[];
  startYs: number[];
}

export interface UseCardDragOptions {
  onDragStart?: (cardIds: number[]) => void;
  onDragMove?: (cardIds: number[], dx: number, dy: number) => void;
  onDragEnd?: (cardIds: number[], x: number, y: number) => void;
}

const DEFAULT_STATE: DragState = {
  isDragging: false,
  cardIds: [],
  startX: 0,
  startY: 0,
  offsetX: 0,
  offsetY: 0,
  startXs: [],
  startYs: [],
};

export function useCardDrag(options: UseCardDragOptions = {}) {
  const [state, setState] = useState<DragState>(DEFAULT_STATE);
  const stateRef = useRef(state);
  stateRef.current = state;

  const startDrag = useCallback(
    (
      cardIds: number[],
      cards: CardType[],
      clientX: number,
      clientY: number,
      panOffset: { x: number; y: number }
    ) => {
      const startXs = cards
        .filter((c) => cardIds.includes(c.id))
        .map((c) => c.x);
      const startYs = cards
        .filter((c) => cardIds.includes(c.id))
        .map((c) => c.y);

      setState({
        isDragging: true,
        cardIds,
        startX: clientX,
        startY: clientY,
        offsetX: panOffset.x,
        offsetY: panOffset.y,
        startXs,
        startYs,
      });

      options.onDragStart?.(cardIds);
    },
    [options]
  );

  const updateDrag = useCallback(
    (clientX: number, clientY: number, zoom: number) => {
      const currentState = stateRef.current;
      if (!currentState.isDragging) return;

      const dx = (clientX - currentState.startX) / zoom;
      const dy = (clientY - currentState.startY) / zoom;

      options.onDragMove?.(currentState.cardIds, dx, dy);
    },
    [options]
  );

  const endDrag = useCallback(
    (clientX: number, clientY: number, zoom: number) => {
      const currentState = stateRef.current;
      if (!currentState.isDragging) return;

      const dx = (clientX - currentState.startX) / zoom;
      const dy = (clientY - currentState.startY) / zoom;

      const finalX = currentState.startXs.map((x) => x + dx);
      const finalY = currentState.startYs.map((y) => y + dy);

      options.onDragEnd?.(currentState.cardIds, finalX[0] || 0, finalY[0] || 0);

      setState(DEFAULT_STATE);
    },
    [options]
  );

  const cancelDrag = useCallback(() => {
    setState(DEFAULT_STATE);
  }, []);

  return {
    ...state,
    startDrag,
    updateDrag,
    endDrag,
    cancelDrag,
  };
}

export interface SelectionBoxState {
  isActive: boolean;
  startX: number;
  startY: number;
  endX: number;
  endY: number;
}

export function useSelectionBox() {
  const [state, setState] = useState<SelectionBoxState>({
    isActive: false,
    startX: 0,
    startY: 0,
    endX: 0,
    endY: 0,
  });

  const startSelection = useCallback((x: number, y: number) => {
    setState({
      isActive: true,
      startX: x,
      startY: y,
      endX: x,
      endY: y,
    });
  }, []);

  const updateSelection = useCallback((x: number, y: number) => {
    setState((prev) => {
      if (!prev.isActive) return prev;
      return { ...prev, endX: x, endY: y };
    });
  }, []);

  const endSelection = useCallback(() => {
    setState((prev) => ({ ...prev, isActive: false }));
  }, []);

  const getBounds = useCallback(() => {
    return {
      x: Math.min(state.startX, state.endX),
      y: Math.min(state.startY, state.endY),
      width: Math.abs(state.endX - state.startX),
      height: Math.abs(state.endY - state.startY),
    };
  }, [state]);

  return {
    ...state,
    startSelection,
    updateSelection,
    endSelection,
    getBounds,
  };
}

/**
 * Hook for managing card selection with click, shift-click, and box selection
 */
export function useCardSelection(
  cards: CardType[],
  onSelectionChange?: (cardIds: number[]) => void
) {
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const lastSelectedId = useRef<number | null>(null);

  const selectCard = useCallback(
    (cardId: number, mode: 'replace' | 'add' | 'toggle' = 'replace') => {
      setSelectedIds((prev) => {
        let next: number[];

        switch (mode) {
          case 'replace':
            next = [cardId];
            break;
          case 'add':
            next = prev.includes(cardId) ? prev : [...prev, cardId];
            break;
          case 'toggle':
            next = prev.includes(cardId)
              ? prev.filter((id) => id !== cardId)
              : [...prev, cardId];
            break;
        }

        onSelectionChange?.(next);
        return next;
      });

      lastSelectedId.current = cardId;
    },
    [onSelectionChange]
  );

  const selectRange = useCallback(
    (fromId: number, toId: number) => {
      const fromIndex = cards.findIndex((c) => c.id === fromId);
      const toIndex = cards.findIndex((c) => c.id === toId);

      if (fromIndex === -1 || toIndex === -1) return;

      const start = Math.min(fromIndex, toIndex);
      const end = Math.max(fromIndex, toIndex);

      const rangeIds = cards.slice(start, end + 1).map((c) => c.id);
      setSelectedIds((prev) => {
        const next = [...new Set([...prev, ...rangeIds])];
        onSelectionChange?.(next);
        return next;
      });
    },
    [cards, onSelectionChange]
  );

  const clearSelection = useCallback(() => {
    setSelectedIds([]);
    onSelectionChange?.([]);
  }, [onSelectionChange]);

  const selectAll = useCallback(() => {
    const allIds = cards.map((c) => c.id);
    setSelectedIds(allIds);
    onSelectionChange?.(allIds);
  }, [cards, onSelectionChange]);

  const handleCardClick = useCallback(
    (cardId: number, event: React.MouseEvent) => {
      if (event.ctrlKey || event.metaKey) {
        selectCard(cardId, 'toggle');
      } else if (event.shiftKey && lastSelectedId.current) {
        selectRange(lastSelectedId.current, cardId);
      } else {
        selectCard(cardId, 'replace');
      }
    },
    [selectCard, selectRange]
  );

  return {
    selectedIds,
    selectCard,
    selectRange,
    clearSelection,
    selectAll,
    handleCardClick,
    isSelected: (cardId: number) => selectedIds.includes(cardId),
  };
}
