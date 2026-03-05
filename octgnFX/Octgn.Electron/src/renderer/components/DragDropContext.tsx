import React, {
  createContext,
  useContext,
  useCallback,
  useState,
  useEffect,
  useRef,
  type ReactNode,
} from 'react';

/** Visual info about the card being dragged, used by the adorner */
export interface DragCardInfo {
  imageUrl: string;
  name: string;
  width: number;
  height: number;
  faceUp: boolean;
  cardBackUrl?: string;
}

export interface DragState {
  /** ID of the card currently being dragged, or null */
  draggingCardId: string | null;
  /** Zone the card was dragged from */
  sourceZone: string | null;
  /** Current cursor/touch position while dragging */
  mousePosition: { x: number; y: number };
  /** Zone the drag is currently hovering over */
  dropTargetZone: string | null;
  /** Whether this is a touch-initiated drag (vs HTML5 drag) */
  isTouchDrag: boolean;
  /** Visual info about the dragged card for the adorner */
  cardInfo: DragCardInfo | null;
  /** Offset from the card's top-left corner where the user grabbed */
  grabOffset: { x: number; y: number };
}

interface DragDropContextValue {
  dragState: DragState;
  /** Call when a card drag begins (HTML5 drag) */
  startDrag: (cardId: string, sourceZone: string, e: React.DragEvent, cardInfo?: DragCardInfo) => void;
  /** Call when a touch drag begins */
  startTouchDrag: (cardId: string, sourceZone: string, x: number, y: number, cardInfo?: DragCardInfo, grabOffset?: { x: number; y: number }) => void;
  /** Call while dragging over a valid zone to update drop target */
  updateDropTarget: (zone: string | null) => void;
  /** Update the cursor position (used by dragOver/touchMove handlers) */
  updateMousePosition: (x: number, y: number) => void;
  /** Call when the drag ends (drop or cancel) */
  endDrag: () => void;
  /** Whether any card is currently being dragged */
  isDragging: boolean;
  /** Card ID that was just dropped — suppresses position transition so the card snaps into place (adorner already provided visual continuity) */
  recentlyDroppedCardId: string | null;
  /** Acknowledge that the dropped card's new position has been painted */
  clearRecentlyDropped: () => void;
}

const initialState: DragState = {
  draggingCardId: null,
  sourceZone: null,
  mousePosition: { x: 0, y: 0 },
  dropTargetZone: null,
  isTouchDrag: false,
  cardInfo: null,
  grabOffset: { x: 0, y: 0 },
};

const DragDropCtx = createContext<DragDropContextValue | null>(null);

export const DragDropProvider: React.FC<{ children: ReactNode }> = ({
  children,
}) => {
  const [dragState, setDragState] = useState<DragState>(initialState);
  const dragStateRef = useRef(dragState);
  dragStateRef.current = dragState;
  const [recentlyDroppedCardId, setRecentlyDroppedCardId] = useState<string | null>(null);

  const startDrag = useCallback(
    (cardId: string, sourceZone: string, e: React.DragEvent, cardInfo?: DragCardInfo) => {
      // Configure the native HTML5 drag transfer
      e.dataTransfer.effectAllowed = 'move';
      e.dataTransfer.setData('application/octgn-card', cardId);
      e.dataTransfer.setData('application/octgn-zone', sourceZone);

      // Create a transparent drag image so we can render our own preview
      const ghost = document.createElement('div');
      ghost.style.width = '1px';
      ghost.style.height = '1px';
      ghost.style.opacity = '0';
      document.body.appendChild(ghost);
      e.dataTransfer.setDragImage(ghost, 0, 0);
      requestAnimationFrame(() => document.body.removeChild(ghost));

      // Compute grab offset from the card element's bounding rect
      const rect = (e.currentTarget as HTMLElement).getBoundingClientRect();
      const grabOffset = { x: e.clientX - rect.left, y: e.clientY - rect.top };

      setDragState({
        draggingCardId: cardId,
        sourceZone,
        mousePosition: { x: e.clientX, y: e.clientY },
        dropTargetZone: null,
        isTouchDrag: false,
        cardInfo: cardInfo ?? null,
        grabOffset,
      });
    },
    []
  );

  const startTouchDrag = useCallback(
    (cardId: string, sourceZone: string, x: number, y: number, cardInfo?: DragCardInfo, grabOffset?: { x: number; y: number }) => {
      setDragState({
        draggingCardId: cardId,
        sourceZone,
        mousePosition: { x, y },
        dropTargetZone: null,
        isTouchDrag: true,
        cardInfo: cardInfo ?? null,
        grabOffset: grabOffset ?? { x: 0, y: 0 },
      });
    },
    []
  );

  const updateDropTarget = useCallback((zone: string | null) => {
    setDragState((prev) => {
      if (prev.dropTargetZone === zone) return prev;
      return { ...prev, dropTargetZone: zone };
    });
  }, []);

  const updateMousePosition = useCallback((x: number, y: number) => {
    setDragState((prev) => ({ ...prev, mousePosition: { x, y } }));
  }, []);

  const endDrag = useCallback(() => {
    const droppedId = dragStateRef.current.draggingCardId;
    setDragState(initialState);
    if (droppedId) setRecentlyDroppedCardId(droppedId);
  }, []);

  /** Call to acknowledge that the dropped card's position has been painted
   *  (re-enables transitions for that card). */
  const clearRecentlyDropped = useCallback(() => {
    setRecentlyDroppedCardId(null);
  }, []);

  const isDragging = dragState.draggingCardId !== null;

  // Track mouse position globally during HTML5 drag (dragover only fires on
  // drop targets — this ensures we get position updates everywhere)
  useEffect(() => {
    if (!isDragging || dragState.isTouchDrag) return;

    const handleGlobalDragOver = (e: DragEvent) => {
      // Don't prevent default here — let drop zones handle that
      setDragState((prev) => ({ ...prev, mousePosition: { x: e.clientX, y: e.clientY } }));
    };

    document.addEventListener('dragover', handleGlobalDragOver);
    return () => document.removeEventListener('dragover', handleGlobalDragOver);
  }, [isDragging, dragState.isTouchDrag]);

  return (
    <DragDropCtx.Provider
      value={{
        dragState,
        startDrag,
        startTouchDrag,
        updateDropTarget,
        updateMousePosition,
        endDrag,
        isDragging,
        recentlyDroppedCardId,
        clearRecentlyDropped,
      }}
    >
      {children}
    </DragDropCtx.Provider>
  );
};

/** Hook to access drag-drop state and actions */
export function useDragDrop(): DragDropContextValue {
  const ctx = useContext(DragDropCtx);
  if (!ctx) {
    throw new Error('useDragDrop must be used within a DragDropProvider');
  }
  return ctx;
}

/** Build DragCardInfo from a Card object */
export function cardToDragInfo(card: { imageUrl: string; name: string; faceUp: boolean; size: { width: number; height: number } }, cardBackUrl?: string): DragCardInfo {
  return {
    imageUrl: card.imageUrl,
    name: card.name,
    width: card.size.width,
    height: card.size.height,
    faceUp: card.faceUp,
    cardBackUrl,
  };
}

export default DragDropProvider;
