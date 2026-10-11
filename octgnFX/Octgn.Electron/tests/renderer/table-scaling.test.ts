import { describe, it, expect } from 'vitest';
import { calculateTableScale, getCardSize } from '@renderer/utils/table-scaling';

describe('calculateTableScale', () => {
  // ─── Basic scale factor calculation ──────────────────────────────────
  describe('scale factor', () => {
    it('returns scale factor that fits table into container (height-limited)', () => {
      // Table 800mm wide, 600mm tall; container 1200px wide, 800px tall
      // scaleX = 1200/800 = 1.5, scaleY = 800/600 = 1.333
      // min(1.5, 1.333) = 1.333 (height-limited)
      const result = calculateTableScale(800, 600, 1200, 800);
      expect(result.scale).toBeCloseTo(800 / 600, 4);
    });

    it('returns scale factor that fits table height into container', () => {
      // Table 400mm wide, 800mm tall; container 1200px wide, 600px tall
      // scaleX = 1200/400 = 3.0, scaleY = 600/800 = 0.75
      // min(3.0, 0.75) = 0.75
      const result = calculateTableScale(400, 800, 1200, 600);
      expect(result.scale).toBeCloseTo(0.75, 4);
    });

    it('returns 1.0 when table and container are same size', () => {
      const result = calculateTableScale(800, 600, 800, 600);
      expect(result.scale).toBeCloseTo(1.0, 4);
    });

    it('returns scale < 1 when table is larger than container', () => {
      const result = calculateTableScale(1600, 1200, 800, 600);
      expect(result.scale).toBeCloseTo(0.5, 4);
    });

    it('returns scale > 1 when table is smaller than container', () => {
      const result = calculateTableScale(400, 300, 800, 600);
      expect(result.scale).toBeCloseTo(2.0, 4);
    });
  });

  // ─── Centering offset ────────────────────────────────────────────────
  describe('centering offset', () => {
    it('centers horizontally when height-limited', () => {
      // Table 800x600, container 1200x600
      // scale = min(1200/800, 600/600) = min(1.5, 1.0) = 1.0
      // scaledWidth = 800*1.0 = 800, offsetX = (1200-800)/2 = 200
      const result = calculateTableScale(800, 600, 1200, 600);
      expect(result.scale).toBeCloseTo(1.0, 4);
      expect(result.offsetX).toBeCloseTo(200, 1);
      expect(result.offsetY).toBeCloseTo(0, 1);
    });

    it('centers vertically when width-limited', () => {
      // Table 800x400, container 800x600
      // scale = min(800/800, 600/400) = min(1.0, 1.5) = 1.0
      // scaledHeight = 400*1.0 = 400, offsetY = (600-400)/2 = 100
      const result = calculateTableScale(800, 400, 800, 600);
      expect(result.scale).toBeCloseTo(1.0, 4);
      expect(result.offsetX).toBeCloseTo(0, 1);
      expect(result.offsetY).toBeCloseTo(100, 1);
    });

    it('has no offset when aspect ratios match exactly', () => {
      const result = calculateTableScale(800, 600, 1600, 1200);
      expect(result.scale).toBeCloseTo(2.0, 4);
      expect(result.offsetX).toBeCloseTo(0, 1);
      expect(result.offsetY).toBeCloseTo(0, 1);
    });
  });

  // ─── Aspect ratio preservation ───────────────────────────────────────
  describe('aspect ratio preservation', () => {
    it('maintains table aspect ratio for landscape table in square container', () => {
      // Table 800x400 (2:1), container 600x600
      // scale = min(600/800, 600/400) = min(0.75, 1.5) = 0.75
      // scaled: 600x300, offset: (0, 150)
      const result = calculateTableScale(800, 400, 600, 600);
      expect(result.scale).toBeCloseTo(0.75, 4);
      const scaledW = 800 * result.scale;
      const scaledH = 400 * result.scale;
      // Aspect ratio of scaled table should match original
      expect(scaledW / scaledH).toBeCloseTo(800 / 400, 4);
    });

    it('maintains table aspect ratio for portrait table in wide container', () => {
      // Table 300x600 (1:2), container 1200x600
      // scale = min(1200/300, 600/600) = min(4.0, 1.0) = 1.0
      const result = calculateTableScale(300, 600, 1200, 600);
      expect(result.scale).toBeCloseTo(1.0, 4);
      const scaledW = 300 * result.scale;
      const scaledH = 600 * result.scale;
      expect(scaledW / scaledH).toBeCloseTo(300 / 600, 4);
    });
  });

  // ─── Edge cases ──────────────────────────────────────────────────────
  describe('edge cases', () => {
    it('returns defaults for zero table width', () => {
      const result = calculateTableScale(0, 600, 800, 600);
      expect(result.scale).toBe(1);
      expect(result.offsetX).toBe(0);
      expect(result.offsetY).toBe(0);
    });

    it('returns defaults for zero table height', () => {
      const result = calculateTableScale(800, 0, 800, 600);
      expect(result.scale).toBe(1);
      expect(result.offsetX).toBe(0);
      expect(result.offsetY).toBe(0);
    });

    it('returns defaults for zero container width', () => {
      const result = calculateTableScale(800, 600, 0, 600);
      expect(result.scale).toBe(1);
      expect(result.offsetX).toBe(0);
      expect(result.offsetY).toBe(0);
    });

    it('returns defaults for zero container height', () => {
      const result = calculateTableScale(800, 600, 800, 0);
      expect(result.scale).toBe(1);
      expect(result.offsetX).toBe(0);
      expect(result.offsetY).toBe(0);
    });

    it('handles very large table dimensions', () => {
      const result = calculateTableScale(10000, 8000, 1920, 1080);
      // scaleX = 1920/10000 = 0.192, scaleY = 1080/8000 = 0.135
      expect(result.scale).toBeCloseTo(0.135, 3);
    });

    it('handles very small table dimensions', () => {
      const result = calculateTableScale(10, 10, 1920, 1080);
      // scaleX = 192, scaleY = 108 -> min = 108
      expect(result.scale).toBeCloseTo(108, 0);
    });
  });

  // ─── Card position mapping ───────────────────────────────────────────
  describe('card position mapping', () => {
    it('card at table center maps to container center', () => {
      const tableW = 800, tableH = 600;
      const containerW = 1600, containerH = 1200;
      const result = calculateTableScale(tableW, tableH, containerW, containerH);
      // scale = 2.0, offsets = 0
      const cardX = 400, cardY = 300; // center of table
      const screenX = cardX * result.scale + result.offsetX;
      const screenY = cardY * result.scale + result.offsetY;
      expect(screenX).toBeCloseTo(800, 1); // center of container
      expect(screenY).toBeCloseTo(600, 1);
    });

    it('card at table origin maps correctly with offset', () => {
      // Table 800x400, container 800x600
      // scale = 1.0, offsetX = 0, offsetY = 100
      const result = calculateTableScale(800, 400, 800, 600);
      const screenX = 0 * result.scale + result.offsetX;
      const screenY = 0 * result.scale + result.offsetY;
      expect(screenX).toBeCloseTo(0, 1);
      expect(screenY).toBeCloseTo(100, 1);
    });

    it('card at table far corner maps to correct screen position', () => {
      // Table 800x600, container 1200x600
      // scale = min(1.5, 1.0) = 1.0
      // offsetX = (1200 - 800)/2 = 200, offsetY = 0
      const result = calculateTableScale(800, 600, 1200, 600);
      const cardX = 800, cardY = 600;
      const screenX = cardX * result.scale + result.offsetX;
      const screenY = cardY * result.scale + result.offsetY;
      expect(screenX).toBeCloseTo(1000, 1); // 800 + 200
      expect(screenY).toBeCloseTo(600, 1);
    });
  });
});

describe('getCardSize', () => {
  it('uses default 100x140 when no game definition card size', () => {
    const size = getCardSize(undefined, undefined);
    expect(size.width).toBe(100);
    expect(size.height).toBe(140);
  });

  it('uses game definition default card size when available', () => {
    const defaultCardSize = { width: 63, height: 88 };
    const size = getCardSize(defaultCardSize, undefined);
    expect(size.width).toBe(63);
    expect(size.height).toBe(88);
  });

  it('uses named card size when specified and available', () => {
    const defaultCardSize = { width: 63, height: 88 };
    const cardSizes = {
      large: { width: 100, height: 150 },
      small: { width: 40, height: 60 },
    };
    const size = getCardSize(defaultCardSize, cardSizes, 'large');
    expect(size.width).toBe(100);
    expect(size.height).toBe(150);
  });

  it('falls back to default card size when named size not found', () => {
    const defaultCardSize = { width: 63, height: 88 };
    const cardSizes = {
      large: { width: 100, height: 150 },
    };
    const size = getCardSize(defaultCardSize, cardSizes, 'nonexistent');
    expect(size.width).toBe(63);
    expect(size.height).toBe(88);
  });

  it('falls back to hardcoded default when named size not found and no default', () => {
    const cardSizes = {
      large: { width: 100, height: 150 },
    };
    const size = getCardSize(undefined, cardSizes, 'nonexistent');
    expect(size.width).toBe(100);
    expect(size.height).toBe(140);
  });
});
