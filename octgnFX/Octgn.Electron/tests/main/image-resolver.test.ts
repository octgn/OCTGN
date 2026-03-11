import { describe, it, expect, vi } from 'vitest';
import { ImageResolver, type ImageResolverIO } from '@main/games/image-resolver';

vi.mock('@main/logger', () => ({
  log: vi.fn(),
  logError: vi.fn(),
}));

function createMockIO(existingFiles: Set<string> = new Set()): ImageResolverIO {
  return {
    fileExists: vi.fn().mockImplementation(async (path: string) => existingFiles.has(path)),
  };
}

describe('ImageResolver', () => {
  const GAME_ID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890';
  const SET_ID = 'set-001-guid';
  const CARD_ID = 'card-aaa-guid';

  describe('resolveCardImage', () => {
    it('returns null when no image exists on disk', async () => {
      const io = createMockIO();
      const resolver = new ImageResolver(io);

      const result = await resolver.resolveCardImage(GAME_ID, SET_ID, CARD_ID);
      expect(result).toBeNull();
    });

    it('finds .png image in APPDATA ImageDatabase', async () => {
      const appdata = process.env.APPDATA ?? '';
      const expectedPath = `${appdata}\\OCTGN\\ImageDatabase\\${GAME_ID}\\Sets\\${SET_ID}\\Cards\\${CARD_ID}.png`;
      const io = createMockIO(new Set([expectedPath]));
      const resolver = new ImageResolver(io);

      const result = await resolver.resolveCardImage(GAME_ID, SET_ID, CARD_ID);
      expect(result).toBe(expectedPath);
    });

    it('finds .jpg image when .png does not exist', async () => {
      const appdata = process.env.APPDATA ?? '';
      const jpgPath = `${appdata}\\OCTGN\\ImageDatabase\\${GAME_ID}\\Sets\\${SET_ID}\\Cards\\${CARD_ID}.jpg`;
      const io = createMockIO(new Set([jpgPath]));
      const resolver = new ImageResolver(io);

      const result = await resolver.resolveCardImage(GAME_ID, SET_ID, CARD_ID);
      expect(result).toBe(jpgPath);
    });

    it('finds .bmp image when .png and .jpg do not exist', async () => {
      const appdata = process.env.APPDATA ?? '';
      const bmpPath = `${appdata}\\OCTGN\\ImageDatabase\\${GAME_ID}\\Sets\\${SET_ID}\\Cards\\${CARD_ID}.bmp`;
      const io = createMockIO(new Set([bmpPath]));
      const resolver = new ImageResolver(io);

      const result = await resolver.resolveCardImage(GAME_ID, SET_ID, CARD_ID);
      expect(result).toBe(bmpPath);
    });

    it('prefers .png over .jpg when both exist', async () => {
      const appdata = process.env.APPDATA ?? '';
      const pngPath = `${appdata}\\OCTGN\\ImageDatabase\\${GAME_ID}\\Sets\\${SET_ID}\\Cards\\${CARD_ID}.png`;
      const jpgPath = `${appdata}\\OCTGN\\ImageDatabase\\${GAME_ID}\\Sets\\${SET_ID}\\Cards\\${CARD_ID}.jpg`;
      const io = createMockIO(new Set([pngPath, jpgPath]));
      const resolver = new ImageResolver(io);

      const result = await resolver.resolveCardImage(GAME_ID, SET_ID, CARD_ID);
      expect(result).toBe(pngPath);
    });

    it('checks LOCALAPPDATA alternate paths', async () => {
      const localAppData = process.env.LOCALAPPDATA ?? '';
      const altPath = `${localAppData}\\Programs\\OCTGN\\Data\\ImageDatabase\\${GAME_ID}\\Sets\\${SET_ID}\\Cards\\${CARD_ID}.png`;
      const io = createMockIO(new Set([altPath]));
      const resolver = new ImageResolver(io);

      const result = await resolver.resolveCardImage(GAME_ID, SET_ID, CARD_ID);
      expect(result).toBe(altPath);
    });

    it('checks legacy LOCALAPPDATA path', async () => {
      const localAppData = process.env.LOCALAPPDATA ?? '';
      const legacyPath = `${localAppData}\\Octgn\\ImageDatabase\\${GAME_ID}\\Sets\\${SET_ID}\\Cards\\${CARD_ID}.png`;
      const io = createMockIO(new Set([legacyPath]));
      const resolver = new ImageResolver(io);

      const result = await resolver.resolveCardImage(GAME_ID, SET_ID, CARD_ID);
      expect(result).toBe(legacyPath);
    });

    it('caches results so second call does not hit filesystem', async () => {
      const appdata = process.env.APPDATA ?? '';
      const pngPath = `${appdata}\\OCTGN\\ImageDatabase\\${GAME_ID}\\Sets\\${SET_ID}\\Cards\\${CARD_ID}.png`;
      const io = createMockIO(new Set([pngPath]));
      const resolver = new ImageResolver(io);

      const result1 = await resolver.resolveCardImage(GAME_ID, SET_ID, CARD_ID);
      const result2 = await resolver.resolveCardImage(GAME_ID, SET_ID, CARD_ID);

      expect(result1).toBe(pngPath);
      expect(result2).toBe(pngPath);
      // fileExists should have been called for the first resolution only
      // On the second call, cache is hit — no new fileExists calls
      const callCountAfterFirst = (io.fileExists as ReturnType<typeof vi.fn>).mock.calls.length;
      // The first call checks paths until it finds .png (1 call to find it).
      // The second call should add 0 more calls.
      // Let's just verify: call count should be the same after both calls
      await resolver.resolveCardImage(GAME_ID, SET_ID, CARD_ID);
      expect((io.fileExists as ReturnType<typeof vi.fn>).mock.calls.length).toBe(callCountAfterFirst);
    });

    it('caches null results too', async () => {
      const io = createMockIO();
      const resolver = new ImageResolver(io);

      const result1 = await resolver.resolveCardImage(GAME_ID, SET_ID, CARD_ID);
      const callCount = (io.fileExists as ReturnType<typeof vi.fn>).mock.calls.length;

      const result2 = await resolver.resolveCardImage(GAME_ID, SET_ID, CARD_ID);

      expect(result1).toBeNull();
      expect(result2).toBeNull();
      expect((io.fileExists as ReturnType<typeof vi.fn>).mock.calls.length).toBe(callCount);
    });

    it('checks proxy path as last resort', async () => {
      const appdata = process.env.APPDATA ?? '';
      const proxyPath = `${appdata}\\OCTGN\\ImageDatabase\\${GAME_ID}\\Sets\\${SET_ID}\\Cards\\Proxies\\${CARD_ID}.png`;
      const io = createMockIO(new Set([proxyPath]));
      const resolver = new ImageResolver(io);

      const result = await resolver.resolveCardImage(GAME_ID, SET_ID, CARD_ID);
      expect(result).toBe(proxyPath);
    });
  });

  describe('getSearchPaths', () => {
    it('returns all candidate paths in correct order', () => {
      const io = createMockIO();
      const resolver = new ImageResolver(io);

      const paths = resolver.getSearchPaths(GAME_ID, SET_ID, CARD_ID);

      // Should have multiple base dirs x extensions + proxy paths
      expect(paths.length).toBeGreaterThan(0);

      // First path should be APPDATA .png
      const appdata = process.env.APPDATA ?? '';
      expect(paths[0]).toBe(
        `${appdata}\\OCTGN\\ImageDatabase\\${GAME_ID}\\Sets\\${SET_ID}\\Cards\\${CARD_ID}.png`
      );

      // Should contain .jpg and .bmp variants
      expect(paths.some(p => p.endsWith('.jpg'))).toBe(true);
      expect(paths.some(p => p.endsWith('.bmp'))).toBe(true);

      // Should contain proxy paths
      expect(paths.some(p => p.includes('Proxies'))).toBe(true);
    });
  });

  describe('buildAssetUrl', () => {
    it('builds octgn-asset URL for a card', () => {
      const io = createMockIO();
      const resolver = new ImageResolver(io);

      const url = resolver.buildAssetUrl(GAME_ID, SET_ID, CARD_ID);
      expect(url).toBe(`octgn-asset://card/${GAME_ID}/${SET_ID}/${CARD_ID}`);
    });
  });

  describe('clearCache', () => {
    it('clears the cache so next call hits filesystem again', async () => {
      const appdata = process.env.APPDATA ?? '';
      const pngPath = `${appdata}\\OCTGN\\ImageDatabase\\${GAME_ID}\\Sets\\${SET_ID}\\Cards\\${CARD_ID}.png`;
      const io = createMockIO(new Set([pngPath]));
      const resolver = new ImageResolver(io);

      await resolver.resolveCardImage(GAME_ID, SET_ID, CARD_ID);
      const callCount = (io.fileExists as ReturnType<typeof vi.fn>).mock.calls.length;

      resolver.clearCache();

      await resolver.resolveCardImage(GAME_ID, SET_ID, CARD_ID);
      expect((io.fileExists as ReturnType<typeof vi.fn>).mock.calls.length).toBeGreaterThan(callCount);
    });
  });
});
