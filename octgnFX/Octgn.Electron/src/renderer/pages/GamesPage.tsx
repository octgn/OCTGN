import React, { useState, useEffect, useRef } from 'react';
import { clsx } from 'clsx';
import GlassPanel from '../components/GlassPanel';
import Button from '../components/Button';
import AppSidebar from '../components/AppSidebar';
import { useDefinitionsStore } from '../stores/definitions-store';
import type { GameDefinition, AvailableGame, GameFeed } from '../../shared/types';

type Tab = 'installed' | 'available';

const GamesPage: React.FC = () => {
  const [tab, setTab] = useState<Tab>('installed');
  const [showFeeds, setShowFeeds] = useState(false);

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
    setTab(t);
    if (t === 'available' && availableGames.length === 0) {
      fetchAvailable();
    }
  };

  return (
    <div className="flex h-full">
      <AppSidebar activePage="games" />

      <div className="flex-1 flex flex-col min-w-0">
        {/* Header */}
        <div className="flex items-center gap-3 px-5 py-3 border-b border-octgn-border/30">
          <h2 className="font-display text-lg font-semibold tracking-wide text-octgn-text">
            Games
          </h2>
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
              </button>
            ))}
          </div>
          <div className="flex-1" />
          {tab === 'available' && (
            <>
              <Button variant="ghost" size="sm" onClick={() => setShowFeeds(!showFeeds)}>
                <FeedsIcon />
                <span className="ml-1.5">Feeds ({feeds.filter((f) => f.enabled).length}/{feeds.length})</span>
              </Button>
              <Button variant="ghost" size="sm" onClick={fetchAvailable} disabled={isLoadingAvailable}>
                {isLoadingAvailable ? 'Loading…' : 'Refresh'}
              </Button>
            </>
          )}
        </div>

        <div className="flex flex-1 min-h-0">
          {/* Feeds panel (slides in from right when open) */}
          {showFeeds && tab === 'available' && (
            <FeedsPanel
              feeds={feeds}
              onToggle={setFeedEnabled}
              onRemove={removeFeed}
              onAdd={addFeed}
              onClose={() => setShowFeeds(false)}
            />
          )}

          {/* Main content */}
          <div className="flex-1 overflow-y-auto p-4 space-y-2">
            {tab === 'installed' && (
              <InstalledTab
                games={installedGames}
                isLoading={isLoadingInstalled}
                onUninstall={uninstall}
              />
            )}
            {tab === 'available' && (
              <AvailableTab
                games={availableGames}
                installedIds={installedGames.map((g) => g.id)}
                installProgress={installProgress}
                isLoading={isLoadingAvailable}
                onInstall={install}
              />
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

/* ── Feeds Panel ───────────────────────────────────────────────────── */

interface FeedsPanelProps {
  feeds: GameFeed[];
  onToggle: (name: string, enabled: boolean) => void;
  onRemove: (name: string) => void;
  onAdd: (name: string, url: string) => Promise<{ success: boolean; error?: string }>;
  onClose: () => void;
}

function FeedsPanel({ feeds, onToggle, onRemove, onAdd, onClose }: FeedsPanelProps) {
  const [addName, setAddName] = useState('');
  const [addUrl, setAddUrl] = useState('');
  const [addError, setAddError] = useState('');
  const [isAdding, setIsAdding] = useState(false);
  const [showAdd, setShowAdd] = useState(false);
  const nameRef = useRef<HTMLInputElement>(null);

  const handleAdd = async () => {
    if (!addName.trim() || !addUrl.trim()) {
      setAddError('Name and URL are required');
      return;
    }
    setIsAdding(true);
    setAddError('');
    const result = await onAdd(addName.trim(), addUrl.trim());
    setIsAdding(false);
    if (result.success) {
      setAddName('');
      setAddUrl('');
      setShowAdd(false);
    } else {
      setAddError(result.error ?? 'Failed to add feed');
    }
  };

  useEffect(() => {
    if (showAdd) nameRef.current?.focus();
  }, [showAdd]);

  return (
    <div className="w-72 shrink-0 border-l border-octgn-border/30 bg-octgn-surface/40 flex flex-col">
      <div className="flex items-center justify-between px-4 py-3 border-b border-octgn-border/20">
        <span className="text-sm font-semibold text-octgn-text">Game Feeds</span>
        <button
          onClick={onClose}
          className="text-octgn-text-dim hover:text-octgn-text transition-colors"
        >
          <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
            <path d="M4 4l8 8M12 4l-8 8" />
          </svg>
        </button>
      </div>

      <div className="flex-1 overflow-y-auto p-2 space-y-1">
        {feeds.map((feed) => (
          <div
            key={feed.name}
            className="flex items-center gap-2 px-3 py-2 rounded-lg hover:bg-white/5 transition-colors"
          >
            {/* Toggle */}
            <button
              onClick={() => onToggle(feed.name, !feed.enabled)}
              className={clsx(
                'relative w-8 h-4 rounded-full transition-colors duration-200 shrink-0',
                feed.enabled ? 'bg-octgn-primary' : 'bg-octgn-border/50'
              )}
            >
              <span className={clsx(
                'absolute top-0.5 w-3 h-3 rounded-full bg-white shadow transition-transform duration-200',
                feed.enabled ? 'translate-x-4' : 'translate-x-0.5'
              )} />
            </button>
            <div className="flex-1 min-w-0">
              <p className={clsx('text-xs font-medium truncate', feed.enabled ? 'text-octgn-text' : 'text-octgn-text-dim')}>
                {feed.name}
              </p>
              {feed.isBuiltIn && (
                <p className="text-[10px] text-octgn-text-dim">Built-in</p>
              )}
            </div>
            {!feed.isBuiltIn && (
              <button
                onClick={() => onRemove(feed.name)}
                className="text-octgn-text-dim hover:text-octgn-danger transition-colors shrink-0"
                title="Remove feed"
              >
                <svg className="w-3.5 h-3.5" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
                  <path d="M4 4l8 8M12 4l-8 8" />
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
              placeholder="https://…/F/myfeed/"
              className="w-full h-8 px-3 text-xs rounded-lg bg-octgn-surface border border-octgn-border/50 text-octgn-text placeholder-octgn-text-dim outline-none focus:border-octgn-primary/50 transition-all"
            />
            {addError && <p className="text-[10px] text-octgn-danger">{addError}</p>}
            <div className="flex gap-2">
              <Button size="sm" variant="primary" onClick={handleAdd} disabled={isAdding} className="flex-1">
                {isAdding ? 'Adding…' : 'Add'}
              </Button>
              <Button size="sm" variant="ghost" onClick={() => { setShowAdd(false); setAddError(''); }}>
                Cancel
              </Button>
            </div>
          </div>
        ) : (
          <Button size="sm" variant="ghost" onClick={() => setShowAdd(true)} className="w-full">
            + Add Feed
          </Button>
        )}
      </div>
    </div>
  );
}

/* ── Installed Tab ─────────────────────────────────────────────────── */

interface InstalledTabProps {
  games: GameDefinition[];
  isLoading: boolean;
  onUninstall: (id: string) => void;
}

function InstalledTab({ games, isLoading, onUninstall }: InstalledTabProps) {
  if (isLoading) return <EmptyState message="Scanning for installed games…" />;
  if (games.length === 0) {
    return (
      <EmptyState
        message="No games installed"
        sub='Go to the "Available" tab to install a game'
      />
    );
  }
  return (
    <>
      {games.map((g) => (
        <GlassPanel key={g.id} variant="light" padding="none" glow="none">
          <div className="flex items-center gap-4 px-4 py-3">
            <div className="flex-1 min-w-0">
              <p className="text-sm font-semibold text-octgn-text">{g.name}</p>
              <p className="text-xs text-octgn-text-dim mt-0.5">
                v{g.version}
                {g.description ? ` — ${g.description}` : ''}
              </p>
            </div>
            <Button
              variant="ghost"
              size="sm"
              className="text-octgn-danger hover:bg-octgn-danger/10"
              onClick={() => onUninstall(g.id)}
            >
              Uninstall
            </Button>
          </div>
        </GlassPanel>
      ))}
    </>
  );
}

/* ── Available Tab ─────────────────────────────────────────────────── */

interface AvailableTabProps {
  games: AvailableGame[];
  installedIds: string[];
  installProgress: Record<string, { phase: string; percent: number; error?: string }>;
  isLoading: boolean;
  onInstall: (id: string, url: string) => void;
}

function AvailableTab({ games, installedIds, installProgress, isLoading, onInstall }: AvailableTabProps) {
  if (isLoading) {
    return <EmptyState message="Fetching games from feeds…" sub="This may take a moment" />;
  }
  if (games.length === 0) {
    return (
      <EmptyState
        message="No games found"
        sub='Enable feeds and click Refresh, or check that feeds are reachable'
      />
    );
  }
  return (
    <>
      <p className="text-xs text-octgn-text-dim px-1 pb-1">{games.length} games available</p>
      {games.map((g) => {
        const isInstalled = installedIds.includes(g.id);
        const progress = installProgress[g.id];
        const isInstalling = progress && progress.phase !== 'done' && progress.phase !== 'error';
        return (
          <GlassPanel key={g.id} variant="light" padding="none" glow="none">
            <div className="flex items-center gap-4 px-4 py-3">
              {g.iconUrl && (
                <img
                  src={g.iconUrl}
                  alt=""
                  className="w-10 h-10 rounded object-contain bg-octgn-surface shrink-0"
                />
              )}
              <div className="flex-1 min-w-0">
                <p className="text-sm font-semibold text-octgn-text">{g.name}</p>
                <p className="text-xs text-octgn-text-dim mt-0.5 line-clamp-1">
                  {g.description || `v${g.version}`}
                </p>
                {isInstalling && progress && (
                  <div className="mt-1.5">
                    <div className="h-1 bg-octgn-surface rounded-full overflow-hidden w-48">
                      <div
                        className="h-full bg-octgn-primary rounded-full transition-all duration-300"
                        style={{ width: `${progress.percent}%` }}
                      />
                    </div>
                    <p className="text-[10px] text-octgn-text-dim mt-0.5 capitalize">
                      {progress.phase}… {progress.percent}%
                    </p>
                  </div>
                )}
                {progress?.phase === 'error' && (
                  <p className="text-[10px] text-octgn-danger mt-0.5">
                    Error: {progress.error}
                  </p>
                )}
              </div>
              <div className="shrink-0">
                {isInstalled ? (
                  <span className="text-xs text-octgn-success px-2 py-1 rounded bg-octgn-success/10">
                    Installed
                  </span>
                ) : (
                  <Button
                    variant="primary"
                    size="sm"
                    disabled={!!isInstalling}
                    onClick={() => onInstall(g.id, g.downloadUrl)}
                  >
                    {isInstalling ? 'Installing…' : 'Install'}
                  </Button>
                )}
              </div>
            </div>
          </GlassPanel>
        );
      })}
    </>
  );
}

function EmptyState({ message, sub }: { message: string; sub?: string }) {
  return (
    <div className="flex flex-col items-center justify-center h-48 text-octgn-text-dim">
      <p className="text-sm">{message}</p>
      {sub && <p className="text-xs mt-1 text-center px-4">{sub}</p>}
    </div>
  );
}

function FeedsIcon() {
  return (
    <svg className="w-3.5 h-3.5" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round">
      <circle cx="3" cy="13" r="1.5" fill="currentColor" stroke="none" />
      <path d="M1.5 8.5A5 5 0 0 1 7.5 13" />
      <path d="M1.5 4A9 9 0 0 1 12 13" />
    </svg>
  );
}

export default GamesPage;
