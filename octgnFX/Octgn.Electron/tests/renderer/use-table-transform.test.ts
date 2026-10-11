import { describe, it, expect, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useTableTransform } from '@renderer/hooks/useTableTransform';

describe('useTableTransform', () => {
  // ─── Default state ──────────────────────────────────────────────────
  describe('initial state', () => {
    it('starts with zoom = 1, panX = 0, panY = 0', () => {
      const { result } = renderHook(() => useTableTransform());
      expect(result.current.zoom).toBe(1);
      expect(result.current.panX).toBe(0);
      expect(result.current.panY).toBe(0);
    });

    it('isPanning is false by default', () => {
      const { result } = renderHook(() => useTableTransform());
      expect(result.current.isPanning).toBe(false);
    });
  });

  // ─── Zoom via wheel ─────────────────────────────────────────────────
  describe('handleWheel', () => {
    it('zooms in on negative deltaY (scroll up)', () => {
      const { result } = renderHook(() => useTableTransform());

      act(() => {
        result.current.handleWheel({
          deltaY: -100,
          clientX: 400,
          clientY: 300,
          preventDefault: () => {},
        } as unknown as WheelEvent);
      });

      expect(result.current.zoom).toBeGreaterThan(1);
    });

    it('zooms out on positive deltaY (scroll down)', () => {
      const { result } = renderHook(() => useTableTransform());

      act(() => {
        result.current.handleWheel({
          deltaY: 100,
          clientX: 400,
          clientY: 300,
          preventDefault: () => {},
        } as unknown as WheelEvent);
      });

      expect(result.current.zoom).toBeLessThan(1);
    });

    it('does not exceed max zoom of 5.0', () => {
      const { result } = renderHook(() => useTableTransform());

      // Zoom in aggressively many times
      for (let i = 0; i < 100; i++) {
        act(() => {
          result.current.handleWheel({
            deltaY: -200,
            clientX: 400,
            clientY: 300,
            preventDefault: () => {},
          } as unknown as WheelEvent);
        });
      }

      expect(result.current.zoom).toBeLessThanOrEqual(5.0);
    });

    it('does not go below min zoom of 0.1', () => {
      const { result } = renderHook(() => useTableTransform());

      // Zoom out aggressively many times
      for (let i = 0; i < 100; i++) {
        act(() => {
          result.current.handleWheel({
            deltaY: 200,
            clientX: 400,
            clientY: 300,
            preventDefault: () => {},
          } as unknown as WheelEvent);
        });
      }

      expect(result.current.zoom).toBeGreaterThanOrEqual(0.1);
    });

    it('zoom changes smoothly (no large jumps)', () => {
      const { result } = renderHook(() => useTableTransform());

      act(() => {
        result.current.handleWheel({
          deltaY: -100,
          clientX: 400,
          clientY: 300,
          preventDefault: () => {},
        } as unknown as WheelEvent);
      });

      // A single scroll step should not jump more than ~20% of current zoom
      expect(result.current.zoom).toBeLessThan(1.25);
      expect(result.current.zoom).toBeGreaterThan(1.0);
    });

    it('zoom centers on cursor position (adjusts pan)', () => {
      const { result } = renderHook(() => useTableTransform());

      // Set a known container rect via the ref callback
      const containerRect = { left: 0, top: 0, width: 800, height: 600 };
      act(() => {
        result.current.setContainerRect(containerRect);
      });

      // Zoom in at a specific cursor position
      act(() => {
        result.current.handleWheel({
          deltaY: -100,
          clientX: 200,
          clientY: 150,
          preventDefault: () => {},
        } as unknown as WheelEvent);
      });

      const { zoom, panX, panY } = result.current;
      expect(zoom).toBeGreaterThan(1);

      // Pan should adjust so the point under the cursor stays in place.
      // When zooming in at (200,150) from zoom=1 to zoom=z:
      // cursorLocalX = (200 - 0 - panX_old) / 1 = 200
      // panX_new = 200 - cursorLocalX * zoom = 200 - 200*z
      // Since z > 1, panX should be negative (shifted left)
      expect(panX).toBeLessThan(0);
      expect(panY).toBeLessThan(0);
    });
  });

  // ─── Pan (drag) ─────────────────────────────────────────────────────
  describe('panning', () => {
    it('handlePanStart sets isPanning to true', () => {
      const { result } = renderHook(() => useTableTransform());

      act(() => {
        result.current.handlePanStart(100, 100);
      });

      expect(result.current.isPanning).toBe(true);
    });

    it('handlePanMove updates panX and panY by delta', () => {
      const { result } = renderHook(() => useTableTransform());

      act(() => {
        result.current.handlePanStart(100, 100);
      });

      act(() => {
        result.current.handlePanMove(150, 120);
      });

      expect(result.current.panX).toBe(50);
      expect(result.current.panY).toBe(20);
    });

    it('handlePanMove accumulates multiple moves', () => {
      const { result } = renderHook(() => useTableTransform());

      act(() => {
        result.current.handlePanStart(100, 100);
      });

      act(() => {
        result.current.handlePanMove(120, 110);
      });
      act(() => {
        result.current.handlePanMove(150, 130);
      });

      // First move: +20, +10 from (100,100)->(120,110)
      // Second move: +30, +20 from (120,110)->(150,130)
      expect(result.current.panX).toBe(50);
      expect(result.current.panY).toBe(30);
    });

    it('handlePanEnd sets isPanning to false', () => {
      const { result } = renderHook(() => useTableTransform());

      act(() => {
        result.current.handlePanStart(100, 100);
      });

      act(() => {
        result.current.handlePanEnd();
      });

      expect(result.current.isPanning).toBe(false);
    });

    it('handlePanMove is a no-op when not panning', () => {
      const { result } = renderHook(() => useTableTransform());

      act(() => {
        result.current.handlePanMove(150, 120);
      });

      expect(result.current.panX).toBe(0);
      expect(result.current.panY).toBe(0);
    });
  });

  // ─── Reset ──────────────────────────────────────────────────────────
  describe('resetView', () => {
    it('resets zoom, panX, and panY to defaults', () => {
      const { result } = renderHook(() => useTableTransform());

      // Change state first
      act(() => {
        result.current.handlePanStart(0, 0);
        result.current.handlePanMove(50, 50);
        result.current.handlePanEnd();
      });
      act(() => {
        result.current.handleWheel({
          deltaY: -100,
          clientX: 400,
          clientY: 300,
          preventDefault: () => {},
        } as unknown as WheelEvent);
      });

      // Now reset
      act(() => {
        result.current.resetView();
      });

      expect(result.current.zoom).toBe(1);
      expect(result.current.panX).toBe(0);
      expect(result.current.panY).toBe(0);
    });
  });

  // ─── Fit to screen ──────────────────────────────────────────────────
  describe('fitToScreen', () => {
    it('calculates zoom to fit table into container (landscape table)', () => {
      const { result } = renderHook(() => useTableTransform());

      act(() => {
        result.current.fitToScreen(1600, 900, 800, 600);
      });

      // Table is 1600x900, container is 800x600
      // Scale by width: 800/1600 = 0.5
      // Scale by height: 600/900 = 0.667
      // Should pick the smaller one: 0.5
      expect(result.current.zoom).toBeCloseTo(0.5, 2);
    });

    it('calculates zoom to fit table into container (portrait table)', () => {
      const { result } = renderHook(() => useTableTransform());

      act(() => {
        result.current.fitToScreen(400, 1200, 800, 600);
      });

      // Scale by width: 800/400 = 2.0
      // Scale by height: 600/1200 = 0.5
      // Should pick the smaller one: 0.5
      expect(result.current.zoom).toBeCloseTo(0.5, 2);
    });

    it('resets pan when fitting to screen', () => {
      const { result } = renderHook(() => useTableTransform());

      // Pan first
      act(() => {
        result.current.handlePanStart(0, 0);
        result.current.handlePanMove(100, 100);
        result.current.handlePanEnd();
      });

      act(() => {
        result.current.fitToScreen(1600, 900, 800, 600);
      });

      // Pan should be reset (or centered)
      // When fitting, we center the content:
      // panX = (containerW - tableW*zoom) / 2 = (800 - 1600*0.5) / 2 = 0
      // panY = (containerH - tableH*zoom) / 2 = (600 - 900*0.5) / 2 = 75
      expect(result.current.panX).toBeCloseTo(0, 1);
      expect(result.current.panY).toBeCloseTo(75, 1);
    });

    it('clamps fit-to-screen zoom within limits', () => {
      const { result } = renderHook(() => useTableTransform());

      // Tiny table, huge container -> would produce zoom > 5
      act(() => {
        result.current.fitToScreen(10, 10, 800, 600);
      });

      expect(result.current.zoom).toBeLessThanOrEqual(5.0);
    });
  });

  // ─── Screen-to-table coordinate conversion ──────────────────────────
  describe('screenToTable', () => {
    it('converts screen coordinates to table coordinates at zoom=1, pan=0', () => {
      const { result } = renderHook(() => useTableTransform());

      act(() => {
        result.current.setContainerRect({ left: 0, top: 0, width: 800, height: 600 });
      });

      const { x, y } = result.current.screenToTable(200, 150);
      expect(x).toBeCloseTo(200, 1);
      expect(y).toBeCloseTo(150, 1);
    });

    it('accounts for zoom when converting coordinates', () => {
      const { result } = renderHook(() => useTableTransform());

      act(() => {
        result.current.setContainerRect({ left: 0, top: 0, width: 800, height: 600 });
      });

      // Manually set zoom to 2x (use multiple wheel events)
      // Instead, let's just test after a zoom-in
      act(() => {
        // Use resetView then directly manipulate... but we can't.
        // Instead zoom in and check the converted coords make sense
        result.current.handleWheel({
          deltaY: -100,
          clientX: 0,
          clientY: 0,
          preventDefault: () => {},
        } as unknown as WheelEvent);
      });

      const zoom = result.current.zoom;
      const panX = result.current.panX;
      const panY = result.current.panY;
      const { x, y } = result.current.screenToTable(200, 150);

      // screenToTable should invert the transform:
      // tableX = (screenX - containerLeft - panX) / zoom
      expect(x).toBeCloseTo((200 - 0 - panX) / zoom, 1);
      expect(y).toBeCloseTo((150 - 0 - panY) / zoom, 1);
    });

    it('accounts for pan when converting coordinates', () => {
      const { result } = renderHook(() => useTableTransform());

      act(() => {
        result.current.setContainerRect({ left: 0, top: 0, width: 800, height: 600 });
      });

      act(() => {
        result.current.handlePanStart(0, 0);
        result.current.handlePanMove(50, 30);
        result.current.handlePanEnd();
      });

      const { x, y } = result.current.screenToTable(200, 150);
      // panX = 50, panY = 30, zoom = 1
      // tableX = (200 - 0 - 50) / 1 = 150
      expect(x).toBeCloseTo(150, 1);
      expect(y).toBeCloseTo(120, 1);
    });
  });
});
