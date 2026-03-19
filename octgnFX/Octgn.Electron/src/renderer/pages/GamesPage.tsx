import React, { useState, useEffect, useRef, useMemo } from 'react';
import { clsx } from 'clsx';
import Button from '../components/Button';
import AppSidebar from '../components/AppSidebar';
import { useDefinitionsStore } from '../stores/definitions-store';
import type { GameUpdate } from '../stores/definitions-store';
import type { GameDefinition, AvailableGame, GameFeed } from '../../shared/types';

type Tab = 'installed' | 'available';

const GamesPage: React.FC = () => {
  const [tab, setTab] = useState<Tab>('installed');
  const [showFeeds, setShowFeeds] = useState(false);
  const [search, setSearch] = useState('');

  const {
    installedGames, availableGames, feeds, installProgress,
    updates, isCheckingUpdates,
    isLoadingInstalled, isLoadingAvailable,
    loadInstalled, fetchAvailable, install, uninstall, checkForUpdates,
    loadFeeds, addFeed, addDirectRepo, addRepoFeed, removeFeed, setFeedEnabled,
  } = useDefinitionsStore();

  // Auto-check for updates once available games are loaded
  const hasCheckedUpdates = useRef(false);
  useEffect(() => {
    if (availableGames.length > 0 && installedGames.length > 0 && !hasCheckedUpdates.current) {
      hasCheckedUpdates.current = true;
      checkForUpdates();
    }
  }, [availableGames, installedGames, checkForUpdates]);

  useEffect(() => {
    loadInstalled();
    loadFeeds();
  }, [loadInstalled, loadFeeds]);

  // Fetch available games on mount so update checks can run
  useEffect(() => {
    if (availableGames.length === 0) fetchAvailable();
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const handleTabChange = (t: Tab) => {
    setSearch('');
    setTab(t);
    if (t === 'available' && availableGames.length === 0) fetchAvailable();
  };

  const updateCount = updates.length;

  const enabledCount = feeds.filter((f) => f.enabled).length;

  return (
    <div className="flex h-full">
      <AppSidebar activePage="games" />

      <div className="flex-1 flex flex-col min-w-0 relative">
        {/* Header */}
        <div className="flex items-center gap-3 px-4 py-3 border-b border-octgn-border/30 flex-wrap gap-y-2">
          <h2 className="font-display text-lg font-semibold tracking-wide text-octgn-text">
            Games
          </h2>
          {/* Tabs */}
          <div className="flex gap-1 bg-octgn-surface rounded-lg p-0.5">
            {(['installed', 'available'] as Tab[]).map((t) => (
              <button
                key={t}
                onClick={() => handleTabChange(t)}
                className={clsx(
                  'px-4 py-1.5 rounded-md text-xs font-medium transition-all duration-150 capitalize',
                  tab === t
                    ? 'bg-octgn-primary/20 text-octgn-primary'
                    : 'text-octgn-text-muted hover:text-octgn-text'
                )}
              >
                {t}
                {t === 'installed' && installedGames.length > 0 && (
                  <span className="ml-1.5 text-[10px] bg-octgn-primary/20 text-octgn-primary rounded-full px-1.5 py-0.5">
                    {installedGames.length}
                  </span>
                )}
                {t === 'installed' && updateCount > 0 && (
                  <span className="ml-1 text-[10px] bg-emerald-500/20 text-emerald-400 rounded-full px-1.5 py-0.5 font-semibold animate-pulse">
                    {updateCount} {updateCount === 1 ? 'update' : 'updates'}
                  </span>
                )}
              </button>
            ))}
          </div>

          {/* Search */}
          <div className="flex-1 min-w-[140px] max-w-xs relative">
            <SearchIcon className="absolute left-2.5 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-octgn-text-dim pointer-events-none" />
            <input
              type="text"
              placeholder={tab === 'installed' ? 'Filter installed…' : 'Search games…'}
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full h-8 pl-8 pr-3 rounded-lg bg-octgn-surface border border-octgn-border/50 text-xs text-octgn-text placeholder-octgn-text-dim outline-none focus:border-octgn-primary/50 transition-all"
            />
          </div>

          <div className="flex items-center gap-2 ml-auto">
            {tab === 'available' && (
              <>
                <button
                  onClick={() => setShowFeeds(!showFeeds)}
                  className={clsx(
                    'flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition-all border',
                    showFeeds
                      ? 'bg-octgn-primary/15 text-octgn-primary border-octgn-primary/30'
                      : 'bg-octgn-surface text-octgn-text-muted border-octgn-border/40 hover:text-octgn-text hover:bg-white/5'
                  )}
                >
                  <FeedsIcon />
                  <span>Feeds</span>
                  <span className={clsx(
                    'text-[10px] rounded-full px-1.5 py-0.5 font-semibold',
                    enabledCount === feeds.length
                      ? 'bg-octgn-primary/20 text-octgn-primary'
                      : 'bg-octgn-warning/20 text-octgn-warning'
                  )}>
                    {enabledCount}/{feeds.length}
                  </span>
                </button>
                <button
                  onClick={fetchAvailable}
                  disabled={isLoadingAvailable}
                  className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium bg-octgn-surface text-octgn-text-muted border border-octgn-border/40 hover:text-octgn-text hover:bg-white/5 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <RefreshIcon className={isLoadingAvailable ? 'animate-spin' : ''} />
                  {isLoadingAvailable ? 'Loading' : 'Refresh'}
                </button>
              </>
            )}
          </div>
        </div>

        {/* Body */}
        <div className="flex flex-1 min-h-0">
          <div className="flex-1 overflow-y-auto">
            {tab === 'installed' && (
              <InstalledTab
                games={installedGames}
                search={search}
                isLoading={isLoadingInstalled}
                updates={updates}
                isCheckingUpdates={isCheckingUpdates}
                onUninstall={uninstall}
                onUpdate={install}
              />
            )}
            {tab === 'available' && (
              <AvailableTab
                games={availableGames}
                search={search}
                installedIds={installedGames.map((g) => g.id)}
                installProgress={installProgress}
                isLoading={isLoadingAvailable}
                onInstall={install}
              />
            )}
          </div>

          {/* Feeds drawer — slides in from right */}
          {showFeeds && tab === 'available' && (
            <>
              {/* Backdrop on narrow windows */}
              <div
                className="absolute inset-0 z-10 bg-black/30 lg:hidden"
                onClick={() => setShowFeeds(false)}
              />
              <FeedsPanel
                feeds={feeds}
                onToggle={setFeedEnabled}
                onRemove={removeFeed}
                onAddNuget={addFeed}
                onAddRepoFeed={addRepoFeed}
                onAddDirectRepo={addDirectRepo}
                onClose={() => setShowFeeds(false)}
              />
            </>
          )}
        </div>
      </div>
    </div>
  );
};

/* ── Installed Tab ─────────────────────────────────────────────────── */

function InstalledTab({
  games, search, isLoading, updates, isCheckingUpdates, onUninstall, onUpdate,
}: {
  games: GameDefinition[];
  search: string;
  isLoading: boolean;
  updates: GameUpdate[];
  isCheckingUpdates: boolean;
  onUninstall: (id: string) => void;
  onUpdate: (id: string, downloadUrl: string) => void;
}) {
  const updateMap = useMemo(
    () => new Map(updates.map((u) => [u.gameId, u])),
    [updates]
  );

  const filtered = useMemo(
    () =>
      search
        ? games.filter((g) => g.name.toLowerCase().includes(search.toLowerCase()))
        : games,
    [games, search]
  );

  // Sort: games with updates first
  const sorted = useMemo(() => {
    return [...filtered].sort((a, b) => {
      const aHas = updateMap.has(a.id) ? 0 : 1;
      const bHas = updateMap.has(b.id) ? 0 : 1;
      if (aHas !== bHas) return aHas - bHas;
      return a.name.localeCompare(b.name);
    });
  }, [filtered, updateMap]);

  if (isLoading) return <LoadingState message="Scanning for installed games..." />;

  if (games.length === 0) {
    return (
      <EmptyState
        icon="📦"
        message="No games installed"
        sub='Browse the "Available" tab to install a game'
      />
    );
  }

  if (filtered.length === 0) {
    return <EmptyState icon="🔍" message={`No games match "${search}"`} />;
  }

  return (
    <div className="p-4 space-y-2">
      {/* Update summary banner */}
      {updates.length > 0 && !search && (
        <div className="flex items-center gap-3 px-4 py-2.5 rounded-xl bg-emerald-500/8 border border-emerald-500/20 mb-1">
          <div className="flex items-center justify-center w-7 h-7 rounded-lg bg-emerald-500/15">
            <UpArrowIcon className="w-3.5 h-3.5 text-emerald-400" />
          </div>
          <p className="text-xs text-emerald-300/90 font-medium">
            {updates.length} {updates.length === 1 ? 'game has' : 'games have'} an update available
          </p>
          {isCheckingUpdates && (
            <span className="ml-auto flex items-center gap-1.5 text-[10px] text-octgn-text-dim">
              <span className="w-2.5 h-2.5 border border-emerald-500/40 border-t-emerald-400 rounded-full animate-spin inline-block" />
              Checking...
            </span>
          )}
        </div>
      )}

      {sorted.map((g) => {
        const update = updateMap.get(g.id);
        return (
          <div
            key={g.id}
            className={clsx(
              'flex items-center gap-4 px-4 py-3 rounded-xl bg-octgn-surface/60 border transition-all',
              update
                ? 'border-emerald-500/25 hover:border-emerald-500/40'
                : 'border-octgn-border/30 hover:border-octgn-border/60'
            )}
          >
            <GameIcon name={g.name} />
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2">
                <p className="text-sm font-semibold text-octgn-text truncate">{g.name}</p>
                {update && (
                  <span className="shrink-0 inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-emerald-500/15 text-emerald-400 text-[10px] font-semibold border border-emerald-500/20">
                    <UpArrowIcon className="w-2.5 h-2.5" />
                    v{update.availableVersion}
                  </span>
                )}
              </div>
              <p className="text-xs text-octgn-text-dim mt-0.5 truncate">
                v{g.version}
                {update && <span className="text-octgn-text-dim/60"> → v{update.availableVersion}</span>}
                {g.description ? <> · <span className="text-octgn-text-muted">{g.description}</span></> : null}
              </p>
            </div>
            <div className="flex items-center gap-2 shrink-0">
              {update && (
                <button
                  onClick={() => onUpdate(g.id, update.downloadUrl)}
                  className="px-3 py-1.5 text-xs font-semibold rounded-lg bg-emerald-500/15 text-emerald-400 border border-emerald-500/25 hover:bg-emerald-500/25 hover:border-emerald-500/40 transition-all"
                >
                  Update
                </button>
              )}
              <button
                onClick={() => onUninstall(g.id)}
                className="px-3 py-1.5 text-xs font-medium rounded-lg text-octgn-danger border border-octgn-danger/30 hover:bg-octgn-danger/10 transition-all"
              >
                Uninstall
              </button>
            </div>
          </div>
        );
      })}
    </div>
  );
}

/* ── Game image with fallback ──────────────────────────────────────── */

function GameImage({ name, iconUrl }: { name: string; iconUrl?: string }) {
  const [failed, setFailed] = React.useState(false);
  if (iconUrl && !failed) {
    return (
      <img
        src={iconUrl}
        alt=""
        className="w-12 h-12 rounded-lg object-contain bg-octgn-surface/80 shrink-0"
        onError={() => setFailed(true)}
      />
    );
  }
  return <GameIcon name={name} size="lg" />;
}

/* ── Available Tab ─────────────────────────────────────────────────── */

function AvailableTab({
  games, search, installedIds, installProgress, isLoading, onInstall,
}: {
  games: AvailableGame[];
  search: string;
  installedIds: string[];
  installProgress: Record<string, { phase: string; percent: number; error?: string }>;
  isLoading: boolean;
  onInstall: (id: string, url: string) => void;
}) {
  const filtered = useMemo(
    () =>
      search
        ? games.filter(
            (g) =>
              g.name.toLowerCase().includes(search.toLowerCase()) ||
              g.description.toLowerCase().includes(search.toLowerCase()) ||
              g.authors.toLowerCase().includes(search.toLowerCase())
          )
        : games,
    [games, search]
  );

  // First load with no data yet — show skeleton
  if (isLoading && games.length === 0) return <LoadingGrid />;

  if (!isLoading && games.length === 0) {
    return (
      <EmptyState
        icon="🌐"
        message="No games found"
        sub="Enable feeds and click Refresh, or check your internet connection"
      />
    );
  }

  if (!isLoading && filtered.length === 0) {
    return <EmptyState icon="🔍" message={`No games match "${search}"`} />;
  }

  return (
    <div className="p-4">
      <div className="flex items-center gap-2 mb-3">
        <p className="text-xs text-octgn-text-dim">
          {filtered.length}{filtered.length !== games.length ? ` of ${games.length}` : ''} game{games.length !== 1 ? 's' : ''} available
        </p>
        {isLoading && (
          <span className="flex items-center gap-1 text-[10px] text-octgn-text-dim">
            <span className="w-2.5 h-2.5 border border-octgn-primary/40 border-t-octgn-primary rounded-full animate-spin inline-block" />
            Refreshing…
          </span>
        )}
      </div>
      {/* Responsive grid: 1 col on narrow, 2 on wider, 3 on wide */}
      <div className={clsx('grid gap-3', isLoading && 'opacity-60 transition-opacity')} style={{ gridTemplateColumns: 'repeat(auto-fill, minmax(240px, 1fr))' }}>
        {filtered.map((g) => {
          const isInstalled = installedIds.includes(g.id);
          const progress = installProgress[g.id];
          const isInstalling = progress && progress.phase !== 'done' && progress.phase !== 'error';
          return (
            <GameCard
              key={g.id}
              game={g}
              isInstalled={isInstalled}
              isInstalling={!!isInstalling}
              progress={progress}
              onInstall={() => onInstall(g.id, g.downloadUrl)}
            />
          );
        })}
      </div>
    </div>
  );
}

function GameCard({
  game, isInstalled, isInstalling, progress, onInstall,
}: {
  game: AvailableGame;
  isInstalled: boolean;
  isInstalling: boolean;
  progress?: { phase: string; percent: number; error?: string };
  onInstall: () => void;
}) {
  const [changelogOpen, setChangelogOpen] = useState(false);

  return (
    <div className="flex flex-col rounded-xl bg-octgn-surface/60 border border-octgn-border/30 hover:border-octgn-border/60 transition-all overflow-hidden">
      {/* Card body */}
      <div className="flex items-start gap-3 p-4 flex-1">
        <GameImage name={game.name} iconUrl={game.iconUrl} />
        <div className="flex-1 min-w-0">
          <p className="text-sm font-semibold text-octgn-text leading-snug">{game.name}</p>
          {game.authors && (
            <p className="text-[11px] text-octgn-text-dim mt-0.5 truncate">{game.authors}</p>
          )}
          {game.description && (
            <p className="text-xs text-octgn-text-muted mt-1.5 line-clamp-2 leading-relaxed">
              {game.description}
            </p>
          )}
          {/* Repo source link */}
          {game.sourceType === 'repo' && game.sourceInfo && (
            <p className="mt-1.5 inline-flex items-center gap-1 text-[10px] text-emerald-400/80 hover:text-emerald-400 transition-colors cursor-default">
              <GitHubMiniIcon />
              <span>{game.sourceInfo}</span>
            </p>
          )}
        </div>
      </div>

      {/* Changelog expandable (repo games only) */}
      {game.sourceType === 'repo' && game.changelog && (
        <div className="px-4 pb-1">
          <button
            onClick={() => setChangelogOpen(!changelogOpen)}
            className="flex items-center gap-1.5 text-[10px] text-octgn-text-dim hover:text-octgn-text-muted transition-colors"
          >
            <svg
              className={clsx('w-2.5 h-2.5 transition-transform duration-150', changelogOpen && 'rotate-90')}
              viewBox="0 0 10 10" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"
            >
              <path d="M3 1.5l4 3.5-4 3.5" />
            </svg>
            Changelog
            {game.versionDate && (
              <span className="text-octgn-text-dim/60 ml-1">· {game.versionDate}</span>
            )}
          </button>
          {changelogOpen && (
            <div className="mt-1.5 mb-1 px-3 py-2 rounded-lg bg-black/15 border border-octgn-border/15 max-h-24 overflow-y-auto">
              <p className="text-[11px] text-octgn-text-muted leading-relaxed whitespace-pre-wrap">
                {game.changelog}
              </p>
            </div>
          )}
        </div>
      )}

      {/* Version date for repo games without changelog */}
      {game.sourceType === 'repo' && !game.changelog && game.versionDate && (
        <div className="px-4 pb-1">
          <p className="text-[10px] text-octgn-text-dim/60">
            Updated {game.versionDate}
          </p>
        </div>
      )}

      {/* Card footer */}
      <div className="flex items-center gap-2 px-4 py-2.5 border-t border-octgn-border/20 bg-black/10">
        <span className="text-[10px] text-octgn-text-dim flex-1 flex items-center gap-1.5 flex-wrap">
          <span>v{game.version}</span>
          {game.downloadCount ? (
            <span className="text-octgn-text-dim">{formatCount(game.downloadCount)} dl</span>
          ) : null}
        </span>

        {/* Progress bar */}
        {isInstalling && progress && (
          <div className="flex items-center gap-2 flex-1">
            <div className="flex-1 h-1 bg-octgn-surface-light rounded-full overflow-hidden">
              <div
                className="h-full bg-octgn-primary rounded-full transition-all duration-300"
                style={{ width: `${progress.percent}%` }}
              />
            </div>
            <span className="text-[10px] text-octgn-text-dim capitalize whitespace-nowrap">
              {progress.phase}...
            </span>
          </div>
        )}
        {progress?.phase === 'error' && (
          <span className="text-[10px] text-octgn-danger truncate" title={progress.error}>
            Install failed
          </span>
        )}

        {!isInstalling && (
          isInstalled ? (
            <span className="flex items-center gap-1 text-[11px] font-medium text-octgn-success">
              <svg className="w-3 h-3" viewBox="0 0 12 12" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
                <path d="M2 6l3 3 5-5" />
              </svg>
              Installed
            </span>
          ) : (
            <button
              onClick={onInstall}
              className="px-3 py-1 text-xs font-semibold rounded-lg bg-octgn-primary text-white hover:bg-octgn-primary/80 active:scale-95 transition-all"
            >
              Install
            </button>
          )
        )}
      </div>
    </div>
  );
}

/* ── Feeds Panel ───────────────────────────────────────────────────── */

type AddFeedType = 'nuget' | 'github' | 'repo';

const FEED_TYPE_BADGE: Record<string, { label: string; color: string; border: string; bg: string }> = {
  'nuget': { label: 'NuGet', color: 'text-blue-400', border: 'border-blue-500/25', bg: 'bg-blue-500/10' },
  'repo-index': { label: 'GitHub', color: 'text-emerald-400', border: 'border-emerald-500/25', bg: 'bg-emerald-500/10' },
  'direct-repo': { label: 'Repo', color: 'text-amber-400', border: 'border-amber-500/25', bg: 'bg-amber-500/10' },
};

function FeedTypeBadge({ feedType }: { feedType: string }) {
  const style = FEED_TYPE_BADGE[feedType] ?? FEED_TYPE_BADGE['nuget'];
  return (
    <span className={clsx('inline-flex items-center px-1.5 py-0.5 rounded text-[9px] font-semibold border', style.color, style.border, style.bg)}>
      {style.label}
    </span>
  );
}

function FeedsPanel({
  feeds, onToggle, onRemove, onAddNuget, onAddRepoFeed, onAddDirectRepo, onClose,
}: {
  feeds: GameFeed[];
  onToggle: (name: string, enabled: boolean) => void;
  onRemove: (name: string) => void;
  onAddNuget: (name: string, url: string) => Promise<{ success: boolean; error?: string }>;
  onAddRepoFeed: (name: string, indexUrl: string) => Promise<{ success: boolean; error?: string }>;
  onAddDirectRepo: (name: string, repoUrl: string, branch?: string) => Promise<{ success: boolean; error?: string }>;
  onClose: () => void;
}) {
  const [addName, setAddName] = useState('');
  const [addUrl, setAddUrl] = useState('');
  const [addBranch, setAddBranch] = useState('');
  const [addError, setAddError] = useState('');
  const [isAdding, setIsAdding] = useState(false);
  const [showAdd, setShowAdd] = useState(false);
  const [addType, setAddType] = useState<AddFeedType>('nuget');
  const nameRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (showAdd) nameRef.current?.focus();
  }, [showAdd]);

  const resetAddForm = () => {
    setAddName(''); setAddUrl(''); setAddBranch(''); setAddError(''); setShowAdd(false);
  };

  const handleAdd = async () => {
    if (!addName.trim()) { setAddError('Name is required'); return; }
    if (!addUrl.trim()) { setAddError('URL is required'); return; }
    setIsAdding(true); setAddError('');

    let result: { success: boolean; error?: string };
    if (addType === 'nuget') {
      result = await onAddNuget(addName.trim(), addUrl.trim());
    } else if (addType === 'github') {
      result = await onAddRepoFeed(addName.trim(), addUrl.trim());
    } else {
      result = await onAddDirectRepo(addName.trim(), addUrl.trim(), addBranch.trim() || undefined);
    }

    setIsAdding(false);
    if (result.success) resetAddForm();
    else setAddError(result.error ?? 'Failed to add feed');
  };

  const urlPlaceholder = addType === 'nuget'
    ? 'https://www.myget.org/F/myfeed/'
    : addType === 'github'
      ? 'https://raw.githubusercontent.com/org/repo/main/index.json'
      : 'owner/repo or https://github.com/owner/repo';

  const addTypeOptions: { key: AddFeedType; label: string }[] = [
    { key: 'nuget', label: 'NuGet' },
    { key: 'github', label: 'GitHub' },
    { key: 'repo', label: 'Repo' },
  ];

  return (
    <div className="relative z-20 w-72 shrink-0 flex flex-col border-l border-octgn-border/30 bg-octgn-bg/95 backdrop-blur-sm">
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 border-b border-octgn-border/20">
        <div>
          <p className="text-sm font-semibold text-octgn-text">Game Feeds</p>
          <p className="text-[11px] text-octgn-text-dim mt-0.5">Toggle sources, add custom feeds</p>
        </div>
        <button
          onClick={onClose}
          className="w-7 h-7 flex items-center justify-center rounded-lg text-octgn-text-dim hover:text-octgn-text hover:bg-white/5 transition-colors"
        >
          <svg className="w-3.5 h-3.5" viewBox="0 0 14 14" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round">
            <path d="M2 2l10 10M12 2L2 12" />
          </svg>
        </button>
      </div>

      {/* Feed list */}
      <div className="flex-1 overflow-y-auto py-2">
        {feeds.map((feed) => (
          <div
            key={feed.name}
            className="flex items-center gap-3 px-4 py-2.5 hover:bg-white/4 transition-colors"
          >
            {/* Toggle — inline styles so position is always correct */}
            <button
              onClick={() => onToggle(feed.name, !feed.enabled)}
              aria-label={feed.enabled ? 'Disable feed' : 'Enable feed'}
              style={{
                position: 'relative',
                width: 40,
                height: 22,
                borderRadius: 11,
                backgroundColor: feed.enabled ? '#3b82f6' : 'rgba(75,85,99,0.5)',
                border: 'none',
                cursor: 'pointer',
                flexShrink: 0,
                transition: 'background-color 200ms',
                padding: 0,
                outline: 'none',
              }}
            >
              <span style={{
                position: 'absolute',
                top: 3,
                left: feed.enabled ? 21 : 3,
                width: 16,
                height: 16,
                borderRadius: '50%',
                backgroundColor: 'white',
                boxShadow: '0 1px 3px rgba(0,0,0,0.35)',
                transition: 'left 180ms ease',
                display: 'block',
              }} />
            </button>

            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-1.5">
                <p className={clsx('text-xs font-medium truncate', feed.enabled ? 'text-octgn-text' : 'text-octgn-text-dim')}>
                  {feed.name}
                </p>
                <FeedTypeBadge feedType={feed.feedType} />
              </div>
              <p className="text-[10px] text-octgn-text-dim truncate mt-0.5">
                {feed.isBuiltIn ? 'Built-in' : (() => { try { return new URL(feed.url).hostname; } catch { return feed.url; } })()}
              </p>
            </div>

            {!feed.isBuiltIn && (
              <button
                onClick={() => onRemove(feed.name)}
                className="w-6 h-6 flex items-center justify-center rounded text-octgn-text-dim hover:text-octgn-danger hover:bg-octgn-danger/10 transition-all shrink-0"
                title="Remove feed"
              >
                <svg className="w-3 h-3" viewBox="0 0 12 12" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round">
                  <path d="M2 2l8 8M10 2L2 10" />
                </svg>
              </button>
            )}
          </div>
        ))}
      </div>

      {/* Add feed */}
      <div className="p-3 border-t border-octgn-border/20">
        {showAdd ? (
          <div className="space-y-2">
            {/* Feed type selector */}
            <div className="flex gap-0.5 bg-octgn-surface rounded-lg p-0.5">
              {addTypeOptions.map((opt) => (
                <button
                  key={opt.key}
                  onClick={() => { setAddType(opt.key); setAddUrl(''); setAddBranch(''); setAddError(''); }}
                  className={clsx(
                    'flex-1 px-2 py-1.5 rounded-md text-[10px] font-semibold transition-all duration-150',
                    addType === opt.key
                      ? 'bg-octgn-primary/20 text-octgn-primary'
                      : 'text-octgn-text-dim hover:text-octgn-text'
                  )}
                >
                  {opt.label}
                </button>
              ))}
            </div>

            <input
              ref={nameRef}
              value={addName}
              onChange={(e) => setAddName(e.target.value)}
              placeholder="Feed name"
              className="w-full h-8 px-3 text-xs rounded-lg bg-octgn-surface border border-octgn-border/50 text-octgn-text placeholder-octgn-text-dim outline-none focus:border-octgn-primary/50 transition-all"
            />
            <input
              value={addUrl}
              onChange={(e) => setAddUrl(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && addType !== 'repo' && handleAdd()}
              placeholder={urlPlaceholder}
              className="w-full h-8 px-3 text-xs rounded-lg bg-octgn-surface border border-octgn-border/50 text-octgn-text placeholder-octgn-text-dim outline-none focus:border-octgn-primary/50 transition-all"
            />
            {addType === 'repo' && (
              <input
                value={addBranch}
                onChange={(e) => setAddBranch(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleAdd()}
                placeholder="Branch (default: main)"
                className="w-full h-8 px-3 text-xs rounded-lg bg-octgn-surface border border-octgn-border/50 text-octgn-text placeholder-octgn-text-dim outline-none focus:border-octgn-primary/50 transition-all"
              />
            )}
            {addError && <p className="text-[10px] text-octgn-danger">{addError}</p>}
            <div className="flex gap-2">
              <button
                onClick={handleAdd}
                disabled={isAdding}
                className="flex-1 h-8 text-xs font-semibold rounded-lg bg-octgn-primary text-white hover:bg-octgn-primary/80 disabled:opacity-50 transition-all"
              >
                {isAdding ? 'Adding...' : addType === 'nuget' ? 'Add Feed' : addType === 'github' ? 'Add Index' : 'Add Repo'}
              </button>
              <button
                onClick={resetAddForm}
                className="h-8 px-3 text-xs rounded-lg text-octgn-text-muted hover:text-octgn-text hover:bg-white/5 transition-all"
              >
                Cancel
              </button>
            </div>
          </div>
        ) : (
          <button
            onClick={() => setShowAdd(true)}
            className="w-full h-9 flex items-center justify-center gap-1.5 text-xs font-medium text-octgn-text-muted border border-dashed border-octgn-border/40 rounded-lg hover:border-octgn-primary/40 hover:text-octgn-primary transition-all"
          >
            <svg className="w-3.5 h-3.5" viewBox="0 0 14 14" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round">
              <path d="M7 1v12M1 7h12" />
            </svg>
            Add Custom Feed
          </button>
        )}
      </div>
    </div>
  );
}

/* ── Shared helpers ────────────────────────────────────────────────── */

function GameIcon({ name, size = 'md' }: { name: string; size?: 'md' | 'lg' }) {
  const initials = name
    .split(/[\s\-:]+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((w) => w[0]?.toUpperCase() ?? '')
    .join('');

  const colors = [
    'from-blue-600/60 to-blue-800/60',
    'from-purple-600/60 to-purple-800/60',
    'from-emerald-600/60 to-emerald-800/60',
    'from-rose-600/60 to-rose-800/60',
    'from-amber-600/60 to-amber-800/60',
    'from-cyan-600/60 to-cyan-800/60',
  ];
  const color = colors[name.charCodeAt(0) % colors.length];
  const dim = size === 'lg' ? 'w-12 h-12 text-base' : 'w-10 h-10 text-sm';

  return (
    <div className={clsx(`shrink-0 rounded-lg bg-gradient-to-br ${color} flex items-center justify-center font-bold text-white`, dim)}>
      {initials}
    </div>
  );
}

function LoadingGrid() {
  return (
    <div className="p-4">
      <div className="h-4 w-32 bg-octgn-surface/80 rounded mb-3 animate-pulse" />
      <div className="grid gap-3" style={{ gridTemplateColumns: 'repeat(auto-fill, minmax(240px, 1fr))' }}>
        {Array.from({ length: 9 }).map((_, i) => (
          <div key={i} className="rounded-xl bg-octgn-surface/60 border border-octgn-border/20 overflow-hidden animate-pulse">
            <div className="flex items-start gap-3 p-4">
              <div className="w-12 h-12 rounded-lg bg-octgn-surface-light/50 shrink-0" />
              <div className="flex-1 space-y-2 pt-1">
                <div className="h-3.5 bg-octgn-surface-light/60 rounded w-3/4" />
                <div className="h-2.5 bg-octgn-surface-light/40 rounded w-1/2" />
                <div className="h-2.5 bg-octgn-surface-light/40 rounded w-full" />
              </div>
            </div>
            <div className="h-10 bg-black/10 border-t border-octgn-border/20" />
          </div>
        ))}
      </div>
    </div>
  );
}

function LoadingState({ message }: { message: string }) {
  return (
    <div className="flex flex-col items-center justify-center h-48 gap-3">
      <div className="w-6 h-6 border-2 border-octgn-primary/30 border-t-octgn-primary rounded-full animate-spin" />
      <p className="text-sm text-octgn-text-dim">{message}</p>
    </div>
  );
}

function EmptyState({ icon, message, sub }: { icon: string; message: string; sub?: string }) {
  return (
    <div className="flex flex-col items-center justify-center h-64 gap-2 text-octgn-text-dim px-6">
      <span className="text-4xl mb-1 opacity-60">{icon}</span>
      <p className="text-sm font-medium text-octgn-text-muted">{message}</p>
      {sub && <p className="text-xs text-center leading-relaxed">{sub}</p>}
    </div>
  );
}

function formatCount(n: number): string {
  if (n >= 1_000_000) return `${(n / 1_000_000).toFixed(1)}M`;
  if (n >= 1_000) return `${(n / 1_000).toFixed(0)}K`;
  return String(n);
}

function SearchIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
      <circle cx="6.5" cy="6.5" r="4" />
      <path d="M10 10l3.5 3.5" />
    </svg>
  );
}

function FeedsIcon() {
  return (
    <svg className="w-3.5 h-3.5" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round">
      <circle cx="3" cy="13" r="1.5" fill="currentColor" stroke="none" />
      <path d="M1.5 8.5A5 5 0 0 1 7.5 13.5" />
      <path d="M1.5 4A9.5 9.5 0 0 1 12 14.5" />
    </svg>
  );
}

function GitHubMiniIcon() {
  return (
    <svg className="w-2.5 h-2.5" viewBox="0 0 16 16" fill="currentColor">
      <path d="M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38 0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82.64-.18 1.32-.27 2-.27.68 0 1.36.09 2 .27 1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55.38A8.013 8.013 0 0016 8c0-4.42-3.58-8-8-8z" />
    </svg>
  );
}

function UpArrowIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 14 14" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
      <path d="M7 12V3" />
      <path d="M3 6l4-4 4 4" />
    </svg>
  );
}

function RefreshIcon({ className }: { className?: string }) {
  return (
    <svg className={clsx('w-3.5 h-3.5', className)} viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
      <path d="M14 8A6 6 0 1 1 9 2.1" />
      <path d="M9 2l4-1-1 4" />
    </svg>
  );
}

export default GamesPage;
