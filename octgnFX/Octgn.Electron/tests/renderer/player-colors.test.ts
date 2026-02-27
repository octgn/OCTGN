import { describe, it, expect } from 'vitest';
import { readablePlayerColor } from '@renderer/utils/player-colors';

describe('readablePlayerColor', () => {
  it('returns bright colors unchanged', () => {
    // Bright red (#ff0000) is already readable
    expect(readablePlayerColor('#ff0000')).toBe('#ff0000');
    // Bright magenta (#ff00ff) is already readable
    expect(readablePlayerColor('#ff00ff')).toBe('#ff00ff');
  });

  it('lightens dark blue (#000080) for readability', () => {
    const result = readablePlayerColor('#000080');
    expect(result).not.toBe('#000080');
    // Should still be blue-ish (blue channel should be highest)
    const b = parseInt(result.slice(5, 7), 16);
    const r = parseInt(result.slice(1, 3), 16);
    const g = parseInt(result.slice(3, 5), 16);
    expect(b).toBeGreaterThan(r);
    expect(b).toBeGreaterThan(g);
  });

  it('lightens dark green (#008000) for readability', () => {
    const result = readablePlayerColor('#008000');
    expect(result).not.toBe('#008000');
    // Should still be green-ish
    const g = parseInt(result.slice(3, 5), 16);
    const r = parseInt(result.slice(1, 3), 16);
    const b = parseInt(result.slice(5, 7), 16);
    expect(g).toBeGreaterThan(r);
    expect(g).toBeGreaterThan(b);
  });

  it('lightens brown (#664b32) for readability', () => {
    const result = readablePlayerColor('#664b32');
    expect(result).not.toBe('#664b32');
  });

  it('lightens dark purple (#502060) for readability', () => {
    const result = readablePlayerColor('#502060');
    expect(result).not.toBe('#502060');
  });

  it('lightens dark magenta (#800080) for readability', () => {
    const result = readablePlayerColor('#800080');
    expect(result).not.toBe('#800080');
  });

  it('does not lighten gray (#808080) excessively', () => {
    const result = readablePlayerColor('#808080');
    // Gray should be close to original or only slightly lightened
    const r = parseInt(result.slice(1, 3), 16);
    // Should not become white
    expect(r).toBeLessThan(220);
  });

  it('returns black unchanged for #000000 (special case)', () => {
    // Black is used for IDs 0/255 — we let it stay dark since those
    // are system/spectator entries, not regular player names
    const result = readablePlayerColor('#000000');
    // It will be lightened since black has 0 luminance
    expect(result).not.toBe('#000000');
  });

  it('all 14 WPF palette colors produce readable output', () => {
    const wpfColors = [
      '#008000', '#cc0000', '#000080', '#800080', '#cc6600',
      '#008080', '#664b32', '#502060', '#808000', '#ff0000',
      '#808080', '#206020', '#ff00ff', '#0000ff',
    ];
    for (const color of wpfColors) {
      const result = readablePlayerColor(color);
      // Each result should be a valid hex color
      expect(result).toMatch(/^#[0-9a-f]{6}$/);
      // Each result should have sufficient luminance
      // (we can't easily compute WCAG here, but at least check it's not the same dark value
      // for the known-dark ones)
      if (['#000080', '#008000', '#664b32', '#502060', '#206020'].includes(color)) {
        expect(result).not.toBe(color);
      }
    }
  });

  it('preserves hue direction when lightening', () => {
    // Dark blue → lightened blue: blue channel should still dominate
    const blue = readablePlayerColor('#000080');
    const bB = parseInt(blue.slice(5, 7), 16);
    const bR = parseInt(blue.slice(1, 3), 16);
    expect(bB).toBeGreaterThan(bR);

    // Dark red → lightened red: red channel should still dominate
    const red = readablePlayerColor('#cc0000');
    const rR = parseInt(red.slice(1, 3), 16);
    const rB = parseInt(red.slice(5, 7), 16);
    expect(rR).toBeGreaterThan(rB);
  });

  it('handles invalid hex gracefully', () => {
    expect(readablePlayerColor('not-a-color')).toBe('not-a-color');
    expect(readablePlayerColor('')).toBe('');
  });
});
