/**
 * Ensure a player color is readable on a dark background.
 *
 * The WPF palette uses many dark colors (dark blue #000080, dark green #008000,
 * brown #664b32, etc.) that are fine on a light UI but illegible on our dark
 * theme (#0a0e17). This function lightens colors that are too dark for text
 * while preserving the hue so the player is still identifiable.
 *
 * The dot/indicator keeps the original color; only text should use this.
 */
export function readablePlayerColor(hex: string): string {
  const rgb = hexToRgb(hex);
  if (!rgb) return hex;

  // Compute relative luminance (WCAG formula)
  const luminance = relativeLuminance(rgb.r, rgb.g, rgb.b);

  // Minimum luminance threshold for readable text on our dark bg (~#0a0e17).
  // WCAG AA requires 4.5:1 contrast for normal text. Our bg luminance is ~0.008.
  // Contrast ratio = (L1 + 0.05) / (L2 + 0.05), solving for L1 >= 0.21 gives ~4.5:1.
  const minLuminance = 0.18;

  if (luminance >= minLuminance) return hex;

  // Lighten by blending toward white until we hit the threshold
  const factor = findLightenFactor(rgb, minLuminance);
  const r = Math.round(rgb.r + (255 - rgb.r) * factor);
  const g = Math.round(rgb.g + (255 - rgb.g) * factor);
  const b = Math.round(rgb.b + (255 - rgb.b) * factor);

  return rgbToHex(r, g, b);
}

function hexToRgb(hex: string): { r: number; g: number; b: number } | null {
  const m = /^#?([0-9a-f]{2})([0-9a-f]{2})([0-9a-f]{2})$/i.exec(hex);
  if (!m) return null;
  return { r: parseInt(m[1], 16), g: parseInt(m[2], 16), b: parseInt(m[3], 16) };
}

function rgbToHex(r: number, g: number, b: number): string {
  return '#' + [r, g, b].map(c => c.toString(16).padStart(2, '0')).join('');
}

function linearize(c: number): number {
  const s = c / 255;
  return s <= 0.03928 ? s / 12.92 : Math.pow((s + 0.055) / 1.055, 2.4);
}

function relativeLuminance(r: number, g: number, b: number): number {
  return 0.2126 * linearize(r) + 0.7152 * linearize(g) + 0.0722 * linearize(b);
}

function findLightenFactor(rgb: { r: number; g: number; b: number }, target: number): number {
  let lo = 0, hi = 1;
  for (let i = 0; i < 20; i++) {
    const mid = (lo + hi) / 2;
    const r = rgb.r + (255 - rgb.r) * mid;
    const g = rgb.g + (255 - rgb.g) * mid;
    const b = rgb.b + (255 - rgb.b) * mid;
    if (relativeLuminance(r, g, b) < target) {
      lo = mid;
    } else {
      hi = mid;
    }
  }
  return hi;
}
