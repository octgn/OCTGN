import React, { useState, useEffect, useRef, useMemo } from 'react';
import { clsx } from 'clsx';
import Button from '../components/Button';
import AppSidebar from '../components/AppSidebar';
import { useDefinitionsStore } from '../stores/definitions-store';
import type { GameDefinition, AvailableGame, GameFeed } from '../../shared/types';

type Tab = 'installed' | 'available';

const GamesPage: React.FC = () => {
  const [tab, setTab] = useState<Tab>('installed');
  const [showFeeds, setShowFeeds] = useState(false);
  const [search, setSearch] = useState('');

  const {
    installedGames, availableGames, feeds, installProgress,
    isLoadingInstalled, isLoadingAvailable,
    loadInstalled, fetchAvailable, install, uninstall,
    loadFeeds, addFeed, removeFeed, setFeedEnabled,
  } = useDefinitionsStore();

  useEffect(() => {
    loadInstalled();
    loadFeeds();
  }, [loadInstalled, loadFeeds]);

  const handleTabChange = (t: Tab) => {
    setSearch('');
    setTab(t);
    if (t === 'available' && availableGames.length === 0) fetchAvailable();
  };

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
                onUninstall={uninstall}
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
                onAdd={addFeed}
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
  games, search, isLoading, onUninstall,
}: {
  games: GameDefinition[];
  search: string;
  isLoading: boolean;
  onUninstall: (id: string) => void;
}) {
  const filtered = useMemo(
    () =>
      search
        ? games.filter((g) => g.name.toLowerCase().includes(search.toLowerCase()))
        : games,
    [games, search]
  );

  if (isLoading) return <LoadingState message="Scanning for installed games…" />;

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
      {filtered.map((g) => (
        <div
          key={g.id}
          className="flex items-center gap-4 px-4 py-3 rounded-xl bg-octgn-surface/60 border border-octgn-border/30 hover:border-octgn-border/60 transition-all"
        >
          <GameIcon name={g.name} />
          <div className="flex-1 min-w-0">
            <p className="text-sm font-semibold text-octgn-text truncate">{g.name}</p>
            <p className="text-xs text-octgn-text-dim mt-0.5 truncate">
              v{g.version}
              {g.description ? <> · <span className="text-octgn-text-muted">{g.description}</span></> : null}
            </p>
          </div>
          <button
            onClick={() => onUninstall(g.id)}
            className="shrink-0 px-3 py-1.5 text-xs font-medium rounded-lg text-octgn-danger border border-octgn-danger/30 hover:bg-octgn-danger/10 transition-all"
          >
            Uninstall
          </button>
        </div>
      ))}
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
      <div className={clsx('grid gap-3', isLoading && 'opacity-60 transition-opacity')} style={{ gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))' }}>
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
        </div>
      </div>

      {/* Card footer */}
      <div className="flex items-center gap-2 px-4 py-2.5 border-t border-octgn-border/20 bg-black/10">
        <span className="text-[10px] text-octgn-text-dim flex-1">
          v{game.version}
          {game.downloadCount ? (
            <> · <span className="text-octgn-text-dim">{formatCount(game.downloadCount)} downloads</span></>
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
              {progress.phase}…
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

function FeedsPanel({
  feeds, onToggle, onRemove, onAdd, onClose,
}: {
  feeds: GameFeed[];
  onToggle: (name: string, enabled: boolean) => void;
  onRemove: (name: string) => void;
  onAdd: (name: string, url: string) => Promise<{ success: boolean; error?: string }>;
  onClose: () => void;
}) {
  const [addName, setAddName] = useState('');
  const [addUrl, setAddUrl] = useState('');
  const [addError, setAddError] = useState('');
  const [isAdding, setIsAdding] = useState(false);
  const [showAdd, setShowAdd] = useState(false);
  const nameRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (showAdd) nameRef.current?.focus();
  }, [showAdd]);

  const handleAdd = async () => {
    if (!addName.trim() || !addUrl.trim()) { setAddError('Name and URL are required'); return; }
    setIsAdding(true); setAddError('');
    const result = await onAdd(addName.trim(), addUrl.trim());
    setIsAdding(false);
    if (result.success) { setAddName(''); setAddUrl(''); setShowAdd(false); }
    else setAddError(result.error ?? 'Failed to add feed');
  };

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
              <p className={clsx('text-xs font-medium truncate', feed.enabled ? 'text-octgn-text' : 'text-octgn-text-dim')}>
                {feed.name}
              </p>
              <p className="text-[10px] text-octgn-text-dim truncate">
                {feed.isBuiltIn ? 'Built-in' : new URL(feed.url).hostname}
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
              onKeyDown={(e) => e.key === 'Enter' && handleAdd()}
              placeholder="https://www.myget.org/F/myfeed/"
              className="w-full h-8 px-3 text-xs rounded-lg bg-octgn-surface border border-octgn-border/50 text-octgn-text placeholder-octgn-text-dim outline-none focus:border-octgn-primary/50 transition-all"
            />
            {addError && <p className="text-[10px] text-octgn-danger">{addError}</p>}
            <div className="flex gap-2">
              <button
                onClick={handleAdd}
                disabled={isAdding}
                className="flex-1 h-8 text-xs font-semibold rounded-lg bg-octgn-primary text-white hover:bg-octgn-primary/80 disabled:opacity-50 transition-all"
              >
                {isAdding ? 'Adding…' : 'Add Feed'}
              </button>
              <button
                onClick={() => { setShowAdd(false); setAddError(''); }}
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
      <div className="grid gap-3" style={{ gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))' }}>
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

function RefreshIcon({ className }: { className?: string }) {
  return (
    <svg className={clsx('w-3.5 h-3.5', className)} viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
      <path d="M14 8A6 6 0 1 1 9 2.1" />
      <path d="M9 2l4-1-1 4" />
    </svg>
  );
}

export default GamesPage;
