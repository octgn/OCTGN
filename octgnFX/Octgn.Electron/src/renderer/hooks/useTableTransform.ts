import { useState, useCallback, useRef } from 'react';

// ─── Constants ────────────────────────────────────────────────────────────────
const MIN_ZOOM = 0.1;
const MAX_ZOOM = 5.0;
const ZOOM_SENSITIVITY = 0.001;

export interface ContainerRect {
  left: number;
  top: number;
  width: number;
  height: number;
}

export interface TableTransform {
  zoom: number;
  panX: number;
  panY: number;
  isPanning: boolean;

  handleWheel: (event: WheelEvent) => void;
  handlePanStart: (clientX: number, clientY: number) => void;
  handlePanMove: (clientX: number, clientY: number) => void;
  handlePanEnd: () => void;
  resetView: () => void;
  fitToScreen: (
    tableWidth: number,
    tableHeight: number,
    containerWidth: number,
    containerHeight: number
  ) => void;
  setContainerRect: (rect: ContainerRect) => void;
  screenToTable: (screenX: number, screenY: number) => { x: number; y: number };
}

/**
 * Hook providing zoom/pan transform state and handlers for the game table.
 *
 * - Zoom centers on mouse cursor position
 * - Pan via middle-click or space+left-click drag
 * - Zoom limits: 0.1x to 5.0x
 */
export function useTableTransform(): TableTransform {
  const [zoom, setZoom] = useState(1);
  const [panX, setPanX] = useState(0);
  const [panY, setPanY] = useState(0);
  const [isPanning, setIsPanning] = useState(false);

  const panStartRef = useRef<{ x: number; y: number }>({ x: 0, y: 0 });
  const containerRectRef = useRef<ContainerRect>({ left: 0, top: 0, width: 800, height: 600 });

  // We store current zoom/pan in refs so wheel handler can read latest values
  // without needing to re-create the callback (avoids stale closures).
  const stateRef = useRef({ zoom: 1, panX: 0, panY: 0 });

  const setContainerRect = useCallback((rect: ContainerRect) => {
    containerRectRef.current = rect;
  }, []);

  const handleWheel = useCallback((event: WheelEvent) => {
    event.preventDefault();

    const { zoom: currentZoom, panX: currentPanX, panY: currentPanY } = stateRef.current;
    const rect = containerRectRef.current;

    // Calculate zoom factor from wheel delta
    const delta = -event.deltaY * ZOOM_SENSITIVITY;
    const factor = 1 + delta;
    let newZoom = currentZoom * factor;

    // Clamp zoom
    newZoom = Math.max(MIN_ZOOM, Math.min(MAX_ZOOM, newZoom));

    if (newZoom === currentZoom) return;

    // Cursor position relative to the container
    const cursorX = event.clientX - rect.left;
    const cursorY = event.clientY - rect.top;

    // Point in table-space under the cursor before zoom:
    // tableX = (cursorX - panX) / currentZoom
    const tableX = (cursorX - currentPanX) / currentZoom;
    const tableY = (cursorY - currentPanY) / currentZoom;

    // After zoom, we want the same table point to stay under the cursor:
    // cursorX = tableX * newZoom + newPanX
    const newPanX = cursorX - tableX * newZoom;
    const newPanY = cursorY - tableY * newZoom;

    stateRef.current = { zoom: newZoom, panX: newPanX, panY: newPanY };
    setZoom(newZoom);
    setPanX(newPanX);
    setPanY(newPanY);
  }, []);

  const handlePanStart = useCallback((clientX: number, clientY: number) => {
    setIsPanning(true);
    panStartRef.current = { x: clientX, y: clientY };
  }, []);

  const handlePanMove = useCallback((clientX: number, clientY: number) => {
    // Only move if we have an active pan
    // We check isPanning via a ref to avoid stale closure issues
    setIsPanning((wasPanning) => {
      if (!wasPanning) return false;

      const dx = clientX - panStartRef.current.x;
      const dy = clientY - panStartRef.current.y;
      panStartRef.current = { x: clientX, y: clientY };

      setPanX((prev) => {
        const newVal = prev + dx;
        stateRef.current.panX = newVal;
        return newVal;
      });
      setPanY((prev) => {
        const newVal = prev + dy;
        stateRef.current.panY = newVal;
        return newVal;
      });

      return true;
    });
  }, []);

  const handlePanEnd = useCallback(() => {
    setIsPanning(false);
  }, []);

  const resetView = useCallback(() => {
    stateRef.current = { zoom: 1, panX: 0, panY: 0 };
    setZoom(1);
    setPanX(0);
    setPanY(0);
  }, []);

  const fitToScreen = useCallback(
    (
      tableWidth: number,
      tableHeight: number,
      containerWidth: number,
      containerHeight: number
    ) => {
      if (tableWidth <= 0 || tableHeight <= 0) return;

      const scaleX = containerWidth / tableWidth;
      const scaleY = containerHeight / tableHeight;
      let newZoom = Math.min(scaleX, scaleY);
      newZoom = Math.max(MIN_ZOOM, Math.min(MAX_ZOOM, newZoom));

      // Center the table in the container
      const newPanX = (containerWidth - tableWidth * newZoom) / 2;
      const newPanY = (containerHeight - tableHeight * newZoom) / 2;

      stateRef.current = { zoom: newZoom, panX: newPanX, panY: newPanY };
      setZoom(newZoom);
      setPanX(newPanX);
      setPanY(newPanY);
    },
    []
  );

  const screenToTable = useCallback(
    (screenX: number, screenY: number): { x: number; y: number } => {
      const { zoom: z, panX: px, panY: py } = stateRef.current;
      const rect = containerRectRef.current;
      return {
        x: (screenX - rect.left - px) / z,
        y: (screenY - rect.top - py) / z,
      };
    },
    []
  );

  return {
    zoom,
    panX,
    panY,
    isPanning,
    handleWheel,
    handlePanStart,
    handlePanMove,
    handlePanEnd,
    resetView,
    fitToScreen,
    setContainerRect,
    screenToTable,
  };
}
