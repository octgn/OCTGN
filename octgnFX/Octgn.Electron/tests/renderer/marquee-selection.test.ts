import { describe, it, expect } from 'vitest';
import {
  computeMarqueeRect,
  findCardsInMarquee,
  type MarqueeRect,
} from '@renderer/utils/marquee-selection';

describe('computeMarqueeRect', () => {
  it('computes rect from top-left to bottom-right drag', () => {
    const rect = computeMarqueeRect(10, 20, 100, 80);
    expect(rect).toEqual({ x: 10, y: 20, width: 90, height: 60 });
  });

  it('computes rect from bottom-right to top-left drag', () => {
    const rect = computeMarqueeRect(100, 80, 10, 20);
    expect(rect).toEqual({ x: 10, y: 20, width: 90, height: 60 });
  });

  it('computes rect from top-right to bottom-left drag', () => {
    const rect = computeMarqueeRect(100, 20, 10, 80);
    expect(rect).toEqual({ x: 10, y: 20, width: 90, height: 60 });
  });

  it('computes rect from bottom-left to top-right drag', () => {
    const rect = computeMarqueeRect(10, 80, 100, 20);
    expect(rect).toEqual({ x: 10, y: 20, width: 90, height: 60 });
  });

  it('handles zero-size rect (start === end)', () => {
    const rect = computeMarqueeRect(50, 50, 50, 50);
    expect(rect).toEqual({ x: 50, y: 50, width: 0, height: 0 });
  });
});

describe('findCardsInMarquee', () => {
  const makeCard = (id: string, x: number, y: number, w = 100, h = 140) => ({
    id,
    position: { x, y },
    size: { width: w, height: h },
  });

  it('returns empty array when no cards intersect', () => {
    const cards = [makeCard('1', 500, 500)];
    const marquee: MarqueeRect = { x: 0, y: 0, width: 100, height: 100 };
    expect(findCardsInMarquee(cards, marquee)).toEqual([]);
  });

  it('selects a card fully inside the marquee', () => {
    const cards = [makeCard('1', 10, 10, 50, 70)];
    const marquee: MarqueeRect = { x: 0, y: 0, width: 200, height: 200 };
    expect(findCardsInMarquee(cards, marquee)).toEqual(['1']);
  });

  it('selects a card partially overlapping the marquee', () => {
    const cards = [makeCard('1', 80, 80, 100, 140)];
    // Marquee covers (0,0)-(100,100), card is at (80,80) with size 100x140
    // Card rect: (80, 80, 180, 220) — overlaps with marquee at (80-100, 80-100)
    const marquee: MarqueeRect = { x: 0, y: 0, width: 100, height: 100 };
    expect(findCardsInMarquee(cards, marquee)).toEqual(['1']);
  });

  it('does not select a card that just touches the edge', () => {
    const cards = [makeCard('1', 100, 100, 50, 70)];
    // Marquee goes from (0,0) to (100,100), card starts exactly at (100,100)
    // Edge touching = no overlap (exclusive boundary)
    const marquee: MarqueeRect = { x: 0, y: 0, width: 100, height: 100 };
    expect(findCardsInMarquee(cards, marquee)).toEqual([]);
  });

  it('selects multiple cards inside marquee', () => {
    const cards = [
      makeCard('a', 10, 10, 30, 30),
      makeCard('b', 50, 50, 30, 30),
      makeCard('c', 500, 500, 30, 30), // far away
    ];
    const marquee: MarqueeRect = { x: 0, y: 0, width: 200, height: 200 };
    expect(findCardsInMarquee(cards, marquee)).toEqual(['a', 'b']);
  });

  it('handles zero-size marquee (selects nothing)', () => {
    const cards = [makeCard('1', 0, 0)];
    const marquee: MarqueeRect = { x: 50, y: 50, width: 0, height: 0 };
    expect(findCardsInMarquee(cards, marquee)).toEqual([]);
  });

  it('handles negative card positions (center-origin table)', () => {
    const cards = [makeCard('1', -200, -150, 100, 140)];
    const marquee: MarqueeRect = { x: -250, y: -200, width: 200, height: 200 };
    expect(findCardsInMarquee(cards, marquee)).toEqual(['1']);
  });
});
