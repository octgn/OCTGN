import React, { useCallback, useEffect, useRef } from 'react';
import { useDragDrop } from './DragDropContext';
import CardDragAdorner from './CardDragAdorner';

/**
 * Global touch drag handler layer.
 *
 * HTML5 drag events don't fire on touch devices. This component listens for
 * global touchmove/touchend during an active touch drag and uses
 * `document.elementFromPoint()` to determine which drop zone the finger
 * is over, updating the DragDropContext accordingly.
 *
 * Drop zones must have a `data-drop-zone` attribute with the zone ID.
 * The table drop zone should also have `data-drop-zone-table="true"`.
 */

export interface TouchDropHandler {
  zone: string;
  onDrop: (cardId: string, x: number, y: number) => void;
}

interface TouchDragLayerProps {
  children: React.ReactNode;
  /** Called when a touch drag drops onto the table zone */
  onTableDrop?: (cardId: string, x: number, y: number) => void;
  /** Called when a touch drag drops onto a group zone */
  onGroupDrop?: (cardId: string, groupId: string, sourceZone: string | null) => void;
}

const TouchDragLayer: React.FC<TouchDragLayerProps> = ({
  children,
  onTableDrop,
  onGroupDrop,
}) => {
  const { dragState, updateMousePosition, updateDropTarget, endDrag, isDragging } = useDragDrop();
  const containerRef = useRef<HTMLDivElement>(null);

  const handleTouchMove = useCallback(
    (e: TouchEvent) => {
      if (!isDragging || !dragState.isTouchDrag) return;
      e.preventDefault(); // Prevent scrolling during drag
      const touch = e.touches[0];
      if (!touch) return;

      updateMousePosition(touch.clientX, touch.clientY);

      // Hit-test which drop zone the finger is over
      const el = document.elementFromPoint(touch.clientX, touch.clientY);
      if (el) {
        const dropZone = (el as HTMLElement).closest?.('[data-drop-zone]');
        if (dropZone) {
          updateDropTarget((dropZone as HTMLElement).dataset.dropZone!);
        } else {
          updateDropTarget(null);
        }
      }
    },
    [isDragging, dragState.isTouchDrag, updateMousePosition, updateDropTarget],
  );

  const handleTouchEnd = useCallback(
    (e: TouchEvent) => {
      if (!isDragging || !dragState.isTouchDrag) return;
      e.preventDefault();

      const cardId = dragState.draggingCardId;
      const zone = dragState.dropTargetZone;
      const sourceZone = dragState.sourceZone;
      const { x, y } = dragState.mousePosition;
      const { grabOffset } = dragState;

      if (cardId && zone) {
        if (zone === 'table' && onTableDrop) {
          onTableDrop(cardId, x - grabOffset.x, y - grabOffset.y);
        } else if (zone !== 'table' && onGroupDrop) {
          onGroupDrop(cardId, zone, sourceZone);
        }
      }

      endDrag();
    },
    [isDragging, dragState, onTableDrop, onGroupDrop, endDrag],
  );

  useEffect(() => {
    if (!isDragging || !dragState.isTouchDrag) return;

    // Use non-passive listeners so we can preventDefault to block scrolling
    document.addEventListener('touchmove', handleTouchMove, { passive: false });
    document.addEventListener('touchend', handleTouchEnd, { passive: false });
    document.addEventListener('touchcancel', handleTouchEnd, { passive: false });

    return () => {
      document.removeEventListener('touchmove', handleTouchMove);
      document.removeEventListener('touchend', handleTouchEnd);
      document.removeEventListener('touchcancel', handleTouchEnd);
    };
  }, [isDragging, dragState.isTouchDrag, handleTouchMove, handleTouchEnd]);

  return (
    <div ref={containerRef} className="contents">
      {children}
      <CardDragAdorner />
    </div>
  );
};

export default TouchDragLayer;
