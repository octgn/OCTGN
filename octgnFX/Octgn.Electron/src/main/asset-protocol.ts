import { protocol, net } from 'electron';
import { join, normalize, resolve } from 'path';
import { ImageResolver } from './games/image-resolver';
import { findGameDir } from './games/game-store';
import { log, logError } from './logger';
import { pathToFileURL } from 'url';

let imageResolver: ImageResolver | null = null;

/**
 * Set the shared ImageResolver instance used by the protocol handler.
 */
export function setImageResolver(resolver: ImageResolver): void {
  imageResolver = resolver;
}

/**
 * Register the octgn-asset:// custom protocol to serve game assets.
 *
 * URL formats:
 *   octgn-asset://card/{gameId}/{setId}/{cardId}  — card images
 *   octgn-asset://game-file/{gameId}/{relativePath} — arbitrary game directory files (board images, etc.)
 *
 * Must be called after app.whenReady() but the scheme must be
 * registered as privileged before app.ready.
 */
export function registerAssetProtocol(): void {
  protocol.handle('octgn-asset', async (request) => {
    try {
      const url = new URL(request.url);
      const parts = url.pathname.split('/').filter(Boolean);

      if (url.hostname === 'card' && parts.length === 3) {
        // Card image route: octgn-asset://card/{gameId}/{setId}/{cardId}
        const [gameId, setId, cardId] = parts;

        if (!imageResolver) {
          log('ASSET', 'ImageResolver not initialized');
          return new Response('Service unavailable', { status: 503 });
        }

        const filePath = await imageResolver.resolveCardImage(gameId, setId, cardId);
        if (!filePath) {
          return new Response('Not found', { status: 404 });
        }

        return net.fetch(pathToFileURL(filePath).toString());
      }

      if (url.hostname === 'game-file' && parts.length >= 2) {
        // Game file route: octgn-asset://game-file/{gameId}/{relativePath...}
        const gameId = parts[0];
        const relativePath = decodeURIComponent(parts.slice(1).join('/'));

        const gameDir = await findGameDir(gameId);
        if (!gameDir) {
          log('ASSET', `Game ${gameId} not found for file request`);
          return new Response('Not found', { status: 404 });
        }

        // Security: ensure the resolved path stays within the game directory
        const resolvedPath = resolve(join(gameDir, relativePath));
        const normalizedGameDir = normalize(gameDir);
        if (!resolvedPath.startsWith(normalizedGameDir)) {
          log('ASSET', `Path traversal attempt blocked: ${relativePath}`);
          return new Response('Forbidden', { status: 403 });
        }

        return net.fetch(pathToFileURL(resolvedPath).toString());
      }

      log('ASSET', `Invalid asset URL: ${request.url}`);
      return new Response('Not found', { status: 404 });
    } catch (err) {
      logError('ASSET', err);
      return new Response('Internal error', { status: 500 });
    }
  });

  log('ASSET', 'Registered octgn-asset:// protocol');
}

/**
 * Register octgn-asset as a privileged scheme. Must be called before app.ready.
 */
export function registerAssetScheme(): void {
  protocol.registerSchemesAsPrivileged([
    {
      scheme: 'octgn-asset',
      privileges: {
        standard: true,
        secure: true,
        supportFetchAPI: true,
        corsEnabled: true,
      },
    },
  ]);
}
