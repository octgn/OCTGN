/** A rectangle in table-space coordinates */
export interface MarqueeRect {
  x: number;
  y: number;
  width: number;
  height: number;
}

/** Minimal card shape needed for hit-testing */
export interface MarqueeCard {
  id: string;
  position: { x: number; y: number };
  size: { width: number; height: number };
}

/**
 * Compute a normalized rectangle from two corner points.
 * Handles drag in any direction (the start/end can be any two corners).
 */
export function computeMarqueeRect(
  startX: number,
  startY: number,
  endX: number,
  endY: number,
): MarqueeRect {
  const x = Math.min(startX, endX);
  const y = Math.min(startY, endY);
  return {
    x,
    y,
    width: Math.abs(endX - startX),
    height: Math.abs(endY - startY),
  };
}

/**
 * Find all cards whose bounding boxes overlap (strictly intersect) the marquee rectangle.
 * Uses axis-aligned bounding box (AABB) overlap test.
 */
export function findCardsInMarquee(
  cards: MarqueeCard[],
  marquee: MarqueeRect,
): string[] {
  if (marquee.width === 0 || marquee.height === 0) return [];

  const mx1 = marquee.x;
  const my1 = marquee.y;
  const mx2 = marquee.x + marquee.width;
  const my2 = marquee.y + marquee.height;

  const result: string[] = [];

  for (const card of cards) {
    const cx1 = card.position.x;
    const cy1 = card.position.y;
    const cx2 = card.position.x + card.size.width;
    const cy2 = card.position.y + card.size.height;

    // AABB strict overlap (exclusive boundaries — touching edges don't count)
    if (cx1 < mx2 && cx2 > mx1 && cy1 < my2 && cy2 > my1) {
      result.push(card.id);
    }
  }

  return result;
}
