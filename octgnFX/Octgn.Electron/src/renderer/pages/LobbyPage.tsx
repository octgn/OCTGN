import React, { useState, useCallback, useEffect } from 'react';
import { clsx } from 'clsx';
import GlassPanel from '../components/GlassPanel';
import Button from '../components/Button';
import CreateGameDialog from '../components/CreateGameDialog';
import { useLobbyStore } from '../stores/lobby-store';
import { useAuthStore } from '../stores/auth-store';
import { useAppStore } from '../stores/app-store';
import type { HostedGame } from '../../shared/types';

const statusLabel: Record<number, string> = {
  0: 'Unknown',
  1: 'Ready',
  2: 'In Progress',
  3: 'Finished',
};

const statusColor: Record<number, string> = {
  0: 'bg-octgn-text-dim/30 text-octgn-text-dim',
  1: 'bg-octgn-success/15 text-octgn-success border border-octgn-success/30',
  2: 'bg-octgn-warning/15 text-octgn-warning border border-octgn-warning/30',
  3: 'bg-octgn-text-dim/15 text-octgn-text-dim border border-octgn-text-dim/30',
};

const LobbyPage: React.FC = () => {
  const [searchQuery, setSearchQuery] = useState('');
  const [showCreateGame, setShowCreateGame] = useState(false);
  const { games, isLoading, joinGame, startAutoRefresh } = useLobbyStore();
  const user = useAuthStore((s) => s.user);
  const navigate = useAppStore((s) => s.navigate);

  useEffect(() => {
    const cleanup = startAutoRefresh();
    return cleanup;
  }, [startAutoRefresh]);

  const filteredGames = games.filter(
    (g: HostedGame) =>
      g.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      g.gameName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      g.hostUser.username.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleJoin = useCallback(
    async (game: HostedGame) => {
      await joinGame(game.id);
    },
    [joinGame]
  );

  return (
    <div className="flex h-full">
      {/* Sidebar */}
      <aside className="w-56 shrink-0 flex flex-col border-r border-octgn-border/40 bg-octgn-surface/50">
        {/* User info */}
        <div className="p-4 border-b border-octgn-border/30">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-full bg-gradient-to-br from-octgn-primary/40 to-octgn-accent/40 flex items-center justify-center text-sm font-bold text-octgn-text uppercase">
              {user?.username?.charAt(0) || '?'}
            </div>
            <div className="min-w-0">
              <p className="text-sm font-semibold text-octgn-text truncate">
                {user?.username || 'Guest'}
              </p>
              {user?.isSubscriber && (
                <span className="text-[10px] font-medium text-octgn-gold tracking-wide">
                  SUBSCRIBER
                </span>
              )}
            </div>
          </div>
        </div>

        {/* Nav */}
        <nav className="flex-1 p-2 space-y-0.5">
          {([
            { id: 'lobby', label: 'Game Lobby', icon: GamepadIcon },
            { id: 'deck-builder', label: 'Deck Builder', icon: LayersIcon },
            { id: 'profile', label: 'Profile', icon: ProfileIcon },
            { id: 'settings', label: 'Settings', icon: GearIcon },
          ] as const).map((item) => (
            <button
              key={item.id}
              onClick={() => navigate(item.id)}
              className={clsx(
                'w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm transition-all duration-150',
                item.id === 'lobby'
                  ? 'bg-octgn-primary/10 text-octgn-primary'
                  : 'text-octgn-text-muted hover:bg-white/5 hover:text-octgn-text'
              )}
            >
              <item.icon />
              {item.label}
            </button>
          ))}
        </nav>

        {/* Sign out */}
        <div className="p-3 border-t border-octgn-border/30">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => useAuthStore.getState().logout()}
            className="w-full justify-start text-octgn-text-dim"
          >
            Sign Out
          </Button>
        </div>
      </aside>

      {/* Main */}
      <div className="flex-1 flex flex-col min-w-0">
        {/* Toolbar */}
        <div className="flex items-center gap-3 px-5 py-3 border-b border-octgn-border/30">
          <h2 className="font-display text-lg font-semibold tracking-wide text-octgn-text">
            Game Lobby
          </h2>
          <div className="flex-1" />

          {/* Search */}
          <div className="relative">
            <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-octgn-text-dim" />
            <input
              type="text"
              placeholder="Search games..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="h-8 pl-9 pr-3 w-56 rounded-lg bg-octgn-surface border border-octgn-border/50 text-xs text-octgn-text placeholder-octgn-text-dim outline-none focus:border-octgn-primary/50 focus:shadow-[0_0_10px_rgba(59,130,246,0.15)] transition-all"
            />
          </div>

          {/* Refresh indicator */}
          <div className="flex items-center gap-1.5 text-[10px] text-octgn-text-dim">
            <span
              className={clsx(
                'w-1.5 h-1.5 rounded-full',
                isLoading ? 'bg-octgn-warning animate-pulse' : 'bg-octgn-success'
              )}
            />
            {isLoading ? 'Refreshing' : 'Live'}
          </div>

          <Button variant="primary" size="sm" onClick={() => setShowCreateGame(true)}>
            Host Game
          </Button>
        </div>

        {/* Games list */}
        <div className="flex-1 overflow-y-auto p-4 space-y-2">
          {filteredGames.length === 0 && !isLoading && (
            <div className="flex flex-col items-center justify-center h-full text-octgn-text-dim">
              <p className="text-sm">No games found</p>
              <p className="text-xs mt-1">Host a game or wait for others to appear</p>
            </div>
          )}

          {filteredGames.map((game: HostedGame) => (
            <GlassPanel
              key={game.id}
              variant="light"
              padding="none"
              glow="none"
              className="group hover:bg-octgn-surface-light/50 transition-colors duration-150 cursor-pointer"
            >
              <div className="flex items-center gap-4 px-4 py-3">
                {/* Game info */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-semibold text-octgn-text truncate">{game.name}</p>
                    {game.hasPassword && (
                      <svg className="w-3 h-3 text-octgn-text-dim shrink-0" viewBox="0 0 16 16" fill="currentColor">
                        <path d="M11.5 6V4.5a3.5 3.5 0 10-7 0V6H3v7.5A1.5 1.5 0 004.5 15h7a1.5 1.5 0 001.5-1.5V6h-1.5zm-5.5 0V4.5a2 2 0 114 0V6H6z" />
                      </svg>
                    )}
                  </div>
                  <p className="text-xs text-octgn-text-dim mt-0.5">
                    {game.gameName}
                    <span className="mx-1.5 text-octgn-border">|</span>
                    Hosted by {game.hostUser.username}
                  </p>
                </div>

                {/* Players */}
                <div className="text-xs text-octgn-text-muted whitespace-nowrap">
                  {game.playerCount}/{game.maxPlayers}
                </div>

                {/* Status badge */}
                <span
                  className={clsx(
                    'px-2 py-0.5 rounded-full text-[10px] font-medium whitespace-nowrap',
                    statusColor[game.status as number]
                  )}
                >
                  {statusLabel[game.status as number]}
                </span>

                {/* Join */}
                <Button
                  variant="primary"
                  size="sm"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleJoin(game);
                  }}
                  className="opacity-0 group-hover:opacity-100 transition-opacity"
                >
                  Join
                </Button>
              </div>
            </GlassPanel>
          ))}
        </div>
      </div>

      {/* Create Game Dialog */}
      {showCreateGame && (
        <CreateGameDialog
          onClose={() => setShowCreateGame(false)}
          gameDefinitions={[]}
        />
      )}
    </div>
  );
};

/* ── Inline SVG icons ──────────────────────────────────────────────── */

function GamepadIcon() {
  return (
    <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" strokeLinejoin="round">
      <rect x="1" y="4" width="14" height="9" rx="2" />
      <path d="M5 7v3M3.5 8.5h3M10.5 7.5h.01M12.5 9.5h.01" />
    </svg>
  );
}

function LayersIcon() {
  return (
    <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" strokeLinejoin="round">
      <path d="M8 1.5L1.5 5.5 8 9.5l6.5-4L8 1.5z" />
      <path d="M1.5 8l6.5 4 6.5-4" />
      <path d="M1.5 10.5l6.5 4 6.5-4" />
    </svg>
  );
}

function GearIcon() {
  return (
    <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.3">
      <circle cx="8" cy="8" r="2.5" />
      <path d="M8 1v1.5M8 13.5V15M1 8h1.5M13.5 8H15M2.9 2.9l1.05 1.05M12.05 12.05l1.05 1.05M13.1 2.9l-1.05 1.05M3.95 12.05L2.9 13.1" strokeLinecap="round" />
    </svg>
  );
}

function SearchIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
      <circle cx="6.5" cy="6.5" r="4.5" />
      <path d="M10 10l4 4" />
    </svg>
  );
}

function ProfileIcon() {
  return (
    <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" strokeLinejoin="round">
      <circle cx="8" cy="5" r="3" />
      <path d="M2 14c0-3.3 2.7-5 6-5s6 1.7 6 5" />
    </svg>
  );
}

export default LobbyPage;
