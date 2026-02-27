import React, {
  createContext,
  useContext,
  useCallback,
  useState,
  useRef,
  type ReactNode,
} from 'react';

export interface DragState {
  /** ID of the card currently being dragged, or null */
  draggingCardId: string | null;
  /** Zone the card was dragged from */
  sourceZone: string | null;
  /** Current mouse/pointer position while dragging */
  mousePosition: { x: number; y: number };
  /** Zone the drag is currently hovering over */
  dropTargetZone: string | null;
}

interface DragDropContextValue {
  dragState: DragState;
  /** Call when a card drag begins */
  startDrag: (cardId: string, sourceZone: string, e: React.DragEvent) => void;
  /** Call while dragging over a valid zone to update drop target */
  updateDropTarget: (zone: string | null) => void;
  /** Update the mouse position (used by dragOver handlers) */
  updateMousePosition: (x: number, y: number) => void;
  /** Call when the drag ends (drop or cancel) */
  endDrag: () => void;
  /** Whether any card is currently being dragged */
  isDragging: boolean;
}

const initialState: DragState = {
  draggingCardId: null,
  sourceZone: null,
  mousePosition: { x: 0, y: 0 },
  dropTargetZone: null,
};

const DragDropCtx = createContext<DragDropContextValue | null>(null);

export const DragDropProvider: React.FC<{ children: ReactNode }> = ({
  children,
}) => {
  const [dragState, setDragState] = useState<DragState>(initialState);
  const dragStateRef = useRef(dragState);
  dragStateRef.current = dragState;

  const startDrag = useCallback(
    (cardId: string, sourceZone: string, e: React.DragEvent) => {
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

      setDragState({
        draggingCardId: cardId,
        sourceZone,
        mousePosition: { x: e.clientX, y: e.clientY },
        dropTargetZone: null,
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
    setDragState(initialState);
  }, []);

  const isDragging = dragState.draggingCardId !== null;

  return (
    <DragDropCtx.Provider
      value={{
        dragState,
        startDrag,
        updateDropTarget,
        updateMousePosition,
        endDrag,
        isDragging,
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

export default DragDropProvider;
