/**
 * OCTGN Game Feed Service
 * 
 * Manages game packages from OCTGN's MyGet feeds
 * Based on Octgn.Library.FeedProvider
 */

import { useState, useEffect, useCallback } from 'react';

// Feed URLs
const FEEDS = {
  official: 'https://www.myget.org/F/octgngames/',
  community: 'https://www.myget.org/f/octgngamedirectory',
  spoils: 'https://www.myget.org/f/thespoils/',
};

export interface GamePackage {
  id: string;
  name: string;
  version: string;
  description?: string;
  iconUrl?: string;
  authors?: string[];
  tags?: string[];
  summary?: string;
  projectUrl?: string;
  licenseUrl?: string;
  downloadUrl: string;
  dependencies?: string[];
  published?: Date;
  feedUrl: string;
  installed: boolean;
  installPath?: string;
}

export interface GameFeed {
  name: string;
  url: string;
  games: GamePackage[];
  loading: boolean;
  error?: string;
}

/**
 * Parse NuGet package feed (OCTGN uses MyGet which is NuGet-compatible)
 */
function parseNuGetFeed(xmlString: string, feedUrl: string): GamePackage[] {
  const parser = new DOMParser();
  const doc = parser.parseFromString(xmlString, 'application/xml');
  const entries = doc.querySelectorAll('entry');
  
  const packages: GamePackage[] = [];
  
  entries.forEach((entry) => {
    const title = entry.querySelector('title')?.textContent || '';
    const id = entry.querySelector('id')?.textContent?.split('/').pop() || title;
    const updated = entry.querySelector('updated')?.textContent;
    const summary = entry.querySelector('summary')?.textContent || '';
    const content = entry.querySelector('content');
    const downloadUrl = content?.getAttribute('src') || '';
    
    // Parse properties from m:properties namespace
    const properties = entry.querySelectorAll('*');
    const props: Record<string, string> = {};
    
    properties.forEach((prop) => {
      const tagName = prop.tagName.split(':').pop() || '';
      if (prop.textContent) {
        props[tagName] = prop.textContent;
      }
    });
    
    packages.push({
      id: id || props.Id || title.toLowerCase().replace(/\s+/g, '-'),
      name: props.Title || title,
      version: props.Version || '1.0.0',
      description: props.Description || summary,
      iconUrl: props.IconUrl,
      authors: props.Authors?.split(',').map((a) => a.trim()),
      tags: props.Tags?.split(' ').map((t) => t.trim()).filter(Boolean),
      summary: props.Summary || summary,
      projectUrl: props.ProjectUrl,
      licenseUrl: props.LicenseUrl,
      downloadUrl: downloadUrl || `${feedUrl}package/${id}`,
      published: updated ? new Date(updated) : undefined,
      feedUrl,
      installed: false,
    });
  });
  
  return packages;
}

/**
 * Fetch and parse a game feed
 */
async function fetchFeed(url: string): Promise<GamePackage[]> {
  try {
    // Try the NuGet v2 feed format first
    const response = await fetch(`${url}Packages()`, {
      headers: {
        'Accept': 'application/atom+xml',
      },
    });
    
    if (!response.ok) {
      // Try alternate format
      const altResponse = await fetch(url, {
        headers: {
          'Accept': 'application/atom+xml',
        },
      });
      
      if (!altResponse.ok) {
        throw new Error(`Failed to fetch feed: ${response.status}`);
      }
      
      const xmlString = await altResponse.text();
      return parseNuGetFeed(xmlString, url);
    }
    
    const xmlString = await response.text();
    return parseNuGetFeed(xmlString, url);
  } catch (error) {
    console.error(`Failed to fetch feed ${url}:`, error);
    return [];
  }
}

/**
 * Game Feed Service
 */
class GameFeedService {
  private feeds: Map<string, GameFeed> = new Map();
  private installedGames: Map<string, string> = new Map();
  private onGamesChanged?: () => void;

  constructor() {
    // Initialize feeds
    Object.entries(FEEDS).forEach(([name, url]) => {
      this.feeds.set(url, { name, url, games: [], loading: false });
    });
    
    this.loadInstalledGames();
  }

  /**
   * Load installed games from IndexedDB/localStorage
   */
  private loadInstalledGames(): void {
    try {
      const stored = localStorage.getItem('octgn_installed_games');
      if (stored) {
        const parsed = JSON.parse(stored);
        Object.entries(parsed).forEach(([id, path]) => {
          this.installedGames.set(id as string, path as string);
        });
      }
    } catch (e) {
      console.error('Failed to load installed games:', e);
    }
  }

  /**
   * Save installed games list
   */
  private saveInstalledGames(): void {
    const obj = Object.fromEntries(this.installedGames);
    localStorage.setItem('octgn_installed_games', JSON.stringify(obj));
  }

  /**
   * Refresh a specific feed
   */
  async refreshFeed(url: string): Promise<GamePackage[]> {
    const feed = this.feeds.get(url);
    if (!feed) return [];
    
    feed.loading = true;
    feed.error = undefined;
    this.notifyChanged();
    
    try {
      const games = await fetchFeed(url);
      
      // Mark installed games
      games.forEach((game) => {
        game.installed = this.installedGames.has(game.id);
        game.installPath = this.installedGames.get(game.id);
      });
      
      feed.games = games;
      feed.loading = false;
      this.notifyChanged();
      
      return games;
    } catch (error: any) {
      feed.error = error.message;
      feed.loading = false;
      this.notifyChanged();
      return [];
    }
  }

  /**
   * Refresh all feeds
   */
  async refreshAll(): Promise<GamePackage[]> {
    const promises = Array.from(this.feeds.keys()).map((url) => this.refreshFeed(url));
    const results = await Promise.all(promises);
    return results.flat();
  }

  /**
   * Get all games from all feeds
   */
  getAllGames(): GamePackage[] {
    const allGames: GamePackage[] = [];
    this.feeds.forEach((feed) => {
      allGames.push(...feed.games);
    });
    return allGames;
  }

  /**
   * Get games from a specific feed
   */
  getFeedGames(url: string): GamePackage[] {
    return this.feeds.get(url)?.games || [];
  }

  /**
   * Get all feeds
   */
  getFeeds(): GameFeed[] {
    return Array.from(this.feeds.values());
  }

  /**
   * Search games across all feeds
   */
  searchGames(query: string): GamePackage[] {
    const lowerQuery = query.toLowerCase();
    return this.getAllGames().filter((game) => 
      game.name.toLowerCase().includes(lowerQuery) ||
      game.description?.toLowerCase().includes(lowerQuery) ||
      game.tags?.some((tag) => tag.toLowerCase().includes(lowerQuery))
    );
  }

  /**
   * Install a game
   */
  async installGame(gameId: string): Promise<boolean> {
    const game = this.getAllGames().find((g) => g.id === gameId);
    if (!game) {
      throw new Error('Game not found');
    }

    try {
      // In Electron, we'd use the main process to download and extract
      // For now, mark as installed (actual download would be done via IPC)
      
      if (window.electronAPI?.installGame) {
        const result = await window.electronAPI.installGame(game.downloadUrl, game.id);
        if (result.success) {
          this.installedGames.set(gameId, result.path);
          this.saveInstalledGames();
          game.installed = true;
          game.installPath = result.path;
          this.notifyChanged();
          return true;
        }
        throw new Error(result.error || 'Installation failed');
      } else {
        // Browser mode - simulate installation
        console.log('Would install game:', game.downloadUrl);
        this.installedGames.set(gameId, `/games/${gameId}`);
        this.saveInstalledGames();
        game.installed = true;
        game.installPath = `/games/${gameId}`;
        this.notifyChanged();
        return true;
      }
    } catch (error: any) {
      console.error('Failed to install game:', error);
      throw error;
    }
  }

  /**
   * Uninstall a game
   */
  async uninstallGame(gameId: string): Promise<boolean> {
    try {
      if (window.electronAPI?.uninstallGame) {
        const result = await window.electronAPI.uninstallGame(gameId);
        if (!result.success) {
          throw new Error(result.error || 'Uninstallation failed');
        }
      }
      
      this.installedGames.delete(gameId);
      this.saveInstalledGames();
      
      // Update game status
      const game = this.getAllGames().find((g) => g.id === gameId);
      if (game) {
        game.installed = false;
        game.installPath = undefined;
      }
      
      this.notifyChanged();
      return true;
    } catch (error: any) {
      console.error('Failed to uninstall game:', error);
      throw error;
    }
  }

  /**
   * Check if a game is installed
   */
  isInstalled(gameId: string): boolean {
    return this.installedGames.has(gameId);
  }

  /**
   * Get installation path for a game
   */
  getInstallPath(gameId: string): string | undefined {
    return this.installedGames.get(gameId);
  }

  /**
   * Get all installed games
   */
  getInstalledGames(): GamePackage[] {
    return this.getAllGames().filter((g) => g.installed);
  }

  /**
   * Set change callback
   */
  setOnGamesChanged(callback: () => void): void {
    this.onGamesChanged = callback;
  }

  private notifyChanged(): void {
    this.onGamesChanged?.();
  }
}

// Singleton instance
export const gameFeedService = new GameFeedService();

/**
 * React hook for game feeds
 */
export function useGameFeeds() {
  const [feeds, setFeeds] = useState<GameFeed[]>([]);
  const [allGames, setAllGames] = useState<GamePackage[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setFeeds(gameFeedService.getFeeds());
    setAllGames(gameFeedService.getAllGames());
    
    gameFeedService.setOnGamesChanged(() => {
      setFeeds(gameFeedService.getFeeds());
      setAllGames(gameFeedService.getAllGames());
    });
  }, []);

  const refreshAll = useCallback(async () => {
    setLoading(true);
    await gameFeedService.refreshAll();
    setLoading(false);
  }, []);

  return {
    feeds,
    allGames,
    loading,
    refreshAll,
  };
}

/**
 * React hook for installed games
 */
export function useInstalledGames() {
  const [games, setGames] = useState<GamePackage[]>([]);

  useEffect(() => {
    setGames(gameFeedService.getInstalledGames());
    
    gameFeedService.setOnGamesChanged(() => {
      setGames(gameFeedService.getInstalledGames());
    });
  }, []);

  const install = useCallback(async (gameId: string) => {
    await gameFeedService.installGame(gameId);
  }, []);

  const uninstall = useCallback(async (gameId: string) => {
    await gameFeedService.uninstallGame(gameId);
  }, []);

  return {
    games,
    install,
    uninstall,
  };
}

/**
 * React hook for searching games
 */
export function useGameSearch(query: string) {
  const [results, setResults] = useState<GamePackage[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!query.trim()) {
      setResults(gameFeedService.getAllGames());
      return;
    }

    setLoading(true);
    const timer = setTimeout(() => {
      setResults(gameFeedService.searchGames(query));
      setLoading(false);
    }, 300);

    return () => clearTimeout(timer);
  }, [query]);

  return { results, loading };
}

export default gameFeedService;
