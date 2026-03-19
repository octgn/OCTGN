import { describe, test, expect, vi, beforeEach } from 'vitest';

// Mock electron
vi.mock('electron', () => ({
  app: {
    getPath: vi.fn().mockReturnValue('/tmp/test-userdata'),
  },
}));

// Mock fs/promises
const mockReadFile = vi.fn();
const mockWriteFile = vi.fn();
const mockMkdir = vi.fn();
vi.mock('fs/promises', async () => {
  return {
    default: {
      readFile: (...args: unknown[]) => mockReadFile(...args),
      writeFile: (...args: unknown[]) => mockWriteFile(...args),
      mkdir: (...args: unknown[]) => mockMkdir(...args),
    },
    readFile: (...args: unknown[]) => mockReadFile(...args),
    writeFile: (...args: unknown[]) => mockWriteFile(...args),
    mkdir: (...args: unknown[]) => mockMkdir(...args),
  };
});

import {
  listFeeds,
  addFeed,
  addDirectRepo,
  addRepoFeed,
  removeFeed,
  setFeedEnabled,
  type GameFeed,
} from '../../src/main/games/feed-manager';

describe('feed-manager', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockReadFile.mockRejectedValue(new Error('ENOENT')); // no saved feeds by default
    mockWriteFile.mockResolvedValue(undefined);
    mockMkdir.mockResolvedValue(undefined);
  });

  test('listFeeds returns built-in feeds including new repo-index feed', async () => {
    const feeds = await listFeeds();

    // Should have the 3 original NuGet feeds plus the new community GitHub feed
    expect(feeds.length).toBe(4);

    const repoFeed = feeds.find((f) => f.feedType === 'repo-index');
    expect(repoFeed).toBeDefined();
    expect(repoFeed!.name).toBe('OCTGN Community (GitHub)');
    expect(repoFeed!.url).toBe(
      'https://raw.githubusercontent.com/octgn/octgn-community-feed/main/index.json',
    );
    expect(repoFeed!.enabled).toBe(true);
    expect(repoFeed!.isBuiltIn).toBe(true);

    // All original feeds should have feedType 'nuget'
    const nugetFeeds = feeds.filter((f) => f.feedType === 'nuget');
    expect(nugetFeeds.length).toBe(3);
  });

  test('backward compat: feeds without feedType default to nuget', async () => {
    // Simulate old saved feeds without feedType
    mockReadFile.mockResolvedValue(
      JSON.stringify([
        { name: 'My Custom Feed', url: 'https://example.com/feed', enabled: true, isBuiltIn: false },
      ]),
    );

    const feeds = await listFeeds();
    const custom = feeds.find((f) => f.name === 'My Custom Feed');
    expect(custom).toBeDefined();
    expect(custom!.feedType).toBe('nuget');
  });

  test('addDirectRepo saves with correct feedType', async () => {
    const result = await addDirectRepo('My Game', 'octocat/my-game');
    expect(result.success).toBe(true);

    // Check what was written
    expect(mockWriteFile).toHaveBeenCalled();
    const written: GameFeed[] = JSON.parse(mockWriteFile.mock.calls[0][1]);
    const saved = written.find((f) => f.name === 'My Game');
    expect(saved).toBeDefined();
    expect(saved!.feedType).toBe('direct-repo');
    expect(saved!.url).toBe('octocat/my-game');
    expect(saved!.isBuiltIn).toBe(false);
    expect(saved!.enabled).toBe(true);
  });

  test('addDirectRepo accepts optional branch parameter', async () => {
    const result = await addDirectRepo('My Game', 'octocat/my-game', 'develop');
    expect(result.success).toBe(true);

    const written: GameFeed[] = JSON.parse(mockWriteFile.mock.calls[0][1]);
    const saved = written.find((f) => f.name === 'My Game');
    expect(saved).toBeDefined();
    expect(saved!.url).toBe('octocat/my-game');
    expect(saved!.branch).toBe('develop');
  });

  test('addRepoFeed saves with correct feedType', async () => {
    const result = await addRepoFeed(
      'Community Index',
      'https://example.com/index.json',
    );
    expect(result.success).toBe(true);

    const written: GameFeed[] = JSON.parse(mockWriteFile.mock.calls[0][1]);
    const saved = written.find((f) => f.name === 'Community Index');
    expect(saved).toBeDefined();
    expect(saved!.feedType).toBe('repo-index');
    expect(saved!.url).toBe('https://example.com/index.json');
    expect(saved!.isBuiltIn).toBe(false);
    expect(saved!.enabled).toBe(true);
  });

  test('addDirectRepo rejects duplicate URLs', async () => {
    // First add succeeds
    mockReadFile.mockRejectedValueOnce(new Error('ENOENT'));
    const first = await addDirectRepo('Game A', 'octocat/my-game');
    expect(first.success).toBe(true);

    // Now simulate saved feeds containing the first entry
    const written: GameFeed[] = JSON.parse(mockWriteFile.mock.calls[0][1]);
    mockReadFile.mockResolvedValue(JSON.stringify(written));

    // Second add with same URL should fail
    const second = await addDirectRepo('Game B', 'octocat/my-game');
    expect(second.success).toBe(false);
    expect(second.error).toBeDefined();
  });

  test('removeFeed works for repo feeds', async () => {
    // Set up a saved direct-repo feed
    mockReadFile.mockResolvedValue(
      JSON.stringify([
        {
          name: 'My Repo Game',
          url: 'octocat/game',
          enabled: true,
          isBuiltIn: false,
          feedType: 'direct-repo',
        },
      ]),
    );

    const result = await removeFeed('My Repo Game');
    expect(result.success).toBe(true);

    const written: GameFeed[] = JSON.parse(mockWriteFile.mock.calls[0][1]);
    expect(written.find((f) => f.name === 'My Repo Game')).toBeUndefined();
  });

  test('setFeedEnabled works for repo feeds', async () => {
    mockReadFile.mockResolvedValue(
      JSON.stringify([
        {
          name: 'My Repo Game',
          url: 'octocat/game',
          enabled: true,
          isBuiltIn: false,
          feedType: 'direct-repo',
        },
      ]),
    );

    await setFeedEnabled('My Repo Game', false);

    const written: GameFeed[] = JSON.parse(mockWriteFile.mock.calls[0][1]);
    const saved = written.find((f) => f.name === 'My Repo Game');
    expect(saved).toBeDefined();
    expect(saved!.enabled).toBe(false);
  });

  test('addFeed still works and defaults to nuget feedType', async () => {
    const result = await addFeed('Legacy Feed', 'https://example.com/nuget/');
    expect(result.success).toBe(true);

    const written: GameFeed[] = JSON.parse(mockWriteFile.mock.calls[0][1]);
    const saved = written.find((f) => f.name === 'Legacy Feed');
    expect(saved).toBeDefined();
    expect(saved!.feedType).toBe('nuget');
  });
});
