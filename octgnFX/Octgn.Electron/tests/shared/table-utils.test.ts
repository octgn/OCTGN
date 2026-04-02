import { describe, it, expect } from 'vitest';
import { isInInvertedZone } from '@shared/table-utils';

describe('isInInvertedZone', () => {
  const cardHeight = 140;

  it('returns false when y >= 0', () => {
    expect(isInInvertedZone(0, cardHeight)).toBe(false);
    expect(isInInvertedZone(100, cardHeight)).toBe(false);
  });

  it('returns false when y is between 0 and -cardHeight/2', () => {
    expect(isInInvertedZone(-30, cardHeight)).toBe(false);
    expect(isInInvertedZone(-69, cardHeight)).toBe(false);
  });

  it('returns false when y is exactly at -cardHeight/2 (not strictly less than)', () => {
    expect(isInInvertedZone(-70, cardHeight)).toBe(false);
  });

  it('returns true when y < -cardHeight/2', () => {
    expect(isInInvertedZone(-71, cardHeight)).toBe(true);
    expect(isInInvertedZone(-200, cardHeight)).toBe(true);
  });

  it('handles zero card height', () => {
    expect(isInInvertedZone(0, 0)).toBe(false);
    expect(isInInvertedZone(-1, 0)).toBe(true);
  });
});
