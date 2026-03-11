/**
 * Table scaling utilities.
 *
 * The OCTGN game protocol uses a virtual coordinate system (in mm units)
 * defined by the game definition's table width/height. Card positions and
 * sizes are all expressed in this virtual space. These utilities compute
 * the scale factor and centering offset needed to map virtual mm-space
 * to screen pixels, maintaining aspect ratio.
 */

export interface TableScaleResult {
  /** Scale factor: multiply mm coordinates by this to get px */
  scale: number;
  /** Horizontal offset (px) to center the table in the container */
  offsetX: number;
  /** Vertical offset (px) to center the table in the container */
  offsetY: number;
}

/**
 * Calculate the scale factor and centering offset to fit a virtual table
 * (in mm units) into a screen container (in px), maintaining aspect ratio.
 *
 * @param tableWidth  - Virtual table width (mm), from game definition
 * @param tableHeight - Virtual table height (mm), from game definition
 * @param containerWidth  - Available screen width (px)
 * @param containerHeight - Available screen height (px)
 * @returns scale factor and centering offsets
 */
export function calculateTableScale(
  tableWidth: number,
  tableHeight: number,
  containerWidth: number,
  containerHeight: number,
): TableScaleResult {
  // Guard against degenerate inputs
  if (
    tableWidth <= 0 ||
    tableHeight <= 0 ||
    containerWidth <= 0 ||
    containerHeight <= 0
  ) {
    return { scale: 1, offsetX: 0, offsetY: 0 };
  }

  const scaleX = containerWidth / tableWidth;
  const scaleY = containerHeight / tableHeight;
  const scale = Math.min(scaleX, scaleY);

  // Center the remaining axis
  const scaledWidth = tableWidth * scale;
  const scaledHeight = tableHeight * scale;
  const offsetX = (containerWidth - scaledWidth) / 2;
  const offsetY = (containerHeight - scaledHeight) / 2;

  return { scale, offsetX, offsetY };
}

/**
 * Default card dimensions (px) when no game definition is available.
 * These match the legacy hardcoded values used throughout the codebase.
 */
export const DEFAULT_CARD_WIDTH = 100;
export const DEFAULT_CARD_HEIGHT = 140;

/**
 * Get the card size to use, resolving from game definition card sizes.
 *
 * Priority:
 * 1. Named card size (if sizeName provided and found in cardSizes map)
 * 2. Default card size from game definition
 * 3. Hardcoded fallback (100x140)
 *
 * @param defaultCardSize - The game definition's default card size
 * @param cardSizes       - Map of named card sizes from the game definition
 * @param sizeName        - Optional named size to look up
 */
export function getCardSize(
  defaultCardSize?: { width: number; height: number },
  cardSizes?: Record<string, { width: number; height: number }>,
  sizeName?: string,
): { width: number; height: number } {
  // Try named size first
  if (sizeName && cardSizes && cardSizes[sizeName]) {
    return {
      width: cardSizes[sizeName].width,
      height: cardSizes[sizeName].height,
    };
  }

  // Fall back to game definition default
  if (defaultCardSize) {
    return {
      width: defaultCardSize.width,
      height: defaultCardSize.height,
    };
  }

  // Hardcoded fallback
  return { width: DEFAULT_CARD_WIDTH, height: DEFAULT_CARD_HEIGHT };
}
