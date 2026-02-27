import { Card as CardType, Group } from '../types/game';

export interface RenderCard extends CardType {
  screenX: number;
  screenY: number;
  screenRotation: number;
  zIndex: number;
}

export interface TableConfig {
  width: number;
  height: number;
  backgroundColor: string;
  gridColor: string;
  gridSpacing: number;
  cardWidth: number;
  cardHeight: number;
  cardRadius: number;
}

const DEFAULT_CONFIG: TableConfig = {
  width: 1920,
  height: 1080,
  backgroundColor: '#1a1a2e', // OCTGN dark
  gridColor: 'rgba(147, 112, 219, 0.1)', // Subtle purple grid
  gridSpacing: 50,
  cardWidth: 200,
  cardHeight: 280,
  cardRadius: 12,
};

export class TableRenderer {
  private ctx: CanvasRenderingContext2D;
  private config: TableConfig;
  private imageCache: Map<string, HTMLImageElement> = new Map();
  private pendingImages: Map<string, Promise<HTMLImageElement | null>> = new Map();

  constructor(ctx: CanvasRenderingContext2D, config: Partial<TableConfig> = {}) {
    this.ctx = ctx;
    this.config = { ...DEFAULT_CONFIG, ...config };
  }

  /**
   * Clear and draw the table background
   */
  renderBackground(panOffset: { x: number; y: number }, zoom: number): void {
    const { width, height } = this.ctx.canvas;
    const ctx = this.ctx;

    // Clear with OCTGN dark
    ctx.fillStyle = '#171717';
    ctx.fillRect(0, 0, width, height);

    // Radial gradient for depth - subtle purple glow at center
    const gradient = ctx.createRadialGradient(
      width / 2, height / 2, 0,
      width / 2, height / 2, Math.max(width, height) / 2
    );
    gradient.addColorStop(0, 'rgba(147, 112, 219, 0.05)');
    gradient.addColorStop(0.5, 'rgba(104, 134, 212, 0.02)');
    gradient.addColorStop(1, 'rgba(0, 0, 0, 0)');
    ctx.fillStyle = gradient;
    ctx.fillRect(0, 0, width, height);

    // Table surface with subtle border
    const tableMargin = 20;
    ctx.strokeStyle = 'rgba(74, 74, 74, 0.5)';
    ctx.lineWidth = 1;
    ctx.strokeRect(tableMargin, tableMargin, width - tableMargin * 2, height - tableMargin * 2);

    // Grid - subtle purple lines
    ctx.strokeStyle = this.config.gridColor;
    ctx.lineWidth = 1;

    const gridSize = this.config.gridSpacing * zoom;
    const offsetX = (panOffset.x % gridSize);
    const offsetY = (panOffset.y % gridSize);

    // Vertical lines
    for (let x = offsetX; x < width; x += gridSize) {
      ctx.beginPath();
      ctx.moveTo(x, 0);
      ctx.lineTo(x, height);
      ctx.stroke();
    }

    // Horizontal lines
    for (let y = offsetY; y < height; y += gridSize) {
      ctx.beginPath();
      ctx.moveTo(0, y);
      ctx.lineTo(width, y);
      ctx.stroke();
    }
  }

  /**
   * Render a card on the table
   */
  renderCard(
    card: RenderCard,
    panOffset: { x: number; y: number },
    zoom: number,
    isSelected: boolean,
    isHovered: boolean
  ): void {
    const ctx = this.ctx;
    const { cardWidth, cardHeight, cardRadius } = this.config;

    const x = card.screenX + panOffset.x;
    const y = card.screenY + panOffset.y;
    const w = cardWidth * zoom;
    const h = cardHeight * zoom;
    const rot = card.screenRotation || 0;

    ctx.save();
    ctx.translate(x + w / 2, y + h / 2);
    ctx.rotate((rot * Math.PI) / 180);

    // Card shadow - deeper for depth
    ctx.shadowColor = 'rgba(0, 0, 0, 0.5)';
    ctx.shadowBlur = 15 * zoom;
    ctx.shadowOffsetX = 4 * zoom;
    ctx.shadowOffsetY = 4 * zoom;

    // Card background
    this.drawRoundedRect(ctx, -w / 2, -h / 2, w, h, cardRadius * zoom);
    ctx.fillStyle = card.faceUp ? '#2a2a2a' : '#171717';
    ctx.fill();

    // Reset shadow for border
    ctx.shadowColor = 'transparent';

    // Selection glow - OCTGN purple
    if (isSelected) {
      ctx.shadowColor = 'rgba(147, 112, 219, 0.6)';
      ctx.shadowBlur = 20 * zoom;
      ctx.strokeStyle = '#9370DB';
      ctx.lineWidth = 3 * zoom;
      ctx.stroke();
      ctx.shadowColor = 'transparent';
    } else if (isHovered) {
      ctx.strokeStyle = 'rgba(147, 112, 219, 0.5)';
      ctx.lineWidth = 2 * zoom;
      ctx.stroke();
    } else {
      // Subtle border
      ctx.strokeStyle = 'rgba(74, 74, 74, 0.5)';
      ctx.lineWidth = 1;
      ctx.stroke();
    }

    // Card content
    if (card.faceUp) {
      this.renderCardFace(ctx, card, w, h, zoom);
    } else {
      this.renderCardBack(ctx, w, h, zoom);
    }

    // Markers
    if (card.markers && card.markers.length > 0 && card.faceUp) {
      this.renderMarkers(ctx, card, w, h, zoom);
    }

    // Highlight color overlay
    if (card.highlighted) {
      ctx.fillStyle = card.highlighted + '40'; // 25% opacity
      this.drawRoundedRect(ctx, -w / 2, -h / 2, w, h, cardRadius * zoom);
      ctx.fill();
    }

    // Target indicator
    if (card.targeted) {
      ctx.strokeStyle = '#10b981';
      ctx.lineWidth = 2 * zoom;
      ctx.setLineDash([5 * zoom, 5 * zoom]);
      this.drawRoundedRect(ctx, -w / 2 + 5, -h / 2 + 5, w - 10, h - 10, cardRadius * zoom - 2);
      ctx.stroke();
      ctx.setLineDash([]);
    }

    // Anchored indicator
    if (card.anchored) {
      ctx.fillStyle = '#fbbf24';
      ctx.beginPath();
      ctx.arc(w / 2 - 15 * zoom, -h / 2 + 15 * zoom, 8 * zoom, 0, Math.PI * 2);
      ctx.fill();
      ctx.fillStyle = '#000';
      ctx.font = `${10 * zoom}px sans-serif`;
      ctx.textAlign = 'center';
      ctx.textBaseline = 'middle';
      ctx.fillText('📌', w / 2 - 15 * zoom, -h / 2 + 15 * zoom);
    }

    ctx.restore();
  }

  /**
   * Render the card face
   */
  private renderCardFace(
    ctx: CanvasRenderingContext2D,
    card: RenderCard,
    w: number,
    h: number,
    zoom: number
  ): void {
    const { cardRadius } = this.config;

    // Try to render image if available
    const cachedImage = card.imageUrl ? this.imageCache.get(card.imageUrl) : null;
    
    if (cachedImage) {
      // Draw clipped image
      ctx.save();
      this.drawRoundedRect(ctx, -w / 2 + 2, -h / 2 + 2, w - 4, h - 4, cardRadius * zoom - 1);
      ctx.clip();
      ctx.drawImage(cachedImage, -w / 2 + 2, -h / 2 + 2, w - 4, h - 4);
      ctx.restore();
    } else {
      // Placeholder - OCTGN dark style
      ctx.fillStyle = '#333333';
      this.drawRoundedRect(ctx, -w / 2 + 5, -h / 2 + 5, w - 10, h - 10, cardRadius * zoom - 2);
      ctx.fill();

      // Gradient overlay
      const gradient = ctx.createLinearGradient(0, -h / 2, 0, h / 2);
      gradient.addColorStop(0, 'rgba(255, 255, 255, 0.05)');
      gradient.addColorStop(1, 'rgba(0, 0, 0, 0.1)');
      ctx.fillStyle = gradient;
      this.drawRoundedRect(ctx, -w / 2 + 5, -h / 2 + 5, w - 10, h - 10, cardRadius * zoom - 2);
      ctx.fill();

      // Card name - white text
      ctx.fillStyle = '#ffffff';
      ctx.font = `bold ${14 * zoom}px sans-serif`;
      ctx.textAlign = 'center';
      ctx.textBaseline = 'top';
      const name = card.name || `Card ${card.id}`;
      this.wrapText(ctx, name, 0, -h / 2 + 20 * zoom, w - 20 * zoom, 16 * zoom);

      // Card properties
      if (card.properties?.type) {
        ctx.fillStyle = '#9370DB'; // OCTGN purple
        ctx.font = `${12 * zoom}px sans-serif`;
        ctx.textBaseline = 'middle';
        ctx.fillText(card.properties.type, 0, 0);
      }

      // Card ID (debug)
      ctx.fillStyle = '#6b7280';
      ctx.font = `${10 * zoom}px monospace`;
      ctx.textBaseline = 'bottom';
      ctx.fillText(`#${card.id}`, 0, h / 2 - 10 * zoom);

      // Load image in background if URL exists
      if (card.imageUrl) {
        this.loadImage(card.imageUrl);
      }
    }
  }

  /**
   * Render the card back - OCTGN style
   */
  private renderCardBack(
    ctx: CanvasRenderingContext2D,
    w: number,
    h: number,
    zoom: number
  ): void {
    const { cardRadius } = this.config;

    // Dark background
    ctx.fillStyle = '#171717';
    this.drawRoundedRect(ctx, -w / 2 + 5, -h / 2 + 5, w - 10, h - 10, cardRadius * zoom - 2);
    ctx.fill();

    // Inner border - subtle
    ctx.strokeStyle = 'rgba(147, 112, 219, 0.3)';
    ctx.lineWidth = 2 * zoom;
    this.drawRoundedRect(ctx, -w / 2 + 10, -h / 2 + 10, w - 20, h - 20, cardRadius * zoom - 4);
    ctx.stroke();

    // Diagonal pattern - subtle
    ctx.strokeStyle = 'rgba(74, 74, 74, 0.3)';
    ctx.lineWidth = 1;
    for (let i = -w; i < w; i += 20 * zoom) {
      ctx.beginPath();
      ctx.moveTo(i, -h / 2);
      ctx.lineTo(i + h, h / 2);
      ctx.stroke();
    }

    // Center decoration - OCTGN purple glow
    ctx.shadowColor = 'rgba(147, 112, 219, 0.5)';
    ctx.shadowBlur = 20 * zoom;
    ctx.fillStyle = '#9370DB';
    ctx.beginPath();
    ctx.arc(0, 0, 25 * zoom, 0, Math.PI * 2);
    ctx.fill();
    ctx.shadowColor = 'transparent';

    // Inner circle
    ctx.fillStyle = '#171717';
    ctx.beginPath();
    ctx.arc(0, 0, 18 * zoom, 0, Math.PI * 2);
    ctx.fill();

    // OCTGN text
    ctx.fillStyle = '#9370DB';
    ctx.font = `bold ${10 * zoom}px sans-serif`;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText('OCTGN', 0, 0);
  }

  /**
   * Render card markers
   */
  private renderMarkers(
    ctx: CanvasRenderingContext2D,
    card: RenderCard,
    w: number,
    h: number,
    zoom: number
  ): void {
    if (!card.markers || card.markers.length === 0) return;

    const markerSize = 24 * zoom;
    let mx = -w / 2 + 5;
    const my = h / 2 - markerSize - 5;

    card.markers.forEach((marker) => {
      // Marker background
      ctx.fillStyle = marker.icon || '#ef4444';
      ctx.beginPath();
      ctx.arc(mx + markerSize / 2, my + markerSize / 2, markerSize / 2, 0, Math.PI * 2);
      ctx.fill();

      // Count
      ctx.fillStyle = '#ffffff';
      ctx.font = `bold ${12 * zoom}px sans-serif`;
      ctx.textAlign = 'center';
      ctx.textBaseline = 'middle';
      ctx.fillText(String(marker.count), mx + markerSize / 2, my + markerSize / 2);

      mx += markerSize + 2;
    });
  }

  /**
   * Render a group/pile indicator
   */
  renderGroup(
    group: Group,
    panOffset: { x: number; y: number },
    zoom: number,
    isHovered: boolean
  ): void {
    if (!group.x || !group.y || !group.width || !group.height) return;

    const ctx = this.ctx;
    const x = group.x + panOffset.x;
    const y = group.y + panOffset.y;
    const w = group.width * zoom;
    const h = group.height * zoom;

    // Group border
    ctx.strokeStyle = isHovered ? '#e94560' : '#0f3460';
    ctx.lineWidth = 2;
    ctx.setLineDash([5, 5]);
    ctx.strokeRect(x, y, w, h);
    ctx.setLineDash([]);

    // Group label
    ctx.fillStyle = '#9ca3af';
    ctx.font = `${12 * zoom}px sans-serif`;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'top';
    ctx.fillText(`${group.name} (${group.cards.length})`, x + w / 2, y + 5);

    // Player indicator if owned
    if (group.controllerId) {
      ctx.fillStyle = '#6b7280';
      ctx.font = `${10 * zoom}px sans-serif`;
      ctx.fillText(`Owner: ${group.controllerId}`, x + w / 2, y + h - 15 * zoom);
    }
  }

  /**
   * Render selection box
   */
  renderSelectionBox(
    start: { x: number; y: number },
    end: { x: number; y: number }
  ): void {
    const ctx = this.ctx;
    const x = Math.min(start.x, end.x);
    const y = Math.min(start.y, end.y);
    const w = Math.abs(end.x - start.x);
    const h = Math.abs(end.y - start.y);

    ctx.strokeStyle = '#e94560';
    ctx.lineWidth = 1;
    ctx.setLineDash([5, 5]);
    ctx.strokeRect(x, y, w, h);
    ctx.setLineDash([]);

    ctx.fillStyle = 'rgba(233, 69, 96, 0.1)';
    ctx.fillRect(x, y, w, h);
  }

  /**
   * Draw rounded rectangle path
   */
  private drawRoundedRect(
    ctx: CanvasRenderingContext2D,
    x: number,
    y: number,
    w: number,
    h: number,
    r: number
  ): void {
    ctx.beginPath();
    ctx.moveTo(x + r, y);
    ctx.lineTo(x + w - r, y);
    ctx.quadraticCurveTo(x + w, y, x + w, y + r);
    ctx.lineTo(x + w, y + h - r);
    ctx.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
    ctx.lineTo(x + r, y + h);
    ctx.quadraticCurveTo(x, y + h, x, y + h - r);
    ctx.lineTo(x, y + r);
    ctx.quadraticCurveTo(x, y, x + r, y);
    ctx.closePath();
  }

  /**
   * Wrap text helper
   */
  private wrapText(
    ctx: CanvasRenderingContext2D,
    text: string,
    x: number,
    y: number,
    maxWidth: number,
    lineHeight: number
  ): void {
    const words = text.split(' ');
    let line = '';
    let currentY = y;

    for (const word of words) {
      const testLine = line + word + ' ';
      const metrics = ctx.measureText(testLine);
      if (metrics.width > maxWidth && line !== '') {
        ctx.fillText(line.trim(), x, currentY);
        line = word + ' ';
        currentY += lineHeight;
      } else {
        line = testLine;
      }
    }
    ctx.fillText(line.trim(), x, currentY);
  }

  /**
   * Load and cache an image
   */
  private loadImage(url: string): Promise<HTMLImageElement | null> {
    if (this.imageCache.has(url)) {
      return Promise.resolve(this.imageCache.get(url)!);
    }

    if (this.pendingImages.has(url)) {
      return this.pendingImages.get(url)!;
    }

    const promise = new Promise<HTMLImageElement | null>((resolve) => {
      const img = new Image();
      img.onload = () => {
        this.imageCache.set(url, img);
        this.pendingImages.delete(url);
        resolve(img);
      };
      img.onerror = () => {
        this.pendingImages.delete(url);
        resolve(null);
      };
      img.src = url;
    });

    this.pendingImages.set(url, promise);
    return promise;
  }

  /**
   * Check if a point is inside a card
   */
  hitTestCard(
    card: RenderCard,
    pointX: number,
    pointY: number,
    panOffset: { x: number; y: number },
    zoom: number
  ): boolean {
    const x = card.screenX + panOffset.x;
    const y = card.screenY + panOffset.y;
    const w = this.config.cardWidth * zoom;
    const h = this.config.cardHeight * zoom;

    return pointX >= x && pointX <= x + w && pointY >= y && pointY <= y + h;
  }

  /**
   * Update configuration
   */
  updateConfig(config: Partial<TableConfig>): void {
    this.config = { ...this.config, ...config };
  }
}
