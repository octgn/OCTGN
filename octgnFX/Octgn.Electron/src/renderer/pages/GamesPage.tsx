import React, { useState, useEffect } from 'react';
import { clsx } from 'clsx';
import GlassPanel from '../components/GlassPanel';
import Button from '../components/Button';
import AppSidebar from '../components/AppSidebar';
import { useDefinitionsStore } from '../stores/definitions-store';
import type { GameDefinition, AvailableGame } from '../../shared/types';

type Tab = 'installed' | 'available';

const GamesPage: React.FC = () => {
  const [tab, setTab] = useState<Tab>('installed');

  const {
    installedGames,
    availableGames,
    installProgress,
    isLoadingInstalled,
    isLoadingAvailable,
    loadInstalled,
    fetchAvailable,
    install,
    uninstall,
  } = useDefinitionsStore();

  useEffect(() => {
    loadInstalled();
  }, [loadInstalled]);

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
        <div className="flex items-center gap-4 px-5 py-3 border-b border-octgn-border/30">
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
            <Button
              variant="ghost"
              size="sm"
              onClick={fetchAvailable}
              disabled={isLoadingAvailable}
            >
              {isLoadingAvailable ? 'Loading…' : 'Refresh'}
            </Button>
          )}
        </div>

        {/* Content */}
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
  );
};

/* ── Installed Tab ─────────────────────────────────────────────────── */

interface InstalledTabProps {
  games: GameDefinition[];
  isLoading: boolean;
  onUninstall: (id: string) => void;
}

function InstalledTab({ games, isLoading, onUninstall }: InstalledTabProps) {
  if (isLoading) {
    return <EmptyState message="Scanning for installed games…" />;
  }
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
    return <EmptyState message="Fetching available games from octgn.net…" />;
  }
  if (games.length === 0) {
    return <EmptyState message="No games found" sub="Click Refresh to load games from the feed" />;
  }
  return (
    <>
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
                <p className="text-xs text-octgn-text-dim mt-0.5 line-clamp-2">
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
      {sub && <p className="text-xs mt-1">{sub}</p>}
    </div>
  );
}

export default GamesPage;
