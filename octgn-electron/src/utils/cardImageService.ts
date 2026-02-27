/**
 * Card Image Service
 * 
 * Handles loading and caching card images from:
 * - Game packages (local files)
 * - Remote URLs
 * - Proxy generated images
 */

interface CachedImage {
  url: string;
  blob: Blob;
  loadedAt: number;
}

class CardImageService {
  private cache: Map<string, CachedImage> = new Map();
  private pending: Map<string, Promise<string>> = new Map();
  private maxCacheSize = 500; // Maximum cached images
  private cacheExpiry = 24 * 60 * 60 * 1000; // 24 hours

  /**
   * Get a card image URL, loading it if necessary
   */
  async getImageUrl(
    source: string | { type: 'package'; path: string; gameId: string } | { type: 'url'; url: string }
  ): Promise<string> {
    const cacheKey = this.getCacheKey(source);

    // Check cache
    const cached = this.cache.get(cacheKey);
    if (cached && Date.now() - cached.loadedAt < this.cacheExpiry) {
      return cached.url;
    }

    // Check if already loading
    const pending = this.pending.get(cacheKey);
    if (pending) {
      return pending;
    }

    // Load image
    const loadPromise = this.loadImage(source);
    this.pending.set(cacheKey, loadPromise);

    try {
      const url = await loadPromise;
      return url;
    } finally {
      this.pending.delete(cacheKey);
    }
  }

  /**
   * Preload multiple card images
   */
  async preloadImages(
    sources: Array<string | { type: 'package'; path: string; gameId: string } | { type: 'url'; url: string }>
  ): Promise<void> {
    await Promise.all(sources.map(source => this.getImageUrl(source).catch(() => {})));
  }

  /**
   * Clear the image cache
   */
  clearCache(): void {
    // Revoke object URLs
    for (const cached of this.cache.values()) {
      if (cached.url.startsWith('blob:')) {
        URL.revokeObjectURL(cached.url);
      }
    }
    this.cache.clear();
  }

  /**
   * Get cache statistics
   */
  getStats(): { size: number; maxSize: number } {
    return {
      size: this.cache.size,
      maxSize: this.maxCacheSize,
    };
  }

  private getCacheKey(source: any): string {
    if (typeof source === 'string') {
      return source;
    }
    if (source.type === 'package') {
      return `pkg:${source.gameId}:${source.path}`;
    }
    if (source.type === 'url') {
      return `url:${source.url}`;
    }
    return JSON.stringify(source);
  }

  private async loadImage(
    source: string | { type: 'package'; path: string; gameId: string } | { type: 'url'; url: string }
  ): Promise<string> {
    let blob: Blob;
    let cacheKey = this.getCacheKey(source);

    if (typeof source === 'string') {
      // Treat as URL
      blob = await this.fetchUrl(source);
    } else if (source.type === 'url') {
      blob = await this.fetchUrl(source.url);
    } else if (source.type === 'package') {
      blob = await this.loadFromPackage(source.gameId, source.path);
    } else {
      throw new Error('Unknown image source type');
    }

    // Create object URL
    const url = URL.createObjectURL(blob);

    // Cache it
    this.cache.set(cacheKey, {
      url,
      blob,
      loadedAt: Date.now(),
    });

    // Prune cache if needed
    this.pruneCache();

    return url;
  }

  private async fetchUrl(url: string): Promise<Blob> {
    const response = await fetch(url);
    if (!response.ok) {
      throw new Error(`Failed to fetch image: ${response.status}`);
    }
    return response.blob();
  }

  private async loadFromPackage(gameId: string, path: string): Promise<Blob> {
    // In Electron, this would read from the game package directory
    // For now, return a placeholder
    if (window.electronAPI?.readFile) {
      // Would use proper binary file reading in production
      throw new Error('Package loading not implemented');
    }
    throw new Error('Package loading requires Electron');
  }

  private pruneCache(): void {
    if (this.cache.size <= this.maxCacheSize) return;

    // Remove oldest entries
    const entries = Array.from(this.cache.entries())
      .sort((a, b) => a[1].loadedAt - b[1].loadedAt);

    const toRemove = entries.slice(0, this.cache.size - this.maxCacheSize);
    for (const [key, cached] of toRemove) {
      if (cached.url.startsWith('blob:')) {
        URL.revokeObjectURL(cached.url);
      }
      this.cache.delete(key);
    }
  }
}

// Singleton instance
export const cardImageService = new CardImageService();

/**
 * React hook for loading card images
 */
export function useCardImage(
  source: string | { type: 'package'; path: string; gameId: string } | { type: 'url'; url: string } | null
): { url: string | null; loading: boolean; error: Error | null } {
  const [url, setUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    if (!source) {
      setUrl(null);
      setLoading(false);
      setError(null);
      return;
    }

    setLoading(true);
    setError(null);

    cardImageService.getImageUrl(source)
      .then(setUrl)
      .catch(setError)
      .finally(() => setLoading(false));
  }, [typeof source === 'string' ? source : JSON.stringify(source)]);

  return { url, loading, error };
}

// Need useState and useEffect for the hook
import { useState, useEffect } from 'react';
