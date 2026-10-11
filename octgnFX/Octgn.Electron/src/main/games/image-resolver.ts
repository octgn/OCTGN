import { access, constants } from 'fs/promises';
import { join } from 'path';
import { log } from '../logger';

export interface ImageResolverIO {
  fileExists: (path: string) => Promise<boolean>;
}

async function defaultFileExists(path: string): Promise<boolean> {
  try {
    await access(path, constants.F_OK);
    return true;
  } catch {
    return false;
  }
}

const defaultIO: ImageResolverIO = {
  fileExists: defaultFileExists,
};

const IMAGE_EXTENSIONS = ['.png', '.jpg', '.bmp'];

/**
 * Resolves card definition GUIDs to image file paths by searching
 * known OCTGN ImageDatabase directories on disk.
 *
 * Search order:
 * 1. %APPDATA%/OCTGN/ImageDatabase/{gameId}/Sets/{setId}/Cards/{cardId}.{ext}
 * 2. %LOCALAPPDATA%/Programs/OCTGN/Data/ImageDatabase/...
 * 3. %LOCALAPPDATA%/Octgn/ImageDatabase/...
 * 4. Proxy images: .../Cards/Proxies/{cardId}.png
 */
export class ImageResolver {
  private cache = new Map<string, string | null>();
  private io: ImageResolverIO;

  constructor(io?: Partial<ImageResolverIO>) {
    this.io = { ...defaultIO, ...io };
  }

  /**
   * Get the list of ImageDatabase base directories to search.
   */
  private getImageDatabasePaths(): string[] {
    const appdata = process.env.APPDATA ?? '';
    const localAppData = process.env.LOCALAPPDATA ?? '';
    return [
      join(appdata, 'OCTGN', 'ImageDatabase'),
      join(localAppData, 'Programs', 'OCTGN', 'Data', 'ImageDatabase'),
      join(localAppData, 'Octgn', 'ImageDatabase'),
    ];
  }

  /**
   * Build all candidate search paths for a card image, in priority order.
   */
  getSearchPaths(gameId: string, setId: string, cardId: string): string[] {
    const baseDirs = this.getImageDatabasePaths();
    const paths: string[] = [];

    // For each base dir, check each extension
    for (const baseDir of baseDirs) {
      const cardsDir = join(baseDir, gameId, 'Sets', setId, 'Cards');
      for (const ext of IMAGE_EXTENSIONS) {
        paths.push(join(cardsDir, `${cardId}${ext}`));
      }
    }

    // Then check proxy paths (only .png for proxies)
    for (const baseDir of baseDirs) {
      const proxyDir = join(baseDir, gameId, 'Sets', setId, 'Cards', 'Proxies');
      paths.push(join(proxyDir, `${cardId}.png`));
    }

    return paths;
  }

  /**
   * Resolve a card's definition ID to an image file path on disk.
   * Returns the first matching file, or null if no image found.
   * Results are cached in memory.
   */
  async resolveCardImage(gameId: string, setId: string, cardId: string): Promise<string | null> {
    const cacheKey = `${gameId}/${setId}/${cardId}`;

    if (this.cache.has(cacheKey)) {
      return this.cache.get(cacheKey)!;
    }

    const paths = this.getSearchPaths(gameId, setId, cardId);

    for (const path of paths) {
      if (await this.io.fileExists(path)) {
        log('IMAGE', `Resolved ${cardId} -> ${path}`);
        this.cache.set(cacheKey, path);
        return path;
      }
    }

    this.cache.set(cacheKey, null);
    return null;
  }

  /**
   * Build a custom protocol URL for loading a card image.
   * The main process registers the octgn-asset:// protocol handler
   * to serve these files.
   */
  buildAssetUrl(gameId: string, setId: string, cardId: string): string {
    return `octgn-asset://card/${gameId}/${setId}/${cardId}`;
  }

  /**
   * Clear the in-memory cache (e.g., after installing new game assets).
   */
  clearCache(): void {
    this.cache.clear();
  }
}
